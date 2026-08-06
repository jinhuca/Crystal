using System.Collections.Frozen;

namespace Crystal.Provider.Mmi.MmiEngine;

/// <summary>
/// Represents the outcome of an MMI method invocation (<c>CimSession.InvokeMethod</c>).
/// </summary>
/// <param name="ReturnValue">
/// The method's return value. For most Win32 provider methods this is a <c>uint</c>
/// status code exposed here as a <see cref="WmiValue"/> of type <see cref="WmiType.Int"/>,
/// or <see langword="null"/> when the method has no return value or the call failed.
/// </param>
/// <param name="OutParameters">
/// The named output parameters produced by the method, keyed by parameter name.
/// Empty when the method produced no output parameters or the call failed.
/// </param>
public record WmiMethodResult(
    WmiValue? ReturnValue,
    FrozenDictionary<string, WmiValue> OutParameters) {
  /// <summary>An empty result used as the safe fallback when a call fails.</summary>
  public static WmiMethodResult Empty { get; } =
      new(null, FrozenDictionary<string, WmiValue>.Empty);

  /// <summary>
  /// Gets the return value as a status code when it is an integer, otherwise <see langword="null"/>.
  /// A value of <c>0</c> conventionally indicates success for Win32 provider methods.
  /// </summary>
  public uint? ReturnCode => ReturnValue is { Type: WmiType.Int } v ? (uint)v.AsInt() : null;
}
