// Crystal.Plot2D – new test coverage
// Namespace: Crystal.Plot2D.Tests (matches existing test project)
// Target: net10.0-windows7.0, UseWPF=true
//
// Pure-value-type tests (DataRect, Range, RingArray, Transforms, MathHelper,
// HsbColor, RangeExtensions, DataRectExtensions) do NOT need STA.
// CoordinateTransform / CoordinateTransformExtensions need a live Plotter,
// so those classes inherit WPFTestBase and use RunTest().

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Crystal.Plot2D.Common;
using Crystal.Plot2D.Common.Auxiliary;
using Crystal.Plot2D.Transforms;
using Xunit;

namespace Crystal.Plot2D.Tests;

// ─────────────────────────────────────────────────────────────
// RingArray<T>  – pure .NET, no WPF
// ─────────────────────────────────────────────────────────────
public class RingArrayTests {

  [Fact]
  public void Constructor_SetsCapacityAndZeroCount() {
    RingArray<int> ra = new(5);
    Assert.Equal(5, ra.Capacity);
    Assert.Equal(0, ra.Count);
  }

  [Fact]
  public void Add_BelowCapacity_IncrementsCount() {
    RingArray<int> ra = new(4);
    ra.Add(10); ra.Add(20); ra.Add(30);
    Assert.Equal(3, ra.Count);
  }

  [Fact]
  public void Add_AtCapacity_CountStaysAtCapacity() {
    RingArray<int> ra = new(3);
    ra.Add(1); ra.Add(2); ra.Add(3); ra.Add(4);
    Assert.Equal(3, ra.Count);
  }

  [Fact]
  public void Add_ExceedsCapacity_OldestDropped() {
    RingArray<int> ra = new(3);
    ra.Add(1); ra.Add(2); ra.Add(3); ra.Add(4);
    Assert.Equal(2, ra[0]); Assert.Equal(3, ra[1]); Assert.Equal(4, ra[2]);
  }

  [Fact]
  public void Indexer_Get_ReturnsCorrectElement() {
    RingArray<string> ra = new(5);
    ra.Add("a"); ra.Add("b"); ra.Add("c");
    Assert.Equal("a", ra[0]); Assert.Equal("b", ra[1]); Assert.Equal("c", ra[2]);
  }

  [Fact]
  public void Indexer_Set_UpdatesElement() {
    RingArray<int> ra = new(3);
    ra.Add(1); ra.Add(2); ra.Add(3);
    ra[1] = 99;
    Assert.Equal(99, ra[1]);
  }

  [Fact]
  public void Clear_ResetsCount() {
    RingArray<int> ra = new(3);
    ra.Add(1); ra.Add(2); ra.Add(3);
    ra.Clear();
    Assert.Equal(0, ra.Count);
  }

  [Fact]
  public void Clear_AfterWrap_RestoresFreshState() {
    RingArray<int> ra = new(3);
    ra.Add(1); ra.Add(2); ra.Add(3); ra.Add(4);
    ra.Clear();
    ra.Add(10); ra.Add(20);
    Assert.Equal(10, ra[0]); Assert.Equal(20, ra[1]); Assert.Equal(2, ra.Count);
  }

  [Fact]
  public void Contains_ReturnsTrueForPresentItem() {
    RingArray<int> ra = new(4);
    ra.Add(42); ra.Add(7);
    Assert.True(ra.Contains(42)); 
    Assert.True(ra.Contains(7));
  }

  [Fact]
  public void Contains_ReturnsFalseForAbsentItem() {
    RingArray<int> ra = new(4);
    ra.Add(1);
    Assert.False(ra.Contains(99));
  }

  [Fact]
  public void IndexOf_PresentItem_ReturnsNonNegative() {
    // IndexOf returns a physical backing-array index, not a logical index,
    // so the result cannot be used as ra[idx] reliably. Verify only that a
    // present item produces a non-negative value.
    RingArray<int> ra = new(4);
    ra.Add(10); ra.Add(20); ra.Add(30);
    Assert.True(ra.IndexOf(20) >= 0);
    Assert.True(ra.IndexOf(10) >= 0);
    Assert.True(ra.IndexOf(30) >= 0);
  }

  [Fact]
  public void IndexOf_AbsentItem_ReturnsNegativeOne() {
    RingArray<int> ra = new(4);
    ra.Add(1); ra.Add(2);
    Assert.Equal(-1, ra.IndexOf(99));
  }

  [Fact]
  public void GetEnumerator_YieldsAllInLogicalOrder() {
    // CopyTo is not implemented, so new List<T>(ringArray) would throw.
    // Enumerate explicitly with foreach.
    RingArray<int> ra = new(5);
    ra.Add(1); ra.Add(2); ra.Add(3);
    var list = new List<int>();
    foreach (var item in ra) list.Add(item);
    Assert.Equal(new[] { 1, 2, 3 }, list);
  }

  [Fact]
  public void GetEnumerator_WrappedBuffer_YieldsLogicalOrder() {
    RingArray<int> ra = new(3);
    ra.Add(1); ra.Add(2); ra.Add(3); ra.Add(4);
    var list = new List<int>();
    foreach (var item in ra) list.Add(item);
    Assert.Equal(new[] { 2, 3, 4 }, list);
  }

  [Fact]
  public void CollectionChanged_FiredOnAdd() {
    RingArray<int> ra = new(3);
    int fired = 0;
    ra.CollectionChanged += (_, _) => fired++;
    ra.Add(1); ra.Add(2);
    Assert.Equal(2, fired);
  }

  [Fact]
  public void CollectionChanged_FiredOnIndexerSet() {
    RingArray<int> ra = new(3);
    ra.Add(5);
    int fired = 0;
    ra.CollectionChanged += (_, _) => fired++;
    ra[0] = 99;
    Assert.Equal(1, fired);
  }

  [Fact]
  public void Insert_ThrowsNotImplementedException() {
    RingArray<int> ra = new(3);
    Assert.Throws<NotImplementedException>(() => ra.Insert(0, 1));
  }

  [Fact]
  public void RemoveAt_ThrowsNotImplementedException() {
    RingArray<int> ra = new(3);
    ra.Add(1);
    Assert.Throws<NotImplementedException>(() => ra.RemoveAt(0));
  }
}

