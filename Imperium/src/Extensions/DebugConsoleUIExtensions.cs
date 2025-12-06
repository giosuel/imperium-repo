namespace Imperium.Extensions;

public static class DebugConsoleUIExtensions
{
    public static bool IsOpen(this DebugConsoleUI debugConsoleUI) => debugConsoleUI.chatActive;
}