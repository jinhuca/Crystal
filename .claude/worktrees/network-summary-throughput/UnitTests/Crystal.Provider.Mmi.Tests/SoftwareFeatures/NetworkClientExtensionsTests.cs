using Crystal.Provider.Mmi.MmiEngine;
using Crystal.Provider.Mmi.SoftwareFeatures.NetworkClient;
using Crystal.Provider.Mmi.Tests.Helpers;
using Xunit;

namespace Crystal.Provider.Mmi.Tests.SoftwareFeatures;

public class NetworkClientExtensionsTests {
  private static System.Collections.Frozen.FrozenDictionary<string, WmiValue> ClientRow() => WmiRow.Build(
      ("Caption", new WmiValue("Client for Microsoft Networks")),
      ("Description", new WmiValue("Client for Microsoft Networks")),
      ("InstallDate", new WmiValue(new DateTime(2021, 8, 3, 0, 0, 0, DateTimeKind.Utc))),
      ("Manufacturer", new WmiValue("Microsoft Corporation")),
      ("Name", new WmiValue("Client for Microsoft Networks")),
      ("Status", new WmiValue("OK"))
  );

  [Fact]
  public async Task FullData_Maps_Name() {
    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { ClientRow() });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Single(results);
    Assert.Equal("Client for Microsoft Networks", results[0].Name);
  }

  [Fact]
  public async Task FullData_Maps_Manufacturer() {
    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { ClientRow() });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Equal("Microsoft Corporation", results[0].Manufacturer);
  }

  [Fact]
  public async Task FullData_Maps_Status_OK() {
    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { ClientRow() });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Equal("OK", results[0].Status);
  }

  [Fact]
  public async Task FullData_Maps_InstallDate() {
    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { ClientRow() });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Equal(new DateTime(2021, 8, 3, 0, 0, 0, DateTimeKind.Utc), results[0].InstallDate);
  }

  [Fact]
  public async Task EmptyInstances_Returns_Empty_List() {
    var provider = new FakeWmiProvider("Win32_NetworkClient", WmiRow.Empty());
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Empty(results);
  }

  [Fact]
  public async Task MissingClass_Returns_Empty_List() {
    var provider = new FakeWmiProvider("SomeOtherClass", WmiRow.Empty());
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Empty(results);
  }

  [Fact]
  public async Task MultipleClients_Returns_All() {
    var client1 = WmiRow.Build(("Name", new WmiValue("Client for Microsoft Networks")));
    var client2 = WmiRow.Build(("Name", new WmiValue("SMB 1.0/CIFS Client")));

    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { client1, client2 });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Equal(2, results.Count);
    Assert.Equal("Client for Microsoft Networks", results[0].Name);
    Assert.Equal("SMB 1.0/CIFS Client", results[1].Name);
  }

  [Fact]
  public async Task PartialData_Leaves_Missing_Fields_Null() {
    var partial = WmiRow.Build(("Name", new WmiValue("Client Without Manufacturer")));

    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { partial });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Single(results);
    Assert.Equal("Client Without Manufacturer", results[0].Name);
    Assert.Null(results[0].Manufacturer);
    Assert.Null(results[0].Status);
  }

  [Fact]
  public async Task WrongTypeValue_Is_Ignored_Not_Miscast() {
    // InstallDate stored as a string instead of DateTime — should be treated as absent, not throw.
    var badRow = WmiRow.Build(("InstallDate", new WmiValue("2021-08-03")));

    var provider = new FakeWmiProvider("Win32_NetworkClient", new[] { badRow });
    var results = await provider.ToSafeNetworkClientMetricsAsync(CancellationToken.None);

    Assert.Single(results);
    Assert.Null(results[0].InstallDate);
  }
}
