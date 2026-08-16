using System.Collections.Generic;

namespace Crystal.Shell.Settings;

/// <summary>
/// A single configurable dashboard graph: a stable <see cref="Id"/> (the persistence key, matched
/// by the dashboard wiring), the owning <see cref="Component"/> (grouping header in the popup) and
/// the <see cref="Metric"/> it plots (row label).
/// </summary>
public sealed record GraphDescriptor(string Id, string Component, string Metric);

/// <summary>
/// The registry of dashboard graphs the settings popup can configure. Seeded with the two CPU
/// graphs from the draft design; other components are added by appending descriptors here (the
/// popup and the persisted settings are both driven off this list, so a new entry needs no other
/// UI change).
/// </summary>
public static class GraphCatalog {
  public static IReadOnlyList<GraphDescriptor> Graphs { get; } = new[] {
    new GraphDescriptor("Cpu.Utilization", "CPU", "Utilization"),
    new GraphDescriptor("Cpu.Clock", "CPU", "Clock"),
    new GraphDescriptor("Cpu.Temperature", "CPU", "Temperature"),
    new GraphDescriptor("Cpu.Voltage", "CPU", "Voltage"),
    new GraphDescriptor("Cpu.Power", "CPU", "Power"),
    new GraphDescriptor("Cpu.Fan", "CPU", "CPU Fan"),
  };
}
