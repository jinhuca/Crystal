using C0503ActivationDeactivation.Views;
using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0503ActivationDeactivation
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
