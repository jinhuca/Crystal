using Crystal.Provider.Mmi.HardwareFeatures.PortableBattery;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class PortableBatteryExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> BatteryRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("BatteryRechargeTime", new WmiValue(120)),
        ("BatteryStatus", new WmiValue(2)),
        ("CapacityMultiplier", new WmiValue(1)),
        ("Caption", new WmiValue("DELL XY123 Battery")),
        ("Chemistry", new WmiValue(6)), // 6 = Lithium Ion
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_PortableBattery")),
        ("Description", new WmiValue("DELL XY123 Battery")),
        ("DesignCapacity", new WmiValue(60000)),
        ("DesignVoltage", new WmiValue(11400UL)),
        ("DeviceID", new WmiValue("Battery1")),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("EstimatedChargeRemaining", new WmiValue(85)),
        ("EstimatedRunTime", new WmiValue(300)),
        ("ExpectedBatteryLife", new WmiValue(0)),
        ("ExpectedLife", new WmiValue(0)),
        ("FullChargeCapacity", new WmiValue(58000)),
        ("InstallDate", new WmiValue(new DateTime(2023, 3, 3, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Location", new WmiValue("Bay 1")),
        ("ManufactureDate", new WmiValue("20230101000000.000000+000")),
        ("Manufacturer", new WmiValue("SMP")),
        ("MaxBatteryError", new WmiValue(0)),
        ("MaxRechargeTime", new WmiValue(0)),
        ("Name", new WmiValue("DELL XY123 Battery")),
        ("PNPDeviceID", new WmiValue("ACPI\\PNP0C0A\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1 })),
        ("PowerManagementSupported", new WmiValue(true)),
        ("SmartBatteryVersion", new WmiValue("1.1")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01")),
        ("TimeOnBattery", new WmiValue(0)),
        ("TimeToFullCharge", new WmiValue(0))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("DELL XY123 Battery", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_DesignVoltage_Ulong()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal(11400UL, results[0].DesignVoltage);
    }

    [Fact]
    public async Task FullData_Maps_EstimatedChargeRemaining_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)85, results[0].EstimatedChargeRemaining);
    }

    [Fact]
    public async Task FullData_Maps_Chemistry_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)6, results[0].Chemistry);
    }

    [Fact]
    public async Task FullData_Maps_FullChargeCapacity_Uint()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal(58000u, results[0].FullChargeCapacity);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { BatteryRow() });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2023, 3, 3, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_PortableBattery", WmiRow.Empty());
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleBatteries_Returns_All()
    {
        var b1 = WmiRow.Build(("DeviceID", new WmiValue("Battery1")));
        var b2 = WmiRow.Build(("DeviceID", new WmiValue("Battery2")));

        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { b1, b2 });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Battery1", results[0].DeviceID);
        Assert.Equal("Battery2", results[1].DeviceID);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("DeviceID", new WmiValue("Battery3")));

        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { partial });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Battery3", results[0].DeviceID);
        Assert.Null(results[0].DesignVoltage);
        Assert.Null(results[0].EstimatedChargeRemaining);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // DesignVoltage stored as an Int instead of ULong — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("DesignVoltage", new WmiValue(11400)));

        var provider = new FakeWmiProvider("Win32_PortableBattery", new[] { badRow });
        var results = await provider.ToSafePortableBatteryMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].DesignVoltage);
    }
}
