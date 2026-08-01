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
/// modulo operation per access and no heap allocation at all.
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
    int writeIndex = (_head + Count) % Capacity;
    _buffer[writeIndex] = value;

    if (Count < Capacity) {
      Count++;
    } else {
      _head = (_head + 1) % Capacity; // evict the old oldest — the one we just overwrote
    }
  }

  /// <summary>Removes all buffered values.</summary>
  public void Clear() {
    Array.Clear(_buffer, 0, _buffer.Length);
    _head = 0;
    Count = 0;
  }

  /// <summary>Gets the value at <paramref name="index"/> — 0 is the oldest, Count - 1 is the newest. O(1).</summary>
  public T this[int index] {
    get {
      if ((uint)index >= (uint)Count) throw new ArgumentOutOfRangeException(nameof(index));
      return _buffer[(_head + index) % Capacity];
    }
  }
}
