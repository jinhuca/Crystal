using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C02ApplicationExtensions
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
