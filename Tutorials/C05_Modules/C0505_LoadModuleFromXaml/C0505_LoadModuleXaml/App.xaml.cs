using System;
using System.Windows;
using C0505_LoadModuleXaml.Views;
using Crystal;

namespace C0505_LoadModuleXaml
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}

		protected override IModuleCatalog CreateModuleCatalog()
		{
			return new XamlModuleCatalog(new Uri("/C0505_LoadModuleXaml;component/ModuleCatalog.xaml", UriKind.Relative));
		}
	}
}
