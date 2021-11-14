using Crystal;
using DelegateCommandDemo.Views;
using System.Windows;

namespace DelegateCommandDemo
{
  public partial class App 
  {
    protected override Window CreateShell() => Container.Resolve<MainWindow>();
  }
}
