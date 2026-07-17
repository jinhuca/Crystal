namespace Crystal.Plot2D.Axes;

public abstract class DateTimeLabelProviderBase : LabelProviderBase<System.DateTime> {
  private string dateFormat;

  protected string DateFormat {
    get => dateFormat;
    set => dateFormat = value;
  }

  protected override string GetStringCore(LabelTickInfo<System.DateTime> tickInfo) {
    return tickInfo.Tick.ToString(format: dateFormat);
  }

  protected virtual string GetDateFormat(DifferenceIn diff) {
    string format = null;

    switch(diff) {
      case DifferenceIn.Year:
        format = "yyyy";
        break;
      case DifferenceIn.Month:
        format = "MMM";
        break;
      case DifferenceIn.Day:
        format = "%d";
        break;
      case DifferenceIn.Hour:
        format = "HH:mm";
        break;
      case DifferenceIn.Minute:
        format = "%m";
        break;
      case DifferenceIn.Second:
        format = "ss";
        break;
      case DifferenceIn.Millisecond:
        format = "fff";
        break;
    }

    return format;
  }
}
