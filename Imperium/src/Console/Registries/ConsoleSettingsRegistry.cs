using Imperium.Core;
using Imperium.Util;

namespace Imperium.Console.Registries;

public static class ConsoleSettingsRegistry
{
    internal static void RegisterSettings(ConsoleManager manager, ImpSettings settings)
    {
        RegisterPlayerSettings(manager, settings);
        RegisterGrabberSettings(manager, settings);
        RegisterVisualizationSettings(manager, settings);
    }

    private static void RegisterPlayerSettings(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterSetting("God Mode", settings.Player.GodMode, "PlayerSettings/GodMode");
        manager.RegisterSetting("Infinite Energy", settings.Player.InfiniteEnergy, "PlayerSettings/InfiniteEnergy");
        manager.RegisterSetting("Invisibility", settings.Player.Invisibility, "PlayerSettings/Invisibility");
        manager.RegisterSetting("Muted", settings.Player.Muted, "PlayerSettings/Muted");
        manager.RegisterSetting("No Tumble Mode", settings.Player.NoTumbleMode, "PlayerSettings/NoTumbleMode");
        // manager.RegisterSetting("Flight", settings.Player.EnableFlying);

        manager.RegisterSetting("Night Vision", settings.Player.NightVision, 0, 100, interfacePath: "NightVision");
        manager.RegisterSetting("Field of View", settings.Player.CustomFieldOfView, interfacePath: "FieldOfView");
        manager.RegisterSetting("Movement Speed", settings.Player.MovementSpeed, interfacePath: "MovementSpeed");
        manager.RegisterSetting("Jump Force", settings.Player.JumpForce, interfacePath: "JumpForce");
    }

    private static void RegisterGrabberSettings(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterSetting("Grabber Strength", settings.Grabber.GrabStrength, interfacePath: "GrabStrength");
        manager.RegisterSetting("Grabber Range", settings.Grabber.BaseRange, interfacePath: "BaseRange");
    }

    private static void RegisterVisualizationSettings(ConsoleManager manager, ImpSettings settings)
    {
        manager.RegisterSetting(
            "Noise Visualizer",
            settings.Visualization.NoiseIndicators,
            customIcon: ImpAssets.IconCommandVisualizer
        );
        manager.RegisterSetting(
            "Proximity Visualizer",
            settings.Visualization.NoiseIndicators,
            customIcon: ImpAssets.IconCommandVisualizer
        );
        manager.RegisterSetting(
            "Level Point Visualizer",
            settings.Visualization.LevelPoints,
            customIcon: ImpAssets.IconCommandVisualizer
        );
        manager.RegisterSetting(
            "Navmesh Visualizer",
            settings.Visualization.NavMeshSurfaces,
            customIcon: ImpAssets.IconCommandVisualizer
        );
    }
}