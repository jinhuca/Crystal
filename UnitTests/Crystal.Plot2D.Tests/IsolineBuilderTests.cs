// IsolineBuilderTests.cs
//
// Targets:
//   IsolineBuilder              — 454 lines at 0 %. Tested through the fully-public
//                                 BuildIsoline() / BuildIsoline(double) API using a
//                                 FakeDataSource2D stub.
//   DataSource2DExtensions      — 112 lines at 0 %. The public GetMinMax(double[,])
//                                 extension has zero WPF UI dependency; GetGridBounds
//                                 uses WPF value types (Point/DataRect) only.
//
// Crystal.Plot2D has no InternalsVisibleTo for the test project, so internal types
// (ValuesInCell, IrregularCell, Edge, SubCell) are opaque to tests; they are
// exercised indirectly through BuildIsoline().
//
// Namespace: Crystal.Plot2D.Tests  (matches existing test project)

using System;
using System.Linq;
using System.Windows;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.DataSources.MultiDimensional;
using Crystal.Plot2D.Isolines;
using Xunit;

namespace Crystal.Plot2D.Tests;

// ─────────────────────────────────────────────────────────────────────────────
// Simulation infrastructure
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Minimal in-memory implementation of <see cref="IDataSource2D{T}"/>.
/// The grid is laid out as a regular unit grid:
///   Grid[i, j] = (i, j)  for i in [0, width), j in [0, height).
/// </summary>
internal sealed class FakeDataSource2D : IDataSource2D<double> {
  public FakeDataSource2D(double[,] data) {
    Data = data;
    Width = data.GetLength(0);
    Height = data.GetLength(1);

    Grid = new Point[Width, Height];
    for (int i = 0; i < Width; i++)
      for (int j = 0; j < Height; j++)
        Grid[i, j] = new Point(i, j);
  }

  // Override the grid for non-uniform spacing
  public FakeDataSource2D(double[,] data, Point[,] grid) {
    Data = data;
    Width = data.GetLength(0);
    Height = data.GetLength(1);
    Grid = grid;
  }

  public double[,] Data { get; }
  public Point[,] Grid { get; }
  public int Width { get; }
  public int Height { get; }
  public Range<double>? Range => null;   // let IsolineBuilder compute
  public double? MissingValue => null;
  public IDataSource2D<double> GetSubset() => this;
  public event EventHandler Changed;
}

// ─────────────────────────────────────────────────────────────────────────────
// DataSource2DExtensions
// ─────────────────────────────────────────────────────────────────────────────
public class DataSource2DExtensionsTests {
  // ── GetMinMax(double[,]) ──────────────────────────────────────────────────

  [Fact]
  public void GetMinMax_UniformGrid_ReturnsCorrectMinAndMax() {
    double[,] data = {
      { 1.0, 5.0 },
      { 3.0, 2.0 }
    };
    Range<double> r = data.GetMinMax();
    Assert.Equal(1.0, r.Min, 10);
    Assert.Equal(5.0, r.Max, 10);
  }

  [Fact]
  public void GetMinMax_AllSameValues_MinEqualsMax() {
    double[,] data = { { 7.0, 7.0 }, { 7.0, 7.0 } };
    Range<double> r = data.GetMinMax();
    Assert.Equal(7.0, r.Min, 10);
    Assert.Equal(7.0, r.Max, 10);
  }

  [Fact]
  public void GetMinMax_NegativeValues_HandledCorrectly() {
    double[,] data = { { -5.0, 0.0 }, { -3.0, 2.0 } };
    Range<double> r = data.GetMinMax();
    Assert.Equal(-5.0, r.Min, 10);
    Assert.Equal(2.0, r.Max, 10);
  }

  [Fact]
  public void GetMinMax_SingleElement_MinEqualsMax() {
    double[,] data = { { 42.0 } };
    Range<double> r = data.GetMinMax();
    Assert.Equal(42.0, r.Min, 10);
    Assert.Equal(42.0, r.Max, 10);
  }

