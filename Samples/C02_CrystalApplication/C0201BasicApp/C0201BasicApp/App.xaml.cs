using Crystal;
using System.Windows;

namespace C0201BasicApp
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();
	}
}
