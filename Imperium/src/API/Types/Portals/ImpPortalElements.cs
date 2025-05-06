using System;
using Librarium.Binding;

namespace Imperium.API.Types.Portals;

public record ImpPortalButton
{
    public string Title { get; init; }
    public Action OnClick { get; init; }
    public Tooltip Tooltip { get; init; } = null;
    public ImpBinaryBinding[] interactableBindings { get; init; } = [];
}

public record Tooltip
{
    public string Title { get; init; }
    public string Description { get; init; }
}