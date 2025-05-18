#region

using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Console.Registries;

public static class ConsoleSettingsRegistry
{
    internal static void RegisterSettings(ConsoleManager manager, ImpSettings settings)
    {
        RegisterPlayerSettings(manager, settings);
        RegisterGrabberSettings(manager, settings);
        RegisterVisualizationSettings(manager, settings);
        RegisterTeleportation(manager, settings);
    }

    private static void RegisterPlayerSettings(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterSetting("God Mode", settings.Player.GodMode, "PlayerSettings/GodMode");
        manager.RegisterSetting("Infinite Energy", settings.Player.InfiniteEnergy, "PlayerSettings/InfiniteEnergy");
        manager.RegisterSetting("Invisibility", settings.Player.Invisibility, "PlayerSettings/Invisibility");
        manager.RegisterSetting("Muted", settings.Player.Muted, "PlayerSettings/Muted");
        manager.RegisterSetting("No Tumble Mode", settings.Player.NoTumbleMode, "PlayerSettings/NoTumbleMode");
        // manager.RegisterSetting("Flight", settings.Player.EnableFlying);

        manager.RegisterSetting(
            "Night Vision"
            , settings.Player.NightVision,
            minValue: 0, maxValue: 100,
            valueUnit: "%",
            interfacePath: "NightVision"
        );

        manager.RegisterSetting(
            "Field of View",
            settings.Player.CustomFieldOfView,
            minValue: 50, maxValue: 160,
            valueUnit: "\u00b0",
            interfacePath: "FieldOfView"
        );

        manager.RegisterSetting(
            "Movement Speed",
            settings.Player.MovementSpeed,
            minValue: 0, maxValue: 10,
            interfacePath: "MovementSpeed"
        );

        manager.RegisterSetting(
            "Jump Force",
            settings.Player.JumpForce,
            minValue: 0, maxValue: 100,
            interfacePath: "JumpForce"
        );
    }

    private static void RegisterGrabberSettings(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterSetting(
            "Grabber Strength",
            settings.Grabber.GrabStrength,
            minValue: 0, maxValue: 20,
            interfacePath: "GrabStrength",
            customIcon: ImpAssets.IconGrabber
        );

        manager.RegisterSetting(
            "Grabber Range",
            settings.Grabber.BaseRange,
            minValue: 0, maxValue: 20,
            interfacePath: "BaseRange",
            customIcon: ImpAssets.IconGrabber
        );
    }

    private static void RegisterVisualizationSettings(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterSetting(
            "Noise Visualizer",
            settings.Visualization.NoiseIndicators,
            customIcon: ImpAssets.IconVisualizer
        );

        manager.RegisterSetting(
            "Proximity Visualizer",
            settings.Visualization.NoiseIndicators,
            customIcon: ImpAssets.IconVisualizer
        );

        manager.RegisterSetting(
            "Level Point Visualizer",
            settings.Visualization.LevelPoints,
            customIcon: ImpAssets.IconVisualizer
        );

        manager.RegisterSetting(
            "Navmesh Visualizer",
            settings.Visualization.NavMeshSurfaces,
            customIcon: ImpAssets.IconVisualizer
        );
    }

    private static void RegisterTeleportation(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterAction("Teleport", Execute, DisplayNameOverride);

        return;

        string DisplayNameOverride(ConsoleQuery query)
        {
            var positionX = query.Args.Length > 0 && float.TryParse(query.Args[0], out var x) ? $"{x:0.#}" : "~";
            var positionY = query.Args.Length > 1 && float.TryParse(query.Args[1], out var y) ? $"{y:0.#}" : "~";
            var positionZ = query.Args.Length > 2 && float.TryParse(query.Args[2], out var z) ? $"{z:0.#}" : "~";

            return $"Teleport to {positionX}/{positionY}/{positionZ}";
        }

        bool Execute(ConsoleQuery query)
        {
            var currentPosition = PlayerAvatar.instance.transform.position;

            var position = new Vector3(
                query.Args.Length > 0 && float.TryParse(query.Args[0], out var x) ? x : currentPosition.x,
                query.Args.Length > 0 && float.TryParse(query.Args[0], out var y) ? y : currentPosition.y,
                query.Args.Length > 0 && float.TryParse(query.Args[0], out var z) ? z : currentPosition.z
            );

            Imperium.PlayerManager.TeleportLocalPlayer(position);

            Imperium.IO.Send($"Teleported to {position.x:0.#}/{position.y:0.#}/{position.z:0.#}");

            return true;
        }
    }
}