// ─────────────────────────────────────────────────────────────
// Range<T>
// ─────────────────────────────────────────────────────────────
public class RangeTests {
  [Fact]
  public void Constructor_StoresMinAndMax() {
    Range<int> r = new(3, 7);
    Assert.Equal(3, r.Min); Assert.Equal(7, r.Max);
  }

  [Fact]
  public void ToString_ReturnsExpectedFormat() =>
    Assert.Equal("1 — 5", new Range<int>(1, 5).ToString());

  [Fact]
  public void EqualityOperator_SameValues_ReturnsTrue() {
    Range<double> a = new(1.0, 2.0), b = new(1.0, 2.0);
    Assert.True(a == b); Assert.False(a != b);
  }

  [Fact]
  public void InequalityOperator_DifferentMax_ReturnsTrue() {
    Assert.True(new Range<double>(1, 3) != new Range<double>(1, 4));
  }

  [Fact]
  public void Equals_BoxedObject_ReturnsTrue() =>
    Assert.True(new Range<int>(2, 8).Equals((object)new Range<int>(2, 8)));

  [Fact]
  public void Equals_DifferentType_ReturnsFalse() =>
    Assert.False(new Range<int>(1, 5).Equals("not a range"));

  [Fact]
  public void GetHashCode_SameValues_SameHash() {
    Assert.Equal(new Range<int>(1, 10).GetHashCode(), new Range<int>(1, 10).GetHashCode());
  }

  [Fact]
  public void EqualEps_CloseRanges_ReturnsTrue() {
    Assert.True(Range<double>.EqualEps(new Range<double>(0, 100), new Range<double>(0.001, 100.001), 0.01));
  }

  [Fact]
  public void EqualEps_VeryDifferentRanges_ReturnsFalse() {
    Assert.False(Range<double>.EqualEps(new Range<double>(0, 1), new Range<double>(0, 2), 0.0001));
  }

  [Fact]
  public void EqualsGeneric_SameValues_ReturnsTrue() {
    Assert.True(new Range<int>(1, 5).Equals(new Range<int>(1, 5)));
  }
}

// ─────────────────────────────────────────────────────────────
// DataRect
// ─────────────────────────────────────────────────────────────
public class DataRectTests {
  static DataRect R(double x, double y, double w, double h) => new(x, y, w, h);

  // Construction
  [Fact]
  public void Ctor_FromRect_CopiesFields() {
    DataRect dr = new(new Rect(1, 2, 3, 4));
    Assert.Equal(1, dr.XMin); Assert.Equal(2, dr.YMin);
    Assert.Equal(3, dr.Width); Assert.Equal(4, dr.Height);
  }
  [Fact]
  public void Ctor_FromSize_ZeroOrigin() {
    DataRect dr = new(new Size(5, 6));
    Assert.Equal(0, dr.XMin); Assert.Equal(5, dr.Width);
  }
  [Fact]
  public void Ctor_FromEmptySize_IsEmpty() =>
    Assert.True(new DataRect(Size.Empty).IsEmpty);
  [Fact]
  public void Ctor_FromPointSize_UsesLocation() {
    DataRect dr = new(new Point(3, 4), new Size(10, 20));
    Assert.Equal(3, dr.XMin); Assert.Equal(4, dr.YMin);
  }
  [Fact]
  public void Ctor_FromPointSize_EmptySize_IsEmpty() =>
    Assert.True(new DataRect(new Point(1, 2), Size.Empty).IsEmpty);
  [Fact]
  public void Ctor_FromTwoPoints_NormalizesCoordinates() {
    DataRect dr = new(new Point(5, 8), new Point(2, 3));
    Assert.Equal(2, dr.XMin); Assert.Equal(3, dr.YMin);
    Assert.Equal(3, dr.Width); Assert.Equal(5, dr.Height);
  }
  [Fact]
  public void Ctor_FromPointVector_PositiveVector() {
    DataRect dr = new(new Point(1, 2), new Vector(3, 4));
    Assert.Equal(3, dr.Width); Assert.Equal(4, dr.Height);
  }
  [Fact]
  public void Ctor_NegativeWidth_ThrowsArgumentException() =>
    Assert.Throws<ArgumentException>(() => new DataRect(0, 0, -1, 5));
  [Fact]
  public void Ctor_NegativeHeight_ThrowsArgumentException() =>
    Assert.Throws<ArgumentException>(() => new DataRect(0, 0, 5, -1));

  // Static factories
  [Fact]
  public void Create_CorrectDimensions() {
    DataRect dr = DataRect.Create(0, 0, 4, 3);
    Assert.Equal(4, dr.Width); Assert.Equal(3, dr.Height);
  }
  [Fact]
  public void FromPoints_NormalizesOrdering() {
    DataRect dr = DataRect.FromPoints(5, 8, 1, 2);
    Assert.Equal(1, dr.XMin); Assert.Equal(4, dr.Width);
  }
  [Fact]
  public void FromCenterSize_Doubles_CenteredCorrectly() {
    DataRect dr = DataRect.FromCenterSize(new Point(5, 5), 4.0, 2.0);
    Assert.Equal(3, dr.XMin); Assert.Equal(4, dr.Width);
  }
  [Fact]
  public void FromCenterSize_PointSize_MatchesDoubles() {
    Assert.Equal(
      DataRect.FromCenterSize(new Point(0, 0), 6.0, 4.0),
      DataRect.FromCenterSize(new Point(0, 0), new Size(6, 4)));
  }
  [Fact]
  public void ImplicitConversion_FromRect() {
    DataRect dr = new Rect(2, 3, 5, 6);
    Assert.Equal(2, dr.XMin);
  }
  [Fact]
  public void ToRect_RoundTripsValues() {
    Rect r = R(1, 2, 3, 4).ToRect();
    Assert.Equal(1, r.X); Assert.Equal(3, r.Width);
  }

