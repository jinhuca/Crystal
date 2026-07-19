using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class MmiConstantsTests {
  [Fact]
  public void ComputerName_IsLocalhost() {
    Assert.Equal("localhost", MmiConstants.ComputerName);
  }

  [Fact]
  public void SessionNamespace_IsRootCimv2() {
    Assert.Equal(@"root\cimv2", MmiConstants.SessionNamespace);
  }

  [Fact]
  public void QueryDialect_IsWql() {
    Assert.Equal("WQL", MmiConstants.QueryDialect);
  }
}
