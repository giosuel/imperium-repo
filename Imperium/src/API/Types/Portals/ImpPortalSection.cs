using System.Collections.Generic;
using Imperium.Core.Portal;

namespace Imperium.API.Types.Portals;

public record ImpPortalSection
{
    internal readonly List<ImpPortalElement> Elements = [];

    internal ImpPortalSection()
    {
    }

    public ImpPortalSection Register(ImpPortalElement element)
    {
        Elements.Add(element);

        return this;
    }
}