using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0101CrystalApplication
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<Shell>();
		}
	}
}
