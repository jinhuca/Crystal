namespace Crystal.Themes.Controls.Dialogs;

public class LoginDialogSettings : CrystalDialogSettings
{
  private const string DefaultUsernameWatermark = "Username...";
  private const string DefaultPasswordWatermark = "Password...";
  private const string DefaultRememberCheckBoxText = "Remember";

  public LoginDialogSettings()
  {
    UsernameWatermark = DefaultUsernameWatermark;
    UsernameCharacterCasing = CharacterCasing.Normal;
    PasswordWatermark = DefaultPasswordWatermark;
    NegativeButtonVisibility = Visibility.Collapsed;
    ShouldHideUsername = false;
    AffirmativeButtonText = "Login";
    EnablePasswordPreview = false;
    RememberCheckBoxVisibility = Visibility.Collapsed;
    RememberCheckBoxText = DefaultRememberCheckBoxText;
    RememberCheckBoxChecked = false;
  }

  public string? InitialUsername { get; set; }

  public string? InitialPassword { get; set; }

  public string UsernameWatermark { get; set; }

  public CharacterCasing UsernameCharacterCasing { get; set; }

  public bool ShouldHideUsername { get; set; }

  public string PasswordWatermark { get; set; }

  public Visibility NegativeButtonVisibility { get; set; }

  public bool EnablePasswordPreview { get; set; }

  public Visibility RememberCheckBoxVisibility { get; set; }

  public string RememberCheckBoxText { get; set; }

  public bool RememberCheckBoxChecked { get; set; }
}