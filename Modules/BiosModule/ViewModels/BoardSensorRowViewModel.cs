namespace BiosModule.ViewModels;

/// <summary>One row in the detail view's live board-sensor table.</summary>
public sealed class BoardSensorRowViewModel {
  public BoardSensorRowViewModel(string name, string value, string min, string max) {
    Name = name;
    Value = value;
    Min = min;
    Max = max;
  }

  public string Name { get; }
  public string Value { get; }
  public string Min { get; }
  public string Max { get; }
}
