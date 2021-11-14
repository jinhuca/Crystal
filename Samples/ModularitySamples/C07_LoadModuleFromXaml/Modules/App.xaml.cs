using Modules.Views;
using System.Windows;
using System;
using Crystal;

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
			return new XamlModuleCatalog(new Uri("/Modules;component/ModuleCatalog.xaml", UriKind.Relative));
		}
	}
}
