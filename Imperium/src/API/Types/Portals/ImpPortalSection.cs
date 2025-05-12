#region

using System.Collections.Generic;

#endregion

namespace Imperium.API.Types.Portals;

public record ImpPortalSection
{
    internal readonly string Name;
    internal readonly List<ImpPortalElement> Elements = [];

    internal ImpPortalSection(string name)
    {
        Name = name;
    }

    public void Register(params ImpPortalElement[] elements) => Elements.AddRange(elements);
}