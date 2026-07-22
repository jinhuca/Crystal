using Crystal.Mmi.HardwareFeatures.SoundDevice;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.HardwareFeatures;

public class SoundDeviceExtensionsTests
{
    private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> RealtekRow() => WmiRow.Build(
        ("Availability", new WmiValue(3)),
        ("Caption", new WmiValue("Realtek High Definition Audio")),
        ("ConfigManagerErrorCode", new WmiValue(0)),
        ("ConfigManagerUserConfig", new WmiValue(false)),
        ("CreationClassName", new WmiValue("Win32_SoundDevice")),
        ("Description", new WmiValue("Realtek High Definition Audio")),
        ("DeviceID", new WmiValue("HDAUDIO\\FUNC_01&VEN_10EC&DEV_0897&SUBSYS_10434099&REV_1001\\4&21A4E7B7&0&0001")),
        ("DMABufferSize", new WmiValue(1)),
        ("ErrorCleared", new WmiValue(false)),
        ("ErrorDescription", new WmiValue("")),
        ("InstallationDate", new WmiValue(new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc))),
        ("LastErrorCode", new WmiValue(0)),
        ("Manufacturer", new WmiValue("Realtek")),
        ("MPU401Address", new WmiValue("")),
        ("Name", new WmiValue("Realtek High Definition Audio")),
        ("PNPDeviceID", new WmiValue("HDAUDIO\\FUNC_01&VEN_10EC&DEV_0897&SUBSYS_10434099&REV_1001\\4&21A4E7B7&0&0001")),
        ("PowerManagementCapabilities", new WmiValue(new ushort[] { 1, 2, 3 })),
        ("PowerManagementSupported", new WmiValue(false)),
        ("ProductID", new WmiValue("0897")),
        ("Status", new WmiValue("OK")),
        ("StatusInfo", new WmiValue(3)),
        ("SystemCreationClassName", new WmiValue("Win32_ComputerSystem")),
        ("SystemName", new WmiValue("DESKTOP-01"))
    );

    [Fact]
    public async Task FullData_Maps_Name()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Realtek High Definition Audio", results[0].Name);
    }

    [Fact]
    public async Task FullData_Maps_Manufacturer()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Realtek", results[0].Manufacturer);
    }

    [Fact]
    public async Task FullData_Maps_Caption()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Realtek High Definition Audio", results[0].Caption);
    }

    [Fact]
    public async Task FullData_Maps_Status_OK()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("OK", results[0].Status);
    }

    [Fact]
    public async Task FullData_Maps_DeviceID()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Contains("HDAUDIO", results[0].DeviceID);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerErrorCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)0, results[0].ConfigManagerErrorCode);
    }

    [Fact]
    public async Task FullData_Maps_ConfigManagerUserConfig_False()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ConfigManagerUserConfig);
    }

    [Fact]
    public async Task FullData_Maps_DMABufferSize_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)1, results[0].DMABufferSize);
    }

    [Fact]
    public async Task FullData_Maps_ErrorCleared_False()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.False(results[0].ErrorCleared);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementSupported_False()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.False(results[0].PowerManagementSupported);
    }

    [Fact]
    public async Task FullData_Maps_PowerManagementCapabilities_Array()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(new ushort[] { 1, 2, 3 }, results[0].PowerManagementCapabilities);
    }

    [Fact]
    public async Task FullData_Maps_Availability_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].Availability);
    }

    [Fact]
    public async Task FullData_Maps_StatusInfo_Ushort()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((ushort)3, results[0].StatusInfo);
    }

    [Fact]
    public async Task FullData_Maps_InstallDate()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(new DateTime(2019, 1, 1, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
    }

    [Fact]
    public async Task FullData_Maps_ProductID()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("0897", results[0].ProductID);
    }

    [Fact]
    public async Task FullData_Maps_LastErrorCode_Uint()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal((uint)0, results[0].LastErrorCode);
    }

    [Fact]
    public async Task Multiple_Devices_Returns_All()
    {
        var dev1 = WmiRow.Build(("Name", new WmiValue("Realtek HD")), ("Manufacturer", new WmiValue("Realtek")));
        var dev2 = WmiRow.Build(("Name", new WmiValue("NVIDIA HDMI")), ("Manufacturer", new WmiValue("NVIDIA")));

        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { dev1, dev2 });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.Equal("Realtek HD", results[0].Name);
        Assert.Equal("NVIDIA HDMI", results[1].Name);
    }

    [Fact]
    public async Task EmptyInstances_Returns_Empty_List()
    {
        var provider = new FakeWmiProvider("Win32_SoundDevice", WmiRow.Empty());
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Cancelled_Token_Returns_Empty_Fallback()
    {
        // SoundDevice extension uses generic catch — swallows OperationCanceledException
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { RealtekRow() });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(cts.Token);

        Assert.Empty(results);
    }

    [Fact]
    public async Task MissingKeys_Return_Null_Fields()
    {
        var row = WmiRow.Build(
            ("Name", new WmiValue("Minimal Audio")),
            ("Status", new WmiValue("OK")));
        var provider = new FakeWmiProvider("Win32_SoundDevice", new[] { row });
        var results = await provider.ToSafeSoundDeviceMetricsAsync(CancellationToken.None);

        Assert.Equal("Minimal Audio", results[0].Name);
        Assert.Equal("OK", results[0].Status);
        Assert.Null(results[0].Manufacturer);
        Assert.Null(results[0].DeviceID);
        Assert.Null(results[0].DMABufferSize);
        Assert.Null(results[0].PowerManagementCapabilities);
    }
}
