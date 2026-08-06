using Crystal.Controls.PerformanceGraphs.Buffers;
using Xunit;

namespace Crystal.Controls.UnitTests.PerformanceGraphs;

public class CircularBufferTests {
  [Fact]
  public void Constructor_NonPositiveCapacity_Throws() {
    Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(0));
    Assert.Throws<ArgumentOutOfRangeException>(() => new CircularBuffer<int>(-1));
  }

  [Fact]
  public void NewBuffer_IsEmpty() {
    var buffer = new CircularBuffer<int>(3);

    Assert.Equal(3, buffer.Capacity);
    Assert.Equal(0, buffer.Count);
  }

  [Fact]
  public void Add_BelowCapacity_AppendsInOrder() {
    var buffer = new CircularBuffer<int>(3);

    buffer.Add(10);
    buffer.Add(20);

    Assert.Equal(2, buffer.Count);
    Assert.Equal(10, buffer[0]);
    Assert.Equal(20, buffer[1]);
  }

  [Fact]
  public void Add_BeyondCapacity_EvictsOldestAndKeepsOrder() {
    var buffer = new CircularBuffer<int>(3);

    buffer.Add(1);
    buffer.Add(2);
    buffer.Add(3);
    buffer.Add(4); // evicts 1
    buffer.Add(5); // evicts 2

    Assert.Equal(3, buffer.Count);
    Assert.Equal(3, buffer[0]); // oldest
    Assert.Equal(4, buffer[1]);
    Assert.Equal(5, buffer[2]); // newest
  }

  [Fact]
  public void Indexer_IsOldestFirst_AfterManyWraps() {
    var buffer = new CircularBuffer<int>(4);
    for (int i = 0; i < 20; i++) buffer.Add(i);

    // The last four values added: 16,17,18,19.
    Assert.Equal(16, buffer[0]);
    Assert.Equal(17, buffer[1]);
    Assert.Equal(18, buffer[2]);
    Assert.Equal(19, buffer[3]);
  }

  [Theory]
  [InlineData(-1)]
  [InlineData(2)]
  public void Indexer_OutOfRange_Throws(int index) {
    var buffer = new CircularBuffer<int>(3);
    buffer.Add(1);
    buffer.Add(2);

    Assert.Throws<ArgumentOutOfRangeException>(() => buffer[index]);
  }

  [Fact]
  public void Clear_ResetsCount_AndReusesCapacity() {
    var buffer = new CircularBuffer<int>(3);
    buffer.Add(1);
    buffer.Add(2);

    buffer.Clear();

    Assert.Equal(0, buffer.Count);
    Assert.Throws<ArgumentOutOfRangeException>(() => buffer[0]);

    buffer.Add(99);
    Assert.Equal(1, buffer.Count);
    Assert.Equal(99, buffer[0]);
  }

  [Fact]
  public void CapacityOne_AlwaysHoldsMostRecentValue() {
    var buffer = new CircularBuffer<int>(1);

    buffer.Add(1);
    buffer.Add(2);
    buffer.Add(3);

    Assert.Equal(1, buffer.Count);
    Assert.Equal(3, buffer[0]);
  }
}