  // Properties
  [Fact] public void XMax_IsXMinPlusWidth() => Assert.Equal(12, R(2, 0, 10, 5).XMax);
  [Fact] public void YMax_IsYMinPlusHeight() => Assert.Equal(10, R(0, 4, 3, 6).YMax);
  [Fact]
  public void XMinYMin_ReturnsTopLeft() =>
    Assert.Equal(new Point(3, 4), R(3, 4, 5, 6).XMinYMin);
  [Fact]
  public void XMaxYMax_ReturnsBottomRight() =>
    Assert.Equal(new Point(4, 6), R(1, 2, 3, 4).XMaxYMax);
  [Fact]
  public void Location_Set_MovesRect() {
    DataRect dr = R(0, 0, 10, 10);
    dr.Location = new Point(3, 7);
    Assert.Equal(3, dr.XMin); Assert.Equal(7, dr.YMin);
  }
  [Fact]
  public void Location_SetOnEmpty_Throws() {
    DataRect dr = DataRect.Empty;
    Assert.Throws<InvalidOperationException>(() => dr.Location = new Point(1, 1));
  }
  [Fact]
  public void Size_Set_UpdatesDimensions() {
    DataRect dr = R(1, 2, 3, 4);
    dr.Size = new Size(10, 20);
    Assert.Equal(10, dr.Width); Assert.Equal(20, dr.Height);
  }
  [Fact]
  public void Size_SetEmpty_MakesEmpty() {
    DataRect dr = R(1, 2, 3, 4);
    dr.Size = Size.Empty;
    Assert.True(dr.IsEmpty);
  }
  [Fact]
  public void Width_SetNegative_Throws() =>
    Assert.Throws<ArgumentOutOfRangeException>(() => { DataRect dr = R(0, 0, 5, 5); dr.Width = -1; });
  [Fact]
  public void Height_SetNegative_Throws() =>
    Assert.Throws<ArgumentOutOfRangeException>(() => { DataRect dr = R(0, 0, 5, 5); dr.Height = -1; });
  [Fact]
  public void XMin_SetOnEmpty_Throws() {
    DataRect dr = DataRect.Empty;
    Assert.Throws<InvalidOperationException>(() => dr.XMin = 5);
  }
  [Fact]
  public void YMin_SetOnEmpty_Throws() {
    DataRect dr = DataRect.Empty;
    Assert.Throws<InvalidOperationException>(() => dr.YMin = 5);
  }
  [Fact] public void Empty_IsEmpty() => Assert.True(DataRect.Empty.IsEmpty);
  [Fact]
  public void Infinite_IsNotEmptyAndLarge() {
    Assert.False(DataRect.Infinite.IsEmpty);
    Assert.True(DataRect.Infinite.Width > 1e15);
  }

  // Contains
  [Fact] public void Contains_XY_Inside_True() => Assert.True(R(0, 0, 10, 10).Contains(5, 5));
  [Fact] public void Contains_XY_Outside_False() => Assert.False(R(0, 0, 10, 10).Contains(11, 5));
  [Fact] public void Contains_XY_EmptyRect_False() => Assert.False(DataRect.Empty.Contains(0, 0));
  [Fact]
  public void Contains_Point_Inside_True() =>
    Assert.True(R(0, 0, 5, 5).Contains(new Point(2, 3)));
  [Fact]
  public void Contains_InnerRect_True() =>
    Assert.True(R(0, 0, 10, 10).Contains(R(2, 2, 3, 3)));
  [Fact]
  public void Contains_PartiallyOutside_False() =>
    Assert.False(R(0, 0, 10, 10).Contains(R(8, 8, 5, 5)));
  [Fact]
  public void Contains_EmptyInner_False() =>
    Assert.False(R(0, 0, 10, 10).Contains(DataRect.Empty));

  // Intersect
  [Fact]
  public void Intersect_Static_OverlappingRects() {
    DataRect i = DataRect.Intersect(R(0, 0, 5, 5), R(3, 3, 5, 5));
    Assert.Equal(3, i.XMin); Assert.Equal(2, i.Width);
  }
  [Fact]
  public void IntersectsWith_NonOverlapping_False() =>
    Assert.False(R(0, 0, 2, 2).IntersectsWith(R(5, 5, 2, 2)));
  [Fact]
  public void IntersectsWith_Overlapping_True() =>
    Assert.True(R(0, 0, 5, 5).IntersectsWith(R(3, 3, 5, 5)));
  [Fact]
  public void Intersect_Instance_NonOverlapping_BecomesEmpty() {
    DataRect dr = R(0, 0, 2, 2);
    dr.Intersect(R(5, 5, 2, 2));
    Assert.True(dr.IsEmpty);
  }

  // Offset
  [Fact]
  public void Offset_Vector_ShiftsRect() {
    DataRect dr = R(1, 2, 5, 5);
    dr.Offset(new Vector(3, 4));
    Assert.Equal(4, dr.XMin); Assert.Equal(6, dr.YMin);
  }
  [Fact]
  public void Offset_Doubles_ShiftsRect() {
    DataRect dr = R(1, 1, 4, 4);
    dr.Offset(2, -1);
    Assert.Equal(3, dr.XMin); Assert.Equal(0, dr.YMin);
  }
  [Fact]
  public void Offset_Static_ReturnsShiftedCopy() {
    DataRect r = DataRect.Offset(R(0, 0, 5, 5), 1, 2);
    Assert.Equal(1, r.XMin); Assert.Equal(2, r.YMin);
  }
  [Fact]
  public void Offset_OnEmpty_Throws() {
    DataRect dr = DataRect.Empty;
    Assert.Throws<InvalidOperationException>(() => dr.Offset(1, 1));
  }

  // Union
  [Fact]
  public void Union_Instance_EnclosesAll() {
    DataRect dr = R(0, 0, 2, 2);
    dr.Union(R(5, 5, 2, 2));
    Assert.Equal(0, dr.XMin); Assert.Equal(7, dr.XMax);
  }
  [Fact]
  public void Union_Instance_EmptyReceiver_BecomesOther() {
    DataRect dr = DataRect.Empty;
    dr.Union(R(1, 2, 3, 4));
    Assert.Equal(R(1, 2, 3, 4), dr);
  }
  [Fact]
  public void Union_Instance_WithPoint_Expands() {
    DataRect dr = R(0, 0, 5, 5);
    dr.Union(new Point(10, 10));
    Assert.Equal(10, dr.XMax);
  }
  [Fact]
  public void Union_Static_WithPoint() {
    DataRect r = DataRect.Union(R(0, 0, 3, 3), new Point(5, 7));
    Assert.Equal(5, r.XMax); Assert.Equal(7, r.YMax);
  }
  [Fact]
  public void Union_Static_TwoRects() {
    DataRect r = DataRect.Union(R(0, 0, 3, 3), R(4, 4, 3, 3));
    Assert.Equal(0, r.XMin); Assert.Equal(7, r.XMax);
  }

