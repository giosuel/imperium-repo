#region

using UnityEngine;

#endregion

namespace Imperium.Core.Scripts;

public class ImpScript : MonoBehaviour
{
    internal static T Create<T>(Transform parent, GameObject prefab = null) where T : ImpScript
    {
        var obj = prefab ? Instantiate(prefab) : new GameObject();
        obj.name = $"Imp_{typeof(T).Name}";
        obj.transform.SetParent(parent);

        var objScript = obj.AddComponent<T>();

        return objScript;
    }
}