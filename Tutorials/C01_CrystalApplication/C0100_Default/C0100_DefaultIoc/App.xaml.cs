using System.Windows;
using Crystal;

namespace C0100_DefaultIoc
{
	public partial class App
	{
		protected override Window CreateShell() => Container.Resolve<MainWindow>();
	}
}
