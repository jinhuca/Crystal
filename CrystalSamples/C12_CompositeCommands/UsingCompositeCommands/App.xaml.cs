using System.Windows;
using Crystal.Ioc;
using Crystal.Modularity;
using Crystal.Unity;
using UsingCompositeCommands.Core;
using UsingCompositeCommands.Views;

namespace UsingCompositeCommands
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleA.ModuleAModule>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
		}
	}
}
