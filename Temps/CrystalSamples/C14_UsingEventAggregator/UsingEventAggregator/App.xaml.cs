using System.Windows;
using Crystal;
using Crystal.Unity;
using UsingEventAggregator.Views;

namespace UsingEventAggregator
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleA.ModuleAModule>();
			moduleCatalog.AddModule<ModuleB.ModuleBModule>();
		}
	}
}
