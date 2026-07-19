using Xunit;
using Crystal.Mmi.Constants;
using Crystal.Mmi.Queries;

namespace Crystal.Mmi.Tests.Queries;

public class QueryMemoryTests {
  public static bool IsWindows => OperatingSystem.IsWindows();

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Constructor_DoesNotThrow() {
    var exception = Record.Exception(() => new QueryMemory());

    Assert.Null(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ContainsRealMemoryProperties() {
    using var queryMemory = new QueryMemory();

    var result = queryMemory.GetInfoDictionary();

    Assert.NotEmpty(result);
    Assert.Contains(MemoryConstants.CapacityKey, result.Keys);
    Assert.Contains(MemoryConstants.ManufacturerKey, result.Keys);
    Assert.Contains(MemoryConstants.SpeedKey, result.Keys);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void GetInfoDictionary_ReturnsTheSameInstanceItExposesAsInfoDictionary() {
    using var queryMemory = new QueryMemory();

    var result = queryMemory.GetInfoDictionary();

    Assert.Same(queryMemory.InfoDictionary, result);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Query_ThrowsNotImplementedException() {
    using var queryMemory = new QueryMemory();

    var exception = Record.Exception(() => queryMemory.Query(MemoryConstants.QueryString));

    Assert.IsType<NotImplementedException>(exception);
  }

  [Fact(Skip = "Requires Windows and the MI/CIM runtime", SkipUnless = nameof(IsWindows))]
  public void Dispose_CanBeCalledMoreThanOnceWithoutThrowing() {
    var queryMemory = new QueryMemory();

    var exception = Record.Exception(() => {
      queryMemory.Dispose();
      queryMemory.Dispose();
    });

    Assert.Null(exception);
  }
}
