# Insights

![Insights](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/insights.png?raw=true)

Imperium insights expose properties and fields of a specific type of `Component` in the insight window when activated.

Insight registration is dynamic and consistent, meaning you can register the same insight multiple times and Imperium will simply override previous settings for that the provided insight.

!!! tip "Insight Inheritance"
    Insights support inheritance and polymorphism, meaning you can define insights for `GameObject` and `EnemyParent` and both will show up in the Enemy's insight. You can also override an insight in `EnemyParent` that has already been defined for `GameObject` and it will be replaced by the most specific one in the Enemy's panel.


Insights can be registered, accessed and managed through the `Imperium.API.Visualization.InsightsFor<T>` function. This function returns the insight definition for the requested object `T` which can be used to register and unregister insights as well as to se various other insight properties.

## Usage Guide
The following code accesses the insight definition for `EnemyParent` objects and registers multiple insights.

```csharp
Imperium.API.Visualization.InsightsFor<EnemyParent>()
    .SetNameGenerator(enemy => enemy.enemyName) // (1)!
    .SetIsDeadGenerator(enemy => !enemy.Spawned) // (2)!
    .RegisterInsight("Health", enemy => $"{enemy.Enemy.Health.health} HP") // (3)!
    .RegisterInsight(
        "Spawn Timer",
        enemy => Librarium.Formatting.FormatSecondsMinutes(enemy.SpawnedTimer)
    ) // (4)!
    .RegisterInsight(
        "Despawn Timer",
        enemy => Librarium.Formatting.FormatSecondsMinutes(enemy.DespawnedTimer)
    ) // (4)!
    .RegisterInsight(
        "Position",
         enemy => Librarium.Formatting.FormatVector(enemy.transform.position)
    ) // (4)!
    .SetConfigKey("Enemies"); // (5)!
```

1. A function to get the name of the object. This will be the title of the insight panel.
2. A function to return a bool that indicates whether the object is currently dead or despawned.
3. Registers an insight with the name "Health" and a function that gets an enemy's current health.
4. Registers an insight with the name "Spawn Timer" and a function that gets an enemy's current spawn timer. Also utilizes a Librarium function to format seconds into a readable string (e.g. `2m 12s`).
5. Sets the config key for the registered insight. The user can toggle groups of insights in the visualization UI.

!!! warning
    Every insight definition's "name generator" function and "is dead generator" function is executed every second. The insight functions are executed every **200 milliseconds**. Keep this in mind when writing expensive insights and maybe consider caching components or objects when calling expensive functions like `GameObject.Find()` in insight functions.