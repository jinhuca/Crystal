using System;
using Xunit;

namespace Crystal.UnitTests.Events
{
  public class DataEventArgsFixture
  {
    [Fact]
    public void CanPassData()
    {
      DataEventArgs<int> e = new(32);
      Assert.Equal(32, e.Value);
    }

    [Fact]
    public void IsEventArgs()
    {
      DataEventArgs<string> dea = new("");
      EventArgs ea = dea as EventArgs;
      Assert.NotNull(ea);
    }
  }
}
