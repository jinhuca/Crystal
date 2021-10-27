using System.Windows;
using C0501_ModuleInAppConfig.Views;
using Crystal;

namespace C0501_ModuleInAppConfig
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			return new ConfigurationModuleCatalog();
		}
	}
}
