# Imperium API

The Imperium API provides debugging tools and visualizers for mod developers to speed up their debugging and testing workflows. The API consists of the following modules.

* **[Visualization API](visualization/static.md)**  
Tools to visualize position, hitboxes and custom information for various game objects.
* **[Event Logging API](event-log.md)**  
Tools to log events in-game to Imperium's event log with additional event information.
* **[Interaction API (WIP)](interaction.md)**  
Interface to add your own custom UI elements to the Imperium's Interaction window.
* **[World API](world.md)**  
Tools to spawn and otherwise interact with game objects such as enemies or valueables.

To complement your debugging experience with Imperium, it is recommended to use the tools available in [Librarium](https://github.com/giosuel/librarium), a library that contains core utility that Imperium is using. For more information, please refer to librarium's code documentation.

## API Availability

The Imperium API is available as soon as Imperium has been **launched** and **enabled**. When Imperium is not currently enabled, all  API calls will result in an `ImperiumAPIException`.

Currently, Imperium is loaded as soon as the first level of the game is loaded and unloaded as soon as the game returns to the lobby or the main menu. This will be changed in the future and access to some parts of the Imperium API will be possible earlier in the loading process.

Imperium updates the binding `Imperium.API.State.IsImperiumEnabled` once it has been enabled or disabled.

![Imperium Startup Routine](https://github.com/giosuel/imperium-repo/blob/development/assets/startup.png?raw=true)

!!! info "Multiplayer Lobbies"
    When playing in multiplayer lobbies, the host can disable Imperium access for clients which will result in the API becoming unavailable until the host re-enables the access to Imperium.

