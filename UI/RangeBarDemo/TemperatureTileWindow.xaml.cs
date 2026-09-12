using System;
using System.Windows;

namespace RangeBarDemo {
  public partial class TemperatureTileWindow : Window {
    public TemperatureTileWindow() {
      InitializeComponent();
      Loaded += (_, _) => SeedSparkline();
    }

    // Fake package-temperature history ending near the current 48.3 °C reading, with a thermal
    // spike toward the session peak so the sparkline has a shape to read.
    private void SeedSparkline() {
      var rng = new Random(11);
      double t = 44;
      for (int i = 0; i < 48; i++) {
        t += (rng.NextDouble() - 0.5) * 5;
        if (i is 18 or 33) t = 70 + rng.NextDouble() * 8;   // load/thermal spike
        t = Math.Clamp(t, 35, 82);
        Spark.AddValue(i < 47 ? t : 48.3);                  // land on the current value
      }
    }
  }
}
