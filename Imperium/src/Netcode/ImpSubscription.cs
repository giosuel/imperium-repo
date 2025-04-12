using System;

namespace Imperium.Netcode;

public record ImpSubscription
{
    public Action<ImpPacket> OnPacket { get; init; }
    public bool AllowUnauthenticated { get; init; }
}