#region

using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Networking;
using Imperium.Util;
using Librarium.Binding;
using RepoSteamNetworking.API;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Lifecycle;

internal class PlayerManager : ImpLifecycleObject
{
    internal readonly ImpBinaryBinding IsFlying = new(false);

    internal readonly ImpNetworkBinding<HashSet<ulong>> MutedPlayers = new(
        "MutedPlayers", Imperium.Networking, []
    );

    internal readonly ImpNetworkBinding<HashSet<ulong>> InvisiblePlayers = new(
        "InvisiblePlayers", Imperium.Networking, []
    );

    private readonly ImpNetMessage<ulong> killPlayerMessage = new("KillPlayer", Imperium.Networking);
    private readonly ImpNetMessage<ulong> revivePlayerMessage = new("RevivePlayer", Imperium.Networking);
    private readonly ImpNetMessage<DropItemRequest> dropItemMessage = new("Dropitem", Imperium.Networking);

    private readonly ImpNetMessage<TeleportPlayerRequest> teleportPlayerMessage = new(
        "TeleportPlayer", Imperium.Networking, true
    );

    internal ImpExternalBinding<Vector3?, bool> TruckTPAnchor;

    internal readonly Dictionary<string, ImpBinding<int>> PlayerUpgradeBinding = [];

    internal bool FlyIsAscending;
    internal bool FlyIsDescending;

    protected override void Init()
    {
        teleportPlayerMessage.OnClientRecive += OnTeleportPlayerClient;

        Imperium.Settings.Player.Invisibility.OnUpdate += isInvisible =>
        {
            ImpUtils.Bindings.ToggleSet(InvisiblePlayers, PlayerAvatar.instance.GetSteamId(), isInvisible);
        };

        Imperium.Settings.Player.Muted.OnUpdate += isMuted =>
        {
            ImpUtils.Bindings.ToggleSet(MutedPlayers, PlayerAvatar.instance.GetSteamId(), isMuted);
        };

        Imperium.InputBindings.BaseMap.ToggleHUD.performed += ToggleHUD;

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            dropItemMessage.OnServerReceive += OnDropItemServer;
            killPlayerMessage.OnServerReceive += OnKillPlayerServer;
            revivePlayerMessage.OnServerReceive += OnRevivePlayerServer;
        }
    }

    protected override void OnLevelLoad()
    {
        if (TruckTPAnchor == null)
            TruckTPAnchor =
                new ImpExternalBinding<Vector3?, bool>(() => GameObject.Find("Truck Item Shelf")?.transform.position);

        TruckTPAnchor.Refresh();

        // Load grabber and render settings as they are reset when the level is loaded
        Imperium.Settings.Grabber.Load();
        Imperium.Settings.Rendering.Load();
    }

    [ImpAttributes.RemoteMethod]
    internal void KillPlayer(ulong playerId) => killPlayerMessage.DispatchToServer(playerId);

    [ImpAttributes.RemoteMethod]
    internal void RevivePlayer(ulong playerId) => revivePlayerMessage.DispatchToServer(playerId);

    [ImpAttributes.RemoteMethod]
    internal void TeleportPlayer(TeleportPlayerRequest request) => teleportPlayerMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void TeleportLocalPlayer(Vector3 position) => TeleportPlayer(new TeleportPlayerRequest
    {
        PlayerId = RepoSteamNetwork.CurrentSteamId,
        Destination = position
    });

    internal static void ToggleLocalAvatar(bool isShown)
    {
        if (!Imperium.IsLevelLoaded) return;

        PlayerAvatar.instance.playerAvatarVisuals.animator.enabled = isShown;
        PlayerAvatar.instance.playerAvatarVisuals.meshParent.SetActive(value: isShown);
    }

    private static void ToggleHUD(InputAction.CallbackContext callbackContext)
    {
        GameDirector.instance.CommandRecordingDirectorToggle();
    }

    internal static void ToggleHUD(bool isHidden)
    {
        switch (isHidden)
        {
            case true when RecordingDirector.instance != null:
                Destroy(RecordingDirector.instance.gameObject);
                FlashlightController.Instance.hideFlashlight = false;
                break;
            case false:
                Instantiate(Resources.Load("Recording Director"));
                break;
        }
    }

    private void LateUpdate()
    {
        if (!Imperium.IsGameLevel.Value || !Imperium.ActiveCamera.Value) return;

        if (!Mathf.Approximately(Imperium.Settings.Player.CustomFieldOfView.Value, ImpConstants.DefaultFOV))
        {
            Imperium.ActiveCamera.Value.fieldOfView = Imperium.Settings.Player.CustomFieldOfView.Value;
        }
    }

    #region RPC Handlers

    [ImpAttributes.HostOnly]
    private void OnDropItemServer(DropItemRequest request, ulong clientId)
    {
        dropItemMessage.DispatchToClients(request);
    }

    [ImpAttributes.LocalMethod]
    private static void OnTeleportPlayerClient(TeleportPlayerRequest request)
    {
        if (request.PlayerId == RepoSteamNetwork.CurrentSteamId)
        {
            Imperium.Player.Spawn(request.Destination, Quaternion.identity);
            Imperium.Player.rb.position = request.Destination;
            PlayerController.instance.rb.position = request.Destination;
        }
    }

    [ImpAttributes.HostOnly]
    private void OnKillPlayerServer(ulong playerId, ulong clientId)
    {
        var player = ((SteamId)playerId).GetPlayerAvatar();
        if (player && player.deadSet == false) player.PlayerDeath(-1);
    }

    [ImpAttributes.HostOnly]
    private void OnRevivePlayerServer(ulong playerId, ulong clientId)
    {
        var player = ((SteamId)playerId).GetPlayerAvatar();
        if (player && player.deadSet) player.Revive();
    }

    #endregion
}