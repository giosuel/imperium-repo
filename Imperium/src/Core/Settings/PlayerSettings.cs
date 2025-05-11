#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core.Settings;

internal class PlayerSettings(ConfigFile config) : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> InfiniteEnergy = new(
        config,
        "Player",
        "InfiniteEnergy",
        false,
        primaryUpdate: value => PlayerController.instance.DebugEnergy = value
    );

    internal readonly ImpConfig<bool> DisableLocking = new(config, "Player", "DisableLocking", false);

    internal readonly ImpConfig<bool> Invisibility = new(config, "Player", "Invisibility", false);
    internal readonly ImpConfig<bool> Muted = new(config, "Player", "Muted", false);

    internal readonly ImpConfig<bool> NoTumbleMode = new(
        config,
        "Player",
        "NoTumbleMode",
        false,
        primaryUpdate: value => PlayerController.instance.DebugNoTumble = value
    );

    internal readonly ImpConfig<bool> EnableFlying = new(
        config,
        "Player",
        "EnableFlying",
        false,
        primaryUpdate: value =>
        {
            if (!value) Imperium.PlayerManager.IsFlying.SetFalse();
        }
    );

    internal readonly ImpConfig<bool> SlowMode = new(
        config,
        "Player",
        "SlowMode",
        false,
        primaryUpdate: value => PlayerController.instance.debugSlow = value
    );

    internal readonly ImpConfig<float> CustomFieldOfView = new(
        config,
        "Player",
        "FieldOfView",
        ImpConstants.DefaultFOV
    );

    internal readonly ImpConfig<bool> GodMode = new(
        config,
        "Player",
        "GodMode",
        false,
        primaryUpdate: value =>
        {
            if (!PlayerAvatar.instance.playerHealth) return;

            // Restore health to full before turning on god mode
            if (value) PlayerAvatar.instance.playerHealth.Heal(100);
            PlayerAvatar.instance.playerHealth.godMode = value;
        }
    );

    internal readonly ImpConfig<float> MovementSpeed = new(
        config,
        "Player",
        "MovementSpeed",
        ImpConstants.DefaultMovementSpeed,
        primaryUpdate: value =>
        {
            PlayerController.instance.MoveSpeed = value;
            PlayerController.instance.playerOriginalMoveSpeed = value;

            // The ratio between the default move speed and sprint speed is 2.5
            var sprintSpeed = value * 2.5f + StatsManager.instance.playerUpgradeSpeed[Imperium.Player.steamID];
            PlayerController.instance.SprintSpeed = sprintSpeed;
            PlayerController.instance.playerOriginalSprintSpeed = sprintSpeed;

            // The ratio between the default move speed and crouch speed is 0.5
            PlayerController.instance.CrouchSpeed = value * 0.5f;
            PlayerController.instance.playerOriginalCrouchSpeed = value * 0.5f;
        }
    );

    internal readonly ImpConfig<float> JumpForce = new(
        config,
        "Player",
        "JumpForce",
        ImpConstants.DefaultJumpForce,
        primaryUpdate: value => PlayerController.instance.JumpForce = value
    );

    internal readonly ImpConfig<float> FlyingSpeed = new(
        config,
        "Player",
        "FlyingSpeed",
        10
    );

    internal readonly ImpConfig<float> NightVision = new(
        config,
        "Player",
        "NightVision",
        0,
        primaryUpdate: value =>
        {
            // We need to reset night vision here for some levels that don't do that (e.g. shop)
            if (value == 0) RenderSettings.ambientLight = LevelGenerator.Instance.Level.FogColor;
        }
    );
}