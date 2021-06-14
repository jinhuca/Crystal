using System.Windows;

namespace RegionWpfMainApp
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			Bootstrapper bootstrapper = new Bootstrapper();
			bootstrapper.Run(); 
		}
	}
}
