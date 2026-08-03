using System.Windows;
using System.Windows.Input;
using BiosModule.Models;
using Crystal.Infrastructure.Constants.Navigation;

namespace BiosModule.ViewModels;

public sealed class BiosViewModel : BindableBase, IBiosViewModel, IDisposable {
  private readonly IDisposable _specsSubscription;
  private string _manufacturer = "—";
  private string _version = "—";
  private string _releaseDate = "—";
  private string _serialNumber = "—";
  private string _smbiosSpecVersion = "—";
  private string _status = "—";

  public BiosViewModel(IBiosModel model, IEventAggregator events) {
    ShowDetailCommand = new DelegateCommand(
        () => events.GetEvent<ShowDetailEvent>().Publish(DetailViewNames.Bios));
    ShowDashboardCommand = new DelegateCommand(
        () => events.GetEvent<ShowDashboardEvent>().Publish());

    _specsSubscription = model.Specs.Subscribe(s => OnUi(() => ApplySpecs(s)));
  }

  public string Manufacturer { get => _manufacturer; private set => SetProperty(ref _manufacturer, value); }
  public string Version { get => _version; private set => SetProperty(ref _version, value); }
  public string ReleaseDate { get => _releaseDate; private set => SetProperty(ref _releaseDate, value); }
  public string SerialNumber { get => _serialNumber; private set => SetProperty(ref _serialNumber, value); }
  public string SmbiosSpecVersion { get => _smbiosSpecVersion; private set => SetProperty(ref _smbiosSpecVersion, value); }
  public string Status { get => _status; private set => SetProperty(ref _status, value); }

  public ICommand ShowDetailCommand { get; }
  public ICommand ShowDashboardCommand { get; }

  private void ApplySpecs(BiosSnapshot snapshot) {
    Manufacturer = Text(snapshot.Manufacturer);
    Version = Text(snapshot.Version);
    ReleaseDate = Text(snapshot.ReleaseDate);
    SerialNumber = Text(snapshot.SerialNumber);
    SmbiosSpecVersion = Text(snapshot.SmbiosSpecVersion);
    Status = Text(snapshot.Status);
  }

  private static string Text(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value!;

  private static void OnUi(Action action) {
    var dispatcher = Application.Current?.Dispatcher;
    if (dispatcher is null || dispatcher.CheckAccess()) action();
    else dispatcher.Invoke(action);
  }

  public void Dispose() => _specsSubscription.Dispose();
}
