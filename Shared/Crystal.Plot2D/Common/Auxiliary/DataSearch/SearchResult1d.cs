namespace Crystal.Plot2D.Common.Auxiliary.DataSearch;

internal struct SearchResult1d {
  public static SearchResult1d Empty => new() { Index = -1 };

  public int Index { get; internal set; }

  public bool IsEmpty => Index == -1;

  public override string ToString() => IsEmpty ? "Empty" : $"Index = {Index}";
}
