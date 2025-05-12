#region

using System;
using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Util;
using Librarium.Binding;
using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking;

#endregion

namespace Imperium.Networking;

public class ImpNetworkBinding<T> : IBinding<T>, INetworkSubscribable
{
    public event Action<T> OnUpdate;

    // For ImpNetworkBinding, secondary updates are only ever executed on the local client that initiated the update
    public event Action<T> OnUpdateSecondary;

    public event Action OnTrigger;

    // For ImpNetworkBinding, secondary triggers are only ever executed on the local client that initiated the update
    public event Action OnTriggerSecondary;

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

        OnUpdate += onUpdateClient;
        this.onUpdateServer = onUpdateServer;
        this.masterBinding = masterBinding;

        networking.SubscribeChannel(this.identifier, OnPacketReceived, allowUnauthenticated);
        networking.RegisterSubscriber(this);

        if (masterBinding != null && SemiFunc.IsMasterClientOrSingleplayer()) Set(masterBinding.Value);
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
            OnClientReceived(
                (BindingUpdateRequest<T>)Convert.ChangeType(packet.Payload, typeof(BindingUpdateRequest<T>))
            );
        }
    }

    private void OnServerReceived(BindingUpdateRequest<T> request, ulong senderId)
    {
        if (senderId != RepoSteamNetwork.CurrentSteamId && !Imperium.Settings.Preferences.AllowClients.Value)
        {
            Imperium.IO.LogDebug($"[NET] Server blocked client message {identifier}.");
            return;
        }

        networking.SendPacket(identifier, request, NetworkDestination.ClientsOnly);
        OnClientReceived(request);
    }

    private void OnClientReceived(BindingUpdateRequest<T> updatedValue)
    {
        Imperium.IO.LogDebug($"[NET] Client received binding update {identifier}.");
        Value = updatedValue.Payload;

        if (updatedValue.InvokePrimaryUpdate)
        {
            OnUpdate?.Invoke(Value);
            OnTrigger?.Invoke();
        }
    }

    public void Set(T updatedValue, bool invokePrimary = true, bool invokeSecondary = true)
    {
        Value = updatedValue;
        if (masterBinding != null && SemiFunc.IsMasterClientOrSingleplayer()) masterBinding.Set(updatedValue);

        if (invokeSecondary)
        {
            OnUpdateSecondary?.Invoke(updatedValue);
            OnTriggerSecondary?.Invoke();
        }

        networking.SendPacket(identifier, new BindingUpdateRequest<T>
        {
            Payload = updatedValue,
            InvokePrimaryUpdate = invokePrimary,
        });
    }

    public void Refresh()
    {
    }

    public void Reset(bool invokePrimary = true, bool invokeSecondary = true)
    {
        Set(DefaultValue, invokePrimary, invokeSecondary);
    }

    public void Unsubscribe()
    {
        networking.Unsubscribe(identifier);
    }

    [ImpAttributes.HostOnly]
    public void BroadcastToClient(ulong clientId)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer()) return;

        networking.SendPacket(identifier, new BindingUpdateRequest<T>
        {
            Payload = Value,
            InvokePrimaryUpdate = true,
        }, destination: NetworkDestination.PacketTarget, clientId);
    }
}

public interface INetworkSubscribable
{
    public void Unsubscribe();
    public void BroadcastToClient(ulong clientId);
}