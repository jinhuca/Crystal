using System.Windows;

namespace C0201_DryIocBootstrapperApp
{
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      var bootstrapper = new DryIocBootstrapper();
      bootstrapper.Run();
    }
  }
}
