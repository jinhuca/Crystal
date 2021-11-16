using Crystal.Themes.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
