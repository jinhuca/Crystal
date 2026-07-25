using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.SoftwareFeatures.TimeZone;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.SoftwareFeatures;

public class TimeZoneExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> EasternTimeRow() => WmiRow.Build(
        ("Bias", new WmiValue(300)),
        ("Caption", new WmiValue("(UTC-05:00) Eastern Time (US & Canada)")),
        ("DaylightBias", new WmiValue(-60)),
        ("DaylightDay", new WmiValue(2)),
        ("DaylightDayOfWeek", new WmiValue(0)),
        ("DaylightHour", new WmiValue(2)),
        ("DaylightMillisecond", new WmiValue(0)),
        ("DaylightMinute", new WmiValue(0)),
        ("DaylightMonth", new WmiValue(3)),
        ("DaylightName", new WmiValue("Eastern Daylight Time")),
        ("DaylightSecond", new WmiValue(0)),
        ("DaylightYear", new WmiValue(0)),
        ("Description", new WmiValue("(UTC-05:00) Eastern Time (US & Canada)")),
        ("SettingID", new WmiValue("Eastern Standard Time")),
        ("StandardBias", new WmiValue(0)),
        ("StandardDay", new WmiValue(1)),
        ("StandardDayOfWeek", new WmiValue(0)),
        ("StandardHour", new WmiValue(2)),
        ("StandardMillisecond", new WmiValue(0)),
        ("StandardMinute", new WmiValue(0)),
        ("StandardMonth", new WmiValue(11)),
        ("StandardName", new WmiValue("Eastern Standard Time")),
        ("StandardSecond", new WmiValue(0)),
        ("StandardYear", new WmiValue(0))
    );

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { EasternTimeRow() });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("(UTC-05:00) Eastern Time (US & Canada)", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Bias_Int()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { EasternTimeRow() });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Equal(300, results[0].Bias);
    }

    [Fact]
    public async Task FullData_Maps_DaylightBias_NegativeInt()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { EasternTimeRow() });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Equal(-60, results[0].DaylightBias);
    }

    [Fact]
    public async Task FullData_Maps_DaylightDayOfWeek_Byte()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { EasternTimeRow() });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Equal((byte)0, results[0].DaylightDayOfWeek);
    }

    [Fact]
    public async Task FullData_Maps_StandardName()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { EasternTimeRow() });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Equal("Eastern Standard Time", results[0].StandardName);
    }

    [Fact]
    public async Task FullData_Maps_SettingID()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { EasternTimeRow() });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Equal("Eastern Standard Time", results[0].SettingID);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_TimeZone", WmiRow.Empty());
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingClass_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MultipleZones_Returns_All()
    {
        var tz1 = WmiRow.Build(("StandardName", new WmiValue("Eastern Standard Time")));
        var tz2 = WmiRow.Build(("StandardName", new WmiValue("Pacific Standard Time")));

        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { tz1, tz2 });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Eastern Standard Time", results[0].StandardName);
        Assert.Equal("Pacific Standard Time", results[1].StandardName);
    }

    [Fact]
    public async Task PartialData_Leaves_Missing_Fields_Null()
    {
        var partial = WmiRow.Build(("StandardName", new WmiValue("UTC")));

        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { partial });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("UTC", results[0].StandardName);
        Assert.Null(results[0].Bias);
        Assert.Null(results[0].DaylightName);
    }

    [Fact]
    public async Task WrongTypeValue_Is_Ignored_Not_Miscast()
    {
        // Bias stored as a bool instead of Int — should be treated as absent, not throw.
        var badRow = WmiRow.Build(("Bias", new WmiValue(true)));

        var provider = new FakeWmiProvider("Win32_TimeZone", new[] { badRow });
        var results = await provider.ToSafeTimeZoneMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Null(results[0].Bias);
    }
}
