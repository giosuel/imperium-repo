using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Imperium.Core.Input.InputUtilsStatic;

public abstract class LcInputActions
{
    private readonly InputActionMap actionMap;
    private InputActionAsset Asset;

    protected LcInputActions()
    {
        actionMap = new InputActionMap(GetType().Name);

        var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<InputActionAttribute>();
            if (attr is null) continue;

            if (prop.PropertyType != typeof(InputAction)) continue;

            attr.ActionId ??= prop.Name;

            var kbmPath = string.IsNullOrWhiteSpace(attr.KbmPath) ? "<InputUtils-Kbm-Not-Bound>" : attr.KbmPath;

            var inputAction = actionMap.AddAction(attr.ActionId, binding: kbmPath);
            // inputAction.Enable();
            prop.SetValue(this, inputAction);
        }
    }

    public void Enable()
    {
        if (!Asset)
        {
            actionMap.Disable();
            Asset = ScriptableObject.CreateInstance<InputActionAsset>();
            Asset.AddActionMap(actionMap);
        }

        actionMap.Enable();
        Asset.Enable();
    }

    public void Disable()
    {
        actionMap.Disable();
        Asset.Disable();
    }
}