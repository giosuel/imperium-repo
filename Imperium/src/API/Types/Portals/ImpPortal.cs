using System.Collections.Generic;
using System.Linq;

namespace Imperium.API.Types.Portals;

/// <summary>
/// A portal is a container that holds multiple <see cref="ImpPortalSection"/> objects. Those section objects contain
/// portal elements that represent UI elements in the portal window.
///
/// Mod developers and debuggers can create those elements and bind them to their desired functionality to be able
/// to execute and track their debugging logic. 
/// </summary>
public record ImpPortal
{
    internal readonly string Name;
    internal readonly Dictionary<string, ImpPortalSection> Sections = new();

    internal int Elements => Sections.Values.Sum(section => section.Elements.Count);

    internal ImpPortal(string name)
    {
        Name = name;
    }

    public ImpPortalSection InSection(string sectionName)
    {
        if (!Sections.TryGetValue(sectionName, out var section))
        {
            section = new ImpPortalSection(sectionName);
            Sections[sectionName] = section;
        }

        return section;
    }
}