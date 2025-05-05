# Static Visualizers

Static visualizers can be used to highlight a group of objects in the game. As soon as a static visualizer is registered, Imperium makes sure the visualizer is shown and updated when the arena is loaded and unloaded. The target objects can be defined by a **Unity Tag**, **Unity Layer** or **Component Type**.

Static visualizers are divided into two categories. Point visualizers and hitbox visualizers.

## Point Visualizers

Point visualizers place a single spherical object at each target object's location. This can be useful to highlight otherwise invisible objects such as level points or spawn locations.

!!! example "Example 1 (Unity Tag)"
    The following example highlights the position of all objects with the `Player` tag with a purple wireframe sphere.

    ```csharp
    Imperium.API.Visualization.Point(
        isOn: true,
        identifier: "Player",
        type: Imperium.API.Types.VisualizerIdentifier.Tag,
        radius: 1f,
        material: Imperium.API.Materials.WireframePurple
    );
    ```

!!! example "Example 2 (Component Type)"
    The following example highlights the position of all objects with the `PlayerAvatar` component with a red wireframe sphere.

    ```csharp
    Imperium.API.Visualization.Point<PlayerAvatar>(
        isOn: true,
        radius: 1f,
        material: Imperium.API.Materials.WireframeRed
    );
    ```

## Hitbox Visualizers

Hitbox visualizers instantiate a sphere, cube or capsule for each sphere, box or capsule collider in the object's hierarchy.

!!! example "Example 1 (Unity Tag)"
    The following example visualizes all colliders of all objects with the `Player` tag with the purple wireframe material.

    ```csharp
    Imperium.API.Visualization.Collider(
        isOn: true,
        identifier: "Player",
        type: Imperium.API.Types.VisualizerIdentifier.Tag,
        radius: 1f,
        material: Imperium.API.Materials.WireframePurple
    );
    ```

!!! example "Example 2 (Component Type)"
    The following example visualizes all colliders of all objects with the `PlayerAvatar` component with the red wireframe material.

    ```csharp
    Imperium.API.Visualization.Collider<PlayerAvatar>(
        isOn: true,
        radius: 1f,
        material: Imperium.API.Materials.WireframeRed
    );
    ```