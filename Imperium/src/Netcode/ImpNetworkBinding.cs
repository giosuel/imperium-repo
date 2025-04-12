#region

using System;
using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking;

#endregion

namespace Imperium.Netcode;

public class ImpNetworkBinding<T> : IBinding<T>, INetworkSubscribable
{
    public event Action<T> onUpdate;
    public event Action<T> onUpdateSecondary;

    public event Action onTrigger;
    public event Action onTriggerSecondary;

    private readonly Action<T> onUpdateServer;

    public T DefaultValue { get; }

    public T Value { get; private set; }

    // This optional binding provides the initial value and is changed only when the local client updates the state.
    private readonly IBinding<T> masterBinding;

    private readonly string identifier;
    private readonly ImpNetworking networking;

    public ImpNetworkBinding(
        string identifier,
        ImpNetworking networking,
        T currentValue = default,
        T defaultValue = default,
        Action<T> onUpdateClient = null,
        Action<T> onUpdateServer = null,
        IBinding<T> masterBinding = null,
        bool allowUnauthenticated = false
    )
    {
        this.identifier = $"{identifier}_binding";
        this.networking = networking;

        Value = currentValue;
        DefaultValue = masterBinding != null
            ? masterBinding.DefaultValue
            : !EqualityComparer<T>.Default.Equals(defaultValue, default)
                ? defaultValue
                : currentValue;

        onUpdate += onUpdateClient;
        this.onUpdateServer = onUpdateServer;
        this.masterBinding = masterBinding;

        networking.SubscribeChannel(this.identifier, OnPacketReceived, allowUnauthenticated);
        networking.RegisterSubscriber(this);

        if (masterBinding != null && PhotonNetwork.IsMasterClient) Set(masterBinding.Value);
    }

    private void OnPacketReceived(ImpPacket packet)
    {
        if (packet.Header.Destination == NetworkDestination.HostOnly)
        {
            Imperium.IO.LogDebug($"[NET] Server received binding update {identifier}.");
            OnServerReceived(
                (BindingUpdateRequest<T>)Convert.ChangeType(packet.Payload, typeof(BindingUpdateRequest<T>)),
                packet.Header.Sender
            );
        }
        else
        {
            Imperium.IO.LogDebug($"[NET] Client received binding update {identifier}.");
            OnClientReceived(
                (BindingUpdateRequest<T>)Convert.ChangeType(packet.Payload, typeof(BindingUpdateRequest<T>))
            );
        }
    }

    private void OnServerReceived(BindingUpdateRequest<T> request, ulong senderId)
    {
        Imperium.IO.LogDebug($"[NET] Server received binding update {identifier}.");

        if (senderId != RepoSteamNetwork.CurrentSteamId && !Imperium.Settings.Preferences.AllowClients.Value)
        {
            Imperium.IO.LogDebug($"[NET] Server blocked client message {identifier}.");
            return;
        }

        // Invoke optional custom binding (e.g. Calls to vanilla client RPCs)
        // if (request.InvokeServerUpdate) onUpdateServer?.Invoke(request.Payload);

        ImpNetworking.SendPacket(identifier, request, NetworkDestination.ClientsOnly);
        OnClientReceived(request);
    }

    private void OnClientReceived(BindingUpdateRequest<T> updatedValue)
    {
        Imperium.IO.LogDebug($"[NET] Client received binding update {identifier}.");
        Value = updatedValue.Payload;

        if (updatedValue.InvokeUpdate)
        {
            onUpdate?.Invoke(Value);
            onTrigger?.Invoke();
        }
    }

    public void Sync(T updatedValue) => Set(updatedValue, false, false);

    public void Set(T updatedValue, bool invokePrimary = true, bool invokeSecondary = true)
    {
        SyncedSet(updatedValue, invokePrimary, true);
    }

    private void SyncedSet(T updatedValue, bool invokeUpdate, bool invokeServerUpdate)
    {
        Value = updatedValue;
        if (masterBinding != null && PhotonNetwork.IsMasterClient) masterBinding.Set(updatedValue);

        if (invokeUpdate)
        {
            onUpdateSecondary?.Invoke(updatedValue);
            onTriggerSecondary?.Invoke();
        }

        ImpNetworking.SendPacket(identifier, new BindingUpdateRequest<T>
        {
            Payload = updatedValue,
            InvokeUpdate = invokeUpdate,
            InvokeServerUpdate = invokeServerUpdate
        });
    }

    public void Refresh()
    {
    }

    public void Reset(bool invokePrimary = true, bool invokeSecondary = true)
    {
        Set(DefaultValue, invokePrimary, invokeSecondary);
    }

    public void Clear()
    {
        networking.ClearSubscription(identifier);
    }

    [ImpAttributes.HostOnly]
    public void BroadcastToClient(ulong clientId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ImpNetworking.SendPacket(identifier, new BindingUpdateRequest<T>
        {
            Payload = Value,
            InvokeUpdate = true
        }, destination: NetworkDestination.PacketTarget, clientId);
    }
}

public interface INetworkSubscribable
{
    public void Clear();
    public void BroadcastToClient(ulong clientId);
}