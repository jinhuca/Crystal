using Crystal;
using ModuleA;
using SharedLibary;
using System.Windows;
using System.Windows.Controls;

namespace RegionSimple
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<Shell>();

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleAModule>();
		}

		protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
		{
			base.ConfigureRegionAdapterMappings(regionAdapterMappings);
			regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
		}
	}
}
