using System.Collections.Generic;

namespace Crystal.Shell.Settings;

/// <summary>
/// A single configurable dashboard graph: a stable <see cref="Id"/> (the persistence key, matched
/// by the dashboard wiring), the owning <see cref="Component"/> (grouping header in the popup) and
/// the <see cref="Metric"/> it plots (row label). <see cref="DefaultKind"/>/<see cref="DefaultAccent"/>
/// are the appearance a graph falls back to when the user has never configured it, so bringing a
/// graph under the settings system preserves its original look on first launch.
/// </summary>
public sealed record GraphDescriptor(
    string Id,
    string Component,
    string Metric,
    GraphKindChoice DefaultKind = GraphKindChoice.SegmentedBar,
    GraphAccent DefaultAccent = GraphAccent.Grey);

/// <summary>
/// The registry of dashboard graphs the settings popup can configure. The popup and the persisted
/// settings are both driven off this list, so a new dashboard graph is surfaced simply by appending
/// a descriptor here and tagging that graph with <c>graphs:GraphIdentity.Id</c> in its view.
/// </summary>
public static class GraphCatalog {
  public static IReadOnlyList<GraphDescriptor> Graphs { get; } = new[] {
    new GraphDescriptor("Cpu.Utilization", "CPU", "Utilization"),
    new GraphDescriptor("Cpu.Clock", "CPU", "Clock"),
    new GraphDescriptor("Cpu.Temperature", "CPU", "Temperature"),
    new GraphDescriptor("Cpu.Voltage", "CPU", "Voltage"),
    new GraphDescriptor("Cpu.Power", "CPU", "Power"),
    new GraphDescriptor("Cpu.Fan", "CPU", "CPU Fan"),

    new GraphDescriptor("Gpu.Utilization", "GPU", "Utilization", GraphKindChoice.FilledLine, GraphAccent.Rose),
    new GraphDescriptor("Gpu.Temperature", "GPU", "Temperature", GraphKindChoice.FilledLine, GraphAccent.Sky),
    new GraphDescriptor("Gpu.Clock", "GPU", "Clock", GraphKindChoice.FilledLine, GraphAccent.Amber),
    new GraphDescriptor("Gpu.Power", "GPU", "Power", GraphKindChoice.FilledLine, GraphAccent.Emerald),

    new GraphDescriptor("Memory.Utilization", "Memory", "Utilization", GraphKindChoice.FilledLine, GraphAccent.Rose),
    new GraphDescriptor("Memory.Used", "Memory", "Used", GraphKindChoice.FilledLine, GraphAccent.Sky),

    new GraphDescriptor("Storage.Activity", "Storage", "Active time", GraphKindChoice.FilledLine, GraphAccent.Amber),
    new GraphDescriptor("Storage.Transfer", "Storage", "Transfer", GraphKindChoice.FilledLine, GraphAccent.Sky),

    new GraphDescriptor("Network.Download", "Network", "Download", GraphKindChoice.FilledLine, GraphAccent.Emerald),
    new GraphDescriptor("Network.Upload", "Network", "Upload", GraphKindChoice.FilledLine, GraphAccent.Amber),

    new GraphDescriptor("Bios.BoardTemp", "BIOS", "Board temp", GraphKindChoice.FilledLine, GraphAccent.Sky),
    new GraphDescriptor("Bios.ChassisFan", "BIOS", "Chassis fan", GraphKindChoice.FilledLine, GraphAccent.Sky),
    new GraphDescriptor("Bios.CmosBattery", "BIOS", "CMOS battery", GraphKindChoice.FilledLine, GraphAccent.Sky),
    new GraphDescriptor("Bios.Rail3V3", "BIOS", "+3.3V", GraphKindChoice.FilledLine, GraphAccent.Sky),
    new GraphDescriptor("Bios.Rail5V", "BIOS", "+5V", GraphKindChoice.FilledLine, GraphAccent.Sky),
    new GraphDescriptor("Bios.Rail12V", "BIOS", "+12V", GraphKindChoice.FilledLine, GraphAccent.Sky),
  };

  /// <summary>
  /// The fallback appearance for a graph the user has never configured: the descriptor's default
  /// kind/accent, or a plain grey segmented bar for an unknown id.
  /// </summary>
  public static GraphSetting DefaultFor(string id) {
    foreach (var g in Graphs)
      if (g.Id == id) return new GraphSetting { Kind = g.DefaultKind, Accent = g.DefaultAccent };
    return new GraphSetting();
  }
}
