#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Imperium.Core;
using Imperium.Interface;
using Imperium.Interface.Common;
using Librarium;
using Librarium.Binding;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;
using Random = System.Random;

#endregion

namespace Imperium.Util;

public abstract class ImpUtils
{
    /// <summary>
    ///     Tries to find value in a dictionary by key. If the key does not exist,
    ///     a new value of type T is created, indexed in the dictionary with the given key and returned.
    ///     Basically a helper function to emulate a default dictionary.
    /// </summary>
    public static T DictionaryGetOrNew<T>(IDictionary<string, T> map, string key) where T : new()
    {
        if (map.TryGetValue(key, out var list)) return list;
        return map[key] = new T();
    }

    public static void ToggleGameObjects(IEnumerable<GameObject> list, bool isOn)
    {
        foreach (var obj in list.Where(obj => obj)) obj.SetActive(isOn);
    }

    /// <summary>
    ///     Clones a random number generator. The new generator will produce the same sequence of numbers as the original.
    /// </summary>
    public static Random CloneRandom(Random random)
    {
        var cloned = new Random();

        // The seed array needs to be deep-copied since arrays are referenced types
        var seedArray = Reflection.Get<Random, int[]>(random, "_seedArray");
        Reflection.Set(cloned, "_seedArray", seedArray.ToArray());

        Reflection.CopyField(random, cloned, "_inextp");
        Reflection.CopyField(random, cloned, "_inext");

        return cloned;
    }

    /// <summary>
    ///     Attempts to invoke a callback.
    ///     If the callback throws a <see cref="NullReferenceException" /> returns default.
    /// </summary>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T InvokeDefaultOnNull<T>(Func<T> callback)
    {
        try
        {
            return callback.Invoke();
        }
        catch (NullReferenceException)
        {
            return default;
        }
    }

    public static int ToggleLayerInMask(int layerMask, int layer)
    {
        if ((layerMask & 1 << layer) != 0)
        {
            return layerMask & ~(1 << layer);
        }

        return layerMask | 1 << layer;
    }

    public static int ToggleLayersInMask(int layerMask, params int[] layers)
    {
        return layers.Aggregate(layerMask, ToggleLayerInMask);
    }

    public static bool RunSafe(Action action, string logTitle = null)
    {
        return RunSafe(action, out _, logTitle);
    }

    /// <summary>
    ///     Runs a function, catches all exceptions and returns a boolean with the status.
    ///     ///
    /// </summary>
    /// <param name="action"></param>
    /// <param name="exception"></param>
    /// <param name="logTitle"></param>
    /// <returns></returns>
    private static bool RunSafe(Action action, out Exception exception, string logTitle = null)
    {
        try
        {
            action.Invoke();
            exception = null;
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            if (logTitle != null)
            {
                Imperium.IO.LogBlock(
                    exception.StackTrace.Split('\n').Select(line => line.Trim()).ToList(),
                    title: $"[ERR] {logTitle}: {exception.Message}"
                );
            }

            return false;
        }
    }

    /// <summary>
    ///     Attempts to deserialize a JSON string into a given object. If <see cref="JsonSerializationException" /> is thrown
    ///     or the resulting object is null, false is returned and default is assigned to the out argument.
    /// </summary>
    /// <param name="jsonString"></param>
    /// <param name="deserializedObj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool DeserializeJsonSafe<T>(string jsonString, out T deserializedObj)
    {
        try
        {
            deserializedObj = JsonConvert.DeserializeObject<T>(jsonString);
            return deserializedObj != null;
        }
        catch (Exception e)
        {
            Imperium.IO.LogError($"[JSON] Exception: {e.Message}");
            deserializedObj = default;
            return false;
        }
    }

    internal abstract class Interface
    {
        /**
         * Binds a dropdown's interactability to a binding. Includes title, label and arrow image, if present.
         */
        internal static void BindDropdownInteractable(IBinding<bool> binding, Transform parent, bool inverted = false)
        {
            var dropdown = parent.Find("Dropdown").GetComponent<TMP_Dropdown>();
            dropdown.interactable = inverted ? !binding.Value : binding.Value;

            var title = parent.Find("Title")?.GetComponent<TMP_Text>();
            if (title) ToggleTextActive(title, inverted ? !binding.Value : binding.Value);

            var label = parent.Find("Dropdown/Label")?.GetComponent<TMP_Text>();
            if (label) ToggleTextActive(label, inverted ? !binding.Value : binding.Value);

            var arrow = parent.Find("Dropdown/Arrow")?.GetComponent<Image>();
            if (arrow) ToggleImageActive(arrow, inverted ? !binding.Value : binding.Value);

            binding.OnUpdate += isActive =>
            {
                dropdown.interactable = inverted ? !isActive : isActive;

                if (title) ToggleTextActive(title, inverted ? !isActive : isActive);
                if (label) ToggleTextActive(label, inverted ? !isActive : isActive);
                if (arrow) ToggleImageActive(arrow, inverted ? !isActive : isActive);
            };
        }

