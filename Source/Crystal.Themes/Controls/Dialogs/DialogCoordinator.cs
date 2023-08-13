namespace Crystal.Themes.Controls.Dialogs;

public class DialogCoordinator : IDialogCoordinator
{
  /// <summary>
  /// Gets the default instance if the dialog coordinator, which can be injected into a view model.
  /// </summary>
  public static readonly IDialogCoordinator Instance = new DialogCoordinator();

  public Task<string?> ShowInputAsync(object context, string title, string message, CrystalDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.ShowInputAsync(title, message, settings));
  }

  public string? ShowModalInputExternal(object context, string title, string message, CrystalDialogSettings? crystalDialogSettings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.ShowModalInputExternal(title, message, crystalDialogSettings);
  }

  public Task<LoginDialogData?> ShowLoginAsync(object context, string title, string message, LoginDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.ShowLoginAsync(title, message, settings));
  }

  public LoginDialogData? ShowModalLoginExternal(object context, string title, string message, LoginDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.ShowModalLoginExternal(title, message, settings);
  }

  public Task<MessageDialogResult> ShowMessageAsync(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CrystalDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.ShowMessageAsync(title, message, style, settings));
  }

  public MessageDialogResult ShowModalMessageExternal(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CrystalDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.ShowModalMessageExternal(title, message, style, settings);
  }

  public Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message, bool isCancelable = false, CrystalDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.ShowProgressAsync(title, message, isCancelable, settings));
  }

  public Task ShowCrystalDialogAsync(object context, CrystalDialogBase dialog, CrystalDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.ShowCrystalDialogAsync(dialog, settings));
  }

  public Task HideCrystalDialogAsync(object context, CrystalDialogBase dialog, CrystalDialogSettings? settings = null)
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.HideCrystalDialogAsync(dialog, settings));
  }

  public Task<TDialog?> GetCurrentDialogAsync<TDialog>(object context)
    where TDialog : CrystalDialogBase
  {
    var crystalWindow = GetCrystalWindow(context);
    return crystalWindow.Invoke(() => crystalWindow.GetCurrentDialogAsync<TDialog>());
  }

  private static CrystalWindow GetCrystalWindow(object context)
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
    var crystalWindow = association.Invoke(() => Window.GetWindow(association) as CrystalWindow);
    if (crystalWindow is null)
    {
      throw new InvalidOperationException($"The context `{context}` is not inside a CrystalWindow.");
    }

    return crystalWindow;
  }
}