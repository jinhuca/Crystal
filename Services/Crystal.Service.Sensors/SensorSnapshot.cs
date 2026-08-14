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
  private static readonly IReadOnlyList<SensorReading> Empty = new List<SensorReading>();

  public SensorSnapshot(IEnumerable<SensorReading> readings) {
    var all = readings?.ToList() ?? new List<SensorReading>();
    Readings = all;
    ByCategory = new ReadOnlyDictionary<SensorCategory, IReadOnlyList<SensorReading>>(
        all.GroupBy(r => r.HardwareType.ToCategory())
           .ToDictionary(g => g.Key, g => (IReadOnlyList<SensorReading>)g.ToList()));
  }

  /// <summary>All readings collected in this snapshot, ungrouped.</summary>
  public IReadOnlyList<SensorReading> Readings { get; }

  /// <summary>Readings grouped by their <see cref="SensorCategory"/>.</summary>
  public IReadOnlyDictionary<SensorCategory, IReadOnlyList<SensorReading>> ByCategory { get; }

  /// <summary>Readings for one category, or an empty list when none were collected.</summary>
  public IReadOnlyList<SensorReading> this[SensorCategory category] =>
      ByCategory.TryGetValue(category, out var readings) ? readings : Empty;

  public IReadOnlyList<SensorReading> Cpu => this[SensorCategory.Cpu];
  public IReadOnlyList<SensorReading> Gpu => this[SensorCategory.Gpu];
  public IReadOnlyList<SensorReading> Memory => this[SensorCategory.Memory];
  public IReadOnlyList<SensorReading> Motherboard => this[SensorCategory.Motherboard];
  public IReadOnlyList<SensorReading> Storage => this[SensorCategory.Storage];
  public IReadOnlyList<SensorReading> Network => this[SensorCategory.Network];
}
