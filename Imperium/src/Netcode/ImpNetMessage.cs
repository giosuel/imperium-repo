#region

using System;
using Imperium.Util;
using Photon.Pun;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking;
using RepoSteamNetworking.Networking.Unity;

#endregion

namespace Imperium.Netcode;

public class ImpNetMessage<T> : INetworkSubscribable
{
    internal event Action<T, ulong> OnServerReceive;
    internal event Action<T> OnClientRecive;

    private readonly string identifier;

    // Whether the server should relay all received packets to the clients directly.
    private readonly bool relayPackets;

    private readonly ImpNetworking networking;

    public ImpNetMessage(
        string identifier,
        ImpNetworking networking,
        bool relayPackets = false,
        bool allowUnauthenticated = false
    )
    {
        this.identifier = $"{identifier}_message";
        this.networking = networking;
        this.relayPackets = relayPackets;

        networking.SubscribeChannel(this.identifier, OnPacketReceived, allowUnauthenticated);
        networking.RegisterSubscriber(this);

        Imperium.IO.LogDebug($"[NET] Message {identifier} has been registered.");
    }

    [ImpAttributes.RemoteMethod]
    internal void DispatchToServer(T data)
    {
        Imperium.IO.LogDebug($"[NET] Client sends message {identifier} to server.");
        ImpNetworking.SendPacket(identifier, data, NetworkDestination.HostOnly);
    }

    [ImpAttributes.HostOnly]
    internal void DispatchToClients(T data)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Imperium.IO.LogError("[NET] Trying to dispatch to clients from non-host. Blocked by Imperium policy.");
            return;
        }

        Imperium.IO.LogDebug($"[NET] Server sends message {identifier} to clients.");
        ImpNetworking.SendPacket(identifier, data, NetworkDestination.ClientsOnly);
        OnClientReceived(data);
    }

    private void OnPacketReceived(ImpPacket packet)
    {
        if (packet.Header.Destination == NetworkDestination.HostOnly)
        {
            Imperium.IO.LogDebug($"[NET] Server received message {identifier}.");
            OnServerReceived((T)Convert.ChangeType(packet.Payload, typeof(T)), packet.Header.Sender);
        }
        else
        {
            Imperium.IO.LogDebug($"[NET] Client received message {identifier}.");
            OnClientReceived((T)Convert.ChangeType(packet.Payload, typeof(T)));
        }
    }

    private void OnServerReceived(T data, ulong senderId)
    {
        // Block packets from clients if client access is disabled
        if (senderId != RepoSteamNetwork.CurrentSteamId && !Imperium.Settings.Preferences.AllowClients.Value)
        {
            Imperium.IO.LogDebug($"[NET] Server blocked client message {identifier}.");
            return;
        }

        if (relayPackets) DispatchToClients(data);

        OnServerReceive?.Invoke(data, senderId);
    }

    private void OnClientReceived(T data) => OnClientRecive?.Invoke(data);

    public void Clear()
    {
        networking.ClearSubscription(identifier);
    }

    public void BroadcastToClient(ulong clientId)
    {
    }
}