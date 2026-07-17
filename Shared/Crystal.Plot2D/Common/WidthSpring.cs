using System;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Plot2D.Common;

/// <summary>
/// Represents a kind of 'spring' that makes width of one plotter's LeftPanel equal to other plotter's LeftPanel.
/// </summary>
public class WidthSpring : FrameworkElement, IPlotterElement {
  /// <summary>
  /// Initializes a new instance of the <see cref="WidthSpring"/> class.
  /// </summary>
  public WidthSpring() {
  }

  #region Properties

  /// <summary>
  /// Gets or sets panel which is a source of width.
  /// </summary>
  /// <value>The source panel.</value>
  public Panel SourcePanel {
    get => (Panel)GetValue(dp: SourcePanelProperty);
    set => SetValue(dp: SourcePanelProperty, value: value);
  }

  public static readonly DependencyProperty SourcePanelProperty = DependencyProperty.Register(
    name: nameof(SourcePanel),
    propertyType: typeof(Panel),
    ownerType: typeof(WidthSpring),
    typeMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnSourcePanelReplaced));

  private static void OnSourcePanelReplaced(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var owner = (WidthSpring)d;
    owner.OnSourcePanelReplaced(prevPanel: (Panel)e.OldValue, currPanel: (Panel)e.NewValue);
  }

  private void OnSourcePanelReplaced(Panel prevPanel, Panel currPanel) {
    if(prevPanel != null) {
      prevPanel.SizeChanged -= OnPanel_SizeChanged;
    }
    if(currPanel != null) {
      currPanel.SizeChanged += OnPanel_SizeChanged;
    }
    UpdateWidth();
  }

  private void OnPanel_SizeChanged(object sender, SizeChangedEventArgs e) {
    UpdateWidth();
  }

  private void UpdateWidth() {
    if(Parent is Panel parentPanel && SourcePanel != null) {
      Width = Math.Max(val1: SourcePanel.ActualWidth - (parentPanel.ActualWidth - ActualWidth), val2: 0);
    }
  }

  #endregion // end of Properties

  #region IPlotterElement Members

  public void OnPlotterAttached(PlotterBase plotter) {
    Plotter = plotter;
    plotter.LeftPanel.Children.Insert(index: 0, element: this);
  }

  public void OnPlotterDetaching(PlotterBase plotter) {
    plotter.LeftPanel.Children.Remove(element: this);
    Plotter = null;
  }

  public PlotterBase Plotter { get; private set; }

  #endregion
}
