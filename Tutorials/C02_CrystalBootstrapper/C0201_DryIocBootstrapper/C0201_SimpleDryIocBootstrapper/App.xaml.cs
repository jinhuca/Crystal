using System.Windows;

namespace C0201_CrystalBootstrapper_DryIoc
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
