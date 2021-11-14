using Crystal;
using System.Windows;

namespace LoadViaAppConfig
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();

		protected override IModuleCatalog CreateModuleCatalog() => new ConfigurationModuleCatalog();
	}
}
