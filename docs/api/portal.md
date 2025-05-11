# Portal API

Imperium's Portal API allows mod developers and game testers to dynamically add their own UI elements to the Imperium UI in order to speed up their workflows. This can be useful to execute your own logic at a press of a button or expose internal properties in a simple and intuitve way.

The portal window is divided into the registered portals which are further divided into sections.

There are currently seven different UI elements from which to pick from to build your portal. Each element inherits from the base class `ImpPortalElement` which provides access to builder functions that let you set generic element properties such as a tooltip and an interactible binding.

## How To Use

 To get access to a portal, you have to either request the portal for your mod GUID or use the *runtime portal*.

!!! note "The Runtime Portal"
    The runtime portal is a shared portal that can be used by anyone. It can also be used to dynamically register UI elements through Unity Explorer at runtime.

The following code is an example on how to request your mod's portal and add elements to it.

```csharp
var toggleBinding = new ImpBinding<bool>(false);

Imperium.API.Portal.ForGuid("<mod-guid>")
    .InSection("<section-name>")
    .Register(
        new ImpPortalButton("Do something", () => {}),
        new ImpPortalToggle("Is enabled", toggleBinding)
    );
```

## Interactable Bindings

Interactable bindings are bindings of the type `IBinding<bool>` that let you control whether your UI elements are interactable by the user or not. This can be useful to only let the user access your elements when a level is loaded. You can set the interactable binding of any UI element like so.

!!! example
    ```csharp
    var button = new ImpPortalButton("asdf", () => {});

    button.SetInteractableBinding(Imperium.API.State.IsLevelLoaded);
    ```
## Tooltips

A portal tooltip is a little overlay that is shown when the cursor hovers over your UI element. It can contain a title and an optional descriptive text. 

!!! example
    ```csharp
    var button = new ImpPortalButton("asdf", () => {});

    button.SetTooltip("Spawn", "Spawns all custom enemies");
    ```

## Portal Elements

### Button Element

**Class:** `Imperium.API.Types.Portals.ImpPortalButton`

