using System.Collections.Generic;

namespace Imperium.API.Types.Portals;

public record ImpPortal
{
    private readonly Dictionary<string, ImpPortalSection> sections = new();

    internal ImpPortal()
    {
    }

    public static ImpPortal ForGuid(string guid)
    {
        return Imperium.PortalManager.GetPortalFor(guid);
    }

    public ImpPortalSection InSection(string name)
    {
        if (!sections.TryGetValue(name, out var section))
        {
            section = new ImpPortalSection();
            sections[name] = section;
        }

        return section;
    }
}