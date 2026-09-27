using Crystal.Infrastructure.Core.Common;

namespace Crystal.Infrastructure.Core.Tests.Common.Mocks;

internal class MockParameters : ParametersBase {
  public MockParameters() { }
  public MockParameters(string query) : base(query) { }
}
