using System.Collections.Generic;

namespace Crystal.Telemetry.Hardware.Storage;

public interface ISmart {
  /// <summary>
  /// Gets all available smart attributes.
  /// </summary>
  IReadOnlyList<SmartAttribute> Attributes { get; }
}
