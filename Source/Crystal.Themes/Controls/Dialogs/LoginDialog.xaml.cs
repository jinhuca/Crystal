using System.Threading;

namespace Crystal.Themes.Controls.Dialogs;

public partial class LoginDialog : CrystalDialogBase
{
  private CancellationTokenRegistration cancellationTokenRegistration;

  /// <summary>Identifies the <see cref="Message"/> dependency property.</summary>
  public static readonly DependencyProperty MessageProperty
    = DependencyProperty.Register(nameof(Message),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata(default(string)));

  public string? Message
  {
    get => (string?)GetValue(MessageProperty);
    set => SetValue(MessageProperty, value);
  }

  /// <summary>Identifies the <see cref="Username"/> dependency property.</summary>
  public static readonly DependencyProperty UsernameProperty
    = DependencyProperty.Register(nameof(Username),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata(default(string)));

  public string? Username
  {
    get => (string?)GetValue(UsernameProperty);
    set => SetValue(UsernameProperty, value);
  }

  /// <summary>Identifies the <see cref="UsernameWatermark"/> dependency property.</summary>
  public static readonly DependencyProperty UsernameWatermarkProperty
    = DependencyProperty.Register(nameof(UsernameWatermark),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata(default(string)));

  public string? UsernameWatermark
  {
    get => (string?)GetValue(UsernameWatermarkProperty);
    set => SetValue(UsernameWatermarkProperty, value);
  }

  /// <summary>Identifies the <see cref="UsernameCharacterCasing"/> dependency property.</summary>
  public static readonly DependencyProperty UsernameCharacterCasingProperty
    = DependencyProperty.Register(nameof(UsernameCharacterCasing),
      typeof(CharacterCasing),
      typeof(LoginDialog),
      new PropertyMetadata(default(CharacterCasing)));

  public CharacterCasing UsernameCharacterCasing
  {
    get => (CharacterCasing)GetValue(UsernameCharacterCasingProperty);
    set => SetValue(UsernameCharacterCasingProperty, value);
  }

  /// <summary>Identifies the <see cref="Password"/> dependency property.</summary>
  public static readonly DependencyProperty PasswordProperty
    = DependencyProperty.Register(nameof(Password),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata(default(string)));

  public string? Password
  {
    get => (string?)GetValue(PasswordProperty);
    set => SetValue(PasswordProperty, value);
  }

  /// <summary>Identifies the <see cref="PasswordWatermark"/> dependency property.</summary>
  public static readonly DependencyProperty PasswordWatermarkProperty
    = DependencyProperty.Register(nameof(PasswordWatermark),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata(default(string)));

  public string? PasswordWatermark
  {
    get => (string?)GetValue(PasswordWatermarkProperty);
    set => SetValue(PasswordWatermarkProperty, value);
  }

  /// <summary>Identifies the <see cref="AffirmativeButtonText"/> dependency property.</summary>
  public static readonly DependencyProperty AffirmativeButtonTextProperty
    = DependencyProperty.Register(nameof(AffirmativeButtonText),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata("OK"));

  public string AffirmativeButtonText
  {
    get => (string)GetValue(AffirmativeButtonTextProperty);
    set => SetValue(AffirmativeButtonTextProperty, value);
  }

  /// <summary>Identifies the <see cref="NegativeButtonText"/> dependency property.</summary>
  public static readonly DependencyProperty NegativeButtonTextProperty
    = DependencyProperty.Register(nameof(NegativeButtonText),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata("Cancel"));

  public string NegativeButtonText
  {
    get => (string)GetValue(NegativeButtonTextProperty);
    set => SetValue(NegativeButtonTextProperty, value);
  }

  /// <summary>Identifies the <see cref="NegativeButtonButtonVisibility"/> dependency property.</summary>
  public static readonly DependencyProperty NegativeButtonButtonVisibilityProperty
    = DependencyProperty.Register(nameof(NegativeButtonButtonVisibility),
      typeof(Visibility),
      typeof(LoginDialog),
      new PropertyMetadata(Visibility.Collapsed));

