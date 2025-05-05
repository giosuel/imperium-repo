using Librarium.Binding;

namespace Imperium.API;

public static class State
{
    public static ImpBinaryBinding IsImperiumEnabled => Imperium.IsImperiumEnabled;
}