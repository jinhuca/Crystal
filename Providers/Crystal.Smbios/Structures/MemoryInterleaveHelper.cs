using Crystal.Smbios.Types;
using System;
using System.Collections.Generic;

namespace Crystal.Smbios.Structures;

/// <summary>
/// Helpers to compute usable address spans for interleaved memory regions
/// described by Type 19 / Type 20 SMBIOS structures.
/// </summary>
public static class MemoryInterleaveHelper
{
    /// <summary>
    /// Compute the list of contiguous byte ranges that belong to a single
    /// partition within an interleaved mapped-address region.
    /// </summary>
    /// <param name="startBytes">Inclusive start address in bytes.</param>
    /// <param name="endBytes">Inclusive end address in bytes.</param>
    /// <param name="partitionWidth">Number of partitions (interleave width).</param>
    /// <param name="interleavePosition">1-based position of the partition within the interleave set.</param>
    /// <param name="interleaveGranularityBytes">Interleave granularity in bytes. When null, the region cannot be split deterministically and a single span covering the full range is returned.</param>
    /// <returns>Sequence of (offset, length) pairs describing the usable spans for the specified partition.</returns>
    public static IEnumerable<(ulong Offset, ulong Length)> ComputeInterleavedSegments(
        ulong startBytes,
        ulong endBytes,
        int partitionWidth,
        int interleavePosition,
        ulong? interleaveGranularityBytes)
    {
        if (endBytes < startBytes)
            yield break;

        if (partitionWidth <= 1)
        {
            // Not interleaved — return single contiguous range.
            yield return (startBytes, endBytes - startBytes + 1UL);
            yield break;
        }

        if (interleavePosition < 1 || interleavePosition > partitionWidth)
            throw new ArgumentOutOfRangeException(nameof(interleavePosition), "Interleave position must be 1-based and <= partitionWidth");

        if (!interleaveGranularityBytes.HasValue || interleaveGranularityBytes.Value == 0UL)
        {
            // Without a granularity we cannot split reliably; return the whole range.
            yield return (startBytes, endBytes - startBytes + 1UL);
            yield break;
        }

        ulong g = interleaveGranularityBytes.Value;
        // The per-chunk stride is granularity * partitionWidth.
        // For partition position p (1-based), the first segment begins at start + (p-1)*g
        // and then repeats every (g * partitionWidth) bytes.
        checked
        {
            ulong stride = g * (ulong)partitionWidth;
            ulong firstStart = startBytes + (ulong)(interleavePosition - 1) * g;

            // Align firstStart to the first stripe that intersects the [startBytes,endBytes] window.
            // If firstStart < startBytes, advance by multiples of stride.
            if (firstStart < startBytes)
            {
                ulong delta = startBytes - firstStart;
                ulong advance = (delta + stride - 1UL) / stride; // ceil
                firstStart += advance * stride;
            }

            for (ulong segStart = firstStart; segStart <= endBytes; segStart += stride)
            {
                // Each segment is at most 'g' bytes, but may be truncated by endBytes.
                ulong segOffset = segStart;
                if (segOffset < startBytes) segOffset = startBytes; // safety
                ulong available = endBytes - segOffset + 1UL;
                ulong segLen = available >= g ? g : available;
                yield return (segOffset, segLen);
                // prevent infinite loop if stride is 0 (shouldn't happen because partitionWidth>1 and g>0)
                if (stride == 0) break;
            }
        }
    }

}