  public Visibility NegativeButtonButtonVisibility
  {
    get => (Visibility)GetValue(NegativeButtonButtonVisibilityProperty);
    set => SetValue(NegativeButtonButtonVisibilityProperty, value);
  }

  /// <summary>Identifies the <see cref="ShouldHideUsername"/> dependency property.</summary>
  public static readonly DependencyProperty ShouldHideUsernameProperty
    = DependencyProperty.Register(nameof(ShouldHideUsername),
      typeof(bool),
      typeof(LoginDialog),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  public bool ShouldHideUsername
  {
    get => (bool)GetValue(ShouldHideUsernameProperty);
    set => SetValue(ShouldHideUsernameProperty, BooleanBoxes.Box(value));
  }

  /// <summary>Identifies the <see cref="RememberCheckBoxVisibility"/> dependency property.</summary>
  public static readonly DependencyProperty RememberCheckBoxVisibilityProperty
    = DependencyProperty.Register(nameof(RememberCheckBoxVisibility),
      typeof(Visibility),
      typeof(LoginDialog),
      new PropertyMetadata(Visibility.Collapsed));

  public Visibility RememberCheckBoxVisibility
  {
    get => (Visibility)GetValue(RememberCheckBoxVisibilityProperty);
    set => SetValue(RememberCheckBoxVisibilityProperty, value);
  }

  /// <summary>Identifies the <see cref="RememberCheckBoxText"/> dependency property.</summary>
  public static readonly DependencyProperty RememberCheckBoxTextProperty
    = DependencyProperty.Register(nameof(RememberCheckBoxText),
      typeof(string),
      typeof(LoginDialog),
      new PropertyMetadata("Remember"));

  public string RememberCheckBoxText
  {
    get => (string)GetValue(RememberCheckBoxTextProperty);
    set => SetValue(RememberCheckBoxTextProperty, value);
  }

  /// <summary>Identifies the <see cref="RememberCheckBoxChecked"/> dependency property.</summary>
  public static readonly DependencyProperty RememberCheckBoxCheckedProperty
    = DependencyProperty.Register(nameof(RememberCheckBoxChecked),
      typeof(bool),
      typeof(LoginDialog),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  public bool RememberCheckBoxChecked
  {
    get => (bool)GetValue(RememberCheckBoxCheckedProperty);
    set => SetValue(RememberCheckBoxCheckedProperty, BooleanBoxes.Box(value));
  }

  internal LoginDialog()
    : this(null)
  {
  }

  internal LoginDialog(CrystalWindow? parentWindow)
    : this(parentWindow, null)
  {
  }

  internal LoginDialog(CrystalWindow? parentWindow, LoginDialogSettings? settings)
    : base(parentWindow, settings ??= new LoginDialogSettings())
  {
    InitializeComponent();

    Username = settings.InitialUsername;
    Password = settings.InitialPassword;
    UsernameCharacterCasing = settings.UsernameCharacterCasing;
    UsernameWatermark = settings.UsernameWatermark;
    PasswordWatermark = settings.PasswordWatermark;
    NegativeButtonButtonVisibility = settings.NegativeButtonVisibility;
    ShouldHideUsername = settings.ShouldHideUsername;
    RememberCheckBoxVisibility = settings.RememberCheckBoxVisibility;
    RememberCheckBoxText = settings.RememberCheckBoxText;
    RememberCheckBoxChecked = settings.RememberCheckBoxChecked;
  }

  private RoutedEventHandler? negativeHandler = null;
  private KeyEventHandler? negativeKeyHandler = null;
  private RoutedEventHandler? affirmativeHandler = null;
  private KeyEventHandler? affirmativeKeyHandler = null;
  private KeyEventHandler? escapeKeyHandler = null;

  internal Task<LoginDialogData?> WaitForButtonPressAsync()
  {
    Dispatcher.BeginInvoke(new Action(() =>
    {
      Focus();
      if (string.IsNullOrEmpty(PART_TextBox.Text) && !ShouldHideUsername)
      {
        PART_TextBox.Focus();
      }
      else
      {
        PART_TextBox2.Focus();
      }
    }));

    var tcs = new TaskCompletionSource<LoginDialogData?>();

    void CleanUpHandlers()
    {
      PART_TextBox.KeyDown -= affirmativeKeyHandler;
      PART_TextBox2.KeyDown -= affirmativeKeyHandler;

      KeyDown -= escapeKeyHandler;

      PART_NegativeButton.Click -= negativeHandler;
      PART_AffirmativeButton.Click -= affirmativeHandler;

      PART_NegativeButton.KeyDown -= negativeKeyHandler;
      PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;

      cancellationTokenRegistration.Dispose();
    }

    cancellationTokenRegistration = DialogSettings
      .CancellationToken
      .Register(() =>
      {
        this.BeginInvoke(() =>
        {
          CleanUpHandlers();
          tcs.TrySetResult(null!);
        });
      });

    escapeKeyHandler = (_, e) =>
    {
      if (e.Key == Key.Escape || (e.Key == Key.System && e.SystemKey == Key.F4))
      {
        CleanUpHandlers();

        tcs.TrySetResult(null!);
      }
    };

    negativeKeyHandler = (_, e) =>
    {
      if (e.Key == Key.Enter)
      {
        CleanUpHandlers();

        tcs.TrySetResult(null!);
      }
    };

    affirmativeKeyHandler = (_, e) =>
    {
      if (e.Key == Key.Enter)
      {
        CleanUpHandlers();
        tcs.TrySetResult(new LoginDialogData
        {
          Username = Username,
          SecurePassword = PART_TextBox2.SecurePassword,
          ShouldRemember = RememberCheckBoxChecked
        });
      }
    };

    negativeHandler = (_, e) =>
    {
      CleanUpHandlers();

      tcs.TrySetResult(null!);

      e.Handled = true;
    };

    affirmativeHandler = (_, e) =>
    {
      CleanUpHandlers();

      tcs.TrySetResult(new LoginDialogData
      {
        Username = Username,
        SecurePassword = PART_TextBox2.SecurePassword,
        ShouldRemember = RememberCheckBoxChecked
      });

      e.Handled = true;
    };

    PART_NegativeButton.KeyDown += negativeKeyHandler;
    PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

    PART_TextBox.KeyDown += affirmativeKeyHandler;
    PART_TextBox2.KeyDown += affirmativeKeyHandler;

    KeyDown += escapeKeyHandler;

    PART_NegativeButton.Click += negativeHandler;
    PART_AffirmativeButton.Click += affirmativeHandler;

    return tcs.Task;
  }

  protected override void OnLoaded()
  {
    if (DialogSettings is LoginDialogSettings settings && settings.EnablePasswordPreview)
    {
      var win8CrystalPasswordStyle = FindResource("Crystal.Styles.PasswordBox.Win8") as Style;
      if (win8CrystalPasswordStyle != null)
      {
        PART_TextBox2.Style = win8CrystalPasswordStyle;
        // apply template again to fire the loaded event which is necessary for revealed password
        PART_TextBox2.ApplyTemplate();
      }
    }

    AffirmativeButtonText = DialogSettings.AffirmativeButtonText;
    NegativeButtonText = DialogSettings.NegativeButtonText;

    switch (DialogSettings.ColorScheme)
    {
      case CrystalDialogColorScheme.Accented:
        PART_NegativeButton.SetResourceReference(StyleProperty, "Crystal.Styles.Button.Dialogs.AccentHighlight");
        PART_TextBox.SetResourceReference(ForegroundProperty, "Crystal.Brushes.ThemeForeground");
        PART_TextBox2.SetResourceReference(ForegroundProperty, "Crystal.Brushes.ThemeForeground");
        break;
    }
  }
}