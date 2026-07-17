using ResourceModule.Controls.Meter;
using System.Windows;

namespace ResourceModule;

public class ResourceModule : IModule {
  public void RegisterTypes(IContainerRegistry containerRegistry) {
    containerRegistry.Register<MeterControl>();
  }

  public void OnInitialized(IContainerProvider containerProvider) {
    var resourceUri = new Uri("pack://application:,,,/ResourceModule;component/Themes/Generic.xaml", UriKind.Absolute);
    var dictionary = new ResourceDictionary { Source = resourceUri };
    Application.Current.Resources.MergedDictionaries.Add(dictionary);
  }
}
