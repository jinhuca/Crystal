using Crystal;
using System.Windows;

namespace Regions
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
