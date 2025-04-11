namespace Imperium.Extensions;

public static class ChatManagerExtensions
{
    public static bool IsOpen(this ChatManager manager) => manager.chatActive;
}