#region

using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Visualizers.Objects;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Visualization.ObjectVisualizerEntries;

public class ObjectVisualizerEntityEntry : MonoBehaviour
{
    internal void Init(EntityGizmoConfig config, IBinding<ImpTheme> themeBinding)
    {
        transform.Find("Name").GetComponent<TMP_Text>().text = config.entityName;

        ImpToggle.Bind("Checkboxes/Pathfinding", transform, config.Pathfinding, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Proximity", transform, config.Proximity, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Vision", transform, config.Vision, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Hearing", transform, config.Hearing, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Custom", transform, config.Custom, theme: themeBinding);
    }
}