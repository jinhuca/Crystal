Memory Interleave Helper — usage examples

This file documents how to compute usable address spans for interleaved
memory regions described by SMBIOS Type 19 (Memory Array Mapped Address)
and Type 20 (Memory Device Mapped Address).

Background
----------

SMBIOS can describe address ranges that are interleaved across multiple
partitions (banks). The important fields are:

- Start/End addresses (legacy DWORDs in KiB or extended QWORDs in bytes)
- PartitionWidth — number of interleave partitions
- InterleavePosition — 1-based position of this partition within the set
- InterleaveGranularityBytes — bundle size used by the interleave

Compute segments
----------------

Use MemoryInterleaveHelper.ComputeInterleavedSegments to compute the per-partition
segments. Example:

```csharp
var segments = MemoryInterleaveHelper.ComputeInterleavedSegments(
    map.StartAddressBytes, // inclusive start in bytes
    map.EndAddressBytes,   // inclusive end in bytes
    partitionWidth,        // number of partitions
    interleavePosition,    // 1-based position for this partition
    map.InterleaveGranularityBytes);

foreach (var (offset, length) in segments)
{
    Console.WriteLine($"Segment: {offset:X16} length={length}");
}
```

Notes
-----

- If InterleaveGranularityBytes is not present the helper returns a single
  span covering the full range because the layout cannot be split deterministically.
- The helper returns byte offsets and lengths — convert to KiB/GiB for display.
