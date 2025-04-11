#region

using Imperium.API.Types.Networking;
using Imperium.Netcode;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;

#endregion

namespace Imperium.Core.Lifecycle;

internal class MoonManager : ImpLifecycleObject
{
    private readonly ImpEvent WeatherEvent = new();

    // private readonly ImpNetMessage<ChangeWeatherRequest> changeWeatherMessage = new("ChangeWeather", Imperium.Networking);

    public int ScrapAmount;
    public int ChallengeScrapAmount;

    protected override void Init()
    {
        // changeWeatherMessage.OnServerReceive += OnWeatherChangeServer;
        // changeWeatherMessage.OnClientRecive += OnWeatherChangeClient;
    }

    // internal void ChangeWeather(ChangeWeatherRequest request) => changeWeatherMessage.DispatchToServer(request);

    internal readonly ImpNetworkBinding<bool> IndoorSpawningPaused = new(
        "IndoorSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> OutdoorSpawningPaused = new(
        "OutdoorSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> DaytimeSpawningPaused = new(
        "DaytimeSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> TimeIsPaused = new(
        "TimeIsPaused",
        Imperium.Networking,
        onUpdateClient: isPaused =>
        {
            // Imperium.TimeOfDay.globalTimeSpeedMultiplier = isPaused ? 0 : ImpConstants.DefaultTimeSpeed;
        }
    );

    internal readonly ImpNetworkBinding<float> TimeSpeed = new(
        "TimeSpeed",
        Imperium.Networking,
        ImpConstants.DefaultTimeSpeed
    );

