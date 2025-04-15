#region

using Imperium.Networking;
using Imperium.Util;
using Photon.Pun;

#endregion

namespace Imperium.Core.Lifecycle;

internal class GameManager : ImpLifecycleObject
{
    internal readonly ImpNetEvent FulfillQuotaEvent = new("FulfillQuota", Imperium.Networking);

    protected override void Init()
    {
        if (PhotonNetwork.IsMasterClient) FulfillQuotaEvent.OnServerReceive += FulfillQuota;
    }

    internal readonly ImpNetworkBinding<int> CustomSeed = new("CustomSeed", Imperium.Networking, -1);
    internal readonly ImpNetworkBinding<int> CustomDungeonFlow = new("CustomDungeonFlow", Imperium.Networking, -1);

    internal readonly ImpNetworkBinding<float> CustomMapSize = new("CustomMapSize", Imperium.Networking, -1);
    internal readonly ImpNetworkBinding<float> CustomModuleAmount = new("CustomModuleAmount", Imperium.Networking, -1);

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

    [ImpAttributes.HostOnly]
    private static void FulfillQuota(ulong clientId)
    {
    }

    protected override void OnSceneLoad()
    {
    }
}