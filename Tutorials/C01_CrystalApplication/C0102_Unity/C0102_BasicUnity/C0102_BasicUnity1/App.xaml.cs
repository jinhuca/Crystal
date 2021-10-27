using System.Windows;
using Crystal;

namespace C0102_CrystalApplication_Unity
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();
	}
}