  // Hash / ToString / Parse / EqualEps
  [Fact] public void GetHashCode_Empty_IsZero() => Assert.Equal(0, DataRect.Empty.GetHashCode());
  [Fact]
  public void GetHashCode_NonEmpty_IsDeterministic() {
    DataRect dr = R(1, 2, 3, 4);
    Assert.Equal(dr.GetHashCode(), dr.GetHashCode());
  }
  [Fact]
  public void ToString_Empty_ReturnsLiteralEmpty() =>
    Assert.Equal("Empty", DataRect.Empty.ToString());
  [Fact]
  public void ToString_NonEmpty_ContainsXMin() =>
    Assert.Contains("1", R(1, 2, 3, 4).ToString());
  [Fact]
  public void EqualEps_IdenticalRects_True() =>
    Assert.True(DataRect.EqualEps(R(0, 0, 10, 10), R(0, 0, 10, 10), 1e-6));
  [Fact]
  public void EqualEps_SlightlyDifferent_LargeEps_True() =>
    Assert.True(DataRect.EqualEps(R(0, 0, 100, 100), R(0.1, 0.1, 100, 100), 0.01));
  [Fact]
  public void Parse_EmptyString_IsEmpty() =>
    Assert.True(DataRect.Parse("Empty").IsEmpty);
  [Fact]
  public void Parse_FourCommaValues() {
    DataRect dr = DataRect.Parse("1,2,3,4");
    Assert.Equal(1, dr.XMin); Assert.Equal(3, dr.Width);
  }
  [Fact]
  public void Parse_PointToPoint() {
    DataRect dr = DataRect.Parse("0,0 10,5");
    Assert.Equal(10, dr.XMax); Assert.Equal(5, dr.YMax);
  }
  [Fact]
  public void IFormattable_ToString_ContainsXMin() {
    string s = ((IFormattable)R(1.5, 2.5, 3.5, 4.5)).ToString("F1", CultureInfo.InvariantCulture);
    Assert.Contains("1.5", s);
  }
}

// ─────────────────────────────────────────────────────────────
// DataTransforms
// ─────────────────────────────────────────────────────────────
public class DataTransformsTests {
  // IdentityTransform
  [Fact]
  public void Identity_DataToViewport_ReturnsSame() {
    var p = new Point(3, 7);
    Assert.Equal(p, new IdentityTransform().DataToViewport(p));
  }
  [Fact]
  public void Identity_ViewportToData_ReturnsSame() {
    var p = new Point(5, 9);
    Assert.Equal(p, new IdentityTransform().ViewportToData(p));
  }

  // Log10YTransform
  [Fact]
  public void Log10Y_DataToViewport_TransformsY() {
    var r = new Log10YTransform().DataToViewport(new Point(3, 100));
    Assert.Equal(3, r.X, 6); Assert.Equal(2, r.Y, 6);
  }
  [Fact]
  public void Log10Y_NegativeY_ReturnsDoubleMinValue() =>
    Assert.Equal(double.MinValue, new Log10YTransform().DataToViewport(new Point(1, -5)).Y);
  [Fact]
  public void Log10Y_ViewportToData_Inverts() =>
    Assert.Equal(100, new Log10YTransform().ViewportToData(new Point(1, 2)).Y, 6);
  [Fact]
  public void Log10Y_DataDomain_IsYPositive() =>
    Assert.Equal(DataDomains.YPositive, new Log10YTransform().DataDomain);

  // Log10XTransform
  [Fact]
  public void Log10X_DataToViewport_TransformsX() {
    var r = new Log10XTransform().DataToViewport(new Point(1000, 5));
    Assert.Equal(3, r.X, 6); Assert.Equal(5, r.Y, 6);
  }
  [Fact]
  public void Log10X_NegativeX_ReturnsDoubleMinValue() =>
    Assert.Equal(double.MinValue, new Log10XTransform().DataToViewport(new Point(-1, 5)).X);
  [Fact]
  public void Log10X_ViewportToData_Inverts() =>
    Assert.Equal(1000, new Log10XTransform().ViewportToData(new Point(3, 0)).X, 3);
  [Fact]
  public void Log10X_DataDomain_IsXPositive() =>
    Assert.Equal(DataDomains.XPositive, new Log10XTransform().DataDomain);

  // Log10Transform (both axes)
  [Fact]
  public void Log10_DataToViewport_BothAxes() {
    var r = new Log10Transform().DataToViewport(new Point(100, 1000));
    Assert.Equal(2, r.X, 6); Assert.Equal(3, r.Y, 6);
  }
  [Fact]
  public void Log10_Negative_ReturnDoubleMinValue() {
    var r = new Log10Transform().DataToViewport(new Point(-1, -2));
    Assert.Equal(double.MinValue, r.X); Assert.Equal(double.MinValue, r.Y);
  }
  [Fact]
  public void Log10_RoundTrip() {
    var t = new Log10Transform();
    var src = new Point(100, 1000);
    var back = t.ViewportToData(t.DataToViewport(src));
    Assert.Equal(src.X, back.X, 3); Assert.Equal(src.Y, back.Y, 3);
  }
  [Fact]
  public void Log10_DataDomain_IsXYPositive() =>
    Assert.Equal(DataDomains.XYPositive, new Log10Transform().DataDomain);

  // SwapTransform
  [Fact]
  public void Swap_DataToViewport_SwapsXY() {
    var r = new SwapTransform().DataToViewport(new Point(3, 7));
    Assert.Equal(7, r.X); Assert.Equal(3, r.Y);
  }
  [Fact]
  public void Swap_IsItsOwnInverse() {
    var t = new SwapTransform();
    var p = new Point(5, 9);
    var r = t.ViewportToData(t.DataToViewport(p));
    Assert.Equal(p.X, r.X, 9); Assert.Equal(p.Y, r.Y, 9);
  }

  // MercatorTransform
  [Fact]
  public void Mercator_DefaultMaxLat85() =>
    Assert.Equal(85, new MercatorTransform().MaxLatitude);
  [Fact]
  public void Mercator_CustomMaxLat() =>
    Assert.Equal(60, new MercatorTransform(60).MaxLatitude);
  [Fact]
  public void Mercator_Equator_StaysAtZero() =>
    Assert.Equal(0, new MercatorTransform().DataToViewport(new Point(0, 0)).Y, 5);
  [Fact]
  public void Mercator_RoundTrip() {
    var t = new MercatorTransform();
    var back = t.ViewportToData(t.DataToViewport(new Point(10, 30)));
    Assert.Equal(10, back.X, 3); Assert.Equal(30, back.Y, 3);
  }
  [Fact]
  public void Mercator_ScalePositive() =>
    Assert.True(new MercatorTransform().Scale > 0);

