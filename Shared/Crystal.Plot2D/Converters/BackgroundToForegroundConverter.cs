using System.Windows.Media;

namespace Crystal.Plot2D.Converters;

public class BackgroundToForegroundConverter : GenericValueConverter<SolidColorBrush> {
  protected override object ConvertCore(SolidColorBrush value) {
    var back = value;
    var diff = back.Color - Colors.Black;
    var summ = diff.R + diff.G + diff.B;
    var border = 3 * 255 / 2;
    return summ > border ? Brushes.Black : Brushes.White;
  }
}
