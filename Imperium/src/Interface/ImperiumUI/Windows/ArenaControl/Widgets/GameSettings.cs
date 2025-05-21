#region

using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ArenaControl.Widgets;

public class GameSettings : ImpWidget
{
    protected override void InitWidget()
    {
        ImpToggle.Bind(
            "GameSettings/DisableGameOver",
            transform,
            Imperium.ArenaManager.DisableGameOver,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Disable Game Over",
                Description = "Prevents the game from ending when everyone dies.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "GameSettings/DisableEnemies",
            transform,
            Imperium.ArenaManager.DisableEnemies,
            theme: theme
        );

        ImpToggle.Bind(
            "EnemySettings/SpawnClose",
            transform,
            Imperium.ArenaManager.SpawnClose,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Spawn Close",
                Description = "Allows enemies to spawn even\nif a player is close.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "EnemySettings/DespawnClose",
            transform,
            Imperium.ArenaManager.DespawnClose,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Despawn Close",
                Description = "Allows enemies to despawn even\nif a player is close.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "EnemySettings/DisableVision",
            transform,
            Imperium.ArenaManager.DisableVision,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Disable Vision",
                Description = "Disables vision of all enemies in the level.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "EnemySettings/ShortAction",
            transform,
            Imperium.ArenaManager.ShortAction,
            theme: theme
        );

        ImpInput.Bind(
            "SpawnTimer",
            transform,
            Imperium.ArenaManager.SpawnTimer,
            theme: theme,
            negativeIsEmpty: true
        );

        ImpButton.Bind(
            "SpawnTimer/Reset",
            transform,
            () => Imperium.ArenaManager.SpawnTimer.Reset(),
            theme: theme
        );

        ImpInput.Bind(
            "DespawnTimer",
            transform,
            Imperium.ArenaManager.DespawnTimer,
            theme: theme,
            negativeIsEmpty: true
        );

        ImpButton.Bind(
            "DespawnTimer/Reset",
            transform,
            () => Imperium.ArenaManager.DespawnTimer.Reset(),
            theme: theme
        );

        ImpToggle.Bind(
            "EnemyGrabSettings/InfiniteDuration",
            transform,
            Imperium.ArenaManager.EnemyGrabInfiniteDuration,
            theme: theme
        );

        ImpToggle.Bind(
            "EnemyGrabSettings/InfiniteStrength",
            transform,
            Imperium.ArenaManager.EnemyGrabInfiniteStrength,
            theme: theme
        );
    }

    protected override void OnOpen()
    {
        // Update this here as it can change from outside of Imperium
        Imperium.Settings.Game.DisableVision.Set(EnemyDirector.instance.debugNoVision);
    }
}