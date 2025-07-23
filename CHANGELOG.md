# Changelog

## Imperium v0.2.4 - Bug fixes

### Bug Fixes

- Removed an unused boolean that caused Quickload to throw an exception.
- Fixed Quickload in singleplayer and multiplayer.

## Imperium v0.2.3 - Bug fixes

### Bug Fixes

- Fixed a bug that made it so spawned enemies entered an infinite spawn-despawn loop.
- Added support for tumble wings and the crouch rest upgrades.

## Imperium v0.2.2 - Bug fixes

### Bug Fixes

- Fixed a bug that made the console crash on duplicate level names.
- Fixed the disable enemies option in the game control window.
- Fixed the spawn timer settings in the game control window.
- Fixed a bug that made it so duplicate enemy, item and valuable names crashed the console.

### QoL Changes

- Added the enemy action timer to the enemy insights.
- Removed the minimap settings window and button.
- Removed multiple unnecessary sliders from the grabber settings menu.

## Imperium v0.2.1 - The Console

### Changes

- Added the new Console UI that allows you to use Imperium via an in-game console.
  - Please refer to the [wiki](https://giosuel.github.io/imperium-repo/console.html) for a detailed usage guide.

- Removed the Imperium dock on the left to make the UI more user-friendly.

### Bug Fixes

- Fixed some buttons in the UI being covered by an invisible element.
- Fixed the tooltip persisting when switching between Imperium UIs.
- Fixed a bug that allowed players to kill and or revive themselves multiple times.

### QoL Changes

- Added icons to each window to increase memorability.
- Changed the event log icon for a more fitting one.
- Added a reference to the Portal API in the portal window.

## Imperium v0.2.0 - Portal API and Upgrades

### Changes

- Introduction of the Portal API for mod developers to add their own UI elements.
  - More information in the Imperium API [documentation](<https://giosuel.github.io/imperium-repo/api/portal.html>).
- Added a new window to add and remove player upgrades.
- Added a health bar and player detection bar to enemies.
- Added damage indicators for enemies.
- Made it so Imperium can now be used from the main menu and the lobby.
- Added an option to force the game to spawn valuables in all possible spawn locations.

### Bug Fixes

- Fixed some bugs that caused errors in the development version of the game.
- Fixed a bug that made disabled elements look enabled after the theme has been changed.
- Fixed an issue where the Spawning UI was not scaling properly on wide monitors.
- Fixed a bug that made it so the hide despawned insight setting didn't work properly.
- Fixed a bug with night vision when crouching.
- Fixed the field of view slider in the control center.
- Fixed the minimap being black after loading into a level for the first time.
- Fixed the spawning of multiple enemies with one spawn command.


### QoL Changes

- Made it so items and valuables no longer spawn beneath the floor or behind walls.
- Made it so input fields now submit on value change instead of on submit.
- Various optimizations and consistency fixes around UI elements.

### Compatibility

Compatible with REPO release version  `v0.1.2` and beta version `v0.1.2.23_beta`.

## Imperium v0.1.4 - Bug fixes and QoL improvements

### Changes

- Moved level generation to its own window and moved grabber settings to the control center.
- Made it so invalid level point connections are highlighted in red.
- Added the `Z` keybind to toggle cinematic mode.
- Made it so the NavMesh visualizer automatically updates with the NavMesh.
- Added an option to change the module override type to fix broken generation.
- Added an option to override the level number.

### Bug Fixes

-  Fixed a bug that made it so the minimap toggles when using the chat.
-  Fixed a bug that made it so the DisableGameOver option did nothing.
-  Fixed some valuable's insight panels being deastically offset.
-  Fixed a bug that messed up the object explorer in singleplayer.

### QoL Changes

- Added various tooltips to the UI.
- Fixed placeholders in the teleportation window.
- Improved the materials and colors of the pathfinding visualizers.
- Made it so the module override list is now sorted alphabetically.
- Fixed the fonts used in insight panels to match the rest of Imperium.
- Removed the "Teleport Here" button from extracton points in the object explorer

### Compatibility

Compatible with REPO release version  `v0.1.2` and beta version `v0.1.2.21_beta`.

## Imperium v0.1.3 - Hotfix

### Bug Fixes

-  Fixed a bug that made it so certain upgrades are ignored.

### Compatibility

Compatible with REPO release version  `v0.1.2` and beta version `v0.1.2.21_beta`.

## Imperium v0.1.2 - Hotfix

### Bug Fixes

-  Fixed a bug that made it so levels would generate with only one module.

### Compatibility

Compatible with REPO release version  `v0.1.2` and beta version `v0.1.2.21_beta`.

## Imperium v0.1.1 - Hotfix

### Changes

-  Removed Unity Explorer as mandatory dependency.

### Compatibility

Compatible with REPO release version  `v0.1.2` and beta version `v0.1.2.21_beta`.

## Imperium v0.1.0 - The Beta Release

Initial beta release version.

### Compatibility

Compatible with REPO release version  `v0.1.2` and beta version `v0.1.2.21_beta`.
