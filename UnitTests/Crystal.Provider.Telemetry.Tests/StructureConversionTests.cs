using Crystal.Provider.Telemetry.Interop.PowerMonitor;
using Xunit;
using WVP2C = Crystal.Provider.Telemetry.Interop.PowerMonitor.WireViewPro2Constants;

namespace Crystal.Provider.Telemetry.Tests;

/// <summary>
/// Covers the pure version-to-version conversions of the WireView Pro 2 device config structs:
/// scalar field pass-through, the version-specific defaults each conversion injects, and the
/// theme&lt;-&gt;color / theme&lt;-&gt;bitmap mapping (V2→V3 expands a theme into explicit colors; V3→V2
/// infers the theme back from the background bitmap id).
/// </summary>
public class StructureConversionTests {
  private static DeviceConfigStructV1 SampleV1(Theme theme = Theme.ThemeTg1) => new() {
    Crc = 0x1234,
    Version = 1,
    FriendlyName = new byte[32],
    FanConfig = new FanConfigStruct { Mode = FanMode.FanModeFixed, DutyMin = 10, DutyMax = 90 },
    BacklightDuty = 55,
    FaultDisplayEnable = 1,
    FaultBuzzerEnable = 0,
    FaultSoftPowerEnable = 1,
    FaultHardPowerEnable = 0,
    TsFaultThreshold = 900,
    OcpFaultThreshold = 12,
    WireOcpFaultThreshold = 34,
    OppFaultThreshold = 600,
    CurrentImbalanceFaultThreshold = 7,
    CurrentImbalanceFaultMinLoad = 3,
    ShutdownWaitTime = 5,
    LoggingInterval = 2,
    Ui = new UiConfigStructV1 {
      CurrentScale = CurrentScale.CurrentScale15A,
      PowerScale = PowerScale.PowerScale600W,
      Theme = theme,
      DisplayRotation = DisplayRotation.DisplayRotation180,
      TimeoutMode = TimeoutMode.TimeoutModeSleep,
      CycleScreens = 6,
      CycleTime = 8,
      Timeout = 30
    }
  };

  [Fact]
  public void V1ToV2_CopiesScalarFields() {
    var v1 = SampleV1();

    var v2 = StructureConversion.ConvertConfigV1ToV2(v1);

    Assert.Equal(v1.Crc, v2.Crc);
    Assert.Equal(v1.Version, v2.Version);
    Assert.Same(v1.FriendlyName, v2.FriendlyName);
    Assert.Equal(v1.BacklightDuty, v2.BacklightDuty);
    Assert.Equal(v1.TsFaultThreshold, v2.TsFaultThreshold);
    Assert.Equal(v1.OppFaultThreshold, v2.OppFaultThreshold);
    Assert.Equal(v1.LoggingInterval, v2.LoggingInterval);
    Assert.Equal(v1.Ui.Theme, v2.Ui.Theme);
  }

  [Fact]
  public void V1ToV2_InjectsDefaultAveragingWindow() {
    var v2 = StructureConversion.ConvertConfigV1ToV2(SampleV1());

    // V1 has no averaging field; the conversion seeds the documented default.
    Assert.Equal(AVG.AVG_1417MS, v2.Average);
  }

  [Fact]
  public void V2ToV1_DropsAveraging_ButPreservesUi() {
    var v2 = StructureConversion.ConvertConfigV1ToV2(SampleV1(Theme.ThemeTg2));
    v2.Average = AVG.AVG_89MS;

    var v1 = StructureConversion.ConvertConfigV2ToV1(v2);

    Assert.Equal(v2.Crc, v1.Crc);
    Assert.Equal(Theme.ThemeTg2, v1.Ui.Theme);
    Assert.Equal(v2.Ui.CycleTime, v1.Ui.CycleTime);
  }

  [Fact]
  public void V1_V2_RoundTrip_PreservesAllV1Fields() {
    var v1 = SampleV1(Theme.ThemeTg3);

    var round = StructureConversion.ConvertConfigV2ToV1(StructureConversion.ConvertConfigV1ToV2(v1));

    Assert.Equal(v1, round); // struct value equality over every field
  }

