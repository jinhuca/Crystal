using System.Collections.ObjectModel;

namespace Crystal.Modularity
{
    public interface IModuleGroupsCatalog
    {
        Collection<IModuleCatalogItem> Items { get; }
    }
}
