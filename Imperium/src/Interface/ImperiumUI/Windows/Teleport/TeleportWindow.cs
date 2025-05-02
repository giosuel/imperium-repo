#region

using Imperium.Interface.ImperiumUI.Windows.Teleport.Widgets;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Teleport;

internal class TeleportWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        RegisterWidget<Waypoints>(content, "Waypoints");
        RegisterWidget<Teleportation>(content, "Teleportation");
    }
}