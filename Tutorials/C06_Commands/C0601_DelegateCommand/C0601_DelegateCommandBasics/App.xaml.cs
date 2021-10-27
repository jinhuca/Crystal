using Crystal;
using Crystal.Unity;
using System.Windows;

namespace C0601_DelegateCommandBasics
{
	public partial class App : CrystalApplication
	{
		protected override Window CreateShell() => Container.Resolve<Window>();
	}
}
