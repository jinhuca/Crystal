using Crystal.Provider.Etw;
using Xunit;

namespace Crystal.Provider.Etw.Tests;

/// <summary>
/// Locks down <see cref="ProcessEtwReader.ClassifyDma"/> — the per-event GPU DMA-packet classifier
/// on the DxgKrnl hot path. The reader caches one verdict per distinct event name, so this pure
/// mapping (name → Ignore/Start/Stop) is what decides whether a packet contributes to GPU busy time.
/// </summary>
public class ClassifyDmaTests {
  [Theory]
  [InlineData("DmaPacketStart")]
  [InlineData("DmaPacket/Start")]
  [InlineData("Context/DmaPacketStart")]
  public void DmaPacket_start_events_classify_as_Start(string name) =>
      Assert.Equal(ProcessEtwReader.DmaKind.Start, ProcessEtwReader.ClassifyDma(name));

  [Theory]
  [InlineData("DmaPacketStop")]
  [InlineData("DmaPacket/Stop")]
  [InlineData("DmaPacketInfo")] // Info is treated as a completion marker, same as Stop.
  public void DmaPacket_stop_and_info_events_classify_as_Stop(string name) =>
      Assert.Equal(ProcessEtwReader.DmaKind.Stop, ProcessEtwReader.ClassifyDma(name));

  [Theory]
  [InlineData("dmapacketstart")]
  [InlineData("DMAPACKETSTART")]
  [InlineData("DmApAcKeTsToP")]
  public void Classification_is_case_insensitive(string name) {
    // Whatever the casing, a start still reads as Start and a stop as Stop.
    var expected = name.ToLowerInvariant().Contains("start")
        ? ProcessEtwReader.DmaKind.Start
        : ProcessEtwReader.DmaKind.Stop;
    Assert.Equal(expected, ProcessEtwReader.ClassifyDma(name));
  }

  [Theory]
  [InlineData("")]
  [InlineData("Present")]
  [InlineData("PagingQueuePacketStart")] // Not a DmaPacket — must not be misattributed as GPU busy.
  [InlineData("VSyncDPC")]
  public void Non_dma_events_are_ignored(string name) =>
      Assert.Equal(ProcessEtwReader.DmaKind.Ignore, ProcessEtwReader.ClassifyDma(name));

  [Fact]
  public void DmaPacket_without_start_or_stop_token_is_ignored() =>
      // A DmaPacket event that is neither start nor stop/info carries no timing edge to act on.
      Assert.Equal(ProcessEtwReader.DmaKind.Ignore, ProcessEtwReader.ClassifyDma("DmaPacket"));
}
