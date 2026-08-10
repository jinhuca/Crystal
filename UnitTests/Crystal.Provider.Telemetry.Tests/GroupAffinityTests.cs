using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class GroupAffinityTests {
  [Fact]
  public void Ctor_StoresGroupAndMask() {
    var a = new GroupAffinity(3, 0xABCD);

    Assert.Equal((ushort)3, a.Group);
    Assert.Equal(0xABCDUL, a.Mask);
  }

  [Theory]
  [InlineData(0, 1UL)]
  [InlineData(1, 2UL)]
  [InlineData(5, 32UL)]
  [InlineData(63, 0x8000000000000000UL)]
  public void Single_SetsOnlyTheIndexedBit(int index, ulong expectedMask) {
    var a = GroupAffinity.Single(2, index);

    Assert.Equal((ushort)2, a.Group);
    Assert.Equal(expectedMask, a.Mask);
  }

  [Fact]
  public void Undefined_IsMaxGroupWithZeroMask() {
    Assert.Equal(ushort.MaxValue, GroupAffinity.Undefined.Group);
    Assert.Equal(0UL, GroupAffinity.Undefined.Mask);
  }

  [Fact]
  public void Equals_SameGroupAndMask_AreEqual() {
    var a = new GroupAffinity(1, 4);
    var b = new GroupAffinity(1, 4);

    Assert.True(a.Equals(b));
    Assert.True(a == b);
    Assert.False(a != b);
    Assert.Equal(a.GetHashCode(), b.GetHashCode());
  }

  [Fact]
  public void Equals_DiffersInGroupOrMask_AreNotEqual() {
    var baseline = new GroupAffinity(1, 4);

    Assert.NotEqual(baseline, new GroupAffinity(2, 4));
    Assert.NotEqual(baseline, new GroupAffinity(1, 8));
    Assert.True(baseline != new GroupAffinity(2, 4));
  }

  [Fact]
  public void Equals_NonAffinityObject_ReturnsFalse() {
    var a = new GroupAffinity(1, 4);

    Assert.False(a.Equals("not an affinity"));
    Assert.False(a.Equals(null));
  }
}
