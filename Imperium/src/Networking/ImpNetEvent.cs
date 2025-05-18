#region

using System;
using System.Linq;
using Imperium.Util;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking;
using Steamworks;

#endregion

namespace Imperium.Networking;

public class ImpNetEvent : INetworkSubscribable
{
    internal event Action<ulong> OnServerReceive;
    internal event Action OnClientRecive;

    private readonly string identifier;

    // Whether the server should relay all received packets to the clients directly.
    private readonly bool relayPackets;

    // If set to true, will never unsubscribe
    private readonly bool isPersistent;

    private readonly ImpNetworking networking;

    public ImpNetEvent(
        string identifier,
        ImpNetworking networking,
        bool relayPackets = false,
        bool allowUnauthenticated = false,
        bool isPersistent = false
    )
    {
        this.identifier = $"{identifier}_event";
        this.networking = networking;
        this.relayPackets = relayPackets;
        this.isPersistent = isPersistent;

        networking.SubscribeChannel(this.identifier, OnPacketReceived, allowUnauthenticated);
        networking.RegisterSubscriber(this);

        Imperium.IO.LogDebug($"[NET] Event {identifier} has been registered.");
    }

    [ImpAttributes.RemoteMethod]
    internal void DispatchToServer()
    {
        Imperium.IO.LogDebug($"[NET] Client sends event {identifier} to server.");
        networking.SendPacket(identifier, null, NetworkDestination.HostOnly);
    }

    [ImpAttributes.HostOnly]
    internal void DispatchToClients()
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Imperium.IO.LogError("[NET] Trying to dispatch to clients from non-host. Blocked by Imperium policy.");
            Imperium.IO.LogError(Environment.StackTrace);
            return;
        }

        Imperium.IO.LogDebug($"[NET] Server sends event {identifier} to clients.");
        networking.SendPacket(identifier, null, NetworkDestination.ClientsOnly);
        OnClientReceived();
    }

    [ImpAttributes.HostOnly]
    internal void DispatchToClients(params ulong[] clientIds)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            Imperium.IO.LogError("[NET] Trying to dispatch to clients from non-host. Blocked by Imperium policy.");
            Imperium.IO.LogError(Environment.StackTrace);
            return;
        }

        Imperium.IO.LogDebug($"[NET] Server sends event {identifier} to clients: {clientIds}.");

        networking.SendPacket(
            identifier, null, NetworkDestination.PacketTarget,
            clientIds.Select(id => (SteamId)id).ToArray()
        );
    }

    private void OnPacketReceived(ImpPacket packet)
    {
        if (packet.Header.Destination == NetworkDestination.HostOnly)
        {
            Imperium.IO.LogDebug($"[NET] Server received event {identifier}.");
            OnServerReceived(packet.Header.Sender);
        }
        else
        {
            Imperium.IO.LogDebug($"[NET] Client received event {identifier}.");
            OnClientReceived();
        }
    }

    private void OnServerReceived(ulong senderId)
    {
        // Block packets from clients if client access is disabled
        if (senderId != RepoSteamNetwork.CurrentSteamId && !Imperium.Settings.Preferences.AllowClients.Value)
        {
            Imperium.IO.LogDebug($"[NET] Server blocked client event {identifier}.");
            return;
        }

        if (relayPackets) DispatchToClients();

        OnServerReceive?.Invoke(senderId);
    }

    private void OnClientReceived() => OnClientRecive?.Invoke();

    public void Unsubscribe()
    {
        if (!isPersistent) networking.Unsubscribe(identifier);
    }

    public void BroadcastToClient(ulong clientId)
    {
    }
}