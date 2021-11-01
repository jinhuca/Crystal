using System.Windows;
using Crystal;

namespace S0201App
{
	public partial class App
	{
		protected override Window CreateShell()
		{
			return Container.Resolve<Shell>();
		}
	}
}
