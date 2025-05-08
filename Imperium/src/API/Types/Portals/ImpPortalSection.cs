using System.Collections.Generic;

namespace Imperium.API.Types.Portals;

public record ImpPortalSection
{
    internal readonly string Name;
    internal readonly List<ImpPortalElement> Elements = [];

    internal ImpPortalSection(string name)
    {
        Name = name;
    }

    public ImpPortalSection Register(ImpPortalElement element)
    {
        Elements.Add(element);

        return this;
    }
}