#region

using System;

#endregion

namespace Imperium.Networking;

public record ImpSubscription
{
    public Action<ImpPacket> OnPacket { get; init; }
    public bool AllowUnauthenticated { get; init; }
}