#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Networking;

#endregion

namespace Imperium.Core.Lifecycle;

internal class GameManager : ImpLifecycleObject
{
    internal readonly ImpNetworkBinding<string> LevelOverride = new("LevelOverride", Imperium.Networking, "");
    internal readonly ImpNetworkBinding<string> ModuleOverride = new("ModuleOverride", Imperium.Networking, "");

    internal readonly ImpNetworkBinding<float> CustomLevelSize = new("CustomMapSize", Imperium.Networking, -1);
    internal readonly ImpNetworkBinding<int> CustomModuleAmount = new("CustomModuleAmount", Imperium.Networking, -1);
    internal readonly ImpNetworkBinding<int> CustomLevelNumber = new("CustomLevelNumber", Imperium.Networking, -1);
    internal readonly ImpNetworkBinding<int> OverrideModuleType = new("OverrideModuleType", Imperium.Networking);
    internal readonly ImpNetworkBinding<int> CustomValuableSpawns = new("CustomValuableSpawns", Imperium.Networking);

    internal readonly ImpNetworkBinding<int> GroupCurrency = new(
        "GroupCurrency",
        Imperium.Networking,
        onUpdateClient: value => SemiFunc.StatSetRunCurrency(value)
    );

    internal readonly ImpNetworkBinding<int> TotalHaul = new(
        "ProfitQuota",
        Imperium.Networking,
        onUpdateClient: value => SemiFunc.StatSetRunTotalHaul(value)
    );

    internal readonly ImpNetworkBinding<bool> LowHaul = new(
        "LowHaul",
        Imperium.Networking,
        onUpdateClient: value => RoundDirector.instance.debugLowHaul = value
    );

    internal bool IsGameLoading { get; set; } = true;

    // Transient fields to temporarily set the level and level number for the next load. Used for the level console commands.
    internal Level NextLevelOverride;
    internal int NextLevelNumberOverride;

    /// <summary>
    ///     A list of value lost UI instances that were instantiated by Imperium when an enemy was damaged.
    ///     This is used to customize the UIs in <see cref="Patches.Objects.WorldSpaceUIValueLostPatch.StartPatch" />.
    /// </summary>
    internal readonly HashSet<WorldSpaceUIValueLost> EnemyValueLostInstances = [];

    protected override void Init()
    {
        LevelOverride.OnUpdate += OnLevelOverrideUpdate;
    }

    private static void OnLevelOverrideUpdate(string levelName)
    {
        if (levelName == "")
        {
            RunManager.instance.debugLevel = null;
            return;
        }

        var customLevel = Imperium.ObjectManager.LoadedLevels.Value.First(level => level.NarrativeName == levelName);
        RunManager.instance.debugLevel = customLevel;
    }

    internal static bool IsGameLevel()
    {
        return RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu &&
               RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu &&
               RunManager.instance.levelCurrent != RunManager.instance.levelLobby &&
               RunManager.instance.levelCurrent != RunManager.instance.levelTutorial;
    }

    internal static string GetEnemyState(EnemyParent enemyParent)
    {
        switch (enemyParent.enemyName)
        {
            case "Animal":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyAnimal>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Banger":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyBang>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Clown":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyBeamer>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Bowtie":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyBowtie>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Peeper":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyCeilingEye>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Apex Predator":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyDuck>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Mentalist":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyFloater>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Gnome":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyGnome>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Hidden":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyHidden>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Huntsman":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyHunter>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Robe":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyRobe>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Reaper":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyRunner>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Spewer":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemySlowMouth>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Trudge":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemySlowWalker>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Shadow Child":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyThinMan>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Chef":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyTumbler>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Upscream":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyUpscream>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
            case "Rugrat":
            {
                var enemyComponent = enemyParent.Enemy.GetComponent<EnemyValuableThrower>();
                if (enemyComponent) return enemyComponent.currentState.ToString();
                break;
            }
        }

        return enemyParent.Enemy.CurrentState.ToString();
    }

    internal static void ReloadLevel()
    {
        RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false);
    }

    internal static void AdvanceLevel()
    {
        RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false);
    }

    internal static void LoadLevel(Level level, int? levelNumber)
    {
        Imperium.GameManager.NextLevelOverride = level;

        if (levelNumber.HasValue) Imperium.GameManager.NextLevelNumberOverride = levelNumber.Value;
        RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false);
    }
}