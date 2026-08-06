using Crystal.Provider.Telemetry.Hardware;
using Crystal.Provider.Telemetry.Hardware.Motherboard;
using Xunit;

namespace Crystal.Provider.Telemetry.Tests;

public class IdentificationTests {
  [Theory]
  [InlineData("ASUSTeK COMPUTER INC.", Manufacturer.ASUS)]
  [InlineData("ASUS ", Manufacturer.ASUS)]
  [InlineData("ASRock", Manufacturer.ASRock)]
  [InlineData("asrock", Manufacturer.ASRock)] // case-insensitive
  [InlineData("Micro-Star International Co., Ltd.", Manufacturer.MSI)]
  [InlineData("MSI", Manufacturer.MSI)]
  [InlineData("Gigabyte Technology Co., Ltd.", Manufacturer.Gigabyte)]
  [InlineData("Dell Inc.", Manufacturer.Dell)]
  [InlineData("Intel Corporation", Manufacturer.Intel)]
  [InlineData("Intel", Manufacturer.Intel)]
  [InlineData("Hewlett-Packard", Manufacturer.HP)]
  [InlineData("HP", Manufacturer.HP)]
  [InlineData("http://www.abit.com.tw/", Manufacturer.Acer)] // substring match
  public void GetManufacturer_MapsKnownNames(string name, Manufacturer expected) {
    Assert.Equal(expected, Identification.GetManufacturer(name));
  }

  [Theory]
  [InlineData("")]
  [InlineData("Some Random OEM")]
  [InlineData("To be filled by O.E.M.")]
  public void GetManufacturer_UnknownOrPlaceholder_ReturnsUnknown(string name) {
    Assert.Equal(Manufacturer.Unknown, Identification.GetManufacturer(name));
  }

  [Theory]
  [InlineData("ROG CROSSHAIR VIII HERO", Model.ROG_CROSSHAIR_VIII_HERO)]
  [InlineData("rog crosshair viii hero", Model.ROG_CROSSHAIR_VIII_HERO)] // case-insensitive
  [InlineData("Z390 AORUS ULTRA", Model.Z390_AORUS_ULTRA)]
  [InlineData("B450 Steel Legend", Model.B450_Steel_Legend)]
  [InlineData("Z77A-GD65", Model.Z77_MS7751)] // multi-alias collapses to one model
  [InlineData("Z77A-GD80 (MS-7757)", Model.Z77_MS7751)]
  public void GetModel_MapsKnownNames(string name, Model expected) {
    Assert.Equal(expected, Identification.GetModel(name));
  }

  [Theory]
  [InlineData("")]
  [InlineData("Nonexistent Board 9000")]
  [InlineData("Base Board Product Name")]
  [InlineData("To be filled by O.E.M.")]
  public void GetModel_UnknownOrPlaceholder_ReturnsUnknown(string name) {
    Assert.Equal(Model.Unknown, Identification.GetModel(name));
  }
}
