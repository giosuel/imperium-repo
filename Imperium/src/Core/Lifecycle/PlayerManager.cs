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

    internal readonly ImpExternalBinding<Vector3?, bool> TruckTPAnchor = new(
        () => GameObject.Find("Truck Item Shelf")?.transform.position
    );

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

    protected override void OnSceneLoad()
    {
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
        if (!Imperium.IsArenaLoaded) return;

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

    private void Update()
    {
        if (!Mathf.Approximately(Imperium.Settings.Player.CustomFieldOfView.Value, 70))
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
        if (player) player.PlayerDeath(-1);
    }

    [ImpAttributes.HostOnly]
    private void OnRevivePlayerServer(ulong playerId, ulong clientId)
    {
        var player = ((SteamId)playerId).GetPlayerAvatar();
        if (player) player.Revive();
    }

    [ImpAttributes.LocalMethod]
    private static void OnRevivePlayerClient(ulong playerId)
    {
        // var player = Imperium.StartOfRound.allPlayerScripts.First(player => player.actualClientId == playerId);
        //
        // Imperium.StartOfRound.livingPlayers++;
        //
        // // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        // if (player.playerBodyAnimator) player.playerBodyAnimator.SetBool("Limp", value: false);
        // // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        // HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
        // // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        // HUDManager.Instance.gameOverAnimator.SetTrigger("revive");
        //
        // player.isClimbingLadder = false;
        // player.thisController.enabled = true;
        // player.health = 100;
        // player.carryWeight = 1;
        // player.disableLookInput = false;
        // player.isPlayerDead = false;
        // player.isPlayerControlled = true;
        // player.isInElevator = true;
        // player.isInHangarShipRoom = true;
        // player.isInsideFactory = false;
        // player.parentedToElevatorLastFrame = false;
        // player.setPositionOfDeadPlayer = false;
        // player.criticallyInjured = false;
        // player.bleedingHeavily = false;
        // player.activatingItem = false;
        // player.twoHanded = false;
        // player.inSpecialInteractAnimation = false;
        // player.disableSyncInAnimation = false;
        // player.inAnimationWithEnemy = null;
        // player.holdingWalkieTalkie = false;
        // player.speakingToWalkieTalkie = false;
        // player.isSinking = false;
        // player.isUnderwater = false;
        // player.sinkingValue = 0f;
        // player.hasBegunSpectating = false;
        // player.hinderedMultiplier = 1f;
        // player.isMovementHindered = 0;
        // player.sourcesCausingSinking = 0;
        // player.spectatedPlayerScript = null;
        // player.helmetLight.enabled = false;
        //
        // player.ResetPlayerBloodObjects(player.isPlayerDead);
        // player.ResetZAndXRotation();
        // player.TeleportPlayer(Imperium.StartOfRound.shipDoorNode.position);
        // player.DisablePlayerModel(player.gameObject, enable: true, disableLocalArms: true);
        // player.Crouch(crouch: false);
        // player.statusEffectAudio.Stop();
        // player.DisableJetpackControlsLocally();
        // Imperium.StartOfRound.SetPlayerObjectExtrapolate(enable: false);
        //
        // HUDManager.Instance.RemoveSpectateUI();
        // HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
        //
        // Imperium.StartOfRound.SetSpectateCameraToGameOverMode(enableGameOver: false, player);
        //
        // // Close interface if player has revived themselves
        // if (playerId == NetworkManager.Singleton.LocalClientId) Imperium.Interface.Close();
        //
        // Imperium.StartOfRound.allPlayersDead = false;
        // Imperium.StartOfRound.UpdatePlayerVoiceEffects();
        // Imperium.StartOfRound.ResetMiscValues();
        //
        // // Respawn UI because for some reason this is not happening already
        // Imperium.Settings.Rendering.PlayerHUD.Set(false);
        // Imperium.Settings.Rendering.PlayerHUD.Set(true);
    }

    #endregion
}