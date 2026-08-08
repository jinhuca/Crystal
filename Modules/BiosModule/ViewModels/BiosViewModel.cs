using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using BiosModule.Models;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Smbios.HardwareFeatures.Firmware;
using Crystal.Provider.Smbios.Types;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;

namespace BiosModule.ViewModels;

public sealed class BiosViewModel : BindableBase, IBiosViewModel, IDisposable {
  private const string Dash = "—";
  private readonly IDisposable _firmwareSubscription;
  private readonly IDisposable _telemetrySubscription;
  private readonly IDisposable _boardReadingsSubscription;

  private string _manufacturer = Dash;
  private string _version = Dash;
  private string _releaseDate = Dash;
  private string _serialNumber = Dash;
  private string _smbiosSpecVersion = Dash;
  private string _status = Dash;
  private string _romSize = Dash;
  private string _firmwareInterface = Dash;
  private string _biosRevision = Dash;
  private string _embeddedControllerRevision = Dash;
  private string _flashUpgradeable = Dash;
  private string _selectableBoot = Dash;
  private string _bootFromCd = Dash;
  private string _systemManufacturer = Dash;
  private string _systemProduct = Dash;
  private string _baseboardManufacturer = Dash;
  private string _baseboardProduct = Dash;
  private string _chassisType = Dash;
  private string _secureBoot = Dash;
  private string _tpm = Dash;
  private string _tpmManufacturer = Dash;
  private string _administratorPassword = Dash;
  private string _powerOnPassword = Dash;
  private string _bootStatus = Dash;
  private bool _hasFirmwareInventory;
  private string _firmwareComponentCount = Dash;
  private string _boardTemperature = Dash;
  private string _cmosVoltage = Dash;
  private string _chassisFanRpm = Dash;
  private string _rail3V3 = Dash;
  private string _rail5V = Dash;
  private string _rail12V = Dash;
  private string _rail3V3Range = "";
  private string _rail5VRange = "";
  private string _rail12VRange = "";
  private ReadingSeverity _cmosSeverity;
  private ReadingSeverity _rail3V3Severity;
  private ReadingSeverity _rail5VSeverity;
  private ReadingSeverity _rail12VSeverity;
  private ReadingSeverity _boardHealth;
  private string _boardHealthDetail = "";
  private bool _hasBoardSensors;
  private PerformanceGraph? _rail3V3Graph;
  private PerformanceGraph? _rail5VGraph;
  private PerformanceGraph? _rail12VGraph;
  // Last severity themed onto each graph, so the trend line is only re-tinted when it actually
  // changes band rather than reallocating brushes on every 1-second tick.
  private ReadingSeverity? _rail3V3GraphSeverity;
  private ReadingSeverity? _rail5VGraphSeverity;
  private ReadingSeverity? _rail12VGraphSeverity;
  private PerformanceGraph? _fanGraph;
  private ReadingSeverity? _fanGraphSeverity;
  private ReadingSeverity _chassisFanSeverity;
  private bool _hasChassisFan;
  private PerformanceGraph? _boardTempGraph;
  private ReadingSeverity? _boardTempGraphSeverity;
  private ReadingSeverity _boardTemperatureSeverity;
  // Latest board temperature, cached from the telemetry tick so the board-readings tick can grade
  // fan stall against it (a stopped fan only matters once the board is warm). Both observables tick
  // together off the shared SensorMonitor, so this is at most one 1-second tick stale.
  private float? _boardTemperatureC;
  private readonly bool _driverInstalled;
  private readonly bool _driverAccessible;
  private string _boardSensorStatus = Dash;
  private bool _hasBoardSensorStatus;

