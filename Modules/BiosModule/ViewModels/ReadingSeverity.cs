namespace BiosModule.ViewModels;

/// <summary>How far a live board reading sits from its healthy range.</summary>
public enum ReadingSeverity {
  Normal,
  Warning,
  Critical,
}

/// <summary>Classifies board voltage rails and the CMOS coin-cell against their nominal values.
/// ATX rails are held to ±5% (warning) / ±10% (critical); the CMOS cell is graded on absolute
/// volts since it droops with age rather than tracking a regulated rail. A null reading (sensor
/// absent) is <see cref="ReadingSeverity.Normal"/> — the value renders as the em-dash placeholder,
/// so there is nothing to flag.</summary>
public static class BoardReadingSeverity {
  public static ReadingSeverity Rail(float? value, float nominal) {
    if (value is not { } v) return ReadingSeverity.Normal;
    float deviation = System.Math.Abs(v - nominal) / nominal;
    return deviation switch {
      > 0.10f => ReadingSeverity.Critical,
      > 0.05f => ReadingSeverity.Warning,
      _ => ReadingSeverity.Normal,
    };
  }

  // A healthy CR2032 reads ~3.0 V; BIOS warns of a dead clock battery around 2.7 V and below.
  public static ReadingSeverity Cmos(float? value) {
    if (value is not { } v) return ReadingSeverity.Normal;
    return v switch {
      < 2.5f => ReadingSeverity.Critical,
      < 2.7f => ReadingSeverity.Warning,
      _ => ReadingSeverity.Normal,
    };
  }
}
