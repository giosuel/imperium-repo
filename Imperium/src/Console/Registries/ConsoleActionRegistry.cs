using Imperium.Util;
using RepoSteamNetworking.API;

namespace Imperium.Console.Registries;

public static class ConsoleActionRegistry
{
    internal static void RegisterActions(ConsoleManager manager)
    {
        manager.RegisterAction(
            "Reload Level",
            _ =>
            {
                Core.Lifecycle.GameManager.ReloadLevel();
                return true;
            },
            customIcon: ImpAssets.IconCommandReload,
            interfacePath: "ReloadLevel",
            enabledBinding: Imperium.IsGameLevel
        );

        manager.RegisterAction(
            "Advance Level",
            _ =>
            {
                Core.Lifecycle.GameManager.AdvanceLevel();
                return true;
            },
            customIcon: ImpAssets.IconCommandReload,
            interfacePath: "AdvanceLevel",
            enabledBinding: Imperium.IsGameLevel
        );

        manager.RegisterAction(
            "Kill Local Player",
            _ =>
            {
                Imperium.PlayerManager.KillPlayer(PlayerAvatar.instance.GetSteamId());
                return true;
            },
            customIcon: ImpAssets.IconKillPlayer,
            enabledBinding: Imperium.IsGameLevel
        );

        manager.RegisterAction(
            "Revive Local Player",
            _ =>
            {
                Imperium.PlayerManager.RevivePlayer(PlayerAvatar.instance.GetSteamId());
                return true;
            },
            customIcon: ImpAssets.IconRevivePlayer,
            enabledBinding: Imperium.IsGameLevel
        );
    }
}