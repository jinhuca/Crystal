using System.Windows;

namespace C0102BootstrapperModule
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
