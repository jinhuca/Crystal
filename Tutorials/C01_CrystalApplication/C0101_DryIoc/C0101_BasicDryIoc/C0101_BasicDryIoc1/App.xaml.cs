using System.Windows;
using Crystal;

namespace C0101_CrystalApplication_DryIoc
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<MainWindow>();
		}
	}
}
