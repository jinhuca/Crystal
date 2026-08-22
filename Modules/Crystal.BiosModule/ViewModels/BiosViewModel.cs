using Crystal.BiosModule.Models;
using Crystal.Controls.PerformanceGraphs;
using Crystal.Controls.PerformanceGraphs.Kinds;
using Crystal.Controls.PerformanceGraphs.Themes;
using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.Infrastructure.DataStructures.Sensors;
using Crystal.Service.Bios;
using Crystal.Service.Sensors;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace Crystal.BiosModule.ViewModels;

/// <summary>
/// Root view model bound to the BIOS summary tile and detail view. Composes the static
/// </summary>
public sealed class BiosViewModel : BindableBase, IBiosViewModel, IDisposable {
  /// <summary>
  /// Represents the dash character used for default values.
  /// </summary>
  private const string Dash = "—";

  /// <summary>
  /// The subscriptions to the model's three streams: firmware, telemetry, and board readings. 
  /// Disposed when the view model is disposed.
  /// </summary>
  private readonly IDisposable _firmwareSubscription;

  /// <summary>
  /// The subscriptions to the model's three streams: firmware, telemetry, and board readings.
  /// </summary>
  private readonly IDisposable _telemetrySubscription;

  /// <summary>
  /// The subscriptions to the model's three streams: firmware, telemetry, and board readings.
  /// </summary>
  private readonly IDisposable _boardReadingsSubscription;

  /// <summary>
  /// The UI thread marshaller for marshaling actions onto the UI thread.
  /// </summary>
  private readonly UiThreadMarshaller _ui = new();

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _manufacturer = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _version = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _releaseDate = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _serialNumber = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _smbiosSpecVersion = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _status = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _romSize = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _firmwareInterface = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _biosRevision = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _embeddedControllerRevision = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _flashUpgradeable = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _selectableBoot = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _bootFromCd = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _systemManufacturer = Dash;

  /// <summary>
  /// 
  /// </summary>
  private string _systemProduct = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _baseboardManufacturer = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _baseboardProduct = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _chassisType = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _secureBoot = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _tpm = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _tpmManufacturer = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _administratorPassword = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _powerOnPassword = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _bootStatus = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private bool _hasFirmwareInventory;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _firmwareComponentCount = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _boardTemperature = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _cmosVoltage = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _chassisFanRpm = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _rail3V3 = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _rail5V = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _rail12V = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _rail3V3Range = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _rail5VRange = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _rail12VRange = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _cmosSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _rail3V3Severity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _rail5VSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _rail12VSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _boardHealth;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _boardHealthDetail = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _boardHealthSummary = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private bool _hasBoardSensors;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// Defaults to true so a desktop shows rails from the first paint; the firmware snapshot flips it
  /// off for laptop-class chassis, which have no ATX +3.3/+5/+12V rails or CMOS coin cell to plot.
  /// </summary>
  private bool _showVoltageRails = true;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private PerformanceGraph? _rail3V3Graph;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private PerformanceGraph? _rail5VGraph;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private PerformanceGraph? _rail12VGraph;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  // Last severity themed onto each graph, so the trend line is only re-tinted when it actually
  // changes band rather than reallocating brushes on every 1-second tick.
  private ReadingSeverity? _rail3V3GraphSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity? _rail5VGraphSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity? _rail12VGraphSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private PerformanceGraph? _fanGraph;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity? _fanGraphSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _chassisFanSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private bool _hasChassisFan;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private PerformanceGraph? _boardTempGraph;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity? _boardTempGraphSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private PerformanceGraph? _cmosGraph;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity? _cmosGraphSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _boardTemperatureSeverity;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// Latest board temperature, cached from the telemetry tick so the board-readings tick can grade
  /// fan stall against it (a stopped fan only matters once the board is warm). Both observables tick
  /// together off the shared SensorMonitor, so this is at most one 1-second tick stale.
  /// </summary>
  private float? _boardTemperatureC;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private readonly bool _driverInstalled;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private readonly bool _driverAccessible;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _boardSensorStatus = Dash;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private bool _hasBoardSensorStatus;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// Session record of out-of-spec episodes, so a rail that dipped critical then recovered still
  /// leaves a durable trail rather than vanishing from the UI the moment it comes back in spec.
  /// </summary>
  private readonly BoardHealthLog _healthLog;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private readonly Func<DateTimeOffset> _clock;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private bool _hasHealthEvents;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private bool _showCriticalOnly;

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _healthEventsFilterHint = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _healthEventsCapNote = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _healthEventsSummary = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private string _sessionPeak = "";

  /// <summary>
  /// The private backing fields for the public properties, initialized to default values.
  /// </summary>
  private ReadingSeverity _sessionPeakSeverity;

  /// <summary>
  /// Initializes a new instance of the <see cref="BiosViewModel"/> class.
  /// </summary>
  /// <param name="model">The BIOS model.</param>
  /// <param name="events">The event aggregator.</param>
  public BiosViewModel(IBiosModel model, IEventAggregator events)
    : this(model, events, () => DateTimeOffset.Now) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="BiosViewModel"/> class with a custom clock function.
  /// Clock is injected so the health log's timestamps and durations are testable off a fake clock.
  /// </summary>
  /// <param name="model">The BIOS model.</param>
  /// <param name="events">The event aggregator.</param>
  /// <param name="clock">The clock function.</param>
  internal BiosViewModel(IBiosModel model,
                         IEventAggregator events,
                         Func<DateTimeOffset> clock) {
    _clock = clock;
    _healthLog = new BoardHealthLog(clock);
    _driverInstalled = model.BoardSensorDriverInstalled;
    _driverAccessible = model.BoardSensorDriverAccessible;
    ShowDetailCommand = new DelegateCommand(() => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Bios));
    ShowDashboardCommand = new DelegateCommand(() => events.GetEvent<ShowDashboardEvent>().Publish());
    ClearHistoryCommand = new DelegateCommand(ClearHistory);

