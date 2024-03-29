﻿namespace Crystal.Themes.Controls.Dialogs;

/// <summary>
/// Use the dialog coordinator to help you interface with dialogs from a view model.
/// </summary>
public interface IDialogCoordinator
{
  /// <summary>
  /// Shows the input dialog.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the MessageDialog.</param>
  /// <param name="message">The message contained within the MessageDialog.</param>
  /// <param name="settings">Optional settings that override the global crystal dialog settings.</param>
  /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
  Task<string?> ShowInputAsync(object context, string title, string message, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Shows the input dialog.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the MessageDialog.</param>
  /// <param name="message">The message contained within the MessageDialog.</param>
  /// <param name="settings">Optional settings that override the global crystal dialog settings.</param>
  /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
  string? ShowModalInputExternal(object context, string title, string message, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Creates a LoginDialog inside of the current window.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the LoginDialog.</param>
  /// <param name="message">The message contained within the LoginDialog.</param>
  /// <param name="settings">Optional settings that override the global crystal dialog settings.</param>
  /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
  Task<LoginDialogData?> ShowLoginAsync(object context, string title, string message, LoginDialogSettings? settings = null);

  /// <summary>
  /// Creates a LoginDialog outside of the current window.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the LoginDialog.</param>
  /// <param name="message">The message contained within the LoginDialog.</param>
  /// <param name="settings">Optional settings that override the global crystal dialog settings.</param>
  /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
  LoginDialogData? ShowModalLoginExternal(object context, string title, string message, LoginDialogSettings? settings = null);

  /// <summary>
  /// Creates a MessageDialog inside of the current window.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the MessageDialog.</param>
  /// <param name="message">The message contained within the MessageDialog.</param>
  /// <param name="style">The type of buttons to use.</param>
  /// <param name="settings">Optional settings that override the global crystal dialog settings.</param>
  /// <returns>A task promising the result of which button was pressed.</returns>
  Task<MessageDialogResult> ShowMessageAsync(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Creates a MessageDialog outside of the current window.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the MessageDialog.</param>
  /// <param name="message">The message contained within the MessageDialog.</param>
  /// <param name="style">The type of buttons to use.</param>
  /// <param name="settings">Optional settings that override the global crystal dialog settings.</param>
  /// <returns>A task promising the result of which button was pressed.</returns>
  MessageDialogResult ShowModalMessageExternal(object context, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Creates a ProgressDialog inside of the current window.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="title">The title of the ProgressDialog.</param>
  /// <param name="message">The message within the ProgressDialog.</param>
  /// <param name="isCancelable">Determines if the cancel button is visible.</param>
  /// <param name="settings">Optional Settings that override the global crystal dialog settings.</param>
  /// <returns>A task promising the instance of ProgressDialogController for this operation.</returns>
  Task<ProgressDialogController> ShowProgressAsync(object context, string title, string message, bool isCancelable = false, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Adds a Crystal Dialog instance to the specified window and makes it visible asynchronously.        
  /// <para>You have to close the resulting dialog yourself with <see cref="HideCrystalDialogAsync"/>.</para>
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="dialog">The dialog instance itself.</param>
  /// <param name="settings">An optional pre-defined settings instance.</param>
  /// <returns>A task representing the operation.</returns>
  /// <exception cref="InvalidOperationException">The <paramref name="dialog"/> is already visible in the window.</exception>
  Task ShowCrystalDialogAsync(object context, CrystalDialogBase dialog, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Hides a visible Crystal Dialog instance.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  /// <param name="dialog">The dialog instance to hide.</param>
  /// <param name="settings">An optional pre-defined settings instance.</param>
  /// <returns>A task representing the operation.</returns>
  /// <exception cref="InvalidOperationException">
  /// The <paramref name="dialog"/> is not visible in the window.
  /// This happens if <see cref="ShowCrystalDialogAsync"/> hasn't been called before.
  /// </exception>
  Task HideCrystalDialogAsync(object context, CrystalDialogBase dialog, CrystalDialogSettings? settings = null);

  /// <summary>
  /// Gets the current shown dialog.
  /// </summary>
  /// <param name="context">Typically this should be the view model, which you register in XAML using <see cref="DialogParticipation.SetRegister"/>.</param>
  Task<TDialog?> GetCurrentDialogAsync<TDialog>(object context)
    where TDialog : CrystalDialogBase;
}