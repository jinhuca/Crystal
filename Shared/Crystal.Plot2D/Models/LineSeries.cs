using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Crystal.Plot2D.Models;

  public class LineSeries
  {
      public IEnumerable<Point> Points { get; set; }
      public Brush Stroke { get; set; }
      public double Thickness { get; set; } = 1.0;
      public Brush Fill { get; set; }
      public bool ShowRange { get; set; } = false;
      public bool Visible { get; set; } = true;
      public int ZIndex { get; set; } = -1000;
      // Baseline used when drawing filled range under the line. If NaN, the control's
      // global YMin (if set) will be used instead.
      public double Baseline { get; set; } = double.NaN;
  }
