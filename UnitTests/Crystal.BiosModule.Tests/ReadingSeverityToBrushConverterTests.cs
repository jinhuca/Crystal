using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Crystal.BiosModule.ViewModels;
using Crystal.BiosModule.Views;
using Xunit;

namespace Crystal.BiosModule.Tests;

public class ReadingSeverityToBrushConverterTests {
  private static readonly ReadingSeverityToBrushConverter Converter = new();

  private static Color Convert(ReadingSeverity severity) =>
      ((SolidColorBrush)Converter.Convert(severity, typeof(Brush), null!, CultureInfo.InvariantCulture)).Color;

  [Fact]
  public void Normal_maps_to_the_neutral_foreground() =>
      Assert.Equal(Color.FromRgb(0xE6, 0xE6, 0xE6), Convert(ReadingSeverity.Normal));

  [Fact]
  public void Warning_maps_to_amber() =>
      Assert.Equal(Color.FromRgb(0xE8, 0xB3, 0x3E), Convert(ReadingSeverity.Warning));

  [Fact]
  public void Critical_maps_to_red() =>
      Assert.Equal(Color.FromRgb(0xE8, 0x5C, 0x5C), Convert(ReadingSeverity.Critical));

  [Fact]
  public void Returned_brush_is_frozen_so_it_can_be_shared_across_the_ui_thread() =>
      Assert.True(((SolidColorBrush)Converter.Convert(
          ReadingSeverity.Warning, typeof(Brush), null!, CultureInfo.InvariantCulture)).IsFrozen);

  [Fact]
  public void Unknown_value_falls_back_to_neutral() =>
      Assert.Equal(Color.FromRgb(0xE6, 0xE6, 0xE6),
          ((SolidColorBrush)Converter.Convert("nonsense", typeof(Brush), null!, CultureInfo.InvariantCulture)).Color);

  [Fact]
  public void ConvertBack_is_a_no_op() =>
      Assert.Same(Binding.DoNothing,
          Converter.ConvertBack(Brushes.Red, typeof(ReadingSeverity), null!, CultureInfo.InvariantCulture));
}
