using Xunit;
using Crystal.Mmi.Constants;

namespace Crystal.Mmi.Tests.Constants;

public class MmiCategoryTests {
  [Fact]
  public void HasElevenCategories() {
    Assert.Equal(11, Enum.GetValues<MmiCategory>().Length);
  }

  // If MmiCategory values are ever persisted (settings, cache keys, etc.), reordering
  // the enum silently changes meaning. This pins the underlying int for each member.
  [Theory]
  [InlineData(MmiCategory.Bios, 0)]
  [InlineData(MmiCategory.Cpu, 1)]
  [InlineData(MmiCategory.Memory, 2)]
  [InlineData(MmiCategory.Disk, 3)]
  [InlineData(MmiCategory.Network, 4)]
  [InlineData(MmiCategory.OperatingSystem, 5)]
  [InlineData(MmiCategory.Process, 6)]
  [InlineData(MmiCategory.Service, 7)]
  [InlineData(MmiCategory.User, 8)]
  [InlineData(MmiCategory.Group, 9)]
  [InlineData(MmiCategory.EventLog, 10)]
  public void UnderlyingValue_IsStable(MmiCategory category, int expected) {
    Assert.Equal(expected, (int)category);
  }
}
