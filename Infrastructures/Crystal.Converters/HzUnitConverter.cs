using System;
using System.Collections.Generic;
using System.Text;

namespace Crystal.Converters;

public class HzUnitConverter {
  public static string ConvertMHzToReadableUnit(double frequencyInMHz) {
    if (frequencyInMHz < 0) {
      throw new ArgumentOutOfRangeException(nameof(frequencyInMHz), "Frequency cannot be negative.");
    }
    if (frequencyInMHz < 1000) {
      return $"{frequencyInMHz:F2} MHz";
    }
    double frequencyInGHz = frequencyInMHz / 1000;
    return $"{frequencyInGHz:F2} GHz";
  }
}