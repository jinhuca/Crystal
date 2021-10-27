using System.Windows;

namespace C0202_CrystalBootstrapper_Unity
{
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			var bootstrapper = new Bootstrapper();
			bootstrapper.Run();
		}
	}
}
