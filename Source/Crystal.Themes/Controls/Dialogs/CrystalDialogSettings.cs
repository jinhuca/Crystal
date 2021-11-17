﻿using System.Threading;

namespace Crystal.Themes.Controls.Dialogs
{
  /// <summary>
  /// A class that represents the settings used by CrystalDialogs.
  /// </summary>
  public class CrystalDialogSettings
  {
    public CrystalDialogSettings()
    {
      OwnerCanCloseWithDialog = false;

      AffirmativeButtonText = "OK";
      NegativeButtonText = "Cancel";

      ColorScheme = CrystalDialogColorScheme.Theme;
      AnimateShow = AnimateHide = true;

      MaximumBodyHeight = double.NaN;

      DefaultText = "";
      DefaultButtonFocus = MessageDialogResult.Negative;
      CancellationToken = CancellationToken.None;
      DialogTitleFontSize = double.NaN;
      DialogMessageFontSize = double.NaN;
      DialogButtonFontSize = double.NaN;
      DialogResultOnCancel = null;
    }

    /// <summary>
    /// Gets or sets whether the owner of the dialog can be closed.
    /// </summary>
    public bool OwnerCanCloseWithDialog { get; set; }

    /// <summary>
    /// Gets or sets the text used for the Affirmative button. For example: "OK" or "Yes".
    /// </summary>
    public string AffirmativeButtonText { get; set; }

    /// <summary>
    /// Enable or disable dialog hiding animation
    /// "True" - play hiding animation.
    /// "False" - skip hiding animation.
    /// </summary>
    public bool AnimateHide { get; set; }

    /// <summary>
    /// Enable or disable dialog showing animation.
    /// "True" - play showing animation.
    /// "False" - skip showing animation.
    /// </summary>
    public bool AnimateShow { get; set; }

    /// <summary>
    /// Gets or sets a token to cancel the dialog.
    /// </summary>
    public CancellationToken CancellationToken { get; set; }

    /// <summary>
    /// Gets or sets whether the Crystal dialog should use the default black/white appearance (theme) or try to use the current accent.
    /// </summary>
    public CrystalDialogColorScheme ColorScheme { get; set; }

    /// <summary>
    /// Gets or sets a custom resource dictionary which can contains custom styles, brushes or something else.
    /// </summary>
    public ResourceDictionary? CustomResourceDictionary { get; set; }

    /// <summary>
    /// Gets or sets which button should be focused by default
    /// </summary>
    public MessageDialogResult DefaultButtonFocus { get; set; }

    /// <summary>
    /// Gets or sets the default text for <see cref="InputDialog"/>.
    /// </summary>
    public string DefaultText { get; set; }

    /// <summary>
    /// Gets or sets the size of the dialog message font.
    /// </summary>
    /// <value>
    /// The size of the dialog message font.
    /// </value>
    public double DialogMessageFontSize { get; set; }

    /// <summary>
    /// Gets or sets the size of the dialog button font.
    /// </summary>
    /// <value>
    /// The size of the dialog button font.
    /// </value>
    public double DialogButtonFontSize { get; set; }

    /// <summary>
    /// Gets or sets the dialog result when the user cancelled the dialog with 'ESC' key
    /// </summary>
    /// <remarks>If the value is <see langword="null"/> the default behavior is determined 
    /// by the <see cref="MessageDialogStyle"/>.
    /// <table>
    /// <tr><td><see cref="MessageDialogStyle"/></td><td><see cref="MessageDialogResult"/></td></tr>
    /// <tr><td><see cref="MessageDialogStyle.Affirmative"/></td><td><see cref="MessageDialogResult.Affirmative"/></td></tr>
    /// <tr><td>
    /// <list type="bullet">
    /// <item><see cref="MessageDialogStyle.AffirmativeAndNegative"/></item>
    /// <item><see cref="MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary"/></item>
    /// <item><see cref="MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary"/></item>
    /// </list></td>
    /// <td><see cref="MessageDialogResult.Negative"/></td></tr></table></remarks>
    public MessageDialogResult? DialogResultOnCancel { get; set; }

    /// <summary>
    /// Gets or sets the size of the dialog title font.
    /// </summary>
    /// <value>
    /// The size of the dialog title font.
    /// </value>
    public double DialogTitleFontSize { get; set; }

    /// <summary>
    /// Gets or sets the text used for the first auxiliary button.
    /// </summary>
    public string? FirstAuxiliaryButtonText { get; set; }

    /// <summary>
    /// Gets or sets the maximum height. (Default is unlimited height, <a href="http://msdn.microsoft.com/de-de/library/system.double.nan">Double.NaN</a>)
    /// </summary>
    public double MaximumBodyHeight { get; set; }

    /// <summary>
    /// Gets or sets the text used for the Negative button. For example: "Cancel" or "No".
    /// </summary>
    public string NegativeButtonText { get; set; }

    /// <summary>
    /// Gets or sets the text used for the second auxiliary button.
    /// </summary>
    public string? SecondAuxiliaryButtonText { get; set; }
  }
}