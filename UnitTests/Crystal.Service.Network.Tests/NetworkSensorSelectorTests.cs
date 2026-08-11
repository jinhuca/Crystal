using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Network;
using Xunit;

namespace Crystal.Service.Network.Tests;

public class NetworkSensorSelectorTests {
  [Fact]
  public void FindValue_MatchesTypeAndNameCaseInsensitively() {
    var sensors = new ISensor[] {
      new StubSensor { SensorType = SensorType.Load, Name = "Network Utilization", Value = 42 },
      new StubSensor { SensorType = SensorType.Throughput, Name = "Upload Speed", Value = 1000 },
    };

    Assert.Equal(42, NetworkSensorSelector.FindValue(sensors, SensorType.Load, "network utilization"));
  }

  [Fact]
  public void FindValue_WrongType_ReturnsZero() {
    var sensors = new ISensor[] {
      new StubSensor { SensorType = SensorType.Throughput, Name = "Network Utilization", Value = 42 },
    };

    // Same name but Throughput, not Load — no match, defaults to 0.
    Assert.Equal(0, NetworkSensorSelector.FindValue(sensors, SensorType.Load, "Network Utilization"));
  }

  [Fact]
  public void FindValue_NoMatch_ReturnsZero() =>
      Assert.Equal(0, NetworkSensorSelector.FindValue([], SensorType.Load, "Network Utilization"));

  [Theory]
  [InlineData(50.0, 50.0)]
  [InlineData(0.0, 0.0)]
  [InlineData(100.0, 100.0)]
  [InlineData(150.0, 100.0)]   // capped at 100
  [InlineData(-5.0, 0.0)]      // floored at 0
  public void Clamp_BoundsFiniteValuesToPercentRange(double input, double expected) =>
      Assert.Equal(expected, NetworkSensorSelector.Clamp(input));

  [Fact]
  public void Clamp_NonFiniteValues_BecomeZero() {
    Assert.Equal(0, NetworkSensorSelector.Clamp(double.NaN));
    Assert.Equal(0, NetworkSensorSelector.Clamp(double.PositiveInfinity));
    Assert.Equal(0, NetworkSensorSelector.Clamp(double.NegativeInfinity));
  }

  [Theory]
  [InlineData(1234.5, 1234.5)]
  [InlineData(0.0, 0.0)]
  [InlineData(-1.0, 0.0)]      // negative → 0
  public void Sanitize_KeepsFiniteNonNegativeValues(double input, double expected) =>
      Assert.Equal(expected, NetworkSensorSelector.Sanitize(input));

  [Fact]
  public void Sanitize_NonFiniteValues_BecomeZero() {
    Assert.Equal(0, NetworkSensorSelector.Sanitize(double.NaN));
    Assert.Equal(0, NetworkSensorSelector.Sanitize(double.PositiveInfinity));
    Assert.Equal(0, NetworkSensorSelector.Sanitize(double.NegativeInfinity));
  }

  [Fact]
  public void ReduceWifiStatus_ConnectedBeatsEverything() {
    var states = new[] {
      WlanInterfaceState.Disabled,
      WlanInterfaceState.Disconnected,
      WlanInterfaceState.Connected,
    };

    Assert.Equal(WifiStatus.Connected, NetworkSensorSelector.ReduceWifiStatus(states));
  }

  [Fact]
  public void ReduceWifiStatus_DisconnectedBeatsDisabled() {
    var states = new[] { WlanInterfaceState.Disabled, WlanInterfaceState.Disconnected };

    Assert.Equal(WifiStatus.Disconnected, NetworkSensorSelector.ReduceWifiStatus(states));
  }

  [Fact]
  public void ReduceWifiStatus_AllDisabled_StaysDisabled() {
    var states = new[] { WlanInterfaceState.Disabled, WlanInterfaceState.Disabled };

    Assert.Equal(WifiStatus.Disabled, NetworkSensorSelector.ReduceWifiStatus(states));
  }

  [Fact]
  public void ReduceWifiStatus_Empty_StaysDisabled() =>
      Assert.Equal(WifiStatus.Disabled, NetworkSensorSelector.ReduceWifiStatus([]));
}
