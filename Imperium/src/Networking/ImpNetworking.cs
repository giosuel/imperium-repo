#region

using System;
using System.Collections;
using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Extensions;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking;
using Steamworks;
using UnityEngine;

#endregion

namespace Imperium.Networking;

public class ImpNetworking
{
    private readonly HashSet<INetworkSubscribable> RegisteredNetworkSubscribers = [];

    internal static readonly ImpBinding<int> ConnectedPlayers = new(1);
    internal readonly ImpNetworkBinding<HashSet<ulong>> ImperiumUsers;

    /// <summary>
    ///     Set to true, when Imperium access is first granted. Always set to true on the host.
    /// </summary>
    internal static bool WasImperiumAccessGranted { get; private set; }

    private readonly ImpNetEvent authenticateEvent;
    private readonly ImpNetEvent enableImperiumEvent;
    private readonly ImpNetEvent disableImperiumEvent;
    private readonly ImpNetEvent clientRequestValues;

    private readonly ImpNetMessage<NetworkNotification> networkLog;
    private readonly Dictionary<string, ImpSubscription> subscribers = new();

    public ImpNetworking()
    {
        RepoSteamNetwork.RegisterPacket<ImpPacket>();
        RepoSteamNetwork.AddCallback<ImpPacket>(OnPacketReceived);

        authenticateEvent = new ImpNetEvent("AuthenticateImperium", this, allowUnauthenticated: true);
        enableImperiumEvent = new ImpNetEvent("EnableImperium", this);
        disableImperiumEvent = new ImpNetEvent("DisableImperium", this);
        networkLog = new ImpNetMessage<NetworkNotification>("NetworkLog", this);
        clientRequestValues = new ImpNetEvent("ClientRequestValues", this);

        ImperiumUsers = new ImpNetworkBinding<HashSet<ulong>>(
            "ImperiumUsers",
            this,
            currentValue: [0]
        );
        authenticateEvent.OnServerReceive += OnAuthenticateRequest;
        clientRequestValues.OnServerReceive += OnClientRequestValues;
        enableImperiumEvent.OnClientRecive += OnEnableImperiumAccess;
        disableImperiumEvent.OnClientRecive += OnDisableImperiumAccess;

        authenticateEvent.OnClientRecive += OnAuthenticateResponse;
        networkLog.OnClientRecive += OnLogReceived;
    }

    private void OnPacketReceived(ImpPacket packet)
    {
        if (!subscribers.TryGetValue(packet.Channel, out var subscription))
        {
            Imperium.IO.LogWarning($"[NET] Received packet in unknown channel: {packet.Channel}. Ignoring.");
            return;
        }

        if (!subscription.AllowUnauthenticated && !ImperiumUsers.Value.Contains((ulong)packet.Header.Target))
        {
            Imperium.IO.LogWarning($"[NET] Received packet from unauthenticated client: {packet.Header.Target}. Blocking.");
            return;
        }

        subscription.OnPacket.Invoke(packet);
    }

    internal void ClearSubscription(string channel) => subscribers.Remove(channel);

    internal void SubscribeChannel(string channel, Action<ImpPacket> callback, bool allowUnauthenticated)
    {
        if (subscribers.TryGetValue(channel, out _))
        {
            Imperium.IO.LogError($"[NET] Subscription with duplicate identifier registered: {channel}");
            return;
        }

        subscribers[channel] = new ImpSubscription
        {
            OnPacket = callback,
            AllowUnauthenticated = allowUnauthenticated
        };
    }

    internal static void SendPacket(
        string channel,
        object data,
        NetworkDestination destination = NetworkDestination.Everyone,
        params SteamId[] clientIds
    )
    {
        var packet = new ImpPacket
        {
            Channel = channel,
            Payload = data,
        };

        if (destination == NetworkDestination.PacketTarget)
        {
            clientIds.SendPacket(packet);
        }
        else
        {
            RepoSteamNetwork.SendPacket(packet, destination);
        }
    }

