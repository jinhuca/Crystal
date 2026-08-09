namespace BiosModule.ViewModels;

/// <summary>One row in the detail view's live board-sensor table.</summary>
public sealed class BoardSensorRowViewModel {
  public BoardSensorRowViewModel(string name, string value, string min, string max,
      ReadingSeverity severity, ReadingSeverity minSeverity, ReadingSeverity maxSeverity,
      float? valueSort = null, float? minSort = null, float? maxSort = null) {
    Name = name;
    Value = value;
    Min = min;
    Max = max;
    Severity = severity;
    MinSeverity = minSeverity;
    MaxSeverity = maxSeverity;
    // A missing reading sorts last regardless of direction; NaN compares greater than every real
    // number in .NET's default comparer, so ascending puts blanks at the bottom (descending at top).
    ValueSort = valueSort ?? double.NaN;
    MinSort = minSort ?? double.NaN;
    MaxSort = maxSort ?? double.NaN;
  }

  public string Name { get; }
  public string Value { get; }
  public string Min { get; }
  public string Max { get; }

  /// <summary>Numeric keys behind the formatted Value/Min/Max strings, so column sorting is numeric
  /// rather than lexical (otherwise "10 V" would sort before "9 V"). Missing readings are NaN.</summary>
  public double ValueSort { get; }
  public double MinSort { get; }
  public double MaxSort { get; }

  /// <summary>How far this reading sits from spec, for rows we can confidently judge (ATX voltage
  /// rails and the CMOS cell); <see cref="ReadingSeverity.Normal"/> for everything else.</summary>
  public ReadingSeverity Severity { get; }

  /// <summary>Severity of the recorded low/high extremes, graded the same way as the live value, so
  /// a rail that dipped critical then recovered still shows the fault in its Min/Max column.</summary>
  public ReadingSeverity MinSeverity { get; }
  public ReadingSeverity MaxSeverity { get; }
}