        /**
         * Adds an ImpTooltip to a UI element.
         */
        internal static void AddTooltip(TooltipDefinition tooltipDefinition, Transform element)
        {
            if (!tooltipDefinition.Tooltip)
            {
                Imperium.IO.LogWarning(
                    $"[UI] Failed to initialize tooltip for '{Debugging.GetTransformPath(element)}'. No tooltip provided."
                );
                return;
            }

            var interactable = element.gameObject.AddComponent<ImpInteractable>();

            interactable.onOver += position => tooltipDefinition.Tooltip.SetPosition(
                tooltipDefinition.Title,
                tooltipDefinition.Description,
                position,
                tooltipDefinition.HasAccess
            );
            interactable.onExit += () => tooltipDefinition.Tooltip.Deactivate();
        }

        internal static void ToggleImageActive(Image image, bool isActive)
        {
            image.color = ChangeAlpha(
                image.color,
                isActive ? ImpConstants.Opacity.Enabled : ImpConstants.Opacity.ImageDisabled
            );
        }

        internal static void ToggleTextActive(TMP_Text text, bool isActive)
        {
            text.color = ChangeAlpha(
                text.color,
                isActive ? ImpConstants.Opacity.Enabled : ImpConstants.Opacity.TextDisabled
            );
        }

        internal static Color ChangeAlpha(Color oldColor, float newAlpha)
        {
            var color = oldColor;
            color.a = newAlpha;
            return color;
        }

        internal static void ToggleCursorState(bool isShown)
        {
            Cursor.lockState = isShown ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isShown;
        }


        internal static IEnumerator AnimateOpacityTo(
            CanvasGroup group,
            float duration,
            float targetAlpha,
            bool setInteractable = true
        )
        {
            var startAlpha = group.alpha;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                group.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            group.alpha = targetAlpha;
            group.interactable = setInteractable;
            group.blocksRaycasts = setInteractable;
        }

        internal static IEnumerator SlideInAnimation(
            RectTransform rect,
            Vector2 direction,
            float offsetValue = 50f,
            CanvasGroup opacityGroup = null
        )
        {
            const float slideDuration = 0.15f;

            var offset = direction.normalized * offsetValue;
            var startPos = rect.anchoredPosition - offset;
            var endPos = rect.anchoredPosition;

            if (opacityGroup) opacityGroup.alpha = 0;

            var elapsed = 0f;

            while (elapsed < slideDuration)
            {
                var t = elapsed / slideDuration;

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t * t);
                if (opacityGroup) opacityGroup.alpha = Mathf.Lerp(0, 1, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            rect.anchoredPosition = endPos;
        }

        internal static IEnumerator SlideInAndBounceAnimation(RectTransform rect, Vector2 direction)
        {
            yield return SlideInAnimation(rect, direction);

            var elapsed = 0f;
            var endPos = rect.anchoredPosition;

            const float springDuration = 0.1f;
            const float springAmount = 10f;
            const float springFrequency = 10f;

            while (elapsed < springDuration)
            {
                var num = elapsed / springDuration;
                var num2 = springFrequency * (1f - num);
                var x = springAmount * Mathf.Sin(num2 * num * MathF.PI * 2f) * (1f - num);
                elapsed += Time.deltaTime;

                rect.anchoredPosition = endPos + direction.normalized * x;
                yield return null;
            }

            rect.anchoredPosition = endPos;
        }
    }

    internal abstract class Bindings
    {
        /// <summary>
        ///     Adds or removes a value to / from a set in a binding and updates the binding.
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="key">The key of the object to add to or remove from the set.</param>
        /// <param name="isOn">Whether the value should be present in the updated set.</param>
        internal static void ToggleSet<T>(IBinding<HashSet<T>> binding, T key, bool isOn)
        {
            if (isOn)
            {
                binding.Value.Add(key);
            }
            else
            {
                binding.Value.Remove(key);
            }

            binding.Set(binding.Value);
        }
    }

    internal abstract class Transpiling
    {
        internal static IEnumerable<CodeInstruction> SkipWaitingForSeconds(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {
                if (i >= 2
                    && codes[i].opcode == OpCodes.Stfld
                    && codes[i - 1].opcode == OpCodes.Newobj
                    && codes[i - 2].opcode == OpCodes.Ldc_R4
                   )
                {
                    codes[i - 2].operand = 0f;
                }
            }

            return codes.AsEnumerable();
        }
    }
}