using System.Windows;
using C0503_LoadModulesDirectory.Views;
using Crystal;

namespace C0503_LoadModulesDirectory
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			return new DirectoryModuleCatalog { ModulePath = @"..\..\Modules\net6.0-windows" };
		}
	}
}