    _firmwareSubscription = model.Firmware.Subscribe(s => OnUi(() => Apply(s)));
    _telemetrySubscription = model.BoardTelemetry.Subscribe(t => OnUi(() => ApplyTelemetry(t)));
    _boardReadingsSubscription = model.BoardReadings.Subscribe(r => OnUi(() => ApplyBoardReadings(r)));
  }

  /// <summary>
  /// The manufacturer of the BIOS firmware.
  /// </summary>
  public string Manufacturer { get => _manufacturer; private set => SetProperty(ref _manufacturer, value); }

  /// <summary>
  /// The version of the BIOS firmware.
  /// </summary>
  public string Version { get => _version; private set => SetProperty(ref _version, value); }

  /// <summary>
  /// The release date of the BIOS firmware.
  /// </summary>
  public string ReleaseDate { get => _releaseDate; private set => SetProperty(ref _releaseDate, value); }

  /// <summary>
  /// The serial number of the BIOS firmware.
  /// </summary>
  public string SerialNumber { get => _serialNumber; private set => SetProperty(ref _serialNumber, value); }

  /// <summary>
  /// The SMBIOS specification version.
  /// </summary>
  public string SmbiosSpecVersion { get => _smbiosSpecVersion; private set => SetProperty(ref _smbiosSpecVersion, value); }

  /// <summary>
  /// The status of the BIOS firmware.
  /// </summary>
  public string Status { get => _status; private set => SetProperty(ref _status, value); }

  /// <summary>
  /// The size of the BIOS ROM.
  /// </summary>
  public string RomSize { get => _romSize; private set => SetProperty(ref _romSize, value); }

  /// <summary>
  /// The firmware interface of the BIOS firmware.
  /// </summary>
  public string FirmwareInterface { get => _firmwareInterface; private set => SetProperty(ref _firmwareInterface, value); }

  /// <summary>
  /// The BIOS revision of the BIOS firmware.
  /// </summary>
  public string BiosRevision { get => _biosRevision; private set => SetProperty(ref _biosRevision, value); }

  /// <summary>
  /// The embedded controller revision of the BIOS firmware.
  /// </summary>
  public string EmbeddedControllerRevision { get => _embeddedControllerRevision; private set => SetProperty(ref _embeddedControllerRevision, value); }

  /// <summary>
  /// The flash upgradeable status of the BIOS firmware.
  /// </summary>
  public string FlashUpgradeable { get => _flashUpgradeable; private set => SetProperty(ref _flashUpgradeable, value); }

  /// <summary>
  /// The selectable boot status of the BIOS firmware.
  /// </summary>
  public string SelectableBoot { get => _selectableBoot; private set => SetProperty(ref _selectableBoot, value); }

  /// <summary>
  /// The boot from CD status of the BIOS firmware.
  /// </summary>
  public string BootFromCd { get => _bootFromCd; private set => SetProperty(ref _bootFromCd, value); }

  /// <summary>
  /// The system manufacturer of the BIOS firmware.
  /// </summary>
  public string SystemManufacturer { get => _systemManufacturer; private set => SetProperty(ref _systemManufacturer, value); }

  /// <summary>
  /// The system product of the BIOS firmware.
  /// </summary>
  public string SystemProduct { get => _systemProduct; private set => SetProperty(ref _systemProduct, value); }

  /// <summary>
  /// The baseboard manufacturer of the BIOS firmware.
  /// </summary>
  public string BaseboardManufacturer { get => _baseboardManufacturer; private set => SetProperty(ref _baseboardManufacturer, value); }

  /// <summary>
  /// The baseboard product of the BIOS firmware.
  /// </summary>
  public string BaseboardProduct { get => _baseboardProduct; private set => SetProperty(ref _baseboardProduct, value); }

  /// <summary>
  /// The chassis type of the BIOS firmware.
  /// </summary>
  public string ChassisType { get => _chassisType; private set => SetProperty(ref _chassisType, value); }

  /// <summary>
  /// The secure boot status of the BIOS firmware.
  /// </summary>
  public string SecureBoot { get => _secureBoot; private set => SetProperty(ref _secureBoot, value); }

  /// <summary>
  /// The TPM status of the BIOS firmware.
  /// </summary>
  public string Tpm { get => _tpm; private set => SetProperty(ref _tpm, value); }

  /// <summary>
  /// The TPM manufacturer of the BIOS firmware.
  /// </summary>
  public string TpmManufacturer { get => _tpmManufacturer; private set => SetProperty(ref _tpmManufacturer, value); }

  /// <summary>
  /// The administrator password status of the BIOS firmware.
  /// </summary>
  public string AdministratorPassword { get => _administratorPassword; private set => SetProperty(ref _administratorPassword, value); }

  /// <summary>
  /// The power-on password status of the BIOS firmware.
  /// </summary>
  public string PowerOnPassword { get => _powerOnPassword; private set => SetProperty(ref _powerOnPassword, value); }

  /// <summary>
  /// The boot status of the BIOS firmware.
  /// </summary>
  public string BootStatus { get => _bootStatus; private set => SetProperty(ref _bootStatus, value); }

  /// <summary>
  /// Gets the firmware inventory.
  /// </summary>
  public ObservableCollection<FirmwareComponentViewModel> FirmwareInventory { get; } = [];

  /// <summary>
  /// Gets a value indicating whether the BIOS has a firmware inventory.
  /// </summary>
  public bool HasFirmwareInventory { get => _hasFirmwareInventory; private set => SetProperty(ref _hasFirmwareInventory, value); }

  /// <summary>
  /// Gets the count of firmware components in the BIOS firmware.
  /// </summary>
  public string FirmwareComponentCount { get => _firmwareComponentCount; private set => SetProperty(ref _firmwareComponentCount, value); }

  /// <summary>
  /// Gets the board temperature.
  /// </summary>
  public string BoardTemperature { get => _boardTemperature; private set => SetProperty(ref _boardTemperature, value); }

  /// <summary>
  /// Gets the CMOS voltage.
  /// </summary>
  public string CmosVoltage { get => _cmosVoltage; private set => SetProperty(ref _cmosVoltage, value); }

  /// <summary>
  /// Gets the chassis fan RPM.
  /// </summary>
  public string ChassisFanRpm { get => _chassisFanRpm; private set => SetProperty(ref _chassisFanRpm, value); }

  /// <summary>
  /// Gets the 3.3V rail voltage.
  /// </summary>
  public string Rail3V3 { get => _rail3V3; private set => SetProperty(ref _rail3V3, value); }

  /// <summary>
  /// Gets the 5V rail voltage.
  /// </summary>
  public string Rail5V { get => _rail5V; private set => SetProperty(ref _rail5V, value); }

  /// <summary>
  /// Gets the 12V rail voltage.
  /// </summary>
  public string Rail12V { get => _rail12V; private set => SetProperty(ref _rail12V, value); }

  /// <summary>
  /// Gets the 3.3V rail voltage range.
  /// </summary>
  public string Rail3V3Range { get => _rail3V3Range; private set => SetProperty(ref _rail3V3Range, value); }

  /// <summary>
  /// Gets the 5V rail voltage range.
  /// </summary>
  public string Rail5VRange { get => _rail5VRange; private set => SetProperty(ref _rail5VRange, value); }

  /// <summary>
  /// Gets the 12V rail voltage range.
  /// </summary>
  public string Rail12VRange { get => _rail12VRange; private set => SetProperty(ref _rail12VRange, value); }

  /// <summary>
  /// Gets the severity of the CMOS voltage reading.
  /// </summary>
  public ReadingSeverity CmosSeverity { get => _cmosSeverity; private set => SetProperty(ref _cmosSeverity, value); }

  /// <summary>
  /// Gets the severity of the 3.3V rail voltage reading.
  /// </summary>
  public ReadingSeverity Rail3V3Severity { get => _rail3V3Severity; private set => SetProperty(ref _rail3V3Severity, value); }

  /// <summary>
  /// Gets the severity of the 5V rail voltage reading.
  /// </summary>
  public ReadingSeverity Rail5VSeverity { get => _rail5VSeverity; private set => SetProperty(ref _rail5VSeverity, value); }

  /// <summary>
  /// Gets the severity of the 12V rail voltage reading.
  /// </summary>
  public ReadingSeverity Rail12VSeverity { get => _rail12VSeverity; private set => SetProperty(ref _rail12VSeverity, value); }

  /// <summary>
  /// Gets the overall board health severity based on the graded rails and CMOS readings.
  /// Worst severity across the graded rails/CMOS — drives the tile's status dot so board health
  /// reads at a glance without expanding the detail view.
  /// </summary>
  public ReadingSeverity BoardHealth { get => _boardHealth; private set => SetProperty(ref _boardHealth, value); }

  /// <summary>
  /// Gets the board health detail, which names the out-of-tolerance rails and CMOS readings.
  /// Names the out-of-tolerance rails (e.g. "+12V critical · CMOS warning") so the dot's tooltip
  /// says what's wrong; empty while everything is in spec.
  /// </summary>
  public string BoardHealthDetail { get => _boardHealthDetail; private set => SetProperty(ref _boardHealthDetail, value); }

  /// <summary>
  /// Gets the board health summary, which provides a compact count of out-of-spec rows for the tile badge.
  /// Compact count of out-of-spec rows for the tile badge, e.g. "1 critical · 2 warnings"; empty
  /// while everything is in spec. Distinct from BoardHealthDetail, which names each offending sensor.
  /// </summary>
  public string BoardHealthSummary { get => _boardHealthSummary; private set => SetProperty(ref _boardHealthSummary, value); }

  /// <summary>
  /// Gets the collection of board sensor rows.
  /// </summary>
  public ObservableCollection<BoardSensorRowViewModel> BoardSensors { get; } = [];

  /// <summary>
  /// Gets a value indicating whether the board has any sensor rows.
  /// </summary>
  public bool HasBoardSensors { get => _hasBoardSensors; private set => SetProperty(ref _hasBoardSensors, value); }

  /// <summary>
  /// Whether the ATX voltage rails (+3.3/+5/+12V) and CMOS battery trend should be shown. False on
  /// laptop-class chassis, where those rails and a serviceable coin cell don't exist, so the detail
  /// and summary views hide those rows rather than plotting perpetually-empty graphs.
  /// </summary>
  public bool ShowVoltageRails { get => _showVoltageRails; private set => SetProperty(ref _showVoltageRails, value); }

  /// <summary>
  /// Session log of out-of-spec episodes, ongoing first. Empty until something first leaves spec.
  /// </summary>
  public ObservableCollection<BoardHealthEventViewModel> HealthEvents { get; } = [];

  /// <summary>
  /// Gets a value indicating whether the board has any health events.
  /// </summary>
  public bool HasHealthEvents { get => _hasHealthEvents; private set => SetProperty(ref _hasHealthEvents, value); }

  /// <summary>
  /// When true the table shows only critical episodes; the count headline and tile peak stay computed
  /// from the full log. Toggling re-filters the already-logged rows immediately (no wait for a tick).
  /// </summary>
  public bool ShowCriticalOnly {
    get => _showCriticalOnly;
    set { if (SetProperty(ref _showCriticalOnly, value)) RefreshHealthEvents(); }
  }

  /// <summary>
  /// "N hidden" while the critical-only filter suppresses rows; empty when the filter is off or
  /// nothing is hidden — so the narrowed table doesn't read as an empty log.
  /// </summary>
  public string HealthEventsFilterHint { get => _healthEventsFilterHint; private set => SetProperty(ref _healthEventsFilterHint, value); }

  /// <summary>
  /// "+N older dropped" once the retention cap has evicted the oldest recovered episodes; empty until
  /// then. Signals the trail is truncated so the oldest kept row isn't read as the session's first.
  /// </summary>
  public string HealthEventsCapNote { get => _healthEventsCapNote; private set => SetProperty(ref _healthEventsCapNote, value); }

  /// <summary>
  /// "3 events · 1 ongoing" beside the section header; empty when the log is empty.
  /// </summary>
  public string HealthEventsSummary { get => _healthEventsSummary; private set => SetProperty(ref _healthEventsSummary, value); }

  /// <summary>
  /// The worst episode this session ("+12V 10.4 V") and its severity, for the tile's peak badge —
  /// so the dashboard shows the session's worst moment without expanding the detail view. Empty /
  /// Normal when nothing has left spec.
  /// </summary>
  public string SessionPeak { get => _sessionPeak; private set => SetProperty(ref _sessionPeak, value); }

  /// <summary>
  /// Gets the severity of the session's peak episode.
  /// </summary>
  public ReadingSeverity SessionPeakSeverity { get => _sessionPeakSeverity; private set => SetProperty(ref _sessionPeakSeverity, value); }

  /// <summary>
  /// Gets the status of the board sensors, which indicates whether the driver is installed and accessible.
  /// </summary>
  public string BoardSensorStatus { get => _boardSensorStatus; private set => SetProperty(ref _boardSensorStatus, value); }

  /// <summary>
  /// Gets a value indicating whether the board has a sensor status.
  /// </summary>
  public bool HasBoardSensorStatus { get => _hasBoardSensorStatus; private set => SetProperty(ref _hasBoardSensorStatus, value); }

  /// <summary>
  /// Gets the command to show detail view.
  /// </summary>
  public ICommand ShowDetailCommand { get; }

  /// <summary>
  /// Gets the command to show dashboard view.
  /// </summary>
  public ICommand ShowDashboardCommand { get; }

  /// <summary>
  /// Resets the session-scoped history this view model owns: the health event log and the trend
  /// graph buffers/markers. Recorded Min/Max in the row table come from the shared upstream sensor
  /// accumulators and are not reset here (that would need a cross-module SensorMonitor reset).
  /// </summary>
  public ICommand ClearHistoryCommand { get; }

  /// <summary>
  /// A view supplies its own graph instances (buffers must not be shared between the tile and the
  /// detail window). Clear the themed-severity cache so the next tick re-tints these fresh graphs
  /// regardless of what the previous ones were showing.
  /// </summary>
  /// <param name="rail3V3">The 3V3 rail graph.</param>
  /// <param name="rail5V">The 5V rail graph.</param>
  /// <param name="rail12V">The 12V rail graph.</param>
  public void AttachRailGraphs(PerformanceGraph rail3V3, PerformanceGraph rail5V, PerformanceGraph rail12V) {
    _rail3V3Graph = ResetMarkers(rail3V3);
    _rail5VGraph = ResetMarkers(rail5V);
    _rail12VGraph = ResetMarkers(rail12V);
    _rail3V3GraphSeverity = _rail5VGraphSeverity = _rail12VGraphSeverity = null;
  }

  /// <summary>
  /// The chassis-fan trend. Kept separate from the rail graphs because it's sourced from the board
  /// rows (which include a stalled fan's 0 RPM) rather than the headline telemetry, whose selector
  /// drops zeros — so a stall shows as the line falling to the floor, not the plot going stale.
  /// </summary>
  /// <param name="fan">The fan graph.</param>
  public void AttachFanGraph(PerformanceGraph fan) {
    _fanGraph = ResetMarkers(fan);
    _fanGraphSeverity = null;
  }

  /// <summary>
  /// The board-temperature trend, fed from the headline telemetry alongside the rails.
  /// </summary>
  /// <param name="boardTemp">The board temperature graph.</param>
  public void AttachBoardTempGraph(PerformanceGraph boardTemp) {
    _boardTempGraph = ResetMarkers(boardTemp);
    _boardTempGraphSeverity = null;
  }

  /// <summary>
  /// The CMOS-battery-voltage trend, fed from the headline telemetry alongside the rails. Its
  /// Y-window sits just above the critical band so a fading coin cell reads as the line sinking
  /// toward the floor.
  /// </summary>
  /// <param name="cmos">The CMOS battery voltage graph.</param>
  public void AttachCmosGraph(PerformanceGraph cmos) {
    _cmosGraph = ResetMarkers(cmos);
    _cmosGraphSeverity = null;
  }

  /// <summary>
  /// A freshly-attached graph starts with no session history, so clear any extreme markers so the
  /// first samples set them rather than folding into a stale attach-time value.
  /// </summary>
  /// <param name="graph">The performance graph.</param>
  /// <returns>The reset performance graph.</returns>
  private static PerformanceGraph ResetMarkers(PerformanceGraph graph) {
    graph.LowMarker = double.NaN;
    graph.HighMarker = double.NaN;
    return graph;
  }

  /// <summary>
  /// Plots a sample and folds it into the graph's session low/high markers, so a past dip or spike
  /// leaves a dashed reference line even after the live value recovers.
  /// </summary>
  /// <param name="graph">The performance graph.</param>
  /// <param name="value">The value to plot.</param>
  private static void Feed(PerformanceGraph? graph, double value) {
    if (graph is null) return;
    graph.AddValue(value);
    graph.LowMarker = double.IsNaN(graph.LowMarker) ? value : System.Math.Min(graph.LowMarker, value);
    graph.HighMarker = double.IsNaN(graph.HighMarker) ? value : System.Math.Max(graph.HighMarker, value);
  }

  /// <summary>
  /// Severity of the headline board temperature, tinting its trend line to match the value color.
  /// </summary>
  public ReadingSeverity BoardTemperatureSeverity {
    get => _boardTemperatureSeverity;
    private set => SetProperty(ref _boardTemperatureSeverity, value);
  }

  /// <summary>
  /// Whether a chassis/system fan was found among the board rows, so the detail view can hide the
  /// fan trend when the board exposes no such sensor.
  /// </summary>
  public bool HasChassisFan { get => _hasChassisFan; private set => SetProperty(ref _hasChassisFan, value); }

  /// <summary>
  /// Stall severity of the headlined chassis fan, tinting its trend line to match the row value.
  /// </summary>
  public ReadingSeverity ChassisFanSeverity {
    get => _chassisFanSeverity;
    private set => SetProperty(ref _chassisFanSeverity, value);
  }

  /// <summary>
  /// Applies the firmware snapshot to the view model, updating all relevant properties.
  /// </summary>
  /// <param name="s">The firmware snapshot.</param>
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
    ShowVoltageRails = !IsLaptopChassis(s.Chassis?.ChassisType);

    SecureBoot = FormatSecureBoot(s.SecureBoot);
    Tpm = FormatTpm(s.Tpm);
    TpmManufacturer = Text(s.Tpm.Manufacturer);
    AdministratorPassword = Text(FormatSecurity(s.HardwareSecurity?.AdministratorPassword));
    PowerOnPassword = Text(FormatSecurity(s.HardwareSecurity?.PowerOnPassword));
    BootStatus = FormatBoot(s.Boot);

    FirmwareInventory.Clear();
    foreach (var c in s.FirmwareInventory) {
      FirmwareInventory.Add(
        new FirmwareComponentViewModel(
          Text(c.ComponentName),
        Text(c.Version),
        Text(c.ReleaseDate),
        c.State.ToString(),
        FormatImageSize(c.ImageSizeBytes)));
    }

    HasFirmwareInventory = FirmwareInventory.Count > 0;
    FirmwareComponentCount = FirmwareInventory.Count > 0
      ? $"{FirmwareInventory.Count} component{(FirmwareInventory.Count == 1 ? "" : "s")}"
      : Dash;
  }

  /// <summary>
  /// Applies the board telemetry snapshot to the view model, updating all relevant properties and trend graphs.
  /// </summary>
  /// <param name="t">The board telemetry snapshot.</param>
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
    if (t.CmosVoltage is { } cv) Feed(_cmosGraph, cv);

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
    _cmosGraphSeverity = ThemeGraph(_cmosGraph, CmosSeverity, _cmosGraphSeverity);
  }

  /// <summary>
  /// Re-themes a rail graph only when its severity band changes, returning the newly-applied
  /// severity so the caller can track it. No-op if the graph isn't attached or nothing changed.
  /// </summary>
  /// <param name="graph">The performance graph to theme.</param>
  /// <param name="severity">The severity level.</param>
  /// <param name="applied">The previously applied severity level.</param>
  /// <returns>The newly applied severity level.</returns>
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

  /// <summary>
  /// Matches the #E85C5C critical value color used by ReadingSeverityToBrushConverter.
  /// </summary>
  private static readonly Color CriticalAccent = Color.FromRgb(0xE8, 0x5C, 0x5C);

  /// <summary>
  /// Rebuilds the bound event list from the log's snapshot. Rebuilt wholesale each tick (the list is small and capped) 
  /// so ongoing-episode durations advance and recovered ones settle without tracking per-row identity in the view.
  /// </summary>
  private void RefreshHealthEvents() {
    var now = _clock();
    var all = _healthLog.Snapshot()
      .Select(e => new BoardHealthEventViewModel(e, now))
      .ToList();

    // The displayed rows can be narrowed to critical-only, but everything below the table (the
    // count headline, the tile's session peak) is computed from the full set — the filter is a
    // display convenience, not a change to what the session actually recorded.
    HealthEvents.Clear();
    foreach (var e in all) {
      if (_showCriticalOnly && e.Severity != ReadingSeverity.Critical) continue;
      HealthEvents.Add(e);
    }
    HasHealthEvents = all.Count > 0;
    HealthEventsSummary = HealthEventsHeadline(all);

    // How many rows the critical-only filter is currently hiding, so the table reads as "narrowed",
    // not "empty". Blank when the filter is off or nothing is hidden.
    int hidden = all.Count - HealthEvents.Count;
    HealthEventsFilterHint = _showCriticalOnly && hidden > 0 ? $"{hidden} hidden" : "";

    // When the retention cap has evicted the oldest recovered episodes, say so — otherwise the
    // bottom of the table looks like the session's first fault when it's really just the oldest kept.
    int dropped = _healthLog.DroppedCount;
    HealthEventsCapNote = dropped > 0 ? $"+{dropped} older dropped" : "";

    // Single worst episode for the tile: highest severity wins, ties broken by the most recent start
    // so the freshest of equally-severe faults is the one surfaced. The peak reading is already
    // formatted with its unit on the row, so reuse it verbatim.
    var worst = all
      .OrderByDescending(e => e.Severity)
      .ThenByDescending(e => e.StartedSort)
      .FirstOrDefault();
    SessionPeak = worst is null ? "" : $"{worst.SensorName} {worst.PeakValue}".Trim();
    SessionPeakSeverity = worst?.Severity ?? ReadingSeverity.Normal;
  }

  /// <summary>
  /// "3 events · 1 ongoing" for the section header — total episode count plus how many are still
  /// active, so an unresolved fault reads at a glance. Empty when nothing's logged; the "ongoing"
  /// clause is dropped once everything has recovered.
  /// </summary>
  /// <param name="all">a collection of health events</param>
  /// <returns>the headline string</returns>
  private static string HealthEventsHeadline(IReadOnlyCollection<BoardHealthEventViewModel> all) {
    int total = all.Count;
    if (total == 0) return "";
    int ongoing = all.Count(e => e.Ongoing);
    string events = $"{total} event{(total == 1 ? "" : "s")}";
    return ongoing > 0 ? $"{events} · {ongoing} ongoing" : events;
  }

  /// <summary>
  /// Tab-separated dump of the current event rows (header + one line each), so the fault trail can be
  /// pasted straight into a bug report or spreadsheet. Projects the already-formatted view-model rows
  /// so the pasted text matches exactly what the table shows. Empty when there's nothing logged.
  /// </summary>
  /// <returns>the text string</returns>
  public string HealthEventsAsText() {
    if (HealthEvents.Count == 0) return "";
    var sb = new System.Text.StringBuilder();
    // Stamp the export with when it was captured, so a pasted/saved log carries its own provenance
    // (event timestamps are clock-of-day only; this pins the date). Uses the injected clock.
    sb.AppendLine($"# Exported {_clock().LocalDateTime:yyyy-MM-dd HH:mm:ss}");
    // Summarize the payload so a pasted/saved dump is self-describing: how many rows it holds and
    // how many are still open. Counts the exported (post-filter) rows so it matches what follows.
    int ongoing = HealthEvents.Count(e => e.Ongoing);
    sb.AppendLine(ongoing > 0
        ? $"# {HealthEvents.Count} event(s), {ongoing} ongoing"
        : $"# {HealthEvents.Count} event(s)");
    // The export mirrors the table, so with the critical-only filter on it carries only the shown
    // rows. Lead with a note so a filtered dump isn't mistaken for the whole log when pasted.
    if (_showCriticalOnly) sb.AppendLine("# Filtered view: critical events only");
    // If the retention cap has evicted older episodes, say so — an exported log should state that
    // it's incomplete rather than looking like the session's full history.
    if (_healthLog.DroppedCount > 0)
      sb.AppendLine($"# {_healthLog.DroppedCount} older recovered event(s) dropped by the retention cap");
    sb.AppendLine("Started\tSensor\tPeak\tReading\tPeak at\tDuration");
    foreach (var e in HealthEvents) {
      sb.AppendLine($"{e.Started}\t{e.SensorName}\t{e.Severity}\t{e.PeakValue}\t{e.PeakAt}\t{e.Duration}");
    }
    return sb.ToString();
  }

  /// <summary>
  /// Starts a fresh observation window: drops the health log and empties every trend graph's buffer
  /// and session low/high markers. The next poll repopulates the graphs and reopens any still-out-of-
  /// spec episode. Upstream Min/Max in the row table are owned by the shared monitor, not reset here.
  /// </summary>
  private void ClearHistory() {
    _healthLog.Clear();
    RefreshHealthEvents();
    foreach (var g in new[] { _rail3V3Graph, _rail5VGraph, _rail12VGraph, _fanGraph, _boardTempGraph, _cmosGraph }) {
      if (g is null) continue;
      g.ClearValues();
      ResetMarkers(g);
    }
  }

  /// <summary>
  /// Gets a detailed summary of the health issues.
  /// </summary>
  /// <param name="offenders">The list of offenders.</param>
  /// <returns>The detailed health summary.</returns>
  private static string HealthDetail(IReadOnlyList<(string Name, ReadingSeverity Severity, string Value)> offenders) =>
    string.Join(" · ", offenders
      .OrderByDescending(o => o.Severity)
      .Select(o => $"{o.Name} {o.Severity.ToString().ToLowerInvariant()}"));

  /// <summary>
  /// "1 critical · 2 warnings" for the tile badge, criticals first; empty when in spec. Counts, not
  /// names — the tooltip (BoardHealthDetail) carries the names, so the badge stays glanceable.
  /// </summary>
  /// <param name="offenders">The list of offenders.</param>
  /// <returns>The health summary.</returns>
  private static string HealthSummary(IReadOnlyList<(string Name, ReadingSeverity Severity, string Value)> offenders) {
    int critical = offenders.Count(o => o.Severity == ReadingSeverity.Critical);
    int warning = offenders.Count(o => o.Severity == ReadingSeverity.Warning);
    var parts = new List<string>(2);
    if (critical > 0) parts.Add($"{critical} critical");
    if (warning > 0) parts.Add($"{warning} warning{(warning == 1 ? "" : "s")}");
    return string.Join(" · ", parts);
  }

  /// <summary>
  /// "11.90–12.10" once both bounds are known; empty until then so the sub-line stays hidden.
  /// </summary>
  /// <param name="rail">The rail reading.</param>
  /// <returns>The range string.</returns>
  private static string RailRange(RailReading rail) =>
    rail is { Min: { } min, Max: { } max } ? $"{min:0.00}–{max:0.00}" : "";

  /// <summary>
  /// Applies the board sensor readings to the view model, updating the BoardSensors collection and overall health status.
  /// </summary>
  /// <param name="readings">The list of sensor readings.</param>
  private void ApplyBoardReadings(IReadOnlyList<SensorReading> readings) {
    BoardSensors.Clear();
    var offenders = new List<(string Name, ReadingSeverity Severity, string Value)>();
    // Grade every row first, then present worst-first (severity desc), falling back to sensor type
    // then name so same-severity rows keep a stable, readable order. This is only the default order;
    // the detail view lets the user re-sort by any column.
    var graded = readings
        .Select(r => (Reading: r, Severity: RowSeverity(r)))
        .OrderByDescending(x => x.Severity)
        .ThenBy(x => x.Reading.SensorType)
        .ThenBy(x => x.Reading.SensorName);
    foreach (var (r, severity) in graded) {
      string unit = r.Unit ?? "";
      string value = FormatValue(r.Value, unit);
      if (severity != ReadingSeverity.Normal) offenders.Add((Text(r.SensorName), severity, value));
      BoardSensors.Add(new BoardSensorRowViewModel(
          Text(r.SensorName),
          value,
          FormatValue(r.Min, unit),
          FormatValue(r.Max, unit),
          severity,
          GradeExtreme(r, r.Min),
          GradeExtreme(r, r.Max),
          r.Value, r.Min, r.Max));
    }
    HasBoardSensors = BoardSensors.Count > 0;

    // Whole-board rollup over every graded row (all rails + fan stall), not just the four headline
    // readings — so a broadened rail or a stalled fan lifts the tile dot and is named in the tooltip.
    BoardHealth = offenders.Count > 0 ? offenders.Max(o => o.Severity) : ReadingSeverity.Normal;
    BoardHealthDetail = HealthDetail(offenders);
    BoardHealthSummary = HealthSummary(offenders);

    // Fold this tick's offenders into the session log, then re-project it: an ongoing episode's
    // duration ticks up live, and a just-recovered one moves into history. offenders already spans
    // every graded row (rails, CMOS, board temp, fan stall) since they all flow through RowSeverity.
    _healthLog.Observe(offenders);
    RefreshHealthEvents();

    // Trend the chassis fan from the board rows so a stall plots as the line dropping to the floor
    // (the headline selector drops zeros, which would freeze the plot instead). A null reading feeds
    // 0 for the same reason. Tint the line to match the fan's stall severity.
    var fan = BoardTelemetrySelector.ChassisFanRow(readings);
    HasChassisFan = fan is not null;
    ChassisFanSeverity = fan is null 
      ? ReadingSeverity.Normal
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
      BoardSensorStatus = !_driverInstalled 
        ? "Board sensors unavailable — PawnIO driver not installed."
        : !_driverAccessible 
          ? "Board sensors unavailable — run elevated with the PawnIO driver running."
          : "No board sensors detected on this system.";
      HasBoardSensorStatus = true;
    }
  }

  /// <summary>
  /// Voltage rows we can map to a known fixed rail are graded against it; the CMOS cell uses its own
  /// absolute-volts rule. Board temperatures grade on their own thresholds; fans are graded for
  /// stall against the current board temperature. Variable-voltage rows (VCore, DRAM…) stay Normal —
  /// we have no spec to judge them against.
  /// </summary>
  /// <param name="r">The sensor reading to grade.</param>
  /// <returns>The severity level of the reading.</returns>
  private ReadingSeverity RowSeverity(SensorReading r) => r.SensorType == SensorType.Fan
    ? BoardReadingSeverity.Fan(r.Value, r.Max, _boardTemperatureC)
    : GradeExtreme(r, r.Value);

  /// <summary>
  /// Grades a single value against the row's spec, the same way the live value is graded, but
  /// omitting fan-stall logic (a fan's recorded Min/Max RPM is a spun-up/idle extreme, not a stall we
  /// can judge without the concurrent board temperature). Used both for the live value of non-fan
  /// rows and for the recorded Min/Max extremes, so a rail that dipped critical then recovered still
  /// shows a red Min column.
  /// </summary>
  /// <param name="r">The sensor reading to grade.</param>
  /// <param name="value">The value to grade.</param>
  /// <returns>The severity level of the reading.</returns>
  private static ReadingSeverity GradeExtreme(SensorReading r, float? value) {
    switch (r.SensorType) {
      case SensorType.Voltage:
        if (BoardTelemetrySelector.IsCmosRail(r.SensorName)) return BoardReadingSeverity.Cmos(value);
        return BoardTelemetrySelector.RailNominal(r.SensorName) is { } nominal
          ? BoardReadingSeverity.Rail(value, nominal)
          : ReadingSeverity.Normal;
      case SensorType.Temperature:
        return BoardReadingSeverity.Temperature(value);
      default:
        return ReadingSeverity.Normal;
    }
  }

  /// <summary>
  /// Formats a reading with its unit, or a dash if the value is null. Used for the headline telemetry
  /// </summary>
  /// <param name="value">The value to format.</param>
  /// <param name="unit">The unit of the value.</param>
  /// <param name="format">The format string.</param>
  /// <returns>The formatted string.</returns>
  private static string Reading(float? value, string unit, string format) =>
      value is { } v ? $"{v.ToString(format)} {unit}".Trim() : Dash;

  /// <summary>
  /// Formats a value with its unit, or a dash if the value is null. Used for the board sensor rows.
  /// </summary>
  /// <param name="value">The value to format.</param>
  /// <param name="unit">The unit of the value.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatValue(float? value, string unit) =>
      value is { } v ? $"{v:0.###} {unit}".Trim() : Dash;

  /// <summary>
  /// Formats the ROM size in bytes to a human-readable string in KB or MB. If the size is less than 1 MB, 
  /// it will be displayed in KB; otherwise, it will be displayed in MB with one decimal place.
  /// </summary>
  /// <param name="bytes">The size in bytes.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatRomSize(long bytes) {
    double mb = bytes / (1024d * 1024d);
    return mb >= 1 ? $"{mb:0.#} MB" : $"{bytes / 1024d:0.#} KB";
  }

  /// <summary>
  /// A firmware component's image size (SMBIOS Type 45), scaled to KB/MB. 0 means the component
  /// didn't report a size, shown as a dash rather than "0 KB".
  /// </summary>
  /// <param name="bytes">The size in bytes.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatImageSize(ulong bytes) {
    if (bytes == 0) return Dash;
    double mb = bytes / (1024d * 1024d);
    return mb >= 1 ? $"{mb:0.#} MB" : $"{bytes / 1024d:0.#} KB";
  }

  /// <summary>
  /// Formats the secure boot status to a human-readable string. If secure boot is not supported,
  /// it will return "Not supported". If secure boot is enabled, it will return "Enabled". 
  /// If secure boot is disabled, it will return "Disabled". If the status is unknown, it will return a dash.
  /// </summary>
  /// <param name="s">The secure boot information.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatSecureBoot(SecureBootInfo s) => s switch {
    { Supported: false } => "Not supported",
    { Enabled: true } => "Enabled",
    { Enabled: false } => "Disabled",
    _ => Dash,
  };

  /// <summary>
  /// Formats the TPM status to a human-readable string. If the TPM is not present, it will return "Not present".
  /// </summary>
  /// <param name="tpm">The TPM information.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatTpm(TpmInfo tpm) {
    if (!tpm.Present) return "Not present";
    string spec = string.IsNullOrWhiteSpace(tpm.SpecVersion) ? "" : $"v{tpm.SpecVersion} ";
    string state = tpm.Enabled switch { true => "Enabled", false => "Disabled", null => "Present" };
    return $"{spec}{state}".Trim();
  }

  /// <summary>
  /// Formats the chassis type to a human-readable string. If the chassis type is unknown or null, it will return a dash.
  /// </summary>
  /// <param name="type">The chassis type.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatChassis(Crystal.Service.Bios.ChassisType? type) =>
    type is null or Crystal.Service.Bios.ChassisType.Unknown ? Dash : SpaceCamelCase(type.Value.ToString());

  /// <summary>
  /// True for portable, battery-powered form factors (laptops, notebooks, tablets, convertibles…),
  /// which draw from a single battery/adapter rail and have no ATX +3.3/+5/+12V rails or a
  /// serviceable CMOS coin cell to trend. A null/unknown chassis is treated as non-laptop so a
  /// desktop that simply didn't report SMBIOS Type 3 still shows its rails.
  /// </summary>
  /// <param name="type">The chassis type.</param>
  /// <returns>True when the chassis is a laptop-class device.</returns>
  private static bool IsLaptopChassis(Crystal.Service.Bios.ChassisType? type) => type is
    Crystal.Service.Bios.ChassisType.Portable or
    Crystal.Service.Bios.ChassisType.Laptop or
    Crystal.Service.Bios.ChassisType.Notebook or
    Crystal.Service.Bios.ChassisType.HandHeld or
    Crystal.Service.Bios.ChassisType.SubNotebook or
    Crystal.Service.Bios.ChassisType.Tablet or
    Crystal.Service.Bios.ChassisType.Convertible or
    Crystal.Service.Bios.ChassisType.Detachable;

  /// <summary>
  /// Formats the hardware security status to a human-readable string. If the status is enabled, it will return "Enabled".
  /// </summary>
  /// <param name="status">The hardware security status.</param>
  /// <returns>The formatted string.</returns>
  private static string? FormatSecurity(HardwareSecurityStatus? status) => status switch {
    HardwareSecurityStatus.Enabled => "Enabled",
    HardwareSecurityStatus.Disabled => "Disabled",
    HardwareSecurityStatus.NotImplemented => "Not implemented",
    _ => null,
  };

  /// <summary>
  /// Formats the boot status to a human-readable string. If the boot status is null, it will return a dash.
  /// </summary>
  /// <param name="boot">The firmware boot information.</param>
  /// <returns>The formatted string.</returns>
  private static string FormatBoot(FirmwareBootInfo? boot) {
    if (boot is null) return Dash;
    return boot.Status is { } status ? SpaceCamelCase(status.ToString()) : $"0x{boot.StatusRaw:X2}";
  }

  /// <summary>
  /// Formats a nullable boolean to "Yes", "No", or a dash if null. Used for firmware capabilities.
  /// </summary>
  /// <param name="value">The nullable boolean value.</param>
  /// <returns>The formatted string.</returns>
  private static string YesNo(bool? value) => value switch { true => "Yes", false => "No", null => Dash };

  /// <summary>
  /// "NoBootableMedia" -> "No Bootable Media"; enum names are the only inputs here.
  /// </summary>
  /// <param name="value">The string to format.</param>
  /// <returns>The formatted string.</returns>
  private static string SpaceCamelCase(string value) {
    var sb = new System.Text.StringBuilder(value.Length + 4);
    for (int i = 0; i < value.Length; i++) {
      char c = value[i];
      if (i > 0 && char.IsUpper(c) && !char.IsUpper(value[i - 1])) sb.Append(' ');
      sb.Append(c);
    }
    return sb.ToString();
  }

  /// <summary>
  /// Returns a dash for null or whitespace, otherwise returns the string itself. Used for optional firmware fields.
  /// </summary>
  /// <param name="value">The string to format.</param>
  /// <returns>The formatted string.</returns>
  private static string Text(string? value) => string.IsNullOrWhiteSpace(value) ? Dash : value!;

  /// <summary>
  /// Posts an action to the UI thread for execution. This is used to update UI-bound properties from background threads.
  /// </summary>
  /// <param name="action">The action to post.</param>
  private void OnUi(Action action) => _ui.Post(action);

  /// <summary>
  /// Disposes of the subscriptions to firmware, telemetry, and board readings. 
  /// This should be called when the view model is no longer needed to clean up resources.
  /// </summary>
  public void Dispose() {
    _firmwareSubscription.Dispose();
    _telemetrySubscription.Dispose();
    _boardReadingsSubscription.Dispose();
  }
}
