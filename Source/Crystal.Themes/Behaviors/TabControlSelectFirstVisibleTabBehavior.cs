using System.Windows.Controls.Primitives;
using Microsoft.Xaml.Behaviors;

namespace Crystal.Themes.Behaviors
{
  /// <summary>
  /// <para>
  ///     Sets the first TabItem with Visibility="<see cref="Visibility.Visible"/>" as
  ///     the SelectedItem of the TabControl.
  /// </para>
  /// <para>
  ///     If there is no visible TabItem, null is set as the SelectedItem
  /// </para>
  /// </summary>
  public class TabControlSelectFirstVisibleTabBehavior : Behavior<TabControl>
  {
    protected override void OnAttached()
    {
      base.OnAttached();

      AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
      // We don't need select the TabItem if the selected item is already visible
      if (AssociatedObject.SelectedItem is TabItem selectedItem && selectedItem.Visibility == Visibility.Visible)
      {
        return;
      }

      // Get the first visible item
      var tabItems = AssociatedObject.Items.OfType<TabItem>().ToList();
      var firstVisible = tabItems.FirstOrDefault(t => t.Visibility == Visibility.Visible);

      if (firstVisible != null)
      {
        AssociatedObject.SetCurrentValue(Selector.SelectedIndexProperty, tabItems.IndexOf(firstVisible));
      }
      else
      {
        // There is no visible item
        // Raises SelectionChanged again one time (second time, oldValue == newValue)
        AssociatedObject.SetCurrentValue(Selector.SelectedItemProperty, null);
      }
    }

    protected override void OnDetaching()
    {
      AssociatedObject.SelectionChanged -= OnSelectionChanged;

      base.OnDetaching();
    }
  }
}