    internal readonly ImpNetworkBinding<float> MaxIndoorPower = new(
        "MaxIndoorPower",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<float> MaxOutdoorPower = new(
        "MaxIndoorPower",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<int> MaxDaytimePower = new(
        "MaxDaytimePower",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<float> IndoorDeviation = new(
        "IndoorDeviation",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<float> DaytimeDeviation = new(
        "DaytimeDeviation",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<int> MinIndoorSpawns = new(
        "MinIndoorSpawns",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<int> MinOutdoorSpawns = new(
        "MinOutdoorSpawns",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<float> WeatherVariable1 = new(
        "WeatherVariable1",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<float> WeatherVariable2 = new(
        "WeatherVariable2",
        Imperium.Networking
    );

    protected override void OnSceneLoad()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // MaxIndoorPower.Sync(Imperium.RoundManager.currentMaxInsidePower);
        // MaxOutdoorPower.Sync(Imperium.RoundManager.currentMaxOutsidePower);
        // MaxDaytimePower.Sync(Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount);
        //
        // IndoorDeviation.Sync(Imperium.RoundManager.currentLevel.spawnProbabilityRange);
        // DaytimeDeviation.Sync(Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange);
        //
        // MinIndoorSpawns.Sync(Imperium.RoundManager.minEnemiesToSpawn);
        // MinOutdoorSpawns.Sync(Imperium.RoundManager.minOutsideEnemiesToSpawn);
        //
        // WeatherVariable1.Sync(Imperium.TimeOfDay.currentWeatherVariable);
        // WeatherVariable2.Sync(Imperium.TimeOfDay.currentWeatherVariable2);
    }

    // [ImpAttributes.HostOnly]
    // private void OnWeatherChangeServer(ChangeWeatherRequest request, ulong clientId)
    // {
    //     changeWeatherMessage.DispatchToClients(request);
    //
    //     // if (WeatherRegistryIntegration.IsEnabled)
    //     // {
    //     //     WeatherRegistryIntegration.ChangeWeather(Imperium.StartOfRound.levels[request.LevelIndex], request.WeatherType);
    //     // }
    // }
    //
    // [ImpAttributes.LocalMethod]
    // private void OnWeatherChangeClient(ChangeWeatherRequest request)
    // {
    //     // Imperium.StartOfRound.levels[request.LevelIndex].currentWeather = request.WeatherType;
    //     //
    //     // var planetName = Imperium.StartOfRound.levels[request.LevelIndex].PlanetName;
    //     // var weatherName = request.WeatherType.ToString();
    //     // Imperium.IO.Send(
    //     //     $"Successfully changed the weather on {planetName} to {weatherName}",
    //     //     type: NotificationType.Confirmation
    //     // );
    //     //
    //     // Imperium.StartOfRound.SetMapScreenInfoToCurrentLevel();
    //     // RefreshWeatherEffects();
    //     // WeatherEvent.Trigger();
    // }

    [ImpAttributes.LocalMethod]
    private static void RefreshWeatherEffects()
    {
        // if (!Imperium.IsSceneLoaded.Value) return;
        //
        // Imperium.RoundManager.SetToCurrentLevelWeather();
        // Imperium.TimeOfDay.SetWeatherBasedOnVariables();
        // for (var i = 0; i < Imperium.TimeOfDay.effects.Length; i++)
        // {
        //     var weatherEffect = Imperium.TimeOfDay.effects[i];
        //     var isEnabled = (int)Imperium.StartOfRound.currentLevel.currentWeather == i;
        //     weatherEffect.effectEnabled = isEnabled;
        //     if (weatherEffect.effectPermanentObject)
        //     {
        //         weatherEffect.effectPermanentObject.SetActive(value: isEnabled);
        //     }
        //
        //     if (weatherEffect.effectObject)
        //     {
        //         weatherEffect.effectObject.SetActive(value: isEnabled);
        //     }
        //
        //     if (Imperium.TimeOfDay.sunAnimator)
        //     {
        //         if (isEnabled && !string.IsNullOrEmpty(weatherEffect.sunAnimatorBool))
        //         {
        //             Imperium.TimeOfDay.sunAnimator.SetBool(weatherEffect.sunAnimatorBool, value: true);
        //         }
        //         else
        //         {
        //             Imperium.TimeOfDay.sunAnimator.Rebind();
        //             Imperium.TimeOfDay.sunAnimator.Update(0);
        //         }
        //     }
        // }
        //
        // // This prevents the player from permanently getting stuck in the underwater effect when turning
        // // off flooded weather while being underwater
        // if (Imperium.StartOfRound.currentLevel.currentWeather != LevelWeatherType.Flooded)
        // {
        //     PlayerAvatar.instance.isUnderwater = false;
        //     PlayerAvatar.instance.sourcesCausingSinking = Mathf.Clamp(PlayerAvatar.instance.sourcesCausingSinking - 1, 0, 100);
        //     PlayerAvatar.instance.isMovementHindered = Mathf.Clamp(PlayerAvatar.instance.isMovementHindered - 1, 0, 100);
        //     PlayerAvatar.instance.hinderedMultiplier = 1;
        // }
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleDoors(bool isOpen)
    {
        // Imperium.ObjectManager.CurrentLevelDoors.Value
        //     .Where(obj => obj)
        //     .ToList()
        //     .ForEach(door => ToggleDoor(door, isOpen));
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleDoorLocks(bool isLocked)
    {
        // Imperium.ObjectManager.CurrentLevelDoors.Value
        //     .Where(obj => obj)
        //     .ToList()
        //     .ForEach(door =>
        //     {
        //         if (isLocked && !door.isLocked)
        //         {
        //             // TODO(giosuel): Sync over network
        //             door.LockDoor();
        //         }
        //         else if (door.isLocked)
        //         {
        //             door.UnlockDoorSyncWithServer();
        //         }
        //     });
    }

    internal static void ToggleSecurityDoors(bool isOn)
    {
        // Imperium.ObjectManager.CurrentLevelSecurityDoors.Value
        //     .Where(obj => obj)
        //     .ToList()
        //     .ForEach(door => door.OnPowerSwitch(!isOn));
    }

    internal static void ToggleTurrets(bool isOn)
    {
        // Imperium.ObjectManager.CurrentLevelTurrets.Value
        //     .Where(obj => obj)
        //     .ToList()
        //     .ForEach(turret => turret.ToggleTurretEnabled(isOn));
    }

    internal static void ToggleLandmines(bool isOn)
    {
        // Imperium.ObjectManager.CurrentLevelLandmines.Value
        //     .Where(obj => obj)
        //     .ToList()
        //     .ForEach(mine => mine.ToggleMine(isOn));
    }

    internal static void ToggleBreakers(bool isOn)
    {
        // Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
        //     .Where(obj => obj)
        //     .ToList()
        //     .ForEach(box => ToggleBreaker(box, isOn));
    }
}