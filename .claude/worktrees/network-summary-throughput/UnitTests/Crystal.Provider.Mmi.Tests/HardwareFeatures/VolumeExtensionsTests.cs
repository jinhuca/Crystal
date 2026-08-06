using Crystal.Provider.Mmi.HardwareFeatures.Volume;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class VolumeExtensionsTests
{
    [Fact]
    public async Task FullData_Maps_Size_And_Computed_Values()
    {
        var row = WmiRow.Build(("Name", new WmiValue("C:\\")), ("DriveLetter", new WmiValue("C:")), ("FileSystem", new WmiValue("NTFS")), ("Capacity", new WmiValue(500UL * 1024 * 1024 * 1024)), ("FreeSpace", new WmiValue(200UL * 1024 * 1024 * 1024)), ("DriveType", new WmiValue(3)), ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_Volume", new[] { row });
        var result = (await provider.ToSafeVolumeMetricsAsync(CancellationToken.None))[0];
        Assert.Equal("C:", result.DriveLetter);
        Assert.Equal("Local Disk", result.DriveTypeName);
        Assert.Equal(500, result.CapacityInGB);
        Assert.Equal(40, result.FreePercent);
        Assert.True(result.IsHealthy());
    }
}