  [Theory]
  [InlineData(Theme.ThemeTg1, WVP2C.THEME_PRIMARY_COLOR_TG1, WVP2C.THEME_SECONDARY_COLOR_TG1,
      WVP2C.THEME_HIGHLIGHT_COLOR_TG1, WVP2C.THEME_BACKGROUND_COLOR_TG1,
      (byte)THEME_BACKGROUND.ThermalGrizzlyOrange, (byte)THEME_FAN.ThermalGrizzlyOrange)]
  [InlineData(Theme.ThemeTg2, WVP2C.THEME_PRIMARY_COLOR_TG2, WVP2C.THEME_SECONDARY_COLOR_TG2,
      WVP2C.THEME_HIGHLIGHT_COLOR_TG2, WVP2C.THEME_BACKGROUND_COLOR_TG2,
      (byte)THEME_BACKGROUND.ThermalGrizzlyDark, (byte)THEME_FAN.ThermalGrizzlyDark)]
  [InlineData(Theme.ThemeTg3, WVP2C.THEME_PRIMARY_COLOR_TG3, WVP2C.THEME_SECONDARY_COLOR_TG3,
      WVP2C.THEME_HIGHLIGHT_COLOR_TG3, WVP2C.THEME_BACKGROUND_COLOR_TG3,
      (byte)THEME_BACKGROUND.Disabled, (byte)THEME_FAN.ThermalGrizzlyBlackWhite)]
  public void V2ToV3_ExpandsThemeIntoColorsAndBitmaps(
      Theme theme, uint primary, uint secondary, uint highlight, uint background,
      byte backgroundBitmap, byte fanBitmap) {
    var v2 = StructureConversion.ConvertConfigV1ToV2(SampleV1(theme));

    var v3 = StructureConversion.ConvertConfigV2ToV3(v2);

    Assert.Equal(primary, v3.Ui.PrimaryColor);
    Assert.Equal(secondary, v3.Ui.SecondaryColor);
    Assert.Equal(highlight, v3.Ui.HighlightColor);
    Assert.Equal(background, v3.Ui.BackgroundColor);
    Assert.Equal(backgroundBitmap, v3.Ui.BackgroundBitmapId);
    Assert.Equal(fanBitmap, v3.Ui.FanBitmapId);
  }

  [Fact]
  public void V2ToV3_SeedsDefaultScreenAndInversion() {
    var v3 = StructureConversion.ConvertConfigV2ToV3(StructureConversion.ConvertConfigV1ToV2(SampleV1()));

    Assert.Equal(Screen.ScreenMain, v3.Ui.DefaultScreen);
    Assert.Equal(DISPLAY_INVERSION.DISPLAY_INVERSION_OFF, v3.Ui.DisplayInversion);
    Assert.Equal(AVG.AVG_1417MS, v3.Average); // carried through from V2
  }

  [Theory]
  [InlineData((byte)THEME_BACKGROUND.ThermalGrizzlyOrange, Theme.ThemeTg1)]
  [InlineData((byte)THEME_BACKGROUND.ThermalGrizzlyDark, Theme.ThemeTg2)]
  [InlineData((byte)THEME_BACKGROUND.Disabled, Theme.ThemeTg3)]
  [InlineData((byte)0, Theme.ThemeTg3)] // any unrecognized bitmap falls back to Tg3
  public void V3ToV2_InfersThemeFromBackgroundBitmap(byte backgroundBitmap, Theme expected) {
    var v3 = StructureConversion.ConvertConfigV2ToV3(StructureConversion.ConvertConfigV1ToV2(SampleV1()));
    v3.Ui.BackgroundBitmapId = backgroundBitmap;

    var v2 = StructureConversion.ConvertConfigV3ToV2(v3);

    Assert.Equal(expected, v2.Ui.Theme);
  }

  [Theory]
  [InlineData(Theme.ThemeTg1)]
  [InlineData(Theme.ThemeTg2)]
  public void V2_V3_ThemeRoundTrips_ForBitmapBackedThemes(Theme theme) {
    // Tg1/Tg2 map to distinct background bitmaps, so the theme survives a V2→V3→V2 round-trip.
    var v2 = StructureConversion.ConvertConfigV1ToV2(SampleV1(theme));

    var round = StructureConversion.ConvertConfigV3ToV2(StructureConversion.ConvertConfigV2ToV3(v2));

    Assert.Equal(theme, round.Ui.Theme);
  }

  [Fact]
  public void V1ToV3_ComposesThroughV2() {
    var v1 = SampleV1(Theme.ThemeTg2);

    var direct = StructureConversion.ConvertConfigV1ToV3(v1);
    var stepwise = StructureConversion.ConvertConfigV2ToV3(StructureConversion.ConvertConfigV1ToV2(v1));

    Assert.Equal(stepwise, direct);
  }

  [Fact]
  public void V3ToV1_ComposesThroughV2() {
    var v3 = StructureConversion.ConvertConfigV1ToV3(SampleV1(Theme.ThemeTg1));

    var direct = StructureConversion.ConvertConfigV3ToV1(v3);
    var stepwise = StructureConversion.ConvertConfigV2ToV1(StructureConversion.ConvertConfigV3ToV2(v3));

    Assert.Equal(stepwise, direct);
  }
}
