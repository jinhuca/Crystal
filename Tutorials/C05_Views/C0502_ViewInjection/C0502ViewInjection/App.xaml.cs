using System.Windows;
using C0502ViewInjection.Views;
using Crystal;
using Crystal.Unity;

namespace C0502ViewInjection
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
