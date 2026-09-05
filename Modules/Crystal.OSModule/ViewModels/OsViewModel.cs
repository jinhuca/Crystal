using Crystal.Controls.Threading;
using Crystal.Infrastructure.Constants.Navigation;
using Crystal.OSModule.Models;
using Crystal.Service.Process;
using System.Windows.Input;

namespace Crystal.OSModule.ViewModels;

public sealed class OsViewModel : BindableBase, IOsViewModel, IDisposable {
  private const string Dash = "—";
  private readonly IDisposable _infoSubscription;
  private readonly IDisposable _liveSubscription;
  private readonly IDisposable _statsSubscription;
  private readonly UiThreadMarshaller _ui = new();

  private string _osName = Dash;
  private string _buildLabel = Dash;
  private string _displayVersion = Dash;
  private string _architecture = Dash;
  private string _uptimeLabel = Dash;
  private string _currentTimeLabel = Dash;
  private string _edition = Dash;
  private string _versionLabel = Dash;
  private string _machineName = Dash;
  private string _userName = Dash;
  private string _registeredOwner = Dash;
  private string _registeredOrganization = Dash;
  private string _systemDirectory = Dash;
  private string _locale = Dash;
  private string _timeZone = Dash;
  private string _installDateLabel = Dash;
  private string _lastBootTimeLabel = Dash;
  private string _processCountLabel = Dash;
  private string _threadCountLabel = Dash;
  private string _handleCountLabel = Dash;

  public OsViewModel(IOsModel model, IEventAggregator events, SystemStatsMonitor stats) {
    ArgumentNullException.ThrowIfNull(model);
    ArgumentNullException.ThrowIfNull(events);
    ArgumentNullException.ThrowIfNull(stats);
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Os));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());
    ShowProcessesCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Process));

    _infoSubscription = model.Info.Subscribe(s => OnUi(() => ApplyInfo(s)));
    _liveSubscription = model.Live.Subscribe(r => OnUi(() => ApplyLive(r)));
    _statsSubscription = stats.Stats.Subscribe(s => OnUi(() => ApplyStats(s)));
  }

  public string OsName { get => _osName; private set => SetProperty(ref _osName, value); }
  public string BuildLabel { get => _buildLabel; private set => SetProperty(ref _buildLabel, value); }
  public string DisplayVersion { get => _displayVersion; private set => SetProperty(ref _displayVersion, value); }
  public string Architecture { get => _architecture; private set => SetProperty(ref _architecture, value); }
  public string UptimeLabel { get => _uptimeLabel; private set => SetProperty(ref _uptimeLabel, value); }
  public string CurrentTimeLabel { get => _currentTimeLabel; private set => SetProperty(ref _currentTimeLabel, value); }
  public string Edition { get => _edition; private set => SetProperty(ref _edition, value); }
  public string VersionLabel { get => _versionLabel; private set => SetProperty(ref _versionLabel, value); }
  public string MachineName { get => _machineName; private set => SetProperty(ref _machineName, value); }
  public string UserName { get => _userName; private set => SetProperty(ref _userName, value); }
  public string RegisteredOwner { get => _registeredOwner; private set => SetProperty(ref _registeredOwner, value); }
  public string RegisteredOrganization { get => _registeredOrganization; private set => SetProperty(ref _registeredOrganization, value); }
  public string SystemDirectory { get => _systemDirectory; private set => SetProperty(ref _systemDirectory, value); }
  public string Locale { get => _locale; private set => SetProperty(ref _locale, value); }
  public string TimeZone { get => _timeZone; private set => SetProperty(ref _timeZone, value); }
  public string InstallDateLabel { get => _installDateLabel; private set => SetProperty(ref _installDateLabel, value); }
  public string LastBootTimeLabel { get => _lastBootTimeLabel; private set => SetProperty(ref _lastBootTimeLabel, value); }
  public string ProcessCountLabel { get => _processCountLabel; private set => SetProperty(ref _processCountLabel, value); }
  public string ThreadCountLabel { get => _threadCountLabel; private set => SetProperty(ref _threadCountLabel, value); }
  public string HandleCountLabel { get => _handleCountLabel; private set => SetProperty(ref _handleCountLabel, value); }

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }
  public ICommand ShowProcessesCommand { get; }

  private void ApplyInfo(OsSnapshot s) {
    OsName = Text(s.Caption);
    BuildLabel = Text(s.BuildNumber);
    DisplayVersion = Text(s.DisplayVersion);
    Architecture = Text(s.Architecture);
    Edition = Text(s.Edition);
    VersionLabel = Text(s.Version);
    MachineName = Text(s.MachineName);
    UserName = Text(s.UserName);
    RegisteredOwner = Text(s.RegisteredOwner);
    RegisteredOrganization = Text(s.RegisteredOrganization);
    SystemDirectory = Text(s.SystemDirectory);
    Locale = Text(s.Locale);
    TimeZone = Text(s.TimeZone);
    InstallDateLabel = DateTime(s.InstallDate);
    LastBootTimeLabel = DateTime(s.LastBootTime);
  }

  private void ApplyLive(OsLiveReading r) {
    UptimeLabel = FormatUptime(r.Uptime);
    CurrentTimeLabel = r.Now.ToString("yyyy-MM-dd HH:mm:ss");
  }

  private void ApplyStats(SystemStats s) {
    ProcessCountLabel = s.Processes.ToString("N0");
    ThreadCountLabel = s.Threads.ToString("N0");
    HandleCountLabel = s.Handles.ToString("N0");
  }

  // "3d 21:22:12" — days only shown once there's at least one, matching the title-bar uptime style.
  internal static string FormatUptime(TimeSpan uptime) {
    if (uptime < TimeSpan.Zero) uptime = TimeSpan.Zero;
    return uptime.Days > 0
        ? $"{uptime.Days}d {uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}"
        : $"{uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}";
  }

  private static string DateTime(DateTimeOffset? value) =>
      value is { } v ? v.ToString("yyyy-MM-dd HH:mm") : Dash;

  private static string Text(string? value) => string.IsNullOrWhiteSpace(value) ? Dash : value!;

  private void OnUi(Action action) => _ui.Post(action);

  public void Dispose() {
    _infoSubscription.Dispose();
    _liveSubscription.Dispose();
    _statsSubscription.Dispose();
  }
}
