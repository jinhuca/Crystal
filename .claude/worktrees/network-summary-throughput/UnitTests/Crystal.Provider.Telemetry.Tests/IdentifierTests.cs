using Crystal.Provider.Telemetry.Hardware;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class IdentifierTests {
  [Fact]
  public void Ctor_JoinsElementsWithLeadingSeparator() {
    var id = new Identifier("cpu", "0", "temperature");
    Assert.Equal("/cpu/0/temperature", id.ToString());
  }

  [Fact]
  public void Ctor_Empty_ProducesEmptyString() {
    var id = new Identifier();
    Assert.Equal(string.Empty, id.ToString());
  }

  [Fact]
  public void Ctor_EscapesReservedCharacters() {
    var id = new Identifier("a/b", "c d");
    Assert.Equal("/a%2Fb/c%20d", id.ToString());
  }

  [Fact]
  public void Ctor_FromBaseIdentifier_AppendsExtensions() {
    var baseId = new Identifier("hardware", "0");
    var id = new Identifier(baseId, "sensor", "1");
    Assert.Equal("/hardware/0/sensor/1", id.ToString());
  }

  [Fact]
  public void Equals_SameElements_AreEqual() {
    var a = new Identifier("cpu", "0");
    var b = new Identifier("cpu", "0");

    Assert.True(a.Equals(b));
    Assert.True(a == b);
    Assert.False(a != b);
    Assert.Equal(a.GetHashCode(), b.GetHashCode());
  }

  [Fact]
  public void Equals_DifferentElements_AreNotEqual() {
    var a = new Identifier("cpu", "0");
    var b = new Identifier("cpu", "1");

    Assert.False(a.Equals(b));
    Assert.False(a == b);
    Assert.True(a != b);
  }

  [Fact]
  public void Equals_NonIdentifierObject_ReturnsFalse() {
    var a = new Identifier("cpu", "0");
    Assert.False(a.Equals("string"));
    Assert.False(a.Equals(null));
  }

  [Fact]
  public void Operators_NullHandling() {
    Identifier a = new Identifier("cpu");
    Identifier? n = null;

    Assert.True(n == null);
    Assert.False(a == null);
    Assert.True(a != null);
    Assert.False(a < null); // non-null is not less than null
    Assert.True(a > null);  // non-null is greater than null (CompareTo(null) == 1)
    Assert.True(null < a);  // null is less than non-null
    Assert.False(null > a); // null is not greater than non-null
  }

  [Fact]
  public void CompareTo_OrdersOrdinally() {
    var a = new Identifier("a");
    var b = new Identifier("b");

    Assert.True(a.CompareTo(b) < 0);
    Assert.True(b.CompareTo(a) > 0);
    Assert.Equal(0, a.CompareTo(new Identifier("a")));
    Assert.Equal(1, a.CompareTo(null));
  }

  [Fact]
  public void ComparisonOperators_ReflectOrdering() {
    var a = new Identifier("a");
    var b = new Identifier("b");

    Assert.True(a < b);
    Assert.True(b > a);
    Assert.False(a > b);
    Assert.False(b < a);
  }
}
