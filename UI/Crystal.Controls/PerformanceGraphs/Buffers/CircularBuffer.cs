using System;

namespace Crystal.Controls.PerformanceGraphs.Buffers;

/// <summary>
/// A fixed-capacity, oldest-first sample buffer: <see cref="Add"/> appends a new value,
/// automatically dropping the oldest one once <see cref="Capacity"/> is reached.
/// </summary>
/// <remarks>
/// True head/tail ring buffer: <see cref="Add"/> and the indexer are both O(1), with no
/// shifting of existing elements. The cost of that is <see cref="this"/> being the only way
/// to read the data — once the buffer has wrapped, the samples are not contiguous in the
/// backing array, so there is no way to hand out a single <c>ReadOnlySpan&lt;T&gt;</c> over
/// them without copying (which would be a per-read allocation, or a persistent scratch
/// buffer that then needs its own bookkeeping). Consumers that need to walk every element —
/// like <see cref="Kinds.FilledLineRenderer"/> — do so via the indexer, which costs one
/// branch-and-subtract per access (see its own remarks) and no heap allocation at all.
/// </remarks>
public sealed class CircularBuffer<T> {
  private readonly T[] _buffer;
  private int _head; // index of the oldest element

  public CircularBuffer(int capacity) {
    if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be positive.");
    Capacity = capacity;
    _buffer = new T[capacity];
  }

  /// <summary>Maximum number of values retained.</summary>
  public int Capacity { get; }

  /// <summary>Number of values currently buffered (0..Capacity).</summary>
  public int Count { get; private set; }

  /// <summary>Appends a value, dropping the oldest one first once the buffer is full. O(1).</summary>
  public void Add(T value) {
    // When full this lands exactly on _head (the oldest slot, about to be overwritten);
    // when not full it's the next free slot. Same formula either way.
    // _head and Count are both < Capacity, so their sum is < 2 x Capacity — a single
    // conditional subtraction wraps it, same result as % Capacity without the idiv.
    int writeIndex = _head + Count;
    if (writeIndex >= Capacity) writeIndex -= Capacity;
    _buffer[writeIndex] = value;

    if (Count < Capacity) {
      Count++;
    } else {
      // Evict the old oldest — the one we just overwrote. _head + 1 <= Capacity, so a single
      // equality check covers the wrap (no case ever needs a full subtraction here).
      _head = _head + 1 == Capacity ? 0 : _head + 1;
    }
  }

  /// <summary>Removes all buffered values.</summary>
  public void Clear() {
    Array.Clear(_buffer, 0, _buffer.Length);
    _head = 0;
    Count = 0;
  }

  /// <summary>Gets the value at <paramref name="index"/> — 0 is the oldest, Count - 1 is the newest. O(1).</summary>
  /// <remarks>
  /// This is the hottest path in the whole buffer — every renderer (FilledLineRenderer,
  /// BarRenderer, SegmentedBarRenderer, DotRenderer, and PerformanceGraphLite's own render pass)
  /// walks every buffered sample once per frame through this indexer. <c>_head</c> and
  /// <c>index</c> are both already less than <see cref="Capacity"/>, so their sum is always less
  /// than <c>2 x Capacity</c> — one conditional subtraction wraps it, the same result as
  /// <c>% Capacity</c> without the idiv a modulo compiles to.
  /// </remarks>
  public T this[int index] {
    get {
      if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
      int i = _head + index;
      if (i >= Capacity) i -= Capacity;
      return _buffer[i];
    }
  }
}
