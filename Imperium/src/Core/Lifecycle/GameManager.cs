#region

using System.Linq;
using Imperium.Networking;
using Imperium.Util;
using Photon.Pun;

#endregion

namespace Imperium.Core.Lifecycle;

internal class GameManager : ImpLifecycleObject
{
    internal readonly ImpNetEvent FulfillQuotaEvent = new("FulfillQuota", Imperium.Networking);

    internal readonly ImpNetworkBinding<string> LevelOverride = new("LevelOverride", Imperium.Networking, "");
    internal readonly ImpNetworkBinding<string> ModuleOverride = new("ModuleOverride", Imperium.Networking, "");
    // internal readonly ImpNetworkBinding<Module.Type> ModuleOverrideType = new("ModuleOverrideType", Imperium.Networking);

    internal readonly ImpNetworkBinding<float> CustomLevelSize = new("CustomMapSize", Imperium.Networking, -1);
    internal readonly ImpNetworkBinding<int> CustomModuleAmount = new("CustomModuleAmount", Imperium.Networking, -1);

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

    protected override void Init()
    {
        LevelOverride.onUpdate += OnLevelOverrideUpdate;

        if (PhotonNetwork.IsMasterClient) FulfillQuotaEvent.OnServerReceive += FulfillQuota;
    }

    internal bool IsGameLoading { get; set; } = true;

    [ImpAttributes.HostOnly]
    private static void FulfillQuota(ulong clientId)
    {
    }

    protected override void OnSceneLoad()
    {
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
}