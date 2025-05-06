#region

using System;
using System.Linq;
using BepInEx.Bootstrap;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Portal;

internal class PortalWindow : ImperiumWindow
{
    private Transform content;

    private GameObject modTitleTemplate;
    private GameObject sectionTitleTemplate;
    private GameObject toggleContainerTemplate;
    private GameObject sliderTemplate;
    private GameObject inputTemplate;
    private GameObject dropdownTemplate;

    protected override void InitWindow()
    {
        content = transform.Find("Content");
    }
}