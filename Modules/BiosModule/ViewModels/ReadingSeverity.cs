namespace BiosModule.ViewModels;

/// <summary>How far a live board reading sits from its healthy range.</summary>
public enum ReadingSeverity {
  Normal,
  Warning,
  Critical,
}

/// <summary>Classifies board voltage rails, the CMOS coin-cell, and fans against their healthy
/// range. ATX rails are held to ±5% (warning) / ±10% (critical); the CMOS cell is graded on
/// absolute volts since it droops with age rather than tracking a regulated rail; a fan is graded
/// only for stall. A null reading (sensor absent) is <see cref="ReadingSeverity.Normal"/> — the
/// value renders as the em-dash placeholder, so there is nothing to flag.</summary>
public static class BoardReadingSeverity {
  public static ReadingSeverity Rail(float? value, float nominal) {
    if (value is not { } v) return ReadingSeverity.Normal;
    // Divide by |nominal| so negative rails (−12V) grade on magnitude rather than flipping sign.
    float deviation = System.Math.Abs(v - nominal) / System.Math.Abs(nominal);
    return deviation switch {
      > 0.10f => ReadingSeverity.Critical,
      > 0.05f => ReadingSeverity.Warning,
      _ => ReadingSeverity.Normal,
    };
  }

  // Board-area temperatures (System / VRM / PCH / chipset) normally idle 30–50 °C. These are
  // ambient/board sensors, not silicon junction temps, so a sustained high reading points at
  // airflow or VRM trouble: warn from 60 °C, critical from 70 °C.
  private const float BoardWarmC = 60f;
  private const float BoardHotC = 70f;

  public static ReadingSeverity Temperature(float? celsius) => celsius switch {
    >= BoardHotC => ReadingSeverity.Critical,
    >= BoardWarmC => ReadingSeverity.Warning,
    _ => ReadingSeverity.Normal,
  };

  // A healthy CR2032 reads ~3.0 V; BIOS warns of a dead clock battery around 2.7 V and below.
  public static ReadingSeverity Cmos(float? value) {
    if (value is not { } v) return ReadingSeverity.Normal;
    return v switch {
      < 2.5f => ReadingSeverity.Critical,
      < 2.7f => ReadingSeverity.Warning,
      _ => ReadingSeverity.Normal,
    };
  }

  // Board temperatures at/above which a stopped fan starts to matter. Below the warm mark a
  // zero-RPM fan is assumed to be in its intentional silent (semi-passive) mode, not stalled.
  private const float FanWarmC = 45f;
  private const float FanHotC = 60f;

  // A fan reading zero RPM has stalled only if it (a) has spun this session — running Max > 0, so
  // it's a populated header rather than an empty connector that always reads zero — and (b) the
  // board is warm enough that it ought to be spinning. Many chassis/EC fans stop on purpose when
  // cool, so grading is gated on board temperature to avoid flagging that normal behavior.
  public static ReadingSeverity Fan(float? rpm, float? runningMax, float? boardTemperature) {
    if (rpm is not { } r || r > 0f) return ReadingSeverity.Normal;
    if (runningMax is not { } max || max <= 0f) return ReadingSeverity.Normal;
    return boardTemperature switch {
      >= FanHotC => ReadingSeverity.Critical,
      >= FanWarmC => ReadingSeverity.Warning,
      _ => ReadingSeverity.Normal,
    };
  }
}
