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

    /// <summary>
    /// A list of value lost UI instances that were instantiated by Imperium when an enemy was damaged.
    ///
    /// This is used to customize the UIs in <see cref="Patches.Objects.WorldSpaceUIValueLostPatch.StartPatch"/>.
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
        Imperium.IO.LogInfo($"is lobby menu: {RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu}");
        Imperium.IO.LogInfo($"is main menu: {RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu}");
        Imperium.IO.LogInfo($"is lobby: {RunManager.instance.levelCurrent == RunManager.instance.levelLobby}");
        Imperium.IO.LogInfo($"is tutorial: {RunManager.instance.levelCurrent == RunManager.instance.levelTutorial}");

        return RunManager.instance.levelCurrent != RunManager.instance.levelLobbyMenu &&
               RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu &&
               RunManager.instance.levelCurrent != RunManager.instance.levelLobby &&
               RunManager.instance.levelCurrent != RunManager.instance.levelTutorial;
    }
}