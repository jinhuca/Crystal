using System.Windows;
using Crystal;
using C3201App.Views;
using C3101Sender;
using C3201Subscriber;

namespace C3201App
{
  public partial class App
  {
    protected override Window CreateShell() => Container.Resolve<Shell>();
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
      base.ConfigureModuleCatalog(moduleCatalog);
      moduleCatalog.AddModule<PublisherModule>();
      moduleCatalog.AddModule<SubscriberModule>();
    }
  }
}
