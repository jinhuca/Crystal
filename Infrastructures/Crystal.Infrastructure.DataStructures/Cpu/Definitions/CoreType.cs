namespace Crystal.Infrastructure.DataStructures.Cpu.Definitions;

/// <summary>
/// Application-neutral processor-core class. Mirrors the telemetry provider's own core-type
/// enumeration so the provider (a LibreHardwareMonitor fork) stays a standalone package with no
/// dependency on this layer; the telemetry-aware service maps provider values onto these at the
/// boundary. Member names are kept identical to the provider's so the mapping is 1:1.
/// </summary>
public enum CoreType {
  /// <summary>The core type is unknown.</summary>
  Unknown = 0,

  /// <summary>A performance ("P") core.</summary>
  Performance = 0x40,

  /// <summary>An efficient ("E") core.</summary>
  Efficient = 0x20,
}
