using Crystal.Themes.Controls;
using System.Windows;

namespace T0203Buttons
{
  public partial class MainWindow
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void CountingButton_OnClick(object sender, RoutedEventArgs e)
    {
      var badge = (CountingBadge.Badge as int?).GetValueOrDefault(0);
      var next = badge + 1;
      CountingBadge.SetCurrentValue(Badged.BadgeProperty, next < 43 ? next : null);
    }
  }
}