    internal void BindAllowClients(IBinding<bool> allowClientsBinding)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            allowClientsBinding.onPrimaryUpdate += ToggleImperiumAccess;
        }
    }

    internal void RegisterSubscriber(INetworkSubscribable subscriber) => RegisteredNetworkSubscribers.Add(subscriber);

    internal void SendLog(NetworkNotification report)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        networkLog.DispatchToClients(report);
    }

    private static void OnLogReceived(NetworkNotification report) => Imperium.IO.Send(
        report.Message, report.Title ?? "Imperium Server", report.IsWarning, report.Type
    );

    [ImpAttributes.HostOnly]
    private void OnClientRequestValues(ulong clientId)
    {
        foreach (var subscribable in RegisteredNetworkSubscribers)
        {
            subscribable.BroadcastToClient(clientId);
        }
    }

    [ImpAttributes.HostOnly]
    private void OnAuthenticateRequest(ulong senderId)
    {
        // Always grant Imperium access if the request comes from the host
        if (senderId == RepoSteamNetwork.CurrentSteamId)
        {
            authenticateEvent.DispatchToClients(RepoSteamNetwork.CurrentSteamId);
            return;
        }

        var playerName = ((SteamId)senderId).GetPlayerAvatar()?.playerName ?? $"#{senderId}";
        if (Imperium.Settings.Preferences.AllowClients.Value)
        {
            Imperium.IO.Send(
                $"Imperium access was granted to client {playerName}.",
                type: NotificationType.AccessControl
            );
            Imperium.IO.LogInfo($"[NET] Client #{senderId} successfully requested Imperium access ({playerName})!");

            authenticateEvent.DispatchToClients(senderId);
            ImperiumUsers.Set(ImperiumUsers.Value.Toggle(senderId));
        }
        else
        {
            Imperium.IO.Send(
                $"Imperium access was denied to client {playerName}.",
                type: NotificationType.AccessControl
            );
            Imperium.IO.LogInfo($"[NET] Client #{senderId} failed to request Imperium access ({playerName})!");
        }
    }

    [ImpAttributes.HostOnly]
    private void ToggleImperiumAccess(bool hasAccess)
    {
        if (hasAccess)
        {
            enableImperiumEvent.DispatchToClients();
        }
        else
        {
            disableImperiumEvent.DispatchToClients();
        }
    }

    [ImpAttributes.LocalMethod]
    private void OnAuthenticateResponse()
    {
        WasImperiumAccessGranted = true;

        Imperium.IO.Send("Imperium access was granted!", type: NotificationType.AccessControl);
        Imperium.IO.LogInfo("[NET] Imperium access was granted! Launching Imperium...");

        if (!ImpUtils.RunSafe(Imperium.Launch, "Imperium startup failed"))
        {
            Imperium.IO.Send("Imperium launch failed! Shutting down.", type: NotificationType.Required);
            Imperium.DisableImperium();
        }

        // Request network values update from server if client is not host
        if (!PhotonNetwork.IsMasterClient) clientRequestValues.DispatchToServer();
    }

    [ImpAttributes.LocalMethod]
    private static void OnDisableImperiumAccess()
    {
        if (PhotonNetwork.IsMasterClient) return;

        Imperium.IO.Send("Imperium access was revoked!", type: NotificationType.AccessControl, isWarning: true);
        Imperium.IO.LogInfo("[NET] Imperium access was revoked!");
        Imperium.DisableImperium();
    }

    [ImpAttributes.LocalMethod]
    private void OnEnableImperiumAccess()
    {
        if (PhotonNetwork.IsMasterClient) return;

        Imperium.IO.Send("Imperium access was granted!", type: NotificationType.AccessControl);
        Imperium.IO.LogInfo("[NET] Imperium access was granted! Launching Imperium...");
        if (WasImperiumAccessGranted)
        {
            Imperium.EnableImperium();
        }
        else
        {
            if (!ImpUtils.RunSafe(Imperium.Launch, "Imperium startup failed")) Imperium.DisableImperium();

            // Request network values update from server if client is not host
            if (!PhotonNetwork.IsMasterClient) clientRequestValues.DispatchToServer();
        }
    }

    [ImpAttributes.RemoteMethod]
    internal void RequestImperiumAccess() => MenuManager.instance.StartCoroutine(waitForImperiumAccess());

    private IEnumerator waitForImperiumAccess()
    {
        authenticateEvent.DispatchToServer();

        yield return new WaitForSeconds(5f);
        if (!Imperium.IsImperiumLaunched)
        {
            Imperium.IO.Send("Failed to acquire Imperium access! Shutting down...", isWarning: true);
        }
    }

    public void Reset()
    {
        // WasImperiumAccessGranted = false;

        foreach (var subscriber in RegisteredNetworkSubscribers) subscriber.Clear();
        RegisteredNetworkSubscribers.Clear();
    }
}