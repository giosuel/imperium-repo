#region

using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core;

public abstract class ImpLifecycleObject : MonoBehaviour
{
    internal static T Create<T>(Transform parent, IBinding<bool> isLevelLoaded, IBinding<int> playersConnected)
        where T : ImpLifecycleObject
    {
        var lifecycleObj = new GameObject($"Imp_{typeof(T).Name}").AddComponent<T>();
        lifecycleObj.transform.SetParent(parent);
        lifecycleObj.Init(playersConnected);

        isLevelLoaded.OnUpdate += lifecycleObj.OnLevelLoadUpdate;

        return lifecycleObj;
    }

    private void Init(IBinding<int> playersConnected)
    {
        playersConnected.OnUpdate += OnPlayersUpdate;

        Init();
    }

    private void OnLevelLoadUpdate(bool isLoaded)
    {
        if (isLoaded)
        {
            OnLevelLoad();
        }
        else
        {
            OnLevelUnload();
        }
    }

    protected virtual void OnLevelLoad()
    {
    }

    protected virtual void OnLevelUnload()
    {
    }

    protected virtual void Init()
    {
    }

    protected virtual void OnPlayersUpdate(int playersConnected)
    {
    }
}