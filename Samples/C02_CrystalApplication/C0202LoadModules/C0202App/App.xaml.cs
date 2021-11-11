using C0202ModuleA;
using Crystal;
using System.Windows;

namespace C0202App
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
		{
			moduleCatalog.AddModule<Module>();
		}
	}
}
