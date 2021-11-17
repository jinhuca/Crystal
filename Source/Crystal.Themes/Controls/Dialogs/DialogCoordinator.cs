namespace Crystal.Themes.Controls.Dialogs
{
  public class DialogCoordinator : IDialogCoordinator
  {
    /// <summary>
    /// Gets the default instance if the dialog coordinator, which can be injected into a view model.
    /// </summary>
    public static readonly IDialogCoordinator Instance = new DialogCoordinator();

    public Task<string?> ShowInputAsync(object context, string title, string message, CrystalDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.ShowInputAsync(title, message, settings));
    }

    public string? ShowModalInputExternal(object context, string title, string message, CrystalDialogSettings? metroDialogSettings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.ShowModalInputExternal(title, message, metroDialogSettings);
    }

    public Task<LoginDialogData?> ShowLoginAsync(object context, string title, string message, LoginDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.ShowLoginAsync(title, message, settings));
    }

    public LoginDialogData? ShowModalLoginExternal(object context, string title, string message, LoginDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.ShowModalLoginExternal(title, message, settings);
    }

    public Task<MessageDialogResult> ShowMessageAsync(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CrystalDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.ShowMessageAsync(title, message, style, settings));
    }

    public MessageDialogResult ShowModalMessageExternal(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CrystalDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.ShowModalMessageExternal(title, message, style, settings);
    }

    public Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message, bool isCancelable = false, CrystalDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.ShowProgressAsync(title, message, isCancelable, settings));
    }

    public Task ShowMetroDialogAsync(object context, BaseMetroDialog dialog, CrystalDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.ShowMetroDialogAsync(dialog, settings));
    }

    public Task HideMetroDialogAsync(object context, BaseMetroDialog dialog, CrystalDialogSettings? settings = null)
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.HideMetroDialogAsync(dialog, settings));
    }

    public Task<TDialog?> GetCurrentDialogAsync<TDialog>(object context)
        where TDialog : BaseMetroDialog
    {
      var metroWindow = GetMetroWindow(context);
      return metroWindow.Invoke(() => metroWindow.GetCurrentDialogAsync<TDialog>());
    }

    private static CrystalWindow GetMetroWindow(object context)
    {
      if (context is null)
      {
        throw new ArgumentNullException(nameof(context));
      }

      if (DialogParticipation.IsRegistered(context) == false)
      {
        throw new InvalidOperationException($"The context `{context}` is not registered. Consider using the DialogParticipation.Register property in XAML to bind in the DataContext.");
      }

      var association = DialogParticipation.GetAssociation(context);
      var metroWindow = association.Invoke(() => Window.GetWindow(association) as CrystalWindow);
      if (metroWindow is null)
      {
        throw new InvalidOperationException($"The context `{context}` is not inside a MetroWindow.");
      }

      return metroWindow;
    }
  }
}