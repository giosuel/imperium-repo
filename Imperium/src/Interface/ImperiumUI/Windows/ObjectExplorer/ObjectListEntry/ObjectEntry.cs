#region

using Imperium.API.Types.Networking;
using Imperium.Extensions;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using JetBrains.Annotations;
using Librarium.Binding;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntry : MonoBehaviour
{
    private TMP_Text objectNameText;

    internal Button completeButton { get; private set; }
    internal Button reviveButton { get; private set; }
    internal Button killButton { get; private set; }
    internal Button destroyButton { get; private set; }
    internal Button despawnButton { get; private set; }
    internal Button spawnButton { get; private set; }
    internal Button teleportHereButton { get; private set; }
    internal Button teleportToButton { get; private set; }
    internal Toggle activeToggle { get; private set; }

    internal string objectName { get; private set; }
    internal GameObject containerObject { get; private set; }
    internal Component component { get; private set; }

    internal int UniqueId { get; private set; }

    internal ImpBinding<bool> IsObjectActive { get; private set; }

    internal ImpTooltip tooltip { get; private set; }

    internal ObjectType Type { get; private set; }

    private RectTransform rect;

    private readonly ImpTimer intervalUpdateTimer = ImpTimer.ForInterval(0.2f);

    internal void InitItem(ImpBinding<ImpTheme> theme)
    {
        rect = gameObject.GetComponent<RectTransform>();

        IsObjectActive = new ImpBinding<bool>(true);
        Imperium.ObjectManager.DisabledObjects.OnUpdate += disabledObjects =>
        {
            if (!ObjectEntryGenerator.CanToggle(this)) return;
            ObjectEntryGenerator.ToggleObject(this, !disabledObjects.Contains(UniqueId));
            IsObjectActive.Set(!disabledObjects.Contains(UniqueId), invokeSecondary: false);
        };

        objectNameText = transform.Find("Name").GetComponent<TMP_Text>();

        activeToggle = ImpToggle.Bind("Active", transform, IsObjectActive, theme: theme);
        // IsObjectActive.onUpdate += isOn => ObjectEntryGenerator.ToggleObject(this, isOn);

        // We subscribe to secondary here to be able to skip feedback loop from DisabledObject.onUpdate() above.
        IsObjectActive.OnTriggerSecondary += () =>
        {
            Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value.Toggle(UniqueId));
        };

        // Teleport to button
        teleportToButton = ImpButton.Bind("TeleportTo", transform,
            () =>
            {
                if (Imperium.Freecam.IsFreecamEnabled.Value)
                {
                    Imperium.Freecam.Teleport(ObjectEntryGenerator.GetTeleportPosition(this));
                }
                else
                {
                    Imperium.PlayerManager.TeleportLocalPlayer(ObjectEntryGenerator.GetTeleportPosition(this));
                    Imperium.Interface.Close();
                }
            },
            isIconButton: true,
            theme: theme
        );

        // Teleport here button
        teleportHereButton = ImpButton.Bind(
            "TeleportHere",
            transform,
            () => ObjectEntryGenerator.TeleportObjectHere(this),
            isIconButton: true,
            theme: theme
        );

        // Destroy button (Unthemed, as it's red in any theme)
        destroyButton = ImpButton.Bind(
            "Destroy",
            transform,
            () => ObjectEntryGenerator.DestroyObject(this)
        );

        // Despawn button
        despawnButton = ImpButton.Bind(
            "Despawn",
            transform,
            () => ObjectEntryGenerator.DespawnObject(this),
            isIconButton: true,
            theme: theme,
            tooltipDefinition: tooltip
                ? new TooltipDefinition
                {
                    Title = "Despawn Enemy",
                    Tooltip = tooltip
                }
                : null
        );

        // Spawn button
        spawnButton = ImpButton.Bind(
            "Spawn",
            transform,
            () => ObjectEntryGenerator.SpawnObject(this),
            isIconButton: true,
            theme: theme,
            tooltipDefinition: tooltip
                ? new TooltipDefinition
                {
                    Title = "Spawn Enemy",
                    Tooltip = tooltip
                }
                : null
        );

        // Drop button
        completeButton = ImpButton.Bind(
            "Complete",
            transform,
            () => ObjectEntryGenerator.CompleteObject(this),
            isIconButton: true,
            theme: theme,
            tooltipDefinition: tooltip
                ? new TooltipDefinition
                {
                    Title = "Complete",
                    Tooltip = tooltip
                }
                : null
        );

        // Kill button (Unthemes, as it is red in every theme)
        killButton = ImpButton.Bind("Kill", transform, () => ObjectEntryGenerator.KillObject(this));

        // Revive button (Unthemed, as it is blue in every theme)
        reviveButton = ImpButton.Bind("Revive", transform, () => ObjectEntryGenerator.ReviveObject(this));
    }

    internal void ClearItem() => ClearItem(0);

    internal void ClearItem(float positionY)
    {
        component = null;
        tooltip = null;

        objectNameText.text = "";
        teleportHereButton.gameObject.SetActive(false);
        teleportToButton.gameObject.SetActive(false);
        destroyButton.gameObject.SetActive(false);
        activeToggle.gameObject.SetActive(false);
        despawnButton.gameObject.SetActive(false);
        spawnButton.gameObject.SetActive(false);
        completeButton.gameObject.SetActive(false);
        killButton.gameObject.SetActive(false);
        reviveButton.gameObject.SetActive(false);

        rect.anchoredPosition = new Vector2(0, -positionY);
    }

    internal void SetItem([CanBeNull] Component entryComponent, ObjectType type, ImpTooltip tooltipObj, float positionY)
    {
        if (!entryComponent) return;

        Type = type;
        component = entryComponent;

        tooltip = tooltipObj;

        rect.anchoredPosition = new Vector2(0, -positionY);

        objectName = ObjectEntryGenerator.GetObjectName(this);
        containerObject = ObjectEntryGenerator.GetContainerObject(this);
        objectNameText.text = objectName;

        UniqueId = SemiFunc.IsMultiplayer()
            ? containerObject.GetComponent<PhotonView>().ViewID
            : containerObject.GetInstanceID();

        // Silently change binding to be consistent with the new object's active status
        if (IsObjectActive.Value == Imperium.ObjectManager.DisabledObjects.Value.Contains(UniqueId))
        {
            IsObjectActive.Set(!Imperium.ObjectManager.DisabledObjects.Value.Contains(UniqueId), false);
        }

        teleportToButton.gameObject.SetActive(ObjectEntryGenerator.CanTeleportTo(this));
        teleportHereButton.gameObject.SetActive(ObjectEntryGenerator.CanTeleportHere(this));
        destroyButton.gameObject.SetActive(ObjectEntryGenerator.CanDestroy(this));
        activeToggle.gameObject.SetActive(ObjectEntryGenerator.CanToggle(this));
        despawnButton.gameObject.SetActive(ObjectEntryGenerator.CanDespawn(this));
        spawnButton.gameObject.SetActive(ObjectEntryGenerator.CanSpawn(this));
        completeButton.gameObject.SetActive(ObjectEntryGenerator.CanComplete(this));
        killButton.gameObject.SetActive(ObjectEntryGenerator.CanKill(this));
        reviveButton.gameObject.SetActive(ObjectEntryGenerator.CanRevive(this));

        ObjectEntryGenerator.InitObject(this);
    }

    private void Update()
    {
        if (intervalUpdateTimer.Tick() && component)
        {
            // These buttons need to be updated since their active state can change when the UI is open
            killButton.gameObject.SetActive(ObjectEntryGenerator.CanKill(this));
            reviveButton.gameObject.SetActive(ObjectEntryGenerator.CanRevive(this));
            spawnButton.gameObject.SetActive(ObjectEntryGenerator.CanSpawn(this));
            despawnButton.gameObject.SetActive(ObjectEntryGenerator.CanDespawn(this));

            ObjectEntryGenerator.IntervalUpdate(this);
            objectName = ObjectEntryGenerator.GetObjectName(this);
        }
    }
}