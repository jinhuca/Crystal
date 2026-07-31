using System;
using Xunit;
using OperatingSystem = Crystal.Telemetry.Software.OperatingSystem;

namespace Crystal.Telemetry.Tests;

public class OperatingSystemTests
{
    [Fact]
    public void Is64Bit_MatchesEnvironment()
    {
        Assert.Equal(Environment.Is64BitOperatingSystem, OperatingSystem.Is64Bit);
    }

    [Fact]
    public void IsUnix_MatchesEnvironmentPlatform()
    {
        var platform = Environment.OSVersion.Platform;
        bool expected = platform is PlatformID.Unix or PlatformID.MacOSX;
        Assert.Equal(expected, OperatingSystem.IsUnix);
    }

    [Fact]
    public void IsWindows8OrGreater_IsFalseOnUnix()
    {
        if (OperatingSystem.IsUnix)
            Assert.False(OperatingSystem.IsWindows8OrGreater);
    }

    [Fact]
    public void IsWindows8OrGreater_ConsistentWithOsVersion()
    {
        if (OperatingSystem.IsUnix)
            return;

        var version = Environment.OSVersion.Version;
        bool expected = (version.Major == 6 && version.Minor >= 2) || version.Major > 6;
        Assert.Equal(expected, OperatingSystem.IsWindows8OrGreater);
    }
}
