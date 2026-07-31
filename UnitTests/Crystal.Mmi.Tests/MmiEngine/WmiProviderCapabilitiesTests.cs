using System.Collections.Frozen;
using Crystal.Mmi.MmiEngine;
using Crystal.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Mmi.Tests.MmiEngine;

public class WmiProviderCapabilitiesTests
{
    private const string TpmNamespace = @"root\cimv2\Security\MicrosoftTpm";

    // --- Namespace-aware GetMultiMetricsForClassAsync overload ---

    [Fact]
    public async Task NamespaceOverload_Returns_Registered_Rows()
    {
        var row = WmiRow.Build(("IsEnabled_InitialValue", new WmiValue(true)));
        var provider = new FakeWmiProvider("Win32_Tpm", new[] { row });

        var results = await provider.GetMultiMetricsForClassAsync(
            TpmNamespace, "Win32_Tpm", CancellationToken.None);

        Assert.Single(results);
        Assert.True(results[0]["IsEnabled_InitialValue"].AsBool());
    }

    [Fact]
    public async Task NamespaceOverload_Unknown_Class_Returns_Empty()
    {
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());

        var results = await provider.GetMultiMetricsForClassAsync(
            TpmNamespace, "Win32_Tpm", CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task NamespaceOverload_Cancelled_Throws()
    {
        var provider = new FakeWmiProvider("Win32_Tpm", WmiRow.Single());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetMultiMetricsForClassAsync(TpmNamespace, "Win32_Tpm", cts.Token));
    }

    // --- QueryAsync ---

    [Fact]
    public async Task QueryAsync_Returns_Rows_For_Registered_Wql()
    {
        const string wql = "SELECT * FROM Win32_Process WHERE Name = 'notepad.exe'";
        var row = WmiRow.Build(("Name", new WmiValue("notepad.exe")), ("ProcessId", new WmiValue(4242)));
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>())
            .WithQuery(wql, new[] { row });

        var results = await provider.QueryAsync(@"root\cimv2", wql, CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("notepad.exe", results[0]["Name"].AsString());
        Assert.Equal(4242, results[0]["ProcessId"].AsInt());
    }

    [Fact]
    public async Task QueryAsync_Unregistered_Wql_Returns_Empty()
    {
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());

        var results = await provider.QueryAsync(@"root\cimv2", "SELECT * FROM Win32_Nope", CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task QueryAsync_Cancelled_Throws()
    {
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.QueryAsync(@"root\cimv2", "SELECT * FROM Win32_Process", cts.Token));
    }

    // --- InvokeStaticMethodAsync ---

    [Fact]
    public async Task InvokeStaticMethodAsync_Returns_Registered_Result()
    {
        var outParams = new Dictionary<string, WmiValue>
        {
            ["ProcessId"] = new WmiValue(9001)
        }.ToFrozenDictionary();
        var expected = new WmiMethodResult(new WmiValue(0), outParams);

        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>())
            .WithMethod("Win32_Process", "Create", expected);

        var result = await provider.InvokeStaticMethodAsync(
            @"root\cimv2", "Win32_Process", "Create",
            new Dictionary<string, WmiValue> { ["CommandLine"] = new WmiValue("notepad.exe") },
            CancellationToken.None);

        Assert.Equal((uint)0, result.ReturnCode);
        Assert.Equal(9001, result.OutParameters["ProcessId"].AsInt());
    }

    [Fact]
    public async Task InvokeStaticMethodAsync_Unregistered_Returns_Empty()
    {
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());

        var result = await provider.InvokeStaticMethodAsync(
            @"root\cimv2", "Win32_Process", "Terminate",
            new Dictionary<string, WmiValue>(), CancellationToken.None);

        Assert.Same(WmiMethodResult.Empty, result);
        Assert.Null(result.ReturnCode);
        Assert.Empty(result.OutParameters);
    }

    [Fact]
    public async Task InvokeStaticMethodAsync_Cancelled_Throws()
    {
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>());
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.InvokeStaticMethodAsync(
                @"root\cimv2", "Win32_Process", "Create",
                new Dictionary<string, WmiValue>(), cts.Token));
    }

    // --- GetAssociatorsAsync (builds ASSOCIATORS OF WQL over QueryAsync) ---

    [Fact]
    public async Task GetAssociatorsAsync_Builds_Query_With_Both_Filters()
    {
        const string expectedWql =
            "ASSOCIATORS OF {Win32_NetworkAdapter.DeviceID=\"1\"} " +
            "WHERE ResultClass = Win32_NetworkAdapterConfiguration AssocClass = Win32_NetworkAdapterSetting";
        var row = WmiRow.Build(("IPEnabled", new WmiValue(true)));
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>())
            .WithQuery(expectedWql, new[] { row });

        var results = await provider.GetAssociatorsAsync(
            "Win32_NetworkAdapter.DeviceID=\"1\"",
            CancellationToken.None,
            resultClass: "Win32_NetworkAdapterConfiguration",
            assocClass: "Win32_NetworkAdapterSetting");

        Assert.Single(results);
        Assert.True(results[0]["IPEnabled"].AsBool());
    }

    [Fact]
    public async Task GetAssociatorsAsync_No_Filters_Builds_Bare_Query()
    {
        const string expectedWql = "ASSOCIATORS OF {Win32_LogicalDisk.DeviceID=\"C:\"}";
        var row = WmiRow.Build(("Name", new WmiValue("Partition #0")));
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>())
            .WithQuery(expectedWql, new[] { row });

        var results = await provider.GetAssociatorsAsync(
            "Win32_LogicalDisk.DeviceID=\"C:\"", CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("Partition #0", results[0]["Name"].AsString());
    }

    // --- GetReferencesAsync ---

    [Fact]
    public async Task GetReferencesAsync_Builds_Query_With_ResultClass()
    {
        const string expectedWql =
            "REFERENCES OF {Win32_NetworkAdapter.DeviceID=\"1\"} WHERE ResultClass = Win32_NetworkAdapterSetting";
        var row = WmiRow.Build(("Element", new WmiValue("ref")));
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>())
            .WithQuery(expectedWql, new[] { row });

        var results = await provider.GetReferencesAsync(
            "Win32_NetworkAdapter.DeviceID=\"1\"",
            CancellationToken.None,
            resultClass: "Win32_NetworkAdapterSetting");

        Assert.Single(results);
        Assert.Equal("ref", results[0]["Element"].AsString());
    }

    [Fact]
    public async Task GetReferencesAsync_No_Filter_Builds_Bare_Query()
    {
        const string expectedWql = "REFERENCES OF {Win32_LogicalDisk.DeviceID=\"C:\"}";
        var provider = new FakeWmiProvider(
            new Dictionary<string, IReadOnlyList<FrozenDictionary<string, WmiValue>>>())
            .WithQuery(expectedWql, WmiRow.Single(("K", new WmiValue("v"))));

        var results = await provider.GetReferencesAsync(
            "Win32_LogicalDisk.DeviceID=\"C:\"", CancellationToken.None);

        Assert.Single(results);
    }

    [Fact]
    public async Task WmiMethodResult_ReturnCode_Null_When_No_Return_Value()
    {
        var result = new WmiMethodResult(null, FrozenDictionary<string, WmiValue>.Empty);
        Assert.Null(result.ReturnCode);
        await Task.CompletedTask;
    }
}
