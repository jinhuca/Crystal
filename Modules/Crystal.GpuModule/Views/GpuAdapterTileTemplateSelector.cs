using Crystal.GpuModule.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.GpuModule.Views;

/// <summary>
/// Chooses the meter layout for one GPU tile: the compact 2x2 block for integrated adapters,
/// the fuller 3x2 + PCIe block for dedicated cards.
/// </summary>
public sealed class GpuAdapterTileTemplateSelector : DataTemplateSelector {
  /// <summary>Tile layout used for an integrated adapter.</summary>
  public DataTemplate? IntegratedTemplate { get; set; }

  /// <summary>Tile layout used for a dedicated adapter.</summary>
  public DataTemplate? DedicatedTemplate { get; set; }

  /// <inheritdoc/>
  public override DataTemplate? SelectTemplate(object item, DependencyObject container) =>
      item is GpuAdapterViewModel { IsIntegrated: true } ? IntegratedTemplate : DedicatedTemplate;
}
