using Crystal;
using Crystal.Unity;
using System.Windows;

namespace ModuleApp
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

		protected override IModuleCatalog CreateModuleCatalog()
		{
			return new ConfigurationModuleCatalog();
		}
	}
}