  public BiosViewModel(IBiosModel model, IEventAggregator events) {
    _driverInstalled = model.BoardSensorDriverInstalled;
    _driverAccessible = model.BoardSensorDriverAccessible;
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Bios));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _firmwareSubscription = model.Firmware.Subscribe(s => OnUi(() => Apply(s)));
    _telemetrySubscription = model.BoardTelemetry.Subscribe(t => OnUi(() => ApplyTelemetry(t)));
    _boardReadingsSubscription = model.BoardReadings.Subscribe(r => OnUi(() => ApplyBoardReadings(r)));
  }

  public string Manufacturer { get => _manufacturer; private set => SetProperty(ref _manufacturer, value); }
  public string Version { get => _version; private set => SetProperty(ref _version, value); }
  public string ReleaseDate { get => _releaseDate; private set => SetProperty(ref _releaseDate, value); }
  public string SerialNumber { get => _serialNumber; private set => SetProperty(ref _serialNumber, value); }
  public string SmbiosSpecVersion { get => _smbiosSpecVersion; private set => SetProperty(ref _smbiosSpecVersion, value); }
  public string Status { get => _status; private set => SetProperty(ref _status, value); }

  public string RomSize { get => _romSize; private set => SetProperty(ref _romSize, value); }
  public string FirmwareInterface { get => _firmwareInterface; private set => SetProperty(ref _firmwareInterface, value); }
  public string BiosRevision { get => _biosRevision; private set => SetProperty(ref _biosRevision, value); }
  public string EmbeddedControllerRevision { get => _embeddedControllerRevision; private set => SetProperty(ref _embeddedControllerRevision, value); }

  public string FlashUpgradeable { get => _flashUpgradeable; private set => SetProperty(ref _flashUpgradeable, value); }
  public string SelectableBoot { get => _selectableBoot; private set => SetProperty(ref _selectableBoot, value); }
  public string BootFromCd { get => _bootFromCd; private set => SetProperty(ref _bootFromCd, value); }

  public string SystemManufacturer { get => _systemManufacturer; private set => SetProperty(ref _systemManufacturer, value); }
  public string SystemProduct { get => _systemProduct; private set => SetProperty(ref _systemProduct, value); }
  public string BaseboardManufacturer { get => _baseboardManufacturer; private set => SetProperty(ref _baseboardManufacturer, value); }
  public string BaseboardProduct { get => _baseboardProduct; private set => SetProperty(ref _baseboardProduct, value); }
  public string ChassisType { get => _chassisType; private set => SetProperty(ref _chassisType, value); }

  public string SecureBoot { get => _secureBoot; private set => SetProperty(ref _secureBoot, value); }
  public string Tpm { get => _tpm; private set => SetProperty(ref _tpm, value); }
  public string TpmManufacturer { get => _tpmManufacturer; private set => SetProperty(ref _tpmManufacturer, value); }
  public string AdministratorPassword { get => _administratorPassword; private set => SetProperty(ref _administratorPassword, value); }
  public string PowerOnPassword { get => _powerOnPassword; private set => SetProperty(ref _powerOnPassword, value); }
  public string BootStatus { get => _bootStatus; private set => SetProperty(ref _bootStatus, value); }

  public ObservableCollection<FirmwareComponentViewModel> FirmwareInventory { get; } = [];
  public bool HasFirmwareInventory { get => _hasFirmwareInventory; private set => SetProperty(ref _hasFirmwareInventory, value); }
  public string FirmwareComponentCount { get => _firmwareComponentCount; private set => SetProperty(ref _firmwareComponentCount, value); }

  public string BoardTemperature { get => _boardTemperature; private set => SetProperty(ref _boardTemperature, value); }
  public string CmosVoltage { get => _cmosVoltage; private set => SetProperty(ref _cmosVoltage, value); }
  public string ChassisFanRpm { get => _chassisFanRpm; private set => SetProperty(ref _chassisFanRpm, value); }
  public string Rail3V3 { get => _rail3V3; private set => SetProperty(ref _rail3V3, value); }
  public string Rail5V { get => _rail5V; private set => SetProperty(ref _rail5V, value); }
  public string Rail12V { get => _rail12V; private set => SetProperty(ref _rail12V, value); }
  public string Rail3V3Range { get => _rail3V3Range; private set => SetProperty(ref _rail3V3Range, value); }
  public string Rail5VRange { get => _rail5VRange; private set => SetProperty(ref _rail5VRange, value); }
  public string Rail12VRange { get => _rail12VRange; private set => SetProperty(ref _rail12VRange, value); }
  public ReadingSeverity CmosSeverity { get => _cmosSeverity; private set => SetProperty(ref _cmosSeverity, value); }
  public ReadingSeverity Rail3V3Severity { get => _rail3V3Severity; private set => SetProperty(ref _rail3V3Severity, value); }
  public ReadingSeverity Rail5VSeverity { get => _rail5VSeverity; private set => SetProperty(ref _rail5VSeverity, value); }
  public ReadingSeverity Rail12VSeverity { get => _rail12VSeverity; private set => SetProperty(ref _rail12VSeverity, value); }
  // Worst severity across the graded rails/CMOS — drives the tile's status dot so board health
  // reads at a glance without expanding the detail view.
  public ReadingSeverity BoardHealth { get => _boardHealth; private set => SetProperty(ref _boardHealth, value); }
  // Names the out-of-tolerance rails (e.g. "+12V critical · CMOS warning") so the dot's tooltip
  // says what's wrong; empty while everything is in spec.
  public string BoardHealthDetail { get => _boardHealthDetail; private set => SetProperty(ref _boardHealthDetail, value); }
  public ObservableCollection<BoardSensorRowViewModel> BoardSensors { get; } = [];
  public bool HasBoardSensors { get => _hasBoardSensors; private set => SetProperty(ref _hasBoardSensors, value); }
  public string BoardSensorStatus { get => _boardSensorStatus; private set => SetProperty(ref _boardSensorStatus, value); }
  public bool HasBoardSensorStatus { get => _hasBoardSensorStatus; private set => SetProperty(ref _hasBoardSensorStatus, value); }

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  // A view supplies its own graph instances (buffers must not be shared between the tile and the
  // detail window). Clear the themed-severity cache so the next tick re-tints these fresh graphs
  // regardless of what the previous ones were showing.
  public void AttachRailGraphs(PerformanceGraph rail3V3, PerformanceGraph rail5V, PerformanceGraph rail12V) {
    _rail3V3Graph = ResetMarkers(rail3V3);
    _rail5VGraph = ResetMarkers(rail5V);
    _rail12VGraph = ResetMarkers(rail12V);
    _rail3V3GraphSeverity = _rail5VGraphSeverity = _rail12VGraphSeverity = null;
  }

  // The chassis-fan trend. Kept separate from the rail graphs because it's sourced from the board
  // rows (which include a stalled fan's 0 RPM) rather than the headline telemetry, whose selector
  // drops zeros — so a stall shows as the line falling to the floor, not the plot going stale.
  public void AttachFanGraph(PerformanceGraph fan) {
    _fanGraph = ResetMarkers(fan);
    _fanGraphSeverity = null;
  }

  // The board-temperature trend, fed from the headline telemetry alongside the rails.
  public void AttachBoardTempGraph(PerformanceGraph boardTemp) {
    _boardTempGraph = ResetMarkers(boardTemp);
    _boardTempGraphSeverity = null;
  }

  // A freshly-attached graph starts with no session history, so clear any extreme markers so the
  // first samples set them rather than folding into a stale attach-time value.
  private static PerformanceGraph ResetMarkers(PerformanceGraph graph) {
    graph.LowMarker = double.NaN;
    graph.HighMarker = double.NaN;
    return graph;
  }

  // Plots a sample and folds it into the graph's session low/high markers, so a past dip or spike
  // leaves a dashed reference line even after the live value recovers.
  private static void Feed(PerformanceGraph? graph, double value) {
    if (graph is null) return;
    graph.AddValue(value);
    graph.LowMarker = double.IsNaN(graph.LowMarker) ? value : System.Math.Min(graph.LowMarker, value);
    graph.HighMarker = double.IsNaN(graph.HighMarker) ? value : System.Math.Max(graph.HighMarker, value);
  }

  // Severity of the headline board temperature, tinting its trend line to match the value color.
  public ReadingSeverity BoardTemperatureSeverity { get => _boardTemperatureSeverity; private set => SetProperty(ref _boardTemperatureSeverity, value); }

  // Whether a chassis/system fan was found among the board rows, so the detail view can hide the
  // fan trend when the board exposes no such sensor.
  public bool HasChassisFan { get => _hasChassisFan; private set => SetProperty(ref _hasChassisFan, value); }
  // Stall severity of the headlined chassis fan, tinting its trend line to match the row value.
  public ReadingSeverity ChassisFanSeverity { get => _chassisFanSeverity; private set => SetProperty(ref _chassisFanSeverity, value); }

  private void Apply(FirmwareSnapshot s) {
    Manufacturer = Text(s.Manufacturer);
    Version = Text(s.Version);
    ReleaseDate = Text(s.ReleaseDate);
    SerialNumber = Text(s.SerialNumber);
    SmbiosSpecVersion = Text(s.SmbiosSpecVersion);
    Status = Text(s.Status);

    RomSize = s.RomSizeBytes is { } bytes and > 0 ? FormatRomSize(bytes) : Dash;
    FirmwareInterface = s.IsUefi switch { true => "UEFI", false => "Legacy BIOS", null => Dash };
    BiosRevision = Text(s.BiosRevision);
    EmbeddedControllerRevision = Text(s.EmbeddedControllerRevision);

    FlashUpgradeable = YesNo(s.Capabilities?.FlashUpgradeable);
    SelectableBoot = YesNo(s.Capabilities?.SelectableBoot);
    BootFromCd = YesNo(s.Capabilities?.BootFromCd);

    SystemManufacturer = Text(s.System?.Manufacturer);
    SystemProduct = Text(s.System?.ProductName);
    BaseboardManufacturer = Text(s.Baseboard?.Manufacturer);
    BaseboardProduct = Text(s.Baseboard?.Product);
    ChassisType = FormatChassis(s.Chassis?.ChassisType);

    SecureBoot = FormatSecureBoot(s.SecureBoot);
    Tpm = FormatTpm(s.Tpm);
    TpmManufacturer = Text(s.Tpm.Manufacturer);
    AdministratorPassword = Text(FormatSecurity(s.HardwareSecurity?.AdministratorPassword));
    PowerOnPassword = Text(FormatSecurity(s.HardwareSecurity?.PowerOnPassword));
    BootStatus = FormatBoot(s.Boot);

    FirmwareInventory.Clear();
    foreach (var c in s.FirmwareInventory) {
      FirmwareInventory.Add(new FirmwareComponentViewModel(
          Text(c.ComponentName), Text(c.Version), Text(c.ReleaseDate), c.State.ToString()));
    }
    HasFirmwareInventory = FirmwareInventory.Count > 0;
    FirmwareComponentCount = FirmwareInventory.Count > 0
        ? $"{FirmwareInventory.Count} component{(FirmwareInventory.Count == 1 ? "" : "s")}"
        : Dash;
  }

  private void ApplyTelemetry(BoardTelemetry t) {
    _boardTemperatureC = t.BoardTemperature;
    BoardTemperature = Reading(t.BoardTemperature, "°C", "0.0");
    CmosVoltage = Reading(t.CmosVoltage, "V", "0.00");
    ChassisFanRpm = Reading(t.ChassisFanRpm, "RPM", "0");
    Rail3V3 = Reading(t.Rail3V3.Value, "V", "0.00");
    Rail5V = Reading(t.Rail5V.Value, "V", "0.00");
    Rail12V = Reading(t.Rail12V.Value, "V", "0.00");

    Rail3V3Range = RailRange(t.Rail3V3);
    Rail5VRange = RailRange(t.Rail5V);
    Rail12VRange = RailRange(t.Rail12V);

    // Trend each rail against its nominal so droop/ripple is visible; a missing reading skips the
    // tick rather than plotting a false zero that would swamp the ±10%-of-nominal window.
    if (t.Rail3V3.Value is { } v3) Feed(_rail3V3Graph, v3);
    if (t.Rail5V.Value is { } v5) Feed(_rail5VGraph, v5);
    if (t.Rail12V.Value is { } v12) Feed(_rail12VGraph, v12);
    if (t.BoardTemperature is { } bt) Feed(_boardTempGraph, bt);

    CmosSeverity = BoardReadingSeverity.Cmos(t.CmosVoltage);
    Rail3V3Severity = BoardReadingSeverity.Rail(t.Rail3V3.Value, 3.3f);
    Rail5VSeverity = BoardReadingSeverity.Rail(t.Rail5V.Value, 5f);
    Rail12VSeverity = BoardReadingSeverity.Rail(t.Rail12V.Value, 12f);
    BoardTemperatureSeverity = BoardReadingSeverity.Temperature(t.BoardTemperature);

    // BoardHealth/BoardHealthDetail are rolled up in ApplyBoardReadings, which sees every graded
    // board row (all rails + fan stall), not just the four headline readings shown here.

    // Tint each trend line to match its severity, so the graph agrees with the value color.
    _rail3V3GraphSeverity = ThemeGraph(_rail3V3Graph, Rail3V3Severity, _rail3V3GraphSeverity);
    _rail5VGraphSeverity = ThemeGraph(_rail5VGraph, Rail5VSeverity, _rail5VGraphSeverity);
    _rail12VGraphSeverity = ThemeGraph(_rail12VGraph, Rail12VSeverity, _rail12VGraphSeverity);
    _boardTempGraphSeverity = ThemeGraph(_boardTempGraph, BoardTemperatureSeverity, _boardTempGraphSeverity);
  }

  // Re-themes a rail graph only when its severity band changes, returning the newly-applied
  // severity so the caller can track it. No-op if the graph isn't attached or nothing changed.
  private static ReadingSeverity? ThemeGraph(PerformanceGraph? graph, ReadingSeverity severity, ReadingSeverity? applied) {
    if (graph is null || severity == applied) return applied;
    graph.ApplyTheme(SeverityTheme(severity));
    return severity;
  }

  private static GraphTheme SeverityTheme(ReadingSeverity severity) => severity switch {
    ReadingSeverity.Warning => GraphThemes.Amber(GraphKind.Line),
    ReadingSeverity.Critical => GraphThemes.FromAccent(CriticalAccent, GraphKind.Line),
    _ => GraphThemes.Sky(GraphKind.Line),
  };

  // Matches the #E85C5C critical value color used by ReadingSeverityToBrushConverter.
  private static readonly Color CriticalAccent = Color.FromRgb(0xE8, 0x5C, 0x5C);

  // "+12V critical · CMOS warning", worst first; empty when nothing is out of tolerance.
  private static string HealthDetail(IReadOnlyList<(string Name, ReadingSeverity Severity)> offenders) =>
      string.Join(" · ", offenders
          .OrderByDescending(o => o.Severity)
          .Select(o => $"{o.Name} {o.Severity.ToString().ToLowerInvariant()}"));

  // "11.90–12.10" once both bounds are known; empty until then so the sub-line stays hidden.
  private static string RailRange(RailReading rail) =>
      rail is { Min: { } min, Max: { } max } ? $"{min:0.00}–{max:0.00}" : "";

  private void ApplyBoardReadings(IReadOnlyList<SensorReading> readings) {
    BoardSensors.Clear();
    var offenders = new List<(string Name, ReadingSeverity Severity)>();
    foreach (var r in readings.OrderBy(r => r.SensorType).ThenBy(r => r.SensorName)) {
      string unit = r.Unit ?? "";
      var severity = RowSeverity(r);
      if (severity != ReadingSeverity.Normal) offenders.Add((Text(r.SensorName), severity));
      BoardSensors.Add(new BoardSensorRowViewModel(
          Text(r.SensorName),
          FormatValue(r.Value, unit),
          FormatValue(r.Min, unit),
          FormatValue(r.Max, unit),
          severity));
    }
    HasBoardSensors = BoardSensors.Count > 0;

    // Whole-board rollup over every graded row (all rails + fan stall), not just the four headline
    // readings — so a broadened rail or a stalled fan lifts the tile dot and is named in the tooltip.
    BoardHealth = offenders.Count > 0 ? offenders.Max(o => o.Severity) : ReadingSeverity.Normal;
    BoardHealthDetail = HealthDetail(offenders);

    // Trend the chassis fan from the board rows so a stall plots as the line dropping to the floor
    // (the headline selector drops zeros, which would freeze the plot instead). A null reading feeds
    // 0 for the same reason. Tint the line to match the fan's stall severity.
    var fan = BoardTelemetrySelector.ChassisFanRow(readings);
    HasChassisFan = fan is not null;
    ChassisFanSeverity = fan is null ? ReadingSeverity.Normal
        : BoardReadingSeverity.Fan(fan.Value, fan.Max, _boardTemperatureC);
    if (HasChassisFan) Feed(_fanGraph, fan!.Value ?? 0d);
    _fanGraphSeverity = ThemeGraph(_fanGraph, ChassisFanSeverity, _fanGraphSeverity);

    // With no readings, explain why. The registry-installed flag alone is misleading (it can be
    // true while the device won't open), so the accessible probe is authoritative: driver not
    // installed, installed-but-not-accessible (not running / not elevated), or accessible but the
    // board simply exposes no Super I/O sensors (common on laptops). Hidden when sensors arrive.
    if (HasBoardSensors) {
      HasBoardSensorStatus = false;
    }
    else {
      BoardSensorStatus =
          !_driverInstalled ? "Board sensors unavailable — PawnIO driver not installed."
          : !_driverAccessible ? "Board sensors unavailable — run elevated with the PawnIO driver running."
          : "No board sensors detected on this system.";
      HasBoardSensorStatus = true;
    }
  }

  // Voltage rows we can map to a known fixed rail are graded against it; the CMOS cell uses its own
  // absolute-volts rule. Board temperatures grade on their own thresholds; fans are graded for
  // stall against the current board temperature. Variable-voltage rows (VCore, DRAM…) stay Normal —
  // we have no spec to judge them against.
  private ReadingSeverity RowSeverity(SensorReading r) {
    switch (r.SensorType) {
      case SensorType.Voltage:
        if (BoardTelemetrySelector.IsCmosRail(r.SensorName)) return BoardReadingSeverity.Cmos(r.Value);
        return BoardTelemetrySelector.RailNominal(r.SensorName) is { } nominal
            ? BoardReadingSeverity.Rail(r.Value, nominal)
            : ReadingSeverity.Normal;
      case SensorType.Temperature:
        return BoardReadingSeverity.Temperature(r.Value);
      case SensorType.Fan:
        return BoardReadingSeverity.Fan(r.Value, r.Max, _boardTemperatureC);
      default:
        return ReadingSeverity.Normal;
    }
  }

  private static string Reading(float? value, string unit, string format) =>
      value is { } v ? $"{v.ToString(format)} {unit}".Trim() : Dash;

  private static string FormatValue(float? value, string unit) =>
      value is { } v ? $"{v:0.###} {unit}".Trim() : Dash;

  private static string FormatRomSize(long bytes) {
    double mb = bytes / (1024d * 1024d);
    return mb >= 1 ? $"{mb:0.#} MB" : $"{bytes / 1024d:0.#} KB";
  }

  private static string FormatSecureBoot(SecureBootInfo s) => s switch {
    { Supported: false } => "Not supported",
    { Enabled: true } => "Enabled",
    { Enabled: false } => "Disabled",
    _ => Dash,
  };

  private static string FormatTpm(TpmInfo tpm) {
    if (!tpm.Present) return "Not present";
    string spec = string.IsNullOrWhiteSpace(tpm.SpecVersion) ? "" : $"v{tpm.SpecVersion} ";
    string state = tpm.Enabled switch { true => "Enabled", false => "Disabled", null => "Present" };
    return $"{spec}{state}".Trim();
  }

  private static string FormatChassis(PhysicalChassisType? type) =>
      type is null or PhysicalChassisType.Unknown ? Dash : SpaceCamelCase(type.Value.ToString());

  private static string? FormatSecurity(HardwareSecurityStatus? status) => status switch {
    HardwareSecurityStatus.Enabled => "Enabled",
    HardwareSecurityStatus.Disabled => "Disabled",
    HardwareSecurityStatus.NotImplemented => "Not implemented",
    _ => null,
  };

  private static string FormatBoot(SmbiosBootInfo? boot) {
    if (boot is null) return Dash;
    return boot.Status is { } status ? SpaceCamelCase(status.ToString()) : $"0x{boot.StatusRaw:X2}";
  }

  private static string YesNo(bool? value) => value switch { true => "Yes", false => "No", null => Dash };

  // "NoBootableMedia" -> "No Bootable Media"; enum names are the only inputs here.
  private static string SpaceCamelCase(string value) {
    var sb = new System.Text.StringBuilder(value.Length + 4);
    for (int i = 0; i < value.Length; i++) {
      char c = value[i];
      if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1])) sb.Append(' ');
      sb.Append(c);
    }
    return sb.ToString();
  }

  private static string Text(string? value) => string.IsNullOrWhiteSpace(value) ? Dash : value!;

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() {
    _firmwareSubscription.Dispose();
    _telemetrySubscription.Dispose();
    _boardReadingsSubscription.Dispose();
  }
}