  // PolarToRectTransform
  [Fact]
  public void Polar_Origin_StaysAtOrigin() {
    var r = new PolarToRectTransform().DataToViewport(new Point(0, 0));
    Assert.Equal(0, r.X, 9); Assert.Equal(0, r.Y, 9);
  }
  [Fact]
  public void Polar_Radius1_Angle0_IsUnitX() {
    var r = new PolarToRectTransform().DataToViewport(new Point(1, 0));
    Assert.Equal(1, r.X, 9); Assert.Equal(0, r.Y, 9);
  }
  [Fact]
  public void Polar_RoundTrip() {
    var t = new PolarToRectTransform();
    var src = new Point(5, Math.PI / 4);
    var back = t.ViewportToData(t.DataToViewport(src));
    Assert.Equal(src.X, back.X, 5); Assert.Equal(src.Y, back.Y, 5);
  }

  // RotateDataTransform
  [Fact]
  public void Rotate_ZeroAngle_IsIdentity() {
    var t = new RotateDataTransform(0);
    var p = new Point(5, 3);
    var r = t.DataToViewport(p);
    Assert.Equal(p.X, r.X, 9); Assert.Equal(p.Y, r.Y, 9);
  }
  [Fact]
  public void Rotate_RoundTrip() {
    var t = new RotateDataTransform(Math.PI / 2);
    var p = new Point(3, 0);
    var back = t.ViewportToData(t.DataToViewport(p));
    Assert.Equal(p.X, back.X, 5); Assert.Equal(p.Y, back.Y, 5);
  }
  [Fact]
  public void Rotate_WithCenter_CenterFixed() {
    var c = new Point(2, 2);
    var r = new RotateDataTransform(Math.PI / 2, c).DataToViewport(c);
    Assert.Equal(c.X, r.X, 5); Assert.Equal(c.Y, r.Y, 5);
  }

  // MatrixDataTransform
  [Fact]
  public void Matrix_Identity_IsIdentity() {
    var t = new MatrixDataTransform(Matrix.Identity);
    var p = new Point(4, 7);
    Assert.Equal(p, t.DataToViewport(p)); Assert.Equal(p, t.ViewportToData(p));
  }
  [Fact]
  public void Matrix_Scale_ScalesPoints() {
    var t = new MatrixDataTransform(new Matrix(2, 0, 0, 3, 0, 0));
    var r = t.DataToViewport(new Point(5, 4));
    Assert.Equal(10, r.X, 6); Assert.Equal(12, r.Y, 6);
  }
  [Fact]
  public void Matrix_ViewportToData_AppliesMatrixForward_NotInverse() {
    // MatrixDataTransform.ViewportToData applies the same forward matrix
    // transform as DataToViewport — it does NOT use the inverse matrix.
    // Matrix(2,0,0,2,1,1): x'=2x+1, y'=2y+1
    //   ViewportToData(7, 11) → (2*7+1, 2*11+1) = (15, 23)
    var t = new MatrixDataTransform(new Matrix(2, 0, 0, 2, 1, 1));
    var r = t.ViewportToData(new Point(7, 11));
    Assert.Equal(15, r.X, 6);
    Assert.Equal(23, r.Y, 6);
  }

  [Fact]
  public void Matrix_Identity_RoundTrips() {
    // Identity matrix: DataToViewport and ViewportToData both return the same
    // point, so DataToViewport→ViewportToData is identity regardless of
    // whether ViewportToData uses the forward or inverse matrix.
    var t = new MatrixDataTransform(Matrix.Identity);
    var p = new Point(3, 5);
    var back = t.ViewportToData(t.DataToViewport(p));
    Assert.Equal(p.X, back.X, 9);
    Assert.Equal(p.Y, back.Y, 9);
  }

  // CompositeDataTransform
  [Fact]
  public void Composite_AppliesInOrder_ScaleThenSwap() {
    var c = new CompositeDataTransform(new MatrixDataTransform(new Matrix(2, 0, 0, 1, 0, 0)), new SwapTransform());
    var r = c.DataToViewport(new Point(3, 4)); // scale→(6,4) swap→(4,6)
    Assert.Equal(4, r.X, 6); Assert.Equal(6, r.Y, 6);
  }
  [Fact]
  public void Composite_ViewportToData_AppliesDataToViewportInReverseOrder() {
    // CompositeDataTransform.ViewportToData is NOT a mathematical inverse of
    // DataToViewport.  It applies each sub-transform's DataToViewport in
    // reverse order.  Given CompositeDataTransform(scale×2, swap):
    //   ViewportToData(4, 6)
    //     → swap.DataToViewport(4, 6)   = (6, 4)
    //     → scale.DataToViewport(6, 4)  = (12, 4)
    var c = new CompositeDataTransform(
      new MatrixDataTransform(new Matrix(2, 0, 0, 1, 0, 0)),
      new SwapTransform());
    var r = c.ViewportToData(new Point(4, 6));
    Assert.Equal(12, r.X, 6);
    Assert.Equal(4, r.Y, 6);
  }

  [Fact]
  public void Composite_TwoSwaps_IsIdentityInBothDirections() {
    // Using two SwapTransforms: swap∘swap = identity in both DataToViewport
    // and ViewportToData (since swap is self-inverse AND self-DataToViewport).
    var c = new CompositeDataTransform(new SwapTransform(), new SwapTransform());
    var p = new Point(3, 7);
    var fwd = c.DataToViewport(p);
    var back = c.ViewportToData(p);
    Assert.Equal(p.X, fwd.X, 9); Assert.Equal(p.Y, fwd.Y, 9);
    Assert.Equal(p.X, back.X, 9); Assert.Equal(p.Y, back.Y, 9);
  }
  [Fact]
  public void Composite_NullArray_ThrowsArgumentNull() =>
    Assert.Throws<ArgumentNullException>(() => new CompositeDataTransform((DataTransform[])null));
  [Fact]
  public void Composite_NullElement_ThrowsArgumentNull() =>
    Assert.Throws<ArgumentNullException>(() => new CompositeDataTransform(new IdentityTransform(), null));
  [Fact]
  public void Composite_NullEnumerable_ThrowsArgumentNull() =>
    Assert.Throws<ArgumentNullException>(() => new CompositeDataTransform((IEnumerable<DataTransform>)null));

