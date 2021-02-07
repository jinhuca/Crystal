using System.Windows;
using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Unity;
using UsingEventAggregator.Views;

namespace UsingEventAggregator
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
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
