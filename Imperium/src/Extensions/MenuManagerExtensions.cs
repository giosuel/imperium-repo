namespace Imperium.Extensions;

public static class MenuManagerExtensions
{
    public static bool IsOpen(this MenuManager manager)
    {
        return manager.currentMenuState == (int)MenuManager.MenuState.Open;
    }
}