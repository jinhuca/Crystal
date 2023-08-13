﻿namespace Crystal;

/// <summary>
/// Helper class for MVVM.
/// </summary>
public static class MvvmHelpers
{
  internal static void AutowireViewModel(object viewOrViewModel)
  {
    if (viewOrViewModel is FrameworkElement { DataContext: null } view && ViewModelLocator.GetAutoWireViewModel(view) is null)
    {
      ViewModelLocator.SetAutoWireViewModel(view, true);
    }
  }

  /// <summary>
  /// Perform an <see cref="Action{T}"/> on a view and viewmodel.
  /// </summary>
  /// <remarks>
  /// The action will be performed on the view and its viewmodel if they implement <typeparamref name="T"/>.
  /// </remarks>
  /// <typeparam name="T">The <see cref="Action{T}"/> parameter type.</typeparam>
  /// <param name="view">The view to perform the <see cref="Action{T}"/> on.</param>
  /// <param name="action">The <see cref="Action{T}"/> to perform.</param>
  public static void ViewAndViewModelAction<T>(object view, Action<T> action) where T : class
  {
    if (view is T viewAsT)
    {
      action(viewAsT);
    }

    if (view is FrameworkElement { DataContext: T viewModelAsT })
    {
      action(viewModelAsT);
    }
  }

  /// <summary>
  /// Get an implementer from a view or viewmodel.
  /// </summary>
  /// <remarks>
  /// If the view implements <typeparamref name="T"/> it will be returned.
  /// Otherwise if the view's DataContext implements <typeparamref name="T"/> it will be returned instead.
  /// </remarks>
  /// <typeparam name="T">The implementer type to get.</typeparam>
  /// <param name="view">The view to get <typeparamref name="T"/> from.</param>
  /// <returns>view or viewmodel as <typeparamref name="T"/>.</returns>
  public static T GetImplementerFromViewOrViewModel<T>(object view) where T : class
  {
    return view switch
    {
      T viewAsT => viewAsT,
      FrameworkElement {DataContext: T vmAsT} => vmAsT,
      _ => null
    };
  }
}