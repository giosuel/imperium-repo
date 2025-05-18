---
hide:
  - navigation
---

# The Console

The Console is an alternative way to interact with many of Imperium's features. It allows for quick executing of certain functionality or changing of settings.

![Imperium Console](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console.png?raw=true)

!!! example "Experimental"
    The console is still WIP and more features and use cases will follow in the future.

In general, the console is designed with maximum user-friendliness in mind. I would recommend to give it a try in-game before diving into the documentation below to get a general feeling for it. The implementation is quite complex and perhaps difficult to understand without ever having played with it before.

## Command Overview

Console commands are divided into several different command categories. The following list will provide an overview of each of the categories and a short description of how they can be used.

Commands don't need to be typed out fully. When the input field is submitted, the console will attempt to execute the currently selected element in the auto-complete menu. If the provided arguments did not match the selected command's argument list, an error message will be displayed at the bottom left.

### Auto-Complete

The console comes with a dropdown-like menu that provides a list of auto-complete suggestions. The search query **does not need to exactly match the command**. As long as the desired command is in the auto-complete list and selected, it will be executed. Entries in the list can also be clicked, which will have the same effect as submitting the input field with that command selected.

For example, submitting the following input field will still spawn ten ducks, even though the search query does not exactly match the spawn command's name.

![Imperium Console Autocomplete](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-autocomplete.png?raw=true)

!!! tip "Settings Locator"
    The icon on the left of *setting commands* that also have a UI element bound to their `IBinding` (e.g. Night Vision) can be clicked to open the location of the corresponding UI element.

### Arguments
To simplify console usage, the tokenizer starts reading the argument list from the back. For example, this means that for commands that only take one argument, all text between the first and the last argument will be ignored. The following commands all set the player's night vision to 50%.

```bash
night 50
night vision 50
night vision asdfasdf 50
```

### Keywords
Keywords are specific terms that can be used to search for commands that belong to a specific category. The following input will return all commands in the `spawn` category in the auto-complete list. 

```bash
spawn
```

If a keyword is used at the beginning of the input, the actual search query will be shifted to the second token. Keywords are only used to make searching for commands easier. They have **no effect on the execution** of the command. The following two commands both spawn 10 ducks into the scene.

```bash
apex predator 10
spawn apex predator 10
```

For a full list of the keywords for each category, please refer to the category descriptions below.


## Command Categories

### Spawn Commands

Spawn commands are used to spawn *entities*, *items* and *valuables* into the current scene.

![Imperium Console Spawn](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-spawn.png?raw=true)

**Arguments:** Spawn commands take either no or one argument. If no argument is provided, a single object of the selected type is spawned.

**Availability:** They are only available when a game level is loaded.

**Keywords:** `spawn`

#### Example

```bash
/* Without keyword */
<enemy|item|valuable> [<amount>]

/* With keyword */
spawn <enemy|item|valuable> [<amount>]
```

### Toggle Commands

Toggle commands are a simpler form of a setting command. They can change the value of a [`IBinding<bool>`](api/index.html#the-imperium-binding).

![Imperium Console Toggle](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-toggle.png?raw=true)

**Arguments:** Toggle commands take either no or one argument. If no argument is provided, the binding will swap its current state.

**Availability:** The availability of toggle commands depends on the availability of the underlying binding. Some can be accessed at any point and some can only be accessed when a game level is loaded.

**Keywords:** `enable`, `disable`, `toggle`

#### Syntax

```java
/* Without keyword */
<setting> enable|disable|toggle

/* With keyword */
enable <setting>
```

### Setting Commands

Setting commands are a bit more complex than toggle commands and can set the value of a [`IBinding<string>`](api/index.html#the-imperium-binding), [`IBinding<int>`](api/index.html#the-imperium-binding) or [`IBinding<float>`](api/index.html#the-imperium-binding).

![Imperium Console Setting](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-setting.png?raw=true)

**Arguments:**The setting commands do require at least one argument.

**Availability:** The availability of setting commands depends on the availability of the underlying binding. Some can be accessed at any point and some can only be accessed when a game level is loaded.

**Keywords:** `set`, `reset`

```java
/* Without keyword */
<setting> <value>|reset

/* With keyword */
set <setting> <value>

/* With reset keyword */
reset <setting>
```

### Window Commands

Window commands can be used to open an Imperium window within the Imperium UI.

![Imperium Console Window](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-window.png?raw=true)

**Availability:** The availability of window commands depends on the window. Some windows can be accessed at any point and some can only be accessed when a game level is loaded.

**Keywords:** `open`

#### Syntax

```bash
/* Without keyword */
<window>

/* With keyword */
open <window>
```

### Level Commands

Level commands can be used to make the game load a new level.

![Imperium Console Level](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-level.png?raw=true)

**Arguments:** Level commands take either no or one argument. If no argument is supplied, level number will be used instead.

**Availability:** Level commands are only available when a game level is loaded.

**Keywords:** `load`, `level`

#### Syntax

```bash
/* Without keyword */
<level> [<level-number>]

/* With keyword */
load <level> [<level-number>]
```

### Action Commands

Action commands are simple function commands that execute arbitrary functionality with optional parameters.

![Imperium Console Action](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/console-action.png?raw=true)

#### Syntax

```bash
<action>
```