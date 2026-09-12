using System;
using System.Windows;

namespace RangeBarDemo {
  public partial class PowerTileWindow : Window {
    public PowerTileWindow() {
      InitializeComponent();
      Loaded += (_, _) => SeedSparkline();
    }

    // Fake package-watts history so the sparkline shows a realistic trend shape ending near the
    // current 12.72 W reading. A couple of load spikes give the plot something to say.
    private void SeedSparkline() {
      var rng = new Random(7);
      double w = 15;
      for (int i = 0; i < 48; i++) {
        w += (rng.NextDouble() - 0.5) * 6;
        if (i is 14 or 30) w = 40 + rng.NextDouble() * 10;   // occasional load spike
        w = Math.Clamp(w, 9, 55);
        Spark.AddValue(i < 47 ? w : 12.72);                  // land on the current value
      }
    }
  }
}