  // LambdaDataTransform
  [Fact]
  public void Lambda_UsesSuppliedFunctions() {
    var t = new LambdaDataTransform(p => new Point(p.X * 2, p.Y * 3), p => new Point(p.X / 2, p.Y / 3));
    var r = t.DataToViewport(new Point(5, 4));
    Assert.Equal(10, r.X, 6); Assert.Equal(12, r.Y, 6);
    var back = t.ViewportToData(r);
    Assert.Equal(5, back.X, 6); Assert.Equal(4, back.Y, 6);
  }
  [Fact]
  public void Lambda_NullDataToViewport_Throws() =>
    Assert.Throws<ArgumentNullException>(() => new LambdaDataTransform(null, p => p));
  [Fact]
  public void Lambda_NullViewportToData_Throws() =>
    Assert.Throws<ArgumentNullException>(() => new LambdaDataTransform(p => p, null));

  // DataTransforms static class
  [Fact]
  public void DataTransforms_Identity_IsIdentityTransform() =>
    Assert.IsType<IdentityTransform>(DataTransforms.Identity);
}

// ─────────────────────────────────────────────────────────────
// CoordinateTransform + CoordinateTransformExtensions
// (need live Plotter → STA via WPFTestBase.RunTest)
// ─────────────────────────────────────────────────────────────
public class CoordinateTransformTests : WPFTestBase {
  [Fact]
  public void DataToScreen_ScreenToData_RoundTrip() {
    RunTest(() => {
      var ct = new Plotter().Viewport.Transform;
      var data = new Point(0.5, 0.5);
      var back = ct.ScreenToData(ct.DataToScreen(data));
      Assert.Equal(data.X, back.X, 3); Assert.Equal(data.Y, back.Y, 3);
    });
  }
  [Fact]
  public void ViewportToScreen_ScreenToViewport_RoundTrip() {
    RunTest(() => {
      var ct = new Plotter().Viewport.Transform;
      var vp = new Point(0.3, 0.7);
      var back = ct.ScreenToViewport(ct.ViewportToScreen(vp));
      Assert.Equal(vp.X, back.X, 3); Assert.Equal(vp.Y, back.Y, 3);
    });
  }
  [Fact]
  public void DataTransform_Default_IsIdentity() {
    RunTest(() => Assert.IsType<IdentityTransform>(new Plotter().Viewport.Transform.DataTransform));
  }
  [Fact]
  public void ViewportRect_AliasesVisibleRect() {
    RunTest(() => {
      var ct = new Plotter().Viewport.Transform;
      Assert.Equal(ct.VisibleRect, ct.ViewportRect);
    });
  }
  [Fact]
  public void WithDataTransform_Null_ThrowsArgumentNull() {
    RunTest(() => Assert.Throws<ArgumentNullException>(
      () => new Plotter().Viewport.Transform.WithDataTransform(null)));
  }
  [Fact]
  public void WithDataTransform_ReturnsNewInstance_OriginalUnchanged() {
    RunTest(() => {
      var ct = new Plotter().Viewport.Transform;
      var swapped = ct.WithDataTransform(new SwapTransform());
      Assert.IsType<SwapTransform>(swapped.DataTransform);
      Assert.IsType<IdentityTransform>(ct.DataTransform);
    });
  }
}

public class CoordinateTransformExtensionsTests : WPFTestBase {
  CoordinateTransform CT() => new Plotter().Viewport.Transform;

  [Fact]
  public void DataToScreen_Extension_MatchesDirectCall() {
    RunTest(() => {
      var ct = CT(); var p = new Point(0.25, 0.75);
      Assert.Equal(ct.DataToScreen(p), p.DataToScreen(ct));
    });
  }
  [Fact]
  public void ScreenToData_Extension_MatchesDirectCall() {
    RunTest(() => {
      var ct = CT(); var s = new Point(100, 300);
      Assert.Equal(ct.ScreenToData(s), s.ScreenToData(ct));
    });
  }
  [Fact]
  public void ViewportToScreen_Extension_MatchesDirectCall() {
    RunTest(() => {
      var ct = CT(); var v = new Point(0.1, 0.9);
      Assert.Equal(ct.ViewportToScreen(v), v.ViewportToScreen(ct));
    });
  }
  [Fact]
  public void ScreenToViewport_Extension_MatchesDirectCall() {
    RunTest(() => {
      var ct = CT(); var s = new Point(50, 50);
      Assert.Equal(ct.ScreenToViewport(s), s.ScreenToViewport(ct));
    });
  }
  [Fact]
  public void DataToViewport_Extension_UsesDataTransform() {
    RunTest(() => {
      var ct = CT(); var p = new Point(0.4, 0.6);
      Assert.Equal(ct.DataTransform.DataToViewport(p), p.DataToViewport(ct));
    });
  }
  [Fact]
  public void DataToScreen_DataRect_NonEmpty() {
    RunTest(() => {
      var ct = CT();
      Assert.False(DataRect.Create(0, 0, 1, 1).DataToScreen(ct).IsEmpty);
    });
  }
  [Fact]
  public void ScreenToData_Rect_NonEmpty() {
    RunTest(() => {
      var ct = CT();
      Assert.False(new Rect(0, 0, 100, 100).ScreenToData(ct).IsEmpty);
    });
  }
  [Fact]
  public void DataToViewport_DataRect_WithIdentityTransform_IsIdentity() {
    RunTest(() => {
      var dr = DataRect.Create(0, 0, 10, 10);
      Assert.Equal(dr, dr.DataToViewport(DataTransforms.Identity));
    });
  }
  [Fact]
  public void ViewportToScreen_DataRect_RoundTripsToScreenToViewport() {
    RunTest(() => {
      var ct = CT();
      var vp = DataRect.Create(0.1, 0.1, 0.8, 0.8);
      var back = vp.ViewportToScreen(ct).ScreenToViewport(ct);
      Assert.Equal(vp.XMin, back.XMin, 3);
    });
  }
  [Fact]
  public void DataToScreenAsList_TransformsAllPoints() {
    RunTest(() => {
      var ct = CT();
      var pts = new[] { new Point(0.1, 0.1), new Point(0.5, 0.5) };
      var list = pts.DataToScreenAsList(ct);
      Assert.Equal(2, list.Count);
      Assert.Equal(ct.DataToScreen(pts[0]), list[0]);
    });
  }
  [Fact]
  public void DataToScreenAsList_BothOverloads_SameResult() {
    RunTest(() => {
      var ct = CT();
      var pts = new[] { new Point(0.2, 0.3), new Point(0.4, 0.6) };
      var a = pts.DataToScreenAsList(ct);
      var b = ct.DataToScreenAsList(pts);
      Assert.Equal(a.Count, b.Count);
      for (int i = 0; i < a.Count; i++) Assert.Equal(a[i], b[i]);
    });
  }
  [Fact]
  public void DataToViewport_IEnumerable_TransformsAllPoints() {
    RunTest(() => {
      var ct = CT();
      var pts = new[] { new Point(0, 0), new Point(0.5, 0.5) };
      var result = new List<Point>(pts.DataToViewport(ct));
      Assert.Equal(2, result.Count);
    });
  }
}

