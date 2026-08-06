using System.Windows;

namespace ProcessModule.Views;

/// <summary>
/// Attached property that names the row-view-model property a <c>GridViewColumn</c> sorts by. The
/// header click handler reads it to tell the view-model which column was clicked, keeping the sort
/// key next to the column definition in XAML instead of hard-coded in code-behind.
/// </summary>
public static class GridViewSort {
  public static readonly DependencyProperty SortPropertyProperty =
      DependencyProperty.RegisterAttached(
          "SortProperty", typeof(string), typeof(GridViewSort), new PropertyMetadata(null));

  public static string? GetSortProperty(DependencyObject obj) => (string?)obj.GetValue(SortPropertyProperty);
  public static void SetSortProperty(DependencyObject obj, string? value) => obj.SetValue(SortPropertyProperty, value);
}
