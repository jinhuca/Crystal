using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Unity;
using MainModule.Views;
using System.Windows;

namespace MainModule
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
			_ = moduleCatalog.AddModule<ModuleA.ModuleAModule>();
		}
	}
}
