using C06ViewActivationDeactivation.Views;
using Crystal.Ioc;
using Crystal.Unity;
using System.Windows;

namespace C06ViewActivationDeactivation
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
	}
}
