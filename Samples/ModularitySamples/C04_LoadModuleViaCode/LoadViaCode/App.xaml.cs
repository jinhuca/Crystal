using Crystal;
using ModuleA;
using System.Windows;

namespace LoadViaCode
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

		protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog) => moduleCatalog.AddModule<Module>();
	}
}
