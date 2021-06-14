using Modules.Views;
using System.Windows;
using Crystal;
using Crystal.Unity;

namespace Modules
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
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
			return new DirectoryModuleCatalog() { ModulePath = @".\Modules" };
		}
	}
}
