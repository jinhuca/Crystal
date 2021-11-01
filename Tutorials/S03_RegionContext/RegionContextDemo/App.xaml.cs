using Crystal;
using RegionContextDemo.Views;
using S0301ModuleA;
using System.Windows;

namespace RegionContextDemo
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleAModule>();
		}
	}
}
