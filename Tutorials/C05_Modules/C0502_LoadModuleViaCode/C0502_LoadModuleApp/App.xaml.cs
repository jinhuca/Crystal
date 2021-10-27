using Crystal;
using System.Windows;
using C0502_ModuleA;
using C0502_LoadModuleApp.Views;

namespace C0502_LoadModuleApp
{
	public partial class App
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
