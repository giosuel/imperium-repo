using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Imperium.Core;

public class StartupManager
{
    internal StartupManager()
    {

    }
}

internal enum LaunchMode
{
    Singleplayer,
    Multiplayer
}