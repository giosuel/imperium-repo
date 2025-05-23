#region

using RepoSteamNetworking.API;
using RepoSteamNetworking.Networking.Serialization;

#endregion

namespace Imperium.Networking;

public class ImpPacket : NetworkPacket<ImpPacket>
{
    public string Channel;
    public object Payload;

    protected override void WriteData(SocketMessage socketMessage)
    {
        socketMessage.Write(Channel);
        socketMessage.Write(Payload);
    }

    protected override void ReadData(SocketMessage socketMessage)
    {
        Channel = socketMessage.Read<string>();
        Payload = socketMessage.Read<object>();
    }
}