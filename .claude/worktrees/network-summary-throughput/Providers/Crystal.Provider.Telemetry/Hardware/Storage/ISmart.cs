using System.Collections.Generic;

namespace Crystal.Provider.Telemetry.Hardware.Storage;

/// <summary>
/// Provides access to the S.M.A.R.T. attributes of a storage device.
/// </summary>
public interface ISmart {
  /// <summary>
  /// Gets all available smart attributes.
  /// </summary>
  IReadOnlyList<SmartAttribute> Attributes { get; }
}
