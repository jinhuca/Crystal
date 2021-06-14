using Crystal.Unity;
using Crystal;
using Modules.Views;
using System.Windows;

namespace Modules
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
			return new DirectoryModuleCatalog { ModulePath = @".\Modules" };
		}
	}
}
