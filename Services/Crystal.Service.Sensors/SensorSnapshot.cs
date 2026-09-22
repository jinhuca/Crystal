using Crystal.Infrastructure.DataStructures.Sensors;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Crystal.Service.Sensors;

/// <summary>
/// An immutable point-in-time view of every system <see cref="SensorReading"/>,
/// grouped by <see cref="SensorCategory"/> (CPU, GPU, ...).
/// </summary>
public sealed class SensorSnapshot {
  /// <summary>
  /// Shared empty list returned by the indexer for a category with no readings, so callers
  /// never see null and no per-lookup allocation occurs.
  /// </summary>
  private static readonly IReadOnlyList<SensorReading> Empty = new List<SensorReading>();

  /// <summary>
  /// Captures <paramref name="readings"/> and groups them by <see cref="SensorCategory"/> up front,
  /// so category lookups are O(1) and the snapshot is fully immutable once constructed.
  /// </summary>
  /// <param name="readings">The flat set of readings for this poll; a null enumerable is treated as empty.</param>
  public SensorSnapshot(IEnumerable<SensorReading> readings) {
    var all = readings?.ToList() ?? new List<SensorReading>();
    Readings = all;
    ByCategory = new ReadOnlyDictionary<SensorCategory, IReadOnlyList<SensorReading>>(
        all.GroupBy(r => r.HardwareType.ToCategory())
           .ToDictionary(g => g.Key, g => (IReadOnlyList<SensorReading>)g.ToList()));
  }

  /// <summary>
  /// All readings collected in this snapshot, ungrouped.
  /// </summary>
  public IReadOnlyList<SensorReading> Readings { get; }

  /// <summary>
  /// Readings grouped by their <see cref="SensorCategory"/>.
  /// </summary>
  public IReadOnlyDictionary<SensorCategory, IReadOnlyList<SensorReading>> ByCategory { get; }

  /// <summary>
  /// Readings for one category, or an empty list when none were collected.
  /// </summary>
  public IReadOnlyList<SensorReading> this[SensorCategory category] =>
      ByCategory.TryGetValue(category, out var readings) ? readings : Empty;

  /// <summary>
  /// Convenience accessor for the CPU category's readings.
  /// </summary>
  public IReadOnlyList<SensorReading> Cpu => this[SensorCategory.Cpu];
  /// <summary>
  /// Convenience accessor for the GPU category's readings.
  /// </summary>
  public IReadOnlyList<SensorReading> Gpu => this[SensorCategory.Gpu];
  /// <summary>
  /// Convenience accessor for the memory category's readings.
  /// </summary>
  public IReadOnlyList<SensorReading> Memory => this[SensorCategory.Memory];
  /// <summary>
  /// Convenience accessor for the motherboard category's readings.
  /// </summary>
  public IReadOnlyList<SensorReading> Motherboard => this[SensorCategory.Motherboard];
  /// <summary>
  /// Convenience accessor for the storage category's readings.
  /// </summary>
  public IReadOnlyList<SensorReading> Storage => this[SensorCategory.Storage];
  /// <summary>
  /// Convenience accessor for the network category's readings.
  /// </summary>
  public IReadOnlyList<SensorReading> Network => this[SensorCategory.Network];
}
