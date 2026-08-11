using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Service.Storage.Tests;

public class StorageSensorSelectorTests {
  [Theory]
  [InlineData(new[] { "nvme", "0" }, 0)]
  [InlineData(new[] { "nvme", "3" }, 3)]
  [InlineData(new[] { "hdd", "12" }, 12)]
  public void DiskIndexOf_ParsesTrailingPhysicalDiskNumber(string[] parts, int expected) {
    Assert.Equal(expected, StorageSensorSelector.DiskIndexOf(new Identifier(parts)));
  }

  [Fact]
  public void DiskIndexOf_NonNumericTrailingToken_ReturnsNull() {
    Assert.Null(StorageSensorSelector.DiskIndexOf(new Identifier("nvme", "generic")));
  }

  [Fact]
  public void FindSensor_MatchesTypeAndNameCaseInsensitively() {
    var sensors = new ISensor[] {
      new StubSensor { SensorType = SensorType.Load, Name = "Total Activity", Value = 42 },
      new StubSensor { SensorType = SensorType.Throughput, Name = "Read Rate", Value = 1000 },
    };

    var hit = StorageSensorSelector.FindSensor(sensors, SensorType.Load, "total activity");

    Assert.NotNull(hit);
    Assert.Equal(42, hit!.Value);
  }

  [Fact]
  public void FindSensor_WrongType_ReturnsNull() {
    var sensors = new ISensor[] {
      new StubSensor { SensorType = SensorType.Throughput, Name = "Total Activity", Value = 42 },
    };

    // Same name but Throughput, not Load — must not match.
    Assert.Null(StorageSensorSelector.FindSensor(sensors, SensorType.Load, "Total Activity"));
  }

  [Fact]
  public void FindSensor_NoMatch_ReturnsNull() =>
      Assert.Null(StorageSensorSelector.FindSensor([], SensorType.Load, "Total Activity"));

  [Fact]
  public void BytesToMBps_ConvertsUsingMebibytes() {
    // 10 MiB/s in bytes → 10.0 MB/s (the app uses 1024*1024, matching Task Manager's binary MB).
    Assert.Equal(10.0, StorageSensorSelector.BytesToMBps(10 * 1024 * 1024));
    Assert.Equal(0.0, StorageSensorSelector.BytesToMBps(0));
  }
}
