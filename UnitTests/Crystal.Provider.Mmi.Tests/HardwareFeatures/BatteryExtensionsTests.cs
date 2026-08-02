using Crystal.Provider.Mmi.HardwareFeatures.Battery;
using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.HardwareFeatures;

public class BatteryExtensionsTests
{
    private static FakeWmiProvider FullRow() => new FakeWmiProvider("Win32_Battery", WmiRow.Single(
        ("Availability",                new WmiValue(2)),
        ("BatteryRechargeTime",         new WmiValue(120)),
        ("BatteryStatus",               new WmiValue(3)),   // 3 = Fully Charged
        ("Caption",                     new WmiValue("Internal Battery")),
        ("Chemistry",                   new WmiValue(6)),   // 6 = Lithium-ion
        ("ConfigManagerErrorCode",      new WmiValue(0)),
        ("ConfigManagerUserConfig",     new WmiValue(false)),
        ("CreationClassName",           new WmiValue("Win32_Battery")),
        ("Description",                 new WmiValue("Internal Battery")),
        ("DesignCapacity",              new WmiValue(4500)),
        ("DesignVoltage",               new WmiValue(11400UL)),
        ("DeviceID",                    new WmiValue("DELL - 1")),
        ("ErrorCleared",                new WmiValue(false)),
        ("ErrorDescription",            new WmiValue("")),
        ("EstimatedChargeRemaining",    new WmiValue(95)),
        ("EstimatedRunTime",            new WmiValue(360)),
        ("ExpectedBatteryLife",         new WmiValue(480)),
        ("ExpectedLife",                new WmiValue(1440)),
        ("FullChargeCapacity",          new WmiValue(4400)),
        ("InstallationDate",                 new WmiValue(new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode",               new WmiValue(0)),
        ("MaxRechargeTime",             new WmiValue(90)),
        ("Name",                        new WmiValue("DELL JHXPY53")),
        ("PNPDeviceID",                 new WmiValue("ACPI\\PNP0C0A\\1")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2 })),
        ("PowerManagementSupported",    new WmiValue(false)),
        ("SmartBatteryVersion",         new WmiValue("1.1")),
        ("Status",                      new WmiValue("OK")),
        ("StatusInfo",                  new WmiValue(3)),
        ("SystemCreationClassName",     new WmiValue("Win32_ComputerSystem")),
        ("SystemName",                  new WmiValue("LAPTOP-01")),
        ("TimeOnBattery",               new WmiValue(0)),
        ("TimeToFullCharge",            new WmiValue(0))
    ));

