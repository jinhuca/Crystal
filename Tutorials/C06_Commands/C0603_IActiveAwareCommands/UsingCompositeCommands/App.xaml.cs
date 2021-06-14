using System.Windows;
using Crystal;
using Crystal.Unity;
using ModuleA;
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

		protected override void RegisterTypes(IContainerRegistry containerRegistry)
		{
			containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
		}

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<ModuleAModule>();
		}
	}
}
