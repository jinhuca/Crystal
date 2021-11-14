using System.Threading;
using System.Windows.Input;
using ControlzEx;

namespace Crystal.Themes.Controls.Dialogs
{
  /// <summary>
  /// An internal control that represents a message dialog. Please use MetroWindow.ShowMessage instead!
  /// </summary>
  public partial class MessageDialog : BaseMetroDialog
  {
    private const string ACCENT_BUTTON_STYLE = "MahApps.Styles.Button.Dialogs.Accent";
    private const string ACCENT_HIGHLIGHT_BUTTON_STYLE = "MahApps.Styles.Button.Dialogs.AccentHighlight";
    private CancellationTokenRegistration cancellationTokenRegistration;

    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
      nameof(Message),
      typeof(string),
      typeof(MessageDialog),
      new PropertyMetadata(default(string)));

    public string? Message
    {
      get => (string?)GetValue(MessageProperty);
      set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty AffirmativeButtonTextProperty = DependencyProperty.Register(
      nameof(AffirmativeButtonText),
      typeof(string),
      typeof(MessageDialog),
      new PropertyMetadata("OK"));

    public string AffirmativeButtonText
    {
      get => (string)GetValue(AffirmativeButtonTextProperty);
      set => SetValue(AffirmativeButtonTextProperty, value);
    }

    public static readonly DependencyProperty NegativeButtonTextProperty = DependencyProperty.Register(
      nameof(NegativeButtonText),
      typeof(string),
      typeof(MessageDialog),
      new PropertyMetadata("Cancel"));

    public string NegativeButtonText
    {
      get => (string)GetValue(NegativeButtonTextProperty);
      set => SetValue(NegativeButtonTextProperty, value);
    }

    public static readonly DependencyProperty FirstAuxiliaryButtonTextProperty = DependencyProperty.Register(
      nameof(FirstAuxiliaryButtonText),
      typeof(string),
      typeof(MessageDialog),
      new PropertyMetadata(default(string)));

    public string? FirstAuxiliaryButtonText
    {
      get => (string?)GetValue(FirstAuxiliaryButtonTextProperty);
      set => SetValue(FirstAuxiliaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty SecondAuxiliaryButtonTextProperty = DependencyProperty.Register(
      nameof(SecondAuxiliaryButtonText),
      typeof(string),
      typeof(MessageDialog),
      new PropertyMetadata(default(string)));

    public string? SecondAuxiliaryButtonText
    {
      get => (string?)GetValue(SecondAuxiliaryButtonTextProperty);
      set => SetValue(SecondAuxiliaryButtonTextProperty, value);
    }

    public static readonly DependencyProperty ButtonStyleProperty = DependencyProperty.Register(
      nameof(ButtonStyle),
      typeof(MessageDialogStyle),
      typeof(MessageDialog),
      new PropertyMetadata(MessageDialogStyle.Affirmative, ButtonStylePropertyChangedCallback));

    private static void ButtonStylePropertyChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
    {
      if (o is MessageDialog dialog)
      {
        SetButtonState(dialog);
      }
    }

    public MessageDialogStyle ButtonStyle
    {
      get => (MessageDialogStyle)GetValue(ButtonStyleProperty);
      set => SetValue(ButtonStyleProperty, value);
    }

    internal MessageDialog() : this(null)
    {
    }

    internal MessageDialog(CrystalWindow? parentWindow) : this(parentWindow, null)
    {
    }

    internal MessageDialog(CrystalWindow? parentWindow, MetroDialogSettings? settings) : base(parentWindow, settings)
    {
      InitializeComponent();

      SetCurrentValue(FirstAuxiliaryButtonTextProperty, "Cancel");
      SetCurrentValue(SecondAuxiliaryButtonTextProperty, "Cancel");

      PART_MessageScrollViewer.Height = DialogSettings.MaximumBodyHeight;
    }

    private RoutedEventHandler? negativeHandler = null;
    private KeyEventHandler? negativeKeyHandler = null;
    private RoutedEventHandler? affirmativeHandler = null;
    private KeyEventHandler? affirmativeKeyHandler = null;
    private RoutedEventHandler? firstAuxHandler = null;
    private KeyEventHandler? firstAuxKeyHandler = null;
    private RoutedEventHandler? secondAuxHandler = null;
    private KeyEventHandler? secondAuxKeyHandler = null;
    private KeyEventHandler? escapeKeyHandler = null;

    internal Task<MessageDialogResult> WaitForButtonPressAsync()
    {
      Dispatcher.BeginInvoke(new Action(() =>
      {
        Focus();

        var defaultButtonFocus = DialogSettings.DefaultButtonFocus;

        //Ensure it's a valid option
        if (!IsApplicable(defaultButtonFocus))
        {
          defaultButtonFocus = ButtonStyle == MessageDialogStyle.Affirmative
          ? MessageDialogResult.Affirmative
          : MessageDialogResult.Negative;
        }

        //kind of acts like a selective 'IsDefault' mechanism.
        switch (defaultButtonFocus)
        {
          case MessageDialogResult.Affirmative:
            PART_AffirmativeButton.SetResourceReference(StyleProperty, ACCENT_BUTTON_STYLE);
            KeyboardNavigationEx.Focus(PART_AffirmativeButton);
            break;
          case MessageDialogResult.Negative:
            PART_NegativeButton.SetResourceReference(StyleProperty, ACCENT_BUTTON_STYLE);
            KeyboardNavigationEx.Focus(PART_NegativeButton);
            break;
          case MessageDialogResult.FirstAuxiliary:
            PART_FirstAuxiliaryButton.SetResourceReference(StyleProperty, ACCENT_BUTTON_STYLE);
            KeyboardNavigationEx.Focus(PART_FirstAuxiliaryButton);
            break;
          case MessageDialogResult.SecondAuxiliary:
            PART_SecondAuxiliaryButton.SetResourceReference(StyleProperty, ACCENT_BUTTON_STYLE);
            KeyboardNavigationEx.Focus(PART_SecondAuxiliaryButton);
            break;
        }
      }));

      var tcs = new TaskCompletionSource<MessageDialogResult>();

      void CleanUpHandlers()
      {
        PART_NegativeButton.Click -= negativeHandler;
        PART_AffirmativeButton.Click -= affirmativeHandler;
        PART_FirstAuxiliaryButton.Click -= firstAuxHandler;
        PART_SecondAuxiliaryButton.Click -= secondAuxHandler;

        PART_NegativeButton.KeyDown -= negativeKeyHandler;
        PART_AffirmativeButton.KeyDown -= affirmativeKeyHandler;
        PART_FirstAuxiliaryButton.KeyDown -= firstAuxKeyHandler;
        PART_SecondAuxiliaryButton.KeyDown -= secondAuxKeyHandler;

        KeyDown -= escapeKeyHandler;

        cancellationTokenRegistration.Dispose();
      }

      cancellationTokenRegistration = DialogSettings
        .CancellationToken
        .Register(() =>
        {
          this.BeginInvoke(() =>
          {
            CleanUpHandlers();
            tcs.TrySetResult(ButtonStyle == MessageDialogStyle.Affirmative ? MessageDialogResult.Affirmative : MessageDialogResult.Negative);
          });
        });

      negativeKeyHandler = (_, e) =>
      {
        if (e.Key == Key.Enter)
        {
          CleanUpHandlers();
          tcs.TrySetResult(MessageDialogResult.Negative);
        }
      };

      affirmativeKeyHandler = (_, e) =>
      {
        if (e.Key == Key.Enter)
        {
          CleanUpHandlers();
          tcs.TrySetResult(MessageDialogResult.Affirmative);
        }
      };

      firstAuxKeyHandler = (_, e) =>
      {
        if (e.Key == Key.Enter)
        {
          CleanUpHandlers();
          tcs.TrySetResult(MessageDialogResult.FirstAuxiliary);
        }
      };

      secondAuxKeyHandler = (_, e) =>
      {
        if (e.Key == Key.Enter)
        {
          CleanUpHandlers();
          tcs.TrySetResult(MessageDialogResult.SecondAuxiliary);
        }
      };

      negativeHandler = (_, e) =>
      {
        CleanUpHandlers();
        tcs.TrySetResult(MessageDialogResult.Negative);
        e.Handled = true;
      };

      affirmativeHandler = (_, e) =>
      {
        CleanUpHandlers();
        tcs.TrySetResult(MessageDialogResult.Affirmative);
        e.Handled = true;
      };

      firstAuxHandler = (_, e) =>
      {
        CleanUpHandlers();
        tcs.TrySetResult(MessageDialogResult.FirstAuxiliary);
        e.Handled = true;
      };

      secondAuxHandler = (_, e) =>
      {
        CleanUpHandlers();
        tcs.TrySetResult(MessageDialogResult.SecondAuxiliary);
        e.Handled = true;
      };

      escapeKeyHandler = (_, e) =>
      {
        if (e.Key == Key.Escape || (e.Key == Key.System && e.SystemKey == Key.F4))
        {
          CleanUpHandlers();
          tcs.TrySetResult(DialogSettings.DialogResultOnCancel ?? (ButtonStyle == MessageDialogStyle.Affirmative ? MessageDialogResult.Affirmative : MessageDialogResult.Negative));
        }
        else if (e.Key == Key.Enter)
        {
          CleanUpHandlers();
          tcs.TrySetResult(MessageDialogResult.Affirmative);
        }
      };

      PART_NegativeButton.KeyDown += negativeKeyHandler;
      PART_AffirmativeButton.KeyDown += affirmativeKeyHandler;
      PART_FirstAuxiliaryButton.KeyDown += firstAuxKeyHandler;
      PART_SecondAuxiliaryButton.KeyDown += secondAuxKeyHandler;

      PART_NegativeButton.Click += negativeHandler;
      PART_AffirmativeButton.Click += affirmativeHandler;
      PART_FirstAuxiliaryButton.Click += firstAuxHandler;
      PART_SecondAuxiliaryButton.Click += secondAuxHandler;

      KeyDown += escapeKeyHandler;

      return tcs.Task;
    }

    private static void SetButtonState(MessageDialog md)
    {
      if (md.PART_AffirmativeButton is null)
      {
        return;
      }

      switch (md.ButtonStyle)
      {
        case MessageDialogStyle.Affirmative:
          {
            md.PART_AffirmativeButton.Visibility = Visibility.Visible;
            md.PART_NegativeButton.Visibility = Visibility.Collapsed;
            md.PART_FirstAuxiliaryButton.Visibility = Visibility.Collapsed;
            md.PART_SecondAuxiliaryButton.Visibility = Visibility.Collapsed;
          }
          break;
        case MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary:
        case MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary:
        case MessageDialogStyle.AffirmativeAndNegative:
          {
            md.PART_AffirmativeButton.Visibility = Visibility.Visible;
            md.PART_NegativeButton.Visibility = Visibility.Visible;

            if (md.ButtonStyle == MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary || md.ButtonStyle == MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary)
            {
              md.PART_FirstAuxiliaryButton.Visibility = Visibility.Visible;
            }

            if (md.ButtonStyle == MessageDialogStyle.AffirmativeAndNegativeAndDoubleAuxiliary)
            {
              md.PART_SecondAuxiliaryButton.Visibility = Visibility.Visible;
            }
          }
          break;
      }

      md.AffirmativeButtonText = md.DialogSettings.AffirmativeButtonText;
      md.NegativeButtonText = md.DialogSettings.NegativeButtonText;
      md.FirstAuxiliaryButtonText = md.DialogSettings.FirstAuxiliaryButtonText;
      md.SecondAuxiliaryButtonText = md.DialogSettings.SecondAuxiliaryButtonText;

      switch (md.DialogSettings.ColorScheme)
      {
        case MetroDialogColorScheme.Accented:
          md.PART_AffirmativeButton.SetResourceReference(StyleProperty, ACCENT_HIGHLIGHT_BUTTON_STYLE);
          md.PART_NegativeButton.SetResourceReference(StyleProperty, ACCENT_HIGHLIGHT_BUTTON_STYLE);
          md.PART_FirstAuxiliaryButton.SetResourceReference(StyleProperty, ACCENT_HIGHLIGHT_BUTTON_STYLE);
          md.PART_SecondAuxiliaryButton.SetResourceReference(StyleProperty, ACCENT_HIGHLIGHT_BUTTON_STYLE);
          break;
      }
    }

    protected override void OnLoaded()
    {
      SetButtonState(this);
    }

    private void OnKeyCopyExecuted(object sender, ExecutedRoutedEventArgs e)
    {
      var message = Message;
      if (message != null)
      {
        Clipboard.SetDataObject(message);
      }
    }

    private bool IsApplicable(MessageDialogResult value)
    {
      return value switch
      {
        MessageDialogResult.Affirmative => PART_AffirmativeButton.IsVisible,
        MessageDialogResult.Negative => PART_NegativeButton.IsVisible,
        MessageDialogResult.FirstAuxiliary => PART_FirstAuxiliaryButton.IsVisible,
        MessageDialogResult.SecondAuxiliary => PART_SecondAuxiliaryButton.IsVisible,
        _ => false
      };
    }
  }
}