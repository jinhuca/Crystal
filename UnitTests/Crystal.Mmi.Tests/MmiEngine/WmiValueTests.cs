using Crystal.Mmi.MmiEngine;
using Xunit;

namespace Crystal.Mmi.Tests.MmiEngine;

public class WmiValueTests
{
    // ── Bool ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Bool_Type_Is_Bool() => Assert.Equal(WmiType.Bool, new WmiValue(true).Type);

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Bool_AsBool_Returns_Value(bool v)
        => Assert.Equal(v, new WmiValue(v).AsBool());

    [Fact]
    public void Bool_AsInt_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(true).AsInt());

    [Fact]
    public void Bool_AsString_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(true).AsString());

    [Fact]
    public void Bool_AsStringArray_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(true).AsStringArray());

    [Fact]
    public void Bool_AsDateTime_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(true).AsDateTime());

    [Fact]
    public void Bool_AsUShortArray_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(true).AsUShortArray());

    [Fact]
    public void Bool_AsULong_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(true).AsULong());

    // ── Int ───────────────────────────────────────────────────────────────────

    [Fact]
    public void Int_Type_Is_Int() => Assert.Equal(WmiType.Int, new WmiValue(42).Type);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void Int_AsInt_Returns_Value(int v)
        => Assert.Equal(v, new WmiValue(v).AsInt());

    [Fact]
    public void Int_AsBool_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(0).AsBool());

    [Fact]
    public void Int_AsString_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(0).AsString());

    // ── String ────────────────────────────────────────────────────────────────

    [Fact]
    public void String_Type_Is_String() => Assert.Equal(WmiType.String, new WmiValue("hello").Type);

    [Theory]
    [InlineData("")]
    [InlineData("hello world")]
    public void String_AsString_Returns_Value(string v)
        => Assert.Equal(v, new WmiValue(v).AsString());

    [Fact]
    public void String_AsBool_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue("x").AsBool());

    [Fact]
    public void String_AsInt_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue("x").AsInt());

    // ── StringArray ───────────────────────────────────────────────────────────

    [Fact]
    public void StringArray_Type_Is_StringArray()
        => Assert.Equal(WmiType.StringArray, new WmiValue(new[] { "a" }).Type);

    [Fact]
    public void StringArray_AsStringArray_Returns_Value()
    {
        var arr = new[] { "alpha", "beta", "gamma" };
        Assert.Equal(arr, new WmiValue(arr).AsStringArray());
    }

    [Fact]
    public void StringArray_AsBool_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(new[] { "x" }).AsBool());

    // ── DateTime ──────────────────────────────────────────────────────────────

    [Fact]
    public void DateTime_Type_Is_DateTime()
    {
        var now = DateTime.UtcNow;
        Assert.Equal(WmiType.DateTime, new WmiValue(now).Type);
    }

    [Fact]
    public void DateTime_AsDateTime_Returns_Value()
    {
        var dt = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        Assert.Equal(dt, new WmiValue(dt).AsDateTime());
    }

    [Fact]
    public void DateTime_AsBool_Throws()
        => Assert.Throws<InvalidCastException>(
            () => new WmiValue(DateTime.UtcNow).AsBool());

    // ── UShortArray ───────────────────────────────────────────────────────────

    [Fact]
    public void UShortArray_Type_Is_UShortArray()
        => Assert.Equal(WmiType.UShortArray, new WmiValue(new ushort[] { 1, 2 }).Type);

    [Fact]
    public void UShortArray_AsUShortArray_Returns_Value()
    {
        var arr = new ushort[] { 10, 20, 30 };
        Assert.Equal(arr, new WmiValue(arr).AsUShortArray());
    }

    [Fact]
    public void UShortArray_AsString_Throws()
        => Assert.Throws<InvalidCastException>(
            () => new WmiValue(new ushort[] { 1 }).AsString());

    // ── ULong ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ULong_Type_Is_ULong() => Assert.Equal(WmiType.ULong, new WmiValue(1UL).Type);

    [Theory]
    [InlineData(0UL)]
    [InlineData(ulong.MaxValue)]
    [InlineData(123_456_789_012UL)]
    public void ULong_AsULong_Returns_Value(ulong v)
        => Assert.Equal(v, new WmiValue(v).AsULong());

    [Fact]
    public void ULong_AsReadOnlyULong_Returns_Same_As_AsULong()
    {
        var wv = new WmiValue(999UL);
        Assert.Equal(wv.AsULong(), wv.AsReadOnlyULong());
    }

    [Fact]
    public void ULong_AsBool_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(1UL).AsBool());

    [Fact]
    public void ULong_AsInt_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(1UL).AsInt());

    [Fact]
    public void ULong_AsString_Throws()
        => Assert.Throws<InvalidCastException>(() => new WmiValue(1UL).AsString());

    // ── ULong explicit constructor zeroes out other fields ─────────────────────

    [Fact]
    public void ULong_Constructor_Stores_Correct_Fields()
    {
        var wv = new WmiValue(42UL);
        Assert.Equal(WmiType.ULong, wv.Type);
        Assert.Equal(42UL, wv.AsULong());
    }
}