    // --- Field mapping ---

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal("DELL JHXPY53", result.Name);
    }

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal("Internal Battery", result.Caption);
    }

    [Fact]
    public async Task FullData_Maps_Status()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal("OK", result.Status);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal("DELL - 1", result.DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_BatteryStatus_Ushort()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)3, result.BatteryStatus);
    }

    [Fact]
    public async Task FullData_Maps_Chemistry_Ushort()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)6, result.Chemistry);
    }

    [Fact]
    public async Task FullData_Maps_EstimatedChargeRemaining_Ushort()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)95, result.EstimatedChargeRemaining);
    }

    [Fact]
    public async Task FullData_Maps_EstimatedRunTime_Uint()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)360, result.EstimatedRunTime);
    }

    [Fact]
    public async Task FullData_Maps_DesignCapacity_Uint()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)4500, result.DesignCapacity);
    }

    [Fact]
    public async Task FullData_Maps_DesignVoltage_ULong()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal(11400UL, result.DesignVoltage);
    }

    [Fact]
    public async Task FullData_Maps_FullChargeCapacity_Uint()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)4400, result.FullChargeCapacity);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerErrorCode_Uint()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)0, result.ConfigManagerErrorCode);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerUserConfig_False()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.False(result.ConfigManagerUserConfig);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementSupported_False()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.False(result.PowerManagementSupported);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal(new ushort[] { 1, 2 }, result.PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal(new DateTime(2022, 6, 1, 0, 0, 0, DateTimeKind.Utc), result.InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_SmartBatteryVersion()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal("1.1", result.SmartBatteryVersion);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((ushort)2, result.Availability);
    }

    [Fact]
    public async Task FullData_Maps_MaxRechargeTime_Uint()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal((uint)90, result.MaxRechargeTime);
    }

    [Fact]
    public async Task FullData_Maps_SystemName()
    {
        var result = await FullRow().ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal("LAPTOP-01", result.SystemName);
    }

    // --- BatteryStatusPhrase computed property ---

    [Theory]
    [InlineData(1,  "Other")]
    [InlineData(2,  "Unknown")]
    [InlineData(3,  "Fully Charged")]
    [InlineData(4,  "Low")]
    [InlineData(5,  "Critical")]
    [InlineData(6,  "Charging")]
    [InlineData(7,  "Charging and High")]
    [InlineData(8,  "Charging and Low")]
    [InlineData(9,  "Charging and Critical")]
    [InlineData(10, "Undefined")]
    [InlineData(11, "Partially Charged")]
    public async Task BatteryStatusPhrase_Known_Codes(int code, string expected)
    {
        var provider = new FakeWmiProvider("Win32_Battery",
            WmiRow.Single(("BatteryStatus", new WmiValue(code))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal(expected, result.BatteryStatusPhrase);
    }

    [Fact]
    public async Task BatteryStatusPhrase_Unknown_Code_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_Battery",
            WmiRow.Single(("BatteryStatus", new WmiValue(99))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Null(result.BatteryStatusPhrase);
    }

    [Fact]
    public async Task BatteryStatusPhrase_Null_BatteryStatus_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_Battery", WmiRow.Single(("Name", new WmiValue("Battery"))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Null(result.BatteryStatusPhrase);
    }

    // --- ChemistryName computed property ---

    [Theory]
    [InlineData(1, "Other")]
    [InlineData(2, "Unknown")]
    [InlineData(3, "Lead Acid")]
    [InlineData(4, "Nickel Cadmium")]
    [InlineData(5, "Nickel Metal Hydride")]
    [InlineData(6, "Lithium-ion")]
    [InlineData(7, "Zinc Air")]
    [InlineData(8, "Lithium Polymer")]
    public async Task ChemistryName_Known_Codes(int code, string expected)
    {
        var provider = new FakeWmiProvider("Win32_Battery",
            WmiRow.Single(("Chemistry", new WmiValue(code))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Equal(expected, result.ChemistryName);
    }

    [Fact]
    public async Task ChemistryName_Unknown_Code_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_Battery",
            WmiRow.Single(("Chemistry", new WmiValue(99))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Null(result.ChemistryName);
    }

    [Fact]
    public async Task ChemistryName_Null_Chemistry_Returns_Null()
    {
        var provider = new FakeWmiProvider("Win32_Battery", WmiRow.Single(("Name", new WmiValue("Battery"))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);
        Assert.Null(result.ChemistryName);
    }

    // --- Fallback behaviour ---

    [Fact]
    public async Task EmptyInstances_Returns_All_Null_Fields()
    {
        var provider = new FakeWmiProvider("Win32_Battery", WmiRow.Empty());
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);

        Assert.Null(result.Name);
        Assert.Null(result.Caption);
        Assert.Null(result.Status);
        Assert.Null(result.BatteryStatus);
        Assert.Null(result.Chemistry);
        Assert.Null(result.EstimatedChargeRemaining);
        Assert.Null(result.DesignVoltage);
        Assert.Null(result.InstallDate);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_For_Those_Fields()
    {
        var provider = new FakeWmiProvider("Win32_Battery",
            WmiRow.Single(("Name", new WmiValue("Minimal Battery"))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);

        Assert.Equal("Minimal Battery", result.Name);
        Assert.Null(result.Caption);
        Assert.Null(result.BatteryStatus);
        Assert.Null(result.Chemistry);
        Assert.Null(result.DesignVoltage);
        Assert.Null(result.EstimatedChargeRemaining);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Fallback_Not_Throw()
    {
        // Battery extension uses a generic catch — swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await FullRow().ToSafeBatteryMetricsAsync(cts.Token);

        Assert.NotNull(result);
        Assert.Null(result.Name);
        Assert.Null(result.BatteryStatus);
    }

    [Fact]
    public async Task WrongValueType_For_Key_Returns_Null()
    {
        // BatteryStatus stored as String instead of Int — GetInt returns null
        var provider = new FakeWmiProvider("Win32_Battery",
            WmiRow.Single(("BatteryStatus", new WmiValue("Charging"))));
        var result = await provider.ToSafeBatteryMetricsAsync(CancellationToken.None);

        Assert.Null(result.BatteryStatus);
    }
}
