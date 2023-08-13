using System.Threading;

namespace Crystal.Themes.Controls.Dialogs;

/// <summary>
/// A class for manipulating an open ProgressDialog.
/// </summary>
public class ProgressDialogController
{
  private CancellationTokenRegistration cancellationTokenRegistration;

  private ProgressDialog WrappedDialog { get; }

  private Func<Task> CloseCallback { get; }

  /// <summary>
  /// This event is raised when the associated <see cref="ProgressDialog"/> was closed programmatically.
  /// </summary>
  public event EventHandler? Closed;

  /// <summary>
  /// This event is raised when the associated <see cref="ProgressDialog"/> was cancelled by the user.
  /// </summary>
  public event EventHandler? Canceled;

  /// <summary>
  /// Gets if the Cancel button has been pressed.
  /// </summary>        
  public bool IsCanceled { get; private set; }

  /// <summary>
  /// Gets if the wrapped ProgressDialog is open.
  /// </summary>        
  public bool IsOpen { get; private set; }

  internal ProgressDialogController(ProgressDialog dialog, Func<Task> closeCallBack)
  {
    WrappedDialog = dialog;
    CloseCallback = closeCallBack;

    IsOpen = dialog.IsVisible;

    WrappedDialog.Invoke(() =>
    {
      WrappedDialog.KeyDown += WrappedDialog_KeyDown;
      WrappedDialog.PART_NegativeButton.Click += PART_NegativeButton_Click;
    });

    cancellationTokenRegistration = dialog.CancellationToken.Register(() => { WrappedDialog.BeginInvoke(Abort); });
  }

  private void WrappedDialog_KeyDown(object sender, KeyEventArgs e)
  {
    if (e.Key == Key.Escape || (e.Key == Key.System && e.SystemKey == Key.F4))
    {
      WrappedDialog.Invoke(Abort);
    }
  }

  private void PART_NegativeButton_Click(object sender, RoutedEventArgs e)
  {
    WrappedDialog.Invoke(Abort);
    e.Handled = true;
  }

  private void Abort()
  {
    if (WrappedDialog.IsCancelable)
    {
      WrappedDialog.PART_NegativeButton.IsEnabled = false;
      IsCanceled = true;
      Canceled?.Invoke(this, EventArgs.Empty);
    }
  }

  /// <summary>
  /// Sets the ProgressBar's IsIndeterminate to true. To set it to false, call SetProgress.
  /// </summary>
  public void SetIndeterminate()
  {
    WrappedDialog.Invoke(() => WrappedDialog.SetIndeterminate());
  }

  /// <summary>
  /// Sets if the Cancel button is visible.
  /// </summary>
  /// <param name="value"></param>
  public void SetCancelable(bool value)
  {
    WrappedDialog.Invoke(() => WrappedDialog.IsCancelable = value);
  }

  /// <summary>
  /// Sets the dialog's progress bar value and sets IsIndeterminate to false.
  /// </summary>
  /// <param name="value">The percentage to set as the value.</param>
  public void SetProgress(double value)
  {
    WrappedDialog.Invoke(() =>
    {
      if (value < WrappedDialog.Minimum || value > WrappedDialog.Maximum)
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      WrappedDialog.ProgressValue = value;
    });
  }

  /// <summary>
  ///  Gets/Sets the minimum restriction of the progress Value property.
  /// </summary>
  public double Minimum
  {
    get => WrappedDialog.Invoke(() => WrappedDialog.Minimum);
    set { WrappedDialog.Invoke(() => WrappedDialog.Minimum = value); }
  }

  /// <summary>
  ///  Gets/Sets the maximum restriction of the progress Value property.
  /// </summary>
  public double Maximum
  {
    get => WrappedDialog.Invoke(() => WrappedDialog.Maximum);
    set { WrappedDialog.Invoke(() => WrappedDialog.Maximum = value); }
  }

  /// <summary>
  /// Sets the dialog's message content.
  /// </summary>
  /// <param name="message">The message to be set.</param>
  public void SetMessage(string message)
  {
    WrappedDialog.Invoke(() => WrappedDialog.Message = message);
  }

  /// <summary>
  /// Sets the dialog's title.
  /// </summary>
  /// <param name="title">The title to be set.</param>
  public void SetTitle(string title)
  {
    WrappedDialog.Invoke(() => WrappedDialog.Title = title);
  }

  /// <summary>
  /// Sets the dialog's progress bar brush.
  /// </summary>
  /// <param name="brush">The brush to use for the progress bar's foreground.</param>
  public void SetProgressBarForegroundBrush(Brush brush)
  {
    WrappedDialog.Invoke(() => WrappedDialog.ProgressBarForeground = brush);
  }

  /// <summary>
  /// Begins an operation to close the ProgressDialog.
  /// </summary>
  /// <returns>A task representing the operation.</returns>
  public Task CloseAsync()
  {
    WrappedDialog.Invoke(() =>
    {
      if (!WrappedDialog.IsVisible)
      {
        throw new InvalidOperationException("Dialog isn't visible to close");
      }

      WrappedDialog.Dispatcher.VerifyAccess();
      WrappedDialog.KeyDown -= WrappedDialog_KeyDown;
      WrappedDialog.PART_NegativeButton.Click -= PART_NegativeButton_Click;

      cancellationTokenRegistration.Dispose();
    });

    return CloseCallback().ContinueWith(_ => WrappedDialog.Invoke(() =>
    {
      IsOpen = false;
      Closed?.Invoke(this, EventArgs.Empty);
    }));
  }
}