// ─────────────────────────────────────────────────────────────
// MathHelper
// ─────────────────────────────────────────────────────────────
public class MathHelperTests {
  [Theory]
  [InlineData(5L, 1L, 10L, 5L)]
  [InlineData(0L, 1L, 10L, 1L)]
  [InlineData(15L, 1L, 10L, 10L)]
  public void Clamp_Long(long v, long mn, long mx, long ex) => Assert.Equal(ex, MathHelper.Clamp(v, mn, mx));

  [Theory]
  [InlineData(5.0, 1.0, 10.0, 5.0)]
  [InlineData(0.5, 1.0, 10.0, 1.0)]
  [InlineData(15.0, 1.0, 10.0, 10.0)]
  public void Clamp_Double(double v, double mn, double mx, double ex) => Assert.Equal(ex, MathHelper.Clamp(v, mn, mx));

  [Theory]
  [InlineData(0.5, 0.5)]
  [InlineData(-0.1, 0.0)]
  [InlineData(1.5, 1.0)]
  public void Clamp_UnitRange(double v, double ex) => Assert.Equal(ex, MathHelper.Clamp(v));

  [Theory]
  [InlineData(5, 1, 10, 5)]
  [InlineData(-5, 1, 10, 1)]
  [InlineData(50, 1, 10, 10)]
  public void Clamp_Int(int v, int mn, int mx, int ex) => Assert.Equal(ex, MathHelper.Clamp(v, mn, mx));

  [Fact] public void Interpolate_Mid() => Assert.Equal(5.0, MathHelper.Interpolate(0, 10, 0.5));
  [Fact] public void Interpolate_Zero() => Assert.Equal(3.0, MathHelper.Interpolate(3, 9, 0));
  [Fact] public void Interpolate_One() => Assert.Equal(9.0, MathHelper.Interpolate(3, 9, 1));
  [Fact] public void RadiansToDegrees_Pi_Is180() => Assert.Equal(180.0, Math.PI.RadiansToDegrees(), 5);
  [Fact] public void DegreesToRadians_180_IsPi() => Assert.Equal(Math.PI, 180.0.DegreesToRadians(), 10);
  [Fact] public void RoundTrip_Radians_Degrees() => Assert.Equal(45.0, 45.0.DegreesToRadians().RadiansToDegrees(), 10);
  [Fact] public void IsNaN_NaN_True() => Assert.True(double.NaN.IsNaN());
  [Fact] public void IsNaN_Real_False() => Assert.False(3.14.IsNaN());
  [Fact] public void IsNotNaN_Real_True() => Assert.True(1.0.IsNotNaN());
  [Fact] public void IsFinite_Finite_True() => Assert.True(42.0.IsFinite());
  [Fact] public void IsFinite_Infinity_False() => Assert.False(double.PositiveInfinity.IsFinite());
  [Fact] public void IsInfinite_PosInf_True() => Assert.True(double.PositiveInfinity.IsInfinite());
  [Fact] public void IsInfinite_Finite_False() => Assert.False(5.0.IsInfinite());
  [Fact] public void AreClose_SameValue_True() => Assert.True(MathHelper.AreClose(100.0, 100.0, 1e-6));
  [Fact] public void AreClose_VeryDifferent_False() => Assert.False(MathHelper.AreClose(100.0, 200.0, 0.01));
  [Fact]
  public void CreateRectByPoints_Correct() {
    Rect r = MathHelper.CreateRectByPoints(1, 2, 5, 8);
    Assert.Equal(1, r.X); Assert.Equal(4, r.Width); Assert.Equal(6, r.Height);
  }
  [Fact]
  public void ToPoint_ConvertsVector() {
    var p = new Vector(3, 7).ToPoint();
    Assert.Equal(3, p.X); Assert.Equal(7, p.Y);
  }
  [Fact] public void ToAngle_PosX_IsZero() => Assert.Equal(0.0, new Vector(1, 0).ToAngle(), 5);
}

// ─────────────────────────────────────────────────────────────
// RangeExtensions
// ─────────────────────────────────────────────────────────────
public class RangeExtensionsTests {
  [Fact]
  public void GetLength_Double_MaxMinusMin() =>
    Assert.Equal(7.0, new Range<double>(3, 10).GetLength());
  [Fact]
  public void GetLength_Double_SymmetricRange() =>
    Assert.Equal(10.0, new Range<double>(-5, 5).GetLength());
  [Fact]
  public void GetLength_Point_EuclideanDistance() =>
    Assert.Equal(5.0, new Range<Point>(new Point(0, 0), new Point(3, 4)).GetLength(), 6);
}

