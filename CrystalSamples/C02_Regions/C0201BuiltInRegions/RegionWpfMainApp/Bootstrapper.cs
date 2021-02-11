using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Regions;
using Crystal.Unity;
using ModuleA;
using System.Windows;

namespace RegionWpfMainApp
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<Shell>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			base.ConfigureModuleCatalog(moduleCatalog);
			moduleCatalog.AddModule<ModuleAModule>();
		}

		protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
		{
			base.ConfigureRegionAdapterMappings(regionAdapterMappings);
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			
		}
	}
}
