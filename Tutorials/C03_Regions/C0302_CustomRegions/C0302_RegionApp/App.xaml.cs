using System.Windows;
using System.Windows.Controls;
using C0302_ModuleA;
using C0302_ModuleB;
using C0302_RegionInfra;
using Crystal;

namespace C0302_RegionApp
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			ModuleCatalog catalog = new();
			catalog.AddModule(typeof(C0302ModuleAModule));
			catalog.AddModule(typeof(C0302ModuleBModule));
			return catalog;
		}

		protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
		{
			base.ConfigureRegionAdapterMappings(regionAdapterMappings);
			regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
		}
	}
}
