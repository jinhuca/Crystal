using C03CustomRegions.Views;
using Crystal.Ioc;
using Crystal.Regions;
using Crystal.Unity;
using System.Windows;
using System.Windows.Controls;

namespace C03CustomRegions
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
		{
			base.ConfigureRegionAdapterMappings(regionAdapterMappings);
			regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
		}
	}
}