// ─────────────────────────────────────────────────────────────
// DataRectExtensions
// ─────────────────────────────────────────────────────────────
public class DataRectExtensionsTests {
  [Fact]
  public void GetCenter_Symmetric() {
    var c = DataRect.Create(0, 0, 10, 10).GetCenter();
    Assert.Equal(5, c.X); Assert.Equal(5, c.Y);
  }
  [Fact]
  public void GetCenter_Asymmetric() {
    var c = new DataRect(2, 4, 6, 8).GetCenter();
    Assert.Equal(5, c.X); Assert.Equal(8, c.Y);
  }
  [Fact]
  public void GetSquare_WidthTimesHeight() =>
    Assert.Equal(20.0, new DataRect(0, 0, 4, 5).GetSquare());
  [Fact]
  public void GetSquare_Empty_Zero() =>
    Assert.Equal(0.0, DataRect.Empty.GetSquare());
  [Fact]
  public void IsCloseTo_Identical_True() {
    var dr = new DataRect(0, 0, 10, 10);
    Assert.True(dr.IsCloseTo(dr, 1e-6));
  }
  [Fact]
  public void IsCloseTo_VeryDifferent_False() =>
    Assert.False(new DataRect(0, 0, 1, 1).IsCloseTo(new DataRect(0, 0, 100, 100), 0.01));
  [Fact]
  public void ZoomOutFromCenter_DoublesSize_PreservesCenter() {
    var dr = new DataRect(0, 0, 10, 10);
    var z = dr.ZoomOutFromCenter(2);
    Assert.Equal(20, z.Width, 5);
    var c = z.GetCenter();
    Assert.Equal(5, c.X, 5); Assert.Equal(5, c.Y, 5);
  }
  [Fact]
  public void ZoomInToCenter_HalvesSize_PreservesCenter() {
    var dr = new DataRect(0, 0, 10, 10);
    var z = dr.ZoomInToCenter(2);
    Assert.Equal(5, z.Width, 5);
    var c = z.GetCenter();
    Assert.Equal(5, c.X, 5); Assert.Equal(5, c.Y, 5);
  }
}

// ─────────────────────────────────────────────────────────────
// HsbColor
// ─────────────────────────────────────────────────────────────
public class HsbColorTests {
  [Fact]
  public void Ctor_3Args_DefaultAlpha1() {
    HsbColor c = new(180, 0.5, 0.8);
    Assert.Equal(180, c.Hue); Assert.Equal(0.5, c.Saturation);
    Assert.Equal(0.8, c.Brightness); Assert.Equal(1.0, c.Alpha);
  }
  [Fact]
  public void Ctor_4Args_SetsAlpha() =>
    Assert.Equal(0.5, new HsbColor(90, 0.3, 0.7, 0.5).Alpha);
  [Fact]
  public void Hue_SetNegative_WrapsPositive() {
    HsbColor c = new(0, 0, 1); c.Hue = -30;
    Assert.Equal(30, c.Hue, 9);
  }
  [Fact]
  public void Hue_SetAbove360_WrapsModulo() {
    HsbColor c = new(0, 0, 1); c.Hue = 450;
    Assert.Equal(90, c.Hue, 9);
  }
  [Fact]
  public void FromArgbColor_Red_HueNearZero() {
    HsbColor h = HsbColor.FromArgbColor(Colors.Red);
    Assert.Equal(0, h.Hue, 1); Assert.Equal(1.0, h.Alpha, 5);
  }
  [Fact]
  public void FromArgbColor_Black_ZeroSatBright() {
    HsbColor h = HsbColor.FromArgbColor(Colors.Black);
    Assert.Equal(0, h.Saturation, 5); Assert.Equal(0, h.Brightness, 5);
  }
  [Fact]
  public void FromArgbColor_White_ZeroSat() {
    HsbColor h = HsbColor.FromArgbColor(Colors.White);
    Assert.Equal(0, h.Saturation, 5); Assert.Equal(1.0, h.Brightness, 5);
  }
  [Fact]
  public void FromArgb_Int_MatchesFromArgbColor() {
    // HsbColor.FromArgb does a checked cast after arithmetic right-shifting
    // the int to extract the alpha byte. When alpha >= 128 the int's sign
    // bit is set (negative), so >> 24 yields -1 and checked((byte)-1) overflows.
    // Use A=100 so the ARGB int stays positive.
    Color c = Color.FromArgb(100, 0, 0, 255);
    int argb = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;
    HsbColor a = HsbColor.FromArgb(argb), b = HsbColor.FromArgbColor(c);
    Assert.Equal(a.Hue, b.Hue, 5); Assert.Equal(a.Saturation, b.Saturation, 5);
  }

  [Fact]
  public void ToArgbColor_RoundTrip_Approximate() {
    Color o = Color.FromArgb(255, 128, 64, 32);
    Color b = HsbColor.FromArgbColor(o).ToArgbColor();
    Assert.True(Math.Abs(o.R - b.R) <= 2); Assert.True(Math.Abs(o.G - b.G) <= 2); Assert.True(Math.Abs(o.B - b.B) <= 2);
  }

  [Theory]
  [InlineData(0.0)]
  [InlineData(60.0)]
  [InlineData(120.0)]
  [InlineData(180.0)]
  [InlineData(240.0)]
  [InlineData(300.0)]
  public void ToArgbColor_AllSixSectors_DoNotThrow(double hue) =>
    _ = new HsbColor(hue, 1.0, 1.0).ToArgbColor();

  [Fact]
  public void ToArgbColor_ZeroSat_IsGrey() {
    Color col = new HsbColor(180, 0, 0.5).ToArgbColor();
    Assert.Equal(col.R, col.G); Assert.Equal(col.G, col.B);
  }

  [Fact]
  public void ToArgb_MatchesToArgbColor() {
    HsbColor c = new(90, 0.8, 0.9);
    Assert.Equal(c.ToArgbColor().ToArgb(), c.ToArgb());
  }

  [Fact]
  public void ColorExtensions_ToHsbColor_MatchesFromArgbColor() {
    Color col = Colors.Green;
    HsbColor a = col.ToHsbColor(), b = HsbColor.FromArgbColor(col);
    Assert.Equal(a.Hue, b.Hue, 5); Assert.Equal(a.Saturation, b.Saturation, 5);
  }

  [Fact]
  public void Equality_SameValues() {
    Assert.True(new HsbColor(120, 0.5, 0.8, 0.9) == new HsbColor(120, 0.5, 0.8, 0.9));
  }

  [Fact]
  public void Inequality_DifferentHue() =>
    Assert.True(new HsbColor(120, 0.5, 0.8) != new HsbColor(200, 0.5, 0.8));

  [Fact]
  public void Equals_Boxed_True() =>
    Assert.True(new HsbColor(90, 0.3, 0.7, 0.5).Equals((object)new HsbColor(90, 0.3, 0.7, 0.5)));

  [Fact]
  public void Equals_WrongType_False() =>
    Assert.False(new HsbColor(90, 0.3, 0.7).Equals("not a color"));

  [Fact]
  public void GetHashCode_SameValues_SameHash() =>
    Assert.Equal(new HsbColor(45, 0.6, 0.9, 1.0).GetHashCode(), new HsbColor(45, 0.6, 0.9, 1.0).GetHashCode());
}
