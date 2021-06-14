using C0301ModuleA;
using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0301RegionApp
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
	}
}
