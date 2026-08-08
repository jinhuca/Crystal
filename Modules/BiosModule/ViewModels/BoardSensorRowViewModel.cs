namespace BiosModule.ViewModels;

/// <summary>One row in the detail view's live board-sensor table.</summary>
public sealed class BoardSensorRowViewModel {
  public BoardSensorRowViewModel(string name, string value, string min, string max, ReadingSeverity severity) {
    Name = name;
    Value = value;
    Min = min;
    Max = max;
    Severity = severity;
  }

  public string Name { get; }
  public string Value { get; }
  public string Min { get; }
  public string Max { get; }

  /// <summary>How far this reading sits from spec, for rows we can confidently judge (ATX voltage
  /// rails and the CMOS cell); <see cref="ReadingSeverity.Normal"/> for everything else.</summary>
  public ReadingSeverity Severity { get; }
}