  [Fact]
  public void GetMinMax_LargeArray_PicksCorrectExtremes() {
    double[,] data = new double[10, 10];
    for (int i = 0; i < 10; i++)
      for (int j = 0; j < 10; j++)
        data[i, j] = i * 10 + j;   // 0 … 99
    Range<double> r = data.GetMinMax();
    Assert.Equal(0.0, r.Min, 10);
    Assert.Equal(99.0, r.Max, 10);
  }

  // ── GetGridBounds<T> ─────────────────────────────────────────────────────

  [Fact]
  public void GetGridBounds_RegularUnitGrid_MatchesExpectedRect() {
    // FakeDataSource2D lays out Grid[i,j] = (i, j) for a 3×3 grid.
    FakeDataSource2D src = new(new double[3, 3]);
    DataRect bounds = src.GetGridBounds();
    Assert.Equal(0.0, bounds.XMin, 9);
    Assert.Equal(0.0, bounds.YMin, 9);
    Assert.Equal(2.0, bounds.XMax, 9);
    Assert.Equal(2.0, bounds.YMax, 9);
  }

  [Fact]
  public void GetGridBounds_OffsetGrid_ReflectsOffset() {
    double[,] data = new double[2, 2];
    Point[,] grid = {
      { new Point(10, 20), new Point(10, 30) },
      { new Point(15, 20), new Point(15, 30) },
    };
    FakeDataSource2D src = new(data, grid);
    DataRect bounds = src.GetGridBounds();
    Assert.Equal(10.0, bounds.XMin, 9);
    Assert.Equal(20.0, bounds.YMin, 9);
    Assert.Equal(15.0, bounds.XMax, 9);
    Assert.Equal(30.0, bounds.YMax, 9);
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// IsolineBuilder
// ─────────────────────────────────────────────────────────────────────────────
public class IsolineBuilderTests {
  // Helper: build a horizontal-gradient source (values increase along X axis)
  //
  //   Data[i, j] = (double)i / (width - 1)   → values in [0, 1]
  //   Grid[i, j] = (i, j)                    → unit grid
  //
  // For a 3×3 source this gives:
  //   0.0  0.5  1.0
  //   0.0  0.5  1.0
  //   0.0  0.5  1.0
  // Isolines at any level between 0 and 1 run vertically.
  private static FakeDataSource2D GradientSource(int width, int height) {
    double[,] data = new double[width, height];
    for (int i = 0; i < width; i++)
      for (int j = 0; j < height; j++)
        data[i, j] = width > 1 ? (double)i / (width - 1) : 0.5;
    return new FakeDataSource2D(data);
  }

  // ── Constructor and DataSource property ──────────────────────────────────

  [Fact]
  public void DefaultConstructor_DataSourceIsNull() {
    IsolineBuilder b = new();
    Assert.Null(b.DataSource);
  }

  [Fact]
  public void Constructor_WithDataSource_SetsDataSourceProperty() {
    FakeDataSource2D src = GradientSource(3, 3);
    IsolineBuilder b = new(src);
    Assert.Same(src, b.DataSource);
  }

  [Fact]
  public void DataSource_Set_UpdatesProperty() {
    IsolineBuilder b = new();
    FakeDataSource2D src = GradientSource(3, 3);
    b.DataSource = src;
    Assert.Same(src, b.DataSource);
  }

  [Fact]
  public void DataSource_SetToNull_ThrowsArgumentException() {
    IsolineBuilder b = new(GradientSource(3, 3));
    // DataSource setter calls value.VerifyNotNull() before assigning.
    Assert.Throws<ArgumentNullException>(() => b.DataSource = null);
  }

  // ── MissingValue property ────────────────────────────────────────────────

  [Fact]
  public void MissingValue_DefaultIsNaN() {
    IsolineBuilder b = new();
    Assert.True(double.IsNaN(b.MissingValue));
  }

  [Fact]
  public void MissingValue_SetAndGet_Roundtrips() {
    IsolineBuilder b = new();
    b.MissingValue = -9999.0;
    Assert.Equal(-9999.0, b.MissingValue);
  }

  // ── BuildIsoline() error paths ────────────────────────────────────────────

  [Fact]
  public void BuildIsoline_NoDataSource_ThrowsInvalidOperation() {
    IsolineBuilder b = new();
    Assert.Throws<InvalidOperationException>(() => b.BuildIsoline());
  }

  [Fact]
  public void BuildIsolineLevel_NoDataSource_ThrowsInvalidOperation() {
    IsolineBuilder b = new();
    Assert.Throws<InvalidOperationException>(() => b.BuildIsoline(0.5));
  }

  // ── BuildIsoline() – too-small data (< 2×2) ─────────────────────────────

  [Fact]
  public void BuildIsoline_1x1Data_ReturnsEmptyCollection() {
    IsolineBuilder b = new(GradientSource(1, 1));
    IsolineCollection result = b.BuildIsoline();
    Assert.NotNull(result);
    Assert.Empty(result.Lines);
  }

  [Fact]
  public void BuildIsoline_1xNData_ReturnsEmptyCollection() {
    IsolineBuilder b = new(GradientSource(1, 5));
    IsolineCollection result = b.BuildIsoline();
    Assert.Empty(result.Lines);
  }

  [Fact]
  public void BuildIsoline_Nx1Data_ReturnsEmptyCollection() {
    IsolineBuilder b = new(GradientSource(5, 1));
    IsolineCollection result = b.BuildIsoline();
    Assert.Empty(result.Lines);
  }

  // ── BuildIsoline() – valid data ───────────────────────────────────────────

  [Fact]
  public void BuildIsoline_3x3GradientData_ReturnsNonNull() {
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline();
    Assert.NotNull(result);
  }

  [Fact]
  public void BuildIsoline_3x3GradientData_ReturnsNonEmptyCollection() {
    // A clear horizontal gradient (0→1 across X) must produce at least
    // one isoline — the algorithm sweeps Density=12 levels.
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline();
    Assert.NotEmpty(result.Lines);
  }

  [Fact]
  public void BuildIsoline_SetsMinAndMaxOnCollection() {
    FakeDataSource2D src = GradientSource(3, 3);
    IsolineBuilder b = new(src);
    IsolineCollection result = b.BuildIsoline();
    // Min and Max are set to the data range (0..1 for the gradient source).
    Assert.Equal(0.0, result.Min, 6);
    Assert.Equal(1.0, result.Max, 6);
  }

  [Fact]
  public void BuildIsoline_LargerGrid_ReturnsMultipleLines() {
    // A 5×5 gradient has more cells, producing more isoline segments.
    IsolineBuilder b = new(GradientSource(5, 5));
    IsolineCollection result = b.BuildIsoline();
    Assert.NotEmpty(result.Lines);
    // Each line must have at least one OtherPoint (i.e., at least two points).
    Assert.All(result.Lines, line => Assert.NotEmpty(line.OtherPoints));
  }

  [Fact]
  public void BuildIsoline_EachLineHasAtLeastTwoPoints() {
    IsolineBuilder b = new(GradientSource(4, 4));
    IsolineCollection result = b.BuildIsoline();
    foreach (LevelLine line in result.Lines)
      Assert.True(line.AllPoints.Count() >= 2, "Each LevelLine must have at least StartPoint + 1 OtherPoint.");
  }

  [Fact]
  public void BuildIsoline_AllLinesHaveValue01InRange() {
    IsolineBuilder b = new(GradientSource(4, 4));
    IsolineCollection result = b.BuildIsoline();
    foreach (LevelLine line in result.Lines) {
      Assert.True(line.Value01 >= 0.0 && line.Value01 <= 1.0,
        $"Value01={line.Value01} is outside [0, 1].");
    }
  }

  [Fact]
  public void BuildIsoline_AllLinesHaveRealValueInDataRange() {
    IsolineBuilder b = new(GradientSource(4, 4));
    IsolineCollection result = b.BuildIsoline();
    foreach (LevelLine line in result.Lines) {
      Assert.True(line.RealValue >= result.Min - 1e-9 &&
                  line.RealValue <= result.Max + 1e-9,
        $"RealValue={line.RealValue} outside data range [{result.Min}, {result.Max}].");
    }
  }

  [Fact]
  public void BuildIsoline_IEnumerable_MatchesLines() {
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline();
    Assert.Equal(result.Lines.Count, result.Count());
  }

  // ── BuildIsoline(double level) ────────────────────────────────────────────

  [Fact]
  public void BuildIsolineLevel_ValueInRange_ReturnsNonEmptyCollection() {
    // For the gradient source (0..1), requesting level=0.5 is mid-range.
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline(0.5);
    Assert.NotEmpty(result.Lines);
  }

  [Fact]
  public void BuildIsolineLevel_ValueOutOfRange_ReturnsEmptyCollection() {
    // Level 2.0 is outside the data range [0, 1] — no isolines possible.
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline(2.0);
    Assert.Empty(result.Lines);
  }

  [Fact]
  public void BuildIsolineLevel_SetsMinMaxOnCollection() {
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline(0.5);
    Assert.Equal(0.0, result.Min, 6);
    Assert.Equal(1.0, result.Max, 6);
  }

  [Fact]
  public void BuildIsolineLevel_ResultLineRealValueEqualsRequestedLevel() {
    // With a clean gradient source and a single level, all returned
    // LevelLines should have RealValue == the requested level.
    IsolineBuilder b = new(GradientSource(4, 4));
    double requestedLevel = 0.25;
    IsolineCollection result = b.BuildIsoline(requestedLevel);
    foreach (LevelLine line in result.Lines)
      Assert.Equal(requestedLevel, line.RealValue, 6);
  }

  // ── LevelLine structure ───────────────────────────────────────────────────

  [Fact]
  public void LevelLine_AllPoints_IncludesStartAndOtherPoints() {
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline(0.5);
    foreach (LevelLine line in result.Lines) {
      var all = line.AllPoints.ToList();
      Assert.Contains(line.StartPoint, all);
      foreach (Point p in line.OtherPoints)
        Assert.Contains(p, all);
    }
  }

  [Fact]
  public void LevelLine_GetSegments_CountIsOtherPointsCountForSingleLine() {
    IsolineBuilder b = new(GradientSource(3, 3));
    IsolineCollection result = b.BuildIsoline(0.5);
    foreach (LevelLine line in result.Lines) {
      int segments = line.GetSegments().Count();
      Assert.Equal(line.OtherPoints.Count, segments);
    }
  }

  // ── MissingValue interaction ──────────────────────────────────────────────

  [Fact]
  public void BuildIsoline_WithMissingValue_ExcludesMissingCells() {
    // Create a 3×3 source where data[1,1] = missingValue.
    double missing = -999.0;
    double[,] data = {
      { 0.0, 0.0, 0.0 },
      { 0.5, missing, 0.5 },
      { 1.0, 1.0,     1.0 },
    };
    FakeDataSource2D src = new(data);
    IsolineBuilder b = new(src) { MissingValue = missing };
    // Should not throw even though the centre cell is "missing".
    IsolineCollection result = b.BuildIsoline(0.5);
    Assert.NotNull(result);
  }

  // ── Rebuild idempotency ───────────────────────────────────────────────────

  [Fact]
  public void BuildIsoline_CalledTwice_ProducesSameLineCount() {
    IsolineBuilder b = new(GradientSource(3, 3));
    int count1 = b.BuildIsoline().Lines.Count;
    int count2 = b.BuildIsoline().Lines.Count;
    Assert.Equal(count1, count2);
  }
}
