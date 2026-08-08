using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Crystal.Controls.Loading;

/// <summary>Lifecycle of a <see cref="LoadingHost"/>: spinning while its content warms, then either the live content or a failure marker.</summary>
public enum LoadingState {
  Loading,
  Ready,
  Failed,
}

/// <summary>
/// A dashboard-tile host that shows a spinner and component label while its backing singleton is
/// warmed on a background thread, then swaps in the real content — or a failure marker if warming
/// threw. Lets each module's tile appear immediately and fill in independently, so one slow
/// component (e.g. Storage blocked on disk IO) never blocks the rest of the dashboard.
/// <para>
/// Templated control: its look lives in <c>Themes/LoadingHostStyles.xaml</c>. The live content is
/// set into <see cref="ContentControl.Content"/> once ready, so the template can present it.
/// </para>
/// </summary>
public sealed class LoadingHost : ContentControl {
  static LoadingHost() {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingHost),
        new FrameworkPropertyMetadata(typeof(LoadingHost)));
  }

  /// <summary>Identifies the <see cref="Label"/> dependency property.</summary>
  public static readonly DependencyProperty LabelProperty =
      DependencyProperty.Register(nameof(Label), typeof(string), typeof(LoadingHost),
          new FrameworkPropertyMetadata(string.Empty));

  /// <summary>Identifies the <see cref="State"/> dependency property.</summary>
  public static readonly DependencyProperty StateProperty =
      DependencyProperty.Register(nameof(State), typeof(LoadingState), typeof(LoadingHost),
          new FrameworkPropertyMetadata(LoadingState.Loading));

  /// <summary>Component name shown next to the spinner (e.g. "CPU", "Storage").</summary>
  public string Label {
    get => (string)GetValue(LabelProperty);
    set => SetValue(LabelProperty, value);
  }

  /// <summary>Current lifecycle state; drives which visual the template shows.</summary>
  public LoadingState State {
    get => (LoadingState)GetValue(StateProperty);
    set => SetValue(StateProperty, value);
  }

  /// <summary>
  /// Warms the tile's backing singleton on a background thread, then builds and shows its content
  /// back on the UI thread. If <paramref name="warm"/> or <paramref name="createContent"/> throws,
  /// the tile shows its failure marker instead of blocking or crashing the dashboard.
  /// </summary>
  /// <param name="warm">Heavy initialization to run off the UI thread (opens hardware sessions).</param>
  /// <param name="createContent">Builds the live view on the UI thread once warming succeeded.</param>
  public void Begin(Action warm, Func<object> createContent) {
    ArgumentNullException.ThrowIfNull(warm);
    ArgumentNullException.ThrowIfNull(createContent);

    Task.Run(() => {
      try {
        warm();
      }
      catch {
        Dispatcher.BeginInvoke(() => State = LoadingState.Failed);
        return;
      }

      Dispatcher.BeginInvoke(() => {
        try {
          Content = createContent();
          State = LoadingState.Ready;
        }
        catch {
          State = LoadingState.Failed;
        }
      });
    });
  }
}
