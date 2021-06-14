using C0302ModuleA;
using C0302ModuleB;
using C0302RegionInfra;
using Crystal;
using Crystal.Unity;
using System.Windows;
using System.Windows.Controls;

namespace C0302CustomRegionApp
{
	public class Bootstrapper : CrystalBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void InitializeShell(DependencyObject shell)
		{
			base.InitializeShell(shell);
			App.Current.MainWindow = (Window)Shell;
			App.Current.MainWindow.Show();
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			ModuleCatalog catalog = new ModuleCatalog();
			catalog.AddModule(typeof(ModuleAModule));
			catalog.AddModule(typeof(ModuleBModule));
			return catalog;
		}

		protected override void ConfigureRegionAdapterMappings(RegionAdapterMappings regionAdapterMappings)
		{
			base.ConfigureRegionAdapterMappings(regionAdapterMappings);
			regionAdapterMappings.RegisterMapping(typeof(StackPanel), Container.Resolve<StackPanelRegionAdapter>());
		}
	}
}
