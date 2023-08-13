using System.Threading;

namespace Crystal.Themes.Controls.Dialogs;

public partial class InputDialog : CrystalDialogBase
{
  private CancellationTokenRegistration cancellationTokenRegistration;

  /// <summary>Identifies the <see cref="Message"/> dependency property.</summary>
  public static readonly DependencyProperty MessageProperty
    = DependencyProperty.Register(nameof(Message),
      typeof(string),
      typeof(InputDialog),
      new PropertyMetadata(default(string)));

  public string? Message
  {
    get => (string?)GetValue(MessageProperty);
    set => SetValue(MessageProperty, value);
  }

  /// <summary>Identifies the <see cref="Input"/> dependency property.</summary>
  public static readonly DependencyProperty InputProperty
    = DependencyProperty.Register(nameof(Input),
      typeof(string),
      typeof(InputDialog),
      new PropertyMetadata(default(string)));

  public string? Input
  {
    get => (string?)GetValue(InputProperty);
    set => SetValue(InputProperty, value);
  }

  /// <summary>Identifies the <see cref="AffirmativeButtonText"/> dependency property.</summary>
  public static readonly DependencyProperty AffirmativeButtonTextProperty
    = DependencyProperty.Register(nameof(AffirmativeButtonText),
      typeof(string),
      typeof(InputDialog),
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
      typeof(InputDialog),
      new PropertyMetadata("Cancel"));

  public string NegativeButtonText
  {
    get => (string)GetValue(NegativeButtonTextProperty);
    set => SetValue(NegativeButtonTextProperty, value);
  }

  internal InputDialog()
    : this(null)
  {
  }

  internal InputDialog(CrystalWindow? parentWindow)
    : this(parentWindow, null)
  {
  }

  internal InputDialog(CrystalWindow? parentWindow, CrystalDialogSettings? settings)
    : base(parentWindow, settings)
  {
    InitializeComponent();
  }

  private RoutedEventHandler? negativeHandler = null;
  private KeyEventHandler? negativeKeyHandler = null;
  private RoutedEventHandler? affirmativeHandler = null;
  private KeyEventHandler? affirmativeKeyHandler = null;
  private KeyEventHandler? escapeKeyHandler = null;

  internal Task<string?> WaitForButtonPressAsync()
  {
    Dispatcher.BeginInvoke(new Action(() =>
    {
      Focus();
      PART_TextBox.Focus();
    }));

    var tcs = new TaskCompletionSource<string?>();

    void CleanUpHandlers()
    {
      PART_TextBox.KeyDown -= affirmativeKeyHandler;

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

        tcs.TrySetResult(Input!);
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

      tcs.TrySetResult(Input!);

      e.Handled = true;
    };

    PART_NegativeButton.KeyDown += negativeKeyHandler;
    PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;

    PART_TextBox.KeyDown += affirmativeKeyHandler;

    KeyDown += escapeKeyHandler;

    PART_NegativeButton.Click += negativeHandler;
    PART_AffirmativeButton.Click += affirmativeHandler;

    return tcs.Task;
  }

  protected override void OnLoaded()
  {
    AffirmativeButtonText = DialogSettings.AffirmativeButtonText;
    NegativeButtonText = DialogSettings.NegativeButtonText;

    switch (DialogSettings.ColorScheme)
    {
      case CrystalDialogColorScheme.Accented:
        PART_NegativeButton.SetResourceReference(StyleProperty, "Crystal.Styles.Button.Dialogs.AccentHighlight");
        PART_TextBox.SetResourceReference(ForegroundProperty, "Crystal.Brushes.ThemeForeground");
        PART_TextBox.SetResourceReference(ControlsHelper.FocusBorderBrushProperty, "Crystal.Brushes.TextBox.Border.Focus");
        break;
    }
  }
}