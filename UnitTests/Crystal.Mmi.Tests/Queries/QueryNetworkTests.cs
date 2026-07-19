using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryNetworkTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryNetwork());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealNetworkAdapterProperties() {
    using var queryNetwork = new QueryNetwork();

    var result = queryNetwork.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(NetworkConstants.NameKey, result.Keys);
    Assert.Contains(NetworkConstants.MACAddressKey, result.Keys);
    Assert.Contains(NetworkConstants.NetConnectionIDKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryNetwork = new QueryNetwork();

    var result = queryNetwork.GetInfoDictionary();

    Assert.Same(queryNetwork.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryNetwork = new QueryNetwork();

    var exception = Record.Exception(() => queryNetwork.Query(NetworkConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryNetwork = new QueryNetwork();

    var exception = Record.Exception(() => {
      queryNetwork.Dispose();
      queryNetwork.Dispose();
    });

    Assert.Null(exception);
  }
}
