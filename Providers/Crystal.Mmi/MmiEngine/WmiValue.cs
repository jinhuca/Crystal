namespace Crystal.Mmi.MmiEngine;

public enum WmiType : byte { None, Bool, Int, String, StringArray, DateTime, UShortArray, ULong }

public readonly struct WmiValue {
  public WmiType Type { get; }
  private readonly int _intValue;
  private readonly bool _boolValue;
  private readonly ulong _ulongValue;
  private readonly DateTime _dateTimeValue;
  private readonly object _refValue;

  public WmiValue(bool value) { Type = WmiType.Bool; _boolValue = value; _refValue = null!; }
  public WmiValue(int value) { Type = WmiType.Int; _intValue = value; _refValue = null!; }
  public WmiValue(string value) { Type = WmiType.String; _refValue = value; }
  public WmiValue(string[] value) { Type = WmiType.StringArray; _refValue = value; }
  public WmiValue(DateTime value) { Type = WmiType.DateTime; _dateTimeValue = value; _refValue = null!; }
  public WmiValue(ushort[] value) { Type = WmiType.UShortArray; _refValue = value; } // Added array type
  public WmiValue(ulong value) {
    Type = WmiType.ULong;
    _ulongValue = value;
    _intValue = 0;
    _boolValue = false;
    _dateTimeValue = default;
    _refValue = null!;
  }

  public bool AsBool() => Type == WmiType.Bool
    ? _boolValue : throw new InvalidCastException();
  public int AsInt() => Type == WmiType.Int
    ? _intValue : throw new InvalidCastException();
  public string AsString() => Type == WmiType.String
    ? (string)_refValue : throw new InvalidCastException();
  public string[] AsStringArray() => Type == WmiType.StringArray
    ? (string[])_refValue : throw new InvalidCastException();
  public DateTime AsDateTime() => Type == WmiType.DateTime
    ? _dateTimeValue : throw new InvalidCastException();
  public ushort[] AsUShortArray() => Type == WmiType.UShortArray
    ? (ushort[])_refValue : throw new InvalidCastException();
  public ulong AsULong() => Type == WmiType.ULong
    ? _ulongValue : throw new InvalidCastException();
  public ulong AsReadOnlyULong() => Type == WmiType.ULong
    ? _ulongValue : throw new InvalidCastException();

  public int? TryAsInt() => Type == WmiType.Int
    ? _intValue : null;
  public bool? TryAsBool() => Type == WmiType.Bool
    ? _boolValue : null;
  public ulong? TryAsULong() => Type == WmiType.ULong
    ? _ulongValue : null;
  public string? TryAsString() => Type == WmiType.String
    ? (string)_refValue : null;
}

