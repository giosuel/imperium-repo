using Imperium.API.Types.Portals;
using Librarium.Binding;
using UnityEngine;

namespace Imperium.API.Examples;

internal class PortalExample : MonoBehaviour
{
    private void Awake()
    {
        var inputFieldBinding = new ImpBinding<string>("");
        var dropdownBinding = new ImpBinding<int>(0);
        var movementSpeedBinding = new ImpBinding<float>(1f);

        API.Portal.ForGuid(LCMPluginInfo.PLUGIN_GUID)
            .InSection("General Config")
            .Register(
                new ImpPortalTextField("Global Modifier", inputFieldBinding, "Generated")
                    .SetTooltip(new ImpPortalTooltip("Global Modifier", "Bla bla bla"))
                    .SetInteractableBinding(API.State.IsGameLevel),
                new ImpPortalSlider("Global Speed", new ImpBinding<float>(5f), 10, 100)
            );

        API.Portal.ForGuid(LCMPluginInfo.PLUGIN_GUID)
            .InSection("Example Enemy")
            .Register(
                new ImpPortalDropdown("Behaviour", dropdownBinding, ["Passive", "Active"], "Generated", allowReset: false),
                new ImpPortalNumberField(
                    "Current Health",
                    new ImpBinding<int>(100),
                    0,
                    100
                ),
                new ImpPortalToggle("Vision Enabled", new ImpBinding<bool>()),
                new ImpPortalToggle("Hearing Enabled", new ImpBinding<bool>()),
                new ImpPortalToggle("Disabled", new ImpBinding<bool>()),
                new ImpPortalSlider("Spawn Chance", movementSpeedBinding, 0, 100, valueUnit: "%"),
                new ImpPortalButton("Spawn", () => {}),
                new ImpPortalButton("Despawn", () => {})
            );
    }
}