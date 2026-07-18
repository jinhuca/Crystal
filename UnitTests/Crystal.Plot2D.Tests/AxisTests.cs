using System.Windows;
using Xunit;
using Crystal.Plot2D;

namespace Crystal.Plot2D.Tests;

public class AxisTests : WPFTestBase {
  [Fact]
  public void Plotter_HasMainHorizontalAndVerticalAxes() {
    RunTest(() => {
      // Arrange & Act
      var plotter = new Plotter();

      // Assert
      Assert.NotNull(plotter.MainHorizontalAxis);
      Assert.NotNull(plotter.MainVerticalAxis);
    });
  }

  [Theory]
  [InlineData(Visibility.Visible)]
  [InlineData(Visibility.Hidden)]
  [InlineData(Visibility.Collapsed)]
  public void MainHorizontalAxis_VisibilityCanBeSet(Visibility visibility) {
    RunTest(() => {
      // Arrange
      var plotter = new Plotter();

      // Act
      plotter.MainHorizontalAxisVisibility = visibility;

      // Assert
      Assert.Equal(visibility, plotter.MainHorizontalAxisVisibility);
    });
  }

  [Theory]
  [InlineData(Visibility.Visible)]
  [InlineData(Visibility.Hidden)]
  [InlineData(Visibility.Collapsed)]
  public void MainVerticalAxis_VisibilityCanBeSet(Visibility visibility) {
    RunTest(() => {
      // Arrange
      var plotter = new Plotter();

      // Act
      plotter.MainVerticalAxisVisibility = visibility;

      // Assert
      Assert.Equal(visibility, plotter.MainVerticalAxisVisibility);
    });
  }

  [Fact]
  public void MainHorizontalAxis_DefaultVisibilityIsNotHidden() {
    RunTest(() => {
      // Arrange
      var plotter = new Plotter();

      // Assert
      Assert.NotEqual(Visibility.Hidden, plotter.MainHorizontalAxisVisibility);
    });
  }

  [Fact]
  public void MainVerticalAxis_DefaultVisibilityIsNotHidden() {
    RunTest(() => {
      // Arrange
      var plotter = new Plotter();

      // Assert
      Assert.NotEqual(Visibility.Hidden, plotter.MainVerticalAxisVisibility);
    });
  }

  [Fact]
  public void Plotter_CanSetMainHorizontalAxis() {
    RunTest(() => {
      // Arrange
      var plotter = new Plotter();
      var originalAxis = plotter.MainHorizontalAxis;

      // Act
      var newAxis = new Crystal.Plot2D.Axes.Numeric.HorizontalAxis();
      plotter.MainHorizontalAxis = newAxis;

      // Assert
      Assert.NotNull(plotter.MainHorizontalAxis);
      Assert.Same(newAxis, plotter.MainHorizontalAxis);
    });
  }

  [Fact]
  public void Plotter_CanSetMainVerticalAxis() {
    RunTest(() => {
      // Arrange
      var plotter = new Plotter();

      // Act
      var newAxis = new Crystal.Plot2D.Axes.Numeric.VerticalAxis();
      plotter.MainVerticalAxis = newAxis;

      // Assert
      Assert.NotNull(plotter.MainVerticalAxis);
      Assert.Same(newAxis, plotter.MainVerticalAxis);
    });
  }
}
