#region

using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Visualization.Widgets;

internal class Visualizers : ImpWidget
{
    protected override void InitWidget()
    {
        ImpToggle.Bind(
            "Overlays/NavMeshSurfaces",
            transform,
            Imperium.Settings.Visualization.NavMeshSurfaces,
            theme
        );

        ImpToggle.Bind(
            "Gizmos/NoiseIndicators",
            transform,
            Imperium.Settings.Visualization.NoiseIndicators,
            theme
        );

        ImpToggle.Bind(
            "Gizmos/PlayerProximity",
            transform,
            Imperium.Settings.Visualization.PlayerProximity,
            theme
        );

        ImpToggle.Bind(
            "Gizmos/LevelPoints",
            transform,
            Imperium.Settings.Visualization.LevelPoints,
            theme
        );
    }
}