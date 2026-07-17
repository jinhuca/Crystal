using Crystal.Plot2D.Common.Auxiliary;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace Crystal.Plot2D.Common.Palettes;

[ContentProperty(name: "Steps")]
public class DiscretePalette : IPalette {
  [DesignerSerializationVisibility(visibility: DesignerSerializationVisibility.Content)]
  public ObservableCollection<LinearPaletteColorStep> Steps { get; } = new();

  public DiscretePalette() { }
  public DiscretePalette(params LinearPaletteColorStep[] steps) => Steps.AddMany(children: steps);

  public Color GetColor(double t) {
    if(t <= 0) {
      return Steps[index: 0].Color;
    }

    if(t >= Steps.Last().Offset) {
      return Steps.Last().Color;
    }

    var i = 0;
    double x = 0;
    while(x < t && i < Steps.Count) {
      x = Steps[index: i].Offset;
      i++;
    }

    var result = Steps[index: i - 1].Color;
    return result;
  }

  #region IPalette Members

#pragma warning disable CS0067 // The event 'DiscretePalette.Changed' is never used
  public event EventHandler Changed;
#pragma warning restore CS0067 // The event 'DiscretePalette.Changed' is never used

  #endregion
}
