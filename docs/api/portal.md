# Portal API

<figure markdown="span" class="float-right-img">
  ![Portal Window](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal.png?raw=true)
  <figcaption>The code for this example can be found <a href="https://github.com/giosuel/imperium-repo/blob/development/Imperium/src/API/Examples/PortalExample.cs">here</a>.</figcaption>
</figure>

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

Portal elements get and set their value via Imperium's bindings. Read more about Imperium bindings [here](http://localhost:8000/imperium-repo/api/index.html#the-imperium-binding).

## Interactable Bindings

Interactable bindings are bindings of the type `IBinding<bool>` that let you control whether your UI elements are interactable by the user or not. This can be useful to only let the user access your elements when a level is loaded. You can set the interactable binding of any UI element like so.

#### Example
```csharp
var button = new ImpPortalButton("asdf", () => {});

button.SetInteractableBinding(Imperium.API.State.IsLevelLoaded);
```

## Tooltips

A portal tooltip is a little overlay that is shown when the cursor hovers over your UI element. It can contain a title and an optional descriptive text. 

#### Example
```csharp
var button = new ImpPortalButton("asdf", () => {});

button.SetTooltip("Spawn", "Spawns all custom enemies");
```

## Portal Elements

### Button Element

![Button Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-button.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalButton`

#### Example
```csharp
var button = new ImpPortalButton(
    label: "Spawn",
    onClick: () => {}
);
```

!!! note
    Buttons will always be grouped into a two-column grid at the end of the section.

### Text Field Element

![Text Field Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-field.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalTextField`

#### Example
```csharp
var binding = new ImpBinding<string>("");

var textField = new ImpPortalTextField(
    label: "Global Modifier",
    valueBinding: binding,
    placeholder: "Generated",
    updateOnSubmit: true, // (1)!
    allowReset: true // (2)!
);
```

1. Whether the value binding should only be updated once the user presses the return key.<br>(Default: `false`)
2. Whether the element should include a reset button to reset the value binding to its default value.<br>(Default: `true`)

### Number Field Element

![Number Field Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-number-field.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalNumberField`

#### Example
```csharp
var binding = new ImpBinding<int>(100);

var numberField = new ImpPortalNumberField(
    label: "Current Health",
    valueBinding: binding,
    minValue: 0, // (1)!
    maxValue: 9999, // (2)!
    placeholder: "Generated",
    updateOnSubmit: true, // (3)!
    allowReset: true // (4)!
);
```

1. The smallest value that can be entered into the field.<br>(Default: `int.MinValue`)
2. The largest value that can be entered into the field.<br>(Default: `int.MaxValue`)
3. Whether the value binding should only be updated once the user presses the return key.<br>(Default: `false`)
4. Whether the element should include a reset button to reset the value binding to its default value.<br>(Default: `true`)

### Decimal Field Element


![Decimal Field Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-number-field.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalDecimalField`

#### Example
```csharp
var binding = new ImpBinding<float>(100f);

var decimalField = new ImpPortalDecimalField(
    label: "Current Health",
    valueBinding: binding,
    minValue: 0f, // (1)!
    maxValue: 9999f, // (2)!
    placeholder: "Generated",
    updateOnSubmit: true, // (3)!
    allowReset: true // (4)!
);
```

1. The smallest value that can be entered into the field.<br>(Default: `float.MinValue`)
2. The largest value that can be entered into the field.<br>(Default: `float.MaxValue`)
3. Whether the value binding should only be updated once the user presses the return key.<br>(Default: `false`)
4. Whether the element should include a reset button to reset the value binding to its default value.<br>(Default: `true`)

### Dropdown Element

![Dropdown Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-dropdown.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalDropdown`

#### Example
```csharp
var binding = new ImpBinding<int>(0); // (1)!
List<string> options = ["Passive", "Active"];

var dropdown = new ImpPortalDropdown(
    label: "Behaviour",
    valueBinding: binding,
    options: options, // (2)!
    placeholder: "Generated",
    allowReset: true // (3)!
);
```

1. The value binding contains the index of the currently selected option in the option list.
2. A list of options from which the user can pick from.
3. Whether the element should include a reset button to reset the value binding to its default value.<br>(Default: `true`)

### Toggle Element

![Toggle Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-toggle.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalToggle`

#### Example
```csharp
var binding = new ImpBinding<bool>(true);

var toggle = new ImpPortalToggle(
    label: "Vision Enabled",
    valueBinding: binding,
    allowReset: true // (1)!
);
```

1. Whether the element should include a reset button to reset the value binding to its default value.<br>(Default: `true`)

!!! note
    Toggles will always be grouped into a two-column grid at the beginning of the section.

### Slider Element

![Slider Element](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/portal-slider.png?raw=true)

**Type:** `Imperium.API.Types.Portals.ImpPortalSlider`

#### Example
```csharp
var binding = new ImpBinding<float>(1f);

var slider = new ImpPortalToggle(
    label: "Spawn Chance",
    valueBinding: binding,
    minValue: 0f, // (1)!
    maxValue: 100f, // (2)!
    debounceTime: 0.1f, // (3)!
    valueUnit: "%", // (4)!
    handleFormatter: value => Mathf.RoundToInt(value), // (5)!
    useWholeNumbers: true, // (6)!
    negativeIsDefault: true, // (7)!
    allowReset: true // (8)!
);
```

1. The slider range's lower bound.
2. The slider range's upper bound.
3. Debounce time for slider updates, useful when value binding is a network binding.<br>(Default: `0`)
4. The unit of the slider's value. Used on the handle and min/max indicators.
5. A formatter function to format the text on the slider's handle.<br>(Default: `Mathf.RoundToInt()`)
6. Whether the slider should snap to whole integer numbers.<br>(Default: `false`) 
7. Whether the value binding's default value should be shown when the binding's value is negative.<br>(Default: `false`)
8. Whether the element should include a reset button to reset the value binding to its default value.<br>(Default: `true`)