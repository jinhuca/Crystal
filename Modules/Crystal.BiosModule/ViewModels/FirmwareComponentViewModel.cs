namespace Crystal.BiosModule.ViewModels;

/// <summary>One row in the firmware-inventory list (SMBIOS Type 45 component).</summary>
public sealed class FirmwareComponentViewModel {
  public FirmwareComponentViewModel(string name, string version, string releaseDate, string state, string imageSize) {
    Name = name;
    Version = version;
    ReleaseDate = releaseDate;
    State = state;
    ImageSize = imageSize;
  }

  public string Name { get; }
  public string Version { get; }
  public string ReleaseDate { get; }
  public string State { get; }
  public string ImageSize { get; }
}
