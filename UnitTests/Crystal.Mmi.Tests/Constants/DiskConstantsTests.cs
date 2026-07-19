using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class DiskConstantsTests {
  [Fact]
  public void QueryString_SelectsAllFromWin32DiskDrive() {
    Assert.Equal("SELECT * FROM Win32_DiskDrive", DiskConstants.QueryString);
  }

  [Theory]
  [InlineData(DiskConstants.AvailabilityKey, "Availability")]
  [InlineData(DiskConstants.BytesPerSectorKey, "BytesPerSector")]
  [InlineData(DiskConstants.CaptionKey, "Caption")]
  [InlineData(DiskConstants.ConfigManagerErrorCodeKey, "ConfigManagerErrorCode")]
  [InlineData(DiskConstants.CreationClassNameKey, "CreationClassName")]
  [InlineData(DiskConstants.DescriptionKey, "Description")]
  [InlineData(DiskConstants.DeviceIDKey, "DeviceID")]
  [InlineData(DiskConstants.FirmwareRevisionKey, "FirmwareRevision")]
  [InlineData(DiskConstants.IndexKey, "Index")]
  [InlineData(DiskConstants.InstallDateKey, "InstallDate")]
  [InlineData(DiskConstants.InterfaceTypeKey, "InterfaceType")]
  [InlineData(DiskConstants.ManufacturerKey, "Manufacturer")]
  [InlineData(DiskConstants.MediaLoadedKey, "MediaLoaded")]
  [InlineData(DiskConstants.MediaTypeKey, "MediaType")]
  [InlineData(DiskConstants.ModelKey, "Model")]
  [InlineData(DiskConstants.NameKey, "Name")]
  [InlineData(DiskConstants.PartitionsKey, "Partitions")]
  [InlineData(DiskConstants.PNPDeviceIDKey, "PNPDeviceID")]
  [InlineData(DiskConstants.SCSIBusKey, "SCSIBus")]
  [InlineData(DiskConstants.SCSILogicalUnitKey, "SCSILogicalUnit")]
  [InlineData(DiskConstants.SCSIPortKey, "SCSIPort")]
  [InlineData(DiskConstants.SCSITargetIdKey, "SCSITargetId")]
  [InlineData(DiskConstants.SectorsPerTrackKey, "SectorsPerTrack")]
  [InlineData(DiskConstants.SerialNumberKey, "SerialNumber")]
  [InlineData(DiskConstants.SignatureKey, "Signature")]
  [InlineData(DiskConstants.SizeKey, "Size")]
  [InlineData(DiskConstants.StatusKey, "Status")]
  [InlineData(DiskConstants.StatusInfoKey, "StatusInfo")]
  [InlineData(DiskConstants.SystemCreationClassNameKey, "SystemCreationClassName")]
  [InlineData(DiskConstants.SystemNameKey, "SystemName")]
  [InlineData(DiskConstants.TotalCylindersKey, "TotalCylinders")]
  [InlineData(DiskConstants.TotalHeadsKey, "TotalHeads")]
  [InlineData(DiskConstants.TotalSectorsKey, "TotalSectors")]
  [InlineData(DiskConstants.TotalTracksKey, "TotalTracks")]
  [InlineData(DiskConstants.TracksPerCylinderKey, "TracksPerCylinder")]
  public void PropertyKey_MatchesWin32DiskDriveSchema(string key, string expected) {
    Assert.Equal(expected, key);
  }
}
