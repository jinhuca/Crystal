using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Crystal.Themes.Automation.Peers;
using Crystal.Themes.Theming;

namespace Crystal.Themes.Controls.Dialogs
{
  /// <summary>
  /// The base class for dialogs.
  ///
  /// You probably don't want to use this class, if you want to add arbitrary content to your dialog,
  /// use the <see cref="CustomDialog"/> class.
  /// </summary>
  [TemplatePart(Name = PART_Top, Type = typeof(ContentPresenter))]
  [TemplatePart(Name = PART_Content, Type = typeof(ContentPresenter))]
  [TemplatePart(Name = PART_Bottom, Type = typeof(ContentPresenter))]
  public abstract class BaseMetroDialog : ContentControl
  {
    private const string PART_Top = "PART_Top";
    private const string PART_Content = "PART_Content";
    private const string PART_Bottom = "PART_Bottom";

    /// <summary>Identifies the <see cref="DialogContentMargin"/> dependency property.</summary>
    public static readonly DependencyProperty DialogContentMarginProperty
        = DependencyProperty.Register(nameof(DialogContentMargin),
                                      typeof(GridLength),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(new GridLength(25, GridUnitType.Star)));

    /// <summary>
    /// Gets or sets the left and right margin for the dialog content.
    /// </summary>
    public GridLength DialogContentMargin
    {
      get => (GridLength)GetValue(DialogContentMarginProperty);
      set => SetValue(DialogContentMarginProperty, value);
    }

    /// <summary>Identifies the <see cref="DialogContentWidth"/> dependency property.</summary>
    public static readonly DependencyProperty DialogContentWidthProperty
        = DependencyProperty.Register(nameof(DialogContentWidth),
                                      typeof(GridLength),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(new GridLength(50, GridUnitType.Star)));

    /// <summary>
    /// Gets or sets the width for the dialog content.
    /// </summary>
    public GridLength DialogContentWidth
    {
      get => (GridLength)GetValue(DialogContentWidthProperty);
      set => SetValue(DialogContentWidthProperty, value);
    }

    /// <summary>Identifies the <see cref="Title"/> dependency property.</summary>
    public static readonly DependencyProperty TitleProperty
        = DependencyProperty.Register(nameof(Title),
                                      typeof(string),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(default(string)));

    /// <summary>
    /// Gets or sets the title of the dialog.
    /// </summary>
    public string? Title
    {
      get => (string?)GetValue(TitleProperty);
      set => SetValue(TitleProperty, value);
    }

    /// <summary>Identifies the <see cref="DialogTop"/> dependency property.</summary>
    public static readonly DependencyProperty DialogTopProperty
        = DependencyProperty.Register(nameof(DialogTop),
                                      typeof(object),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(null, UpdateLogicalChild));

    /// <summary>
    /// Gets or sets the content above the dialog.
    /// </summary>
    public object? DialogTop
    {
      get => GetValue(DialogTopProperty);
      set => SetValue(DialogTopProperty, value);
    }

    /// <summary>Identifies the <see cref="DialogBottom"/> dependency property.</summary>
    public static readonly DependencyProperty DialogBottomProperty
        = DependencyProperty.Register(nameof(DialogBottom),
                                      typeof(object),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(null, UpdateLogicalChild));

    /// <summary>
    /// Gets or sets the content below the dialog.
    /// </summary>
    public object? DialogBottom
    {
      get => GetValue(DialogBottomProperty);
      set => SetValue(DialogBottomProperty, value);
    }

    /// <summary>Identifies the <see cref="DialogTitleFontSize"/> dependency property.</summary>
    public static readonly DependencyProperty DialogTitleFontSizeProperty
        = DependencyProperty.Register(nameof(DialogTitleFontSize),
                                      typeof(double),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(26D));

    /// <summary>
    /// Gets or sets the font size of the dialog title.
    /// </summary>
    public double DialogTitleFontSize
    {
      get => (double)GetValue(DialogTitleFontSizeProperty);
      set => SetValue(DialogTitleFontSizeProperty, value);
    }

    /// <summary>Identifies the <see cref="DialogMessageFontSize"/> dependency property.</summary>
    public static readonly DependencyProperty DialogMessageFontSizeProperty
        = DependencyProperty.Register(nameof(DialogMessageFontSize),
                                      typeof(double),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(15D));

    /// <summary>
    /// Gets or sets the font size of the dialog message text.
    /// </summary>
    public double DialogMessageFontSize
    {
      get => (double)GetValue(DialogMessageFontSizeProperty);
      set => SetValue(DialogMessageFontSizeProperty, value);
    }

    /// <summary>Identifies the <see cref="DialogButtonFontSize"/> dependency property.</summary>
    public static readonly DependencyProperty DialogButtonFontSizeProperty
        = DependencyProperty.Register(nameof(DialogButtonFontSize),
                                      typeof(double),
                                      typeof(BaseMetroDialog),
                                      new PropertyMetadata(SystemFonts.MessageFontSize));

    /// <summary>
    /// Gets or sets the font size of any dialog buttons.
    /// </summary>
    public double DialogButtonFontSize
    {
      get => (double)GetValue(DialogButtonFontSizeProperty);
      set => SetValue(DialogButtonFontSizeProperty, value);
    }

    public CrystalDialogSettings DialogSettings { get; private set; } = null!;

    internal SizeChangedEventHandler? SizeChangedHandler { get; set; }

    static BaseMetroDialog()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseMetroDialog), new FrameworkPropertyMetadata(typeof(BaseMetroDialog)));
    }

    /// <summary>
    /// Initializes a new <see cref="BaseMetroDialog"/>.
    /// </summary>
    /// <param name="owningWindow">The window that is the parent of the dialog.</param>
    /// <param name="settings">The settings for the message dialog.</param>
    protected BaseMetroDialog(CrystalWindow? owningWindow, CrystalDialogSettings? settings)
    {
      Initialize(owningWindow, settings);
    }

    /// <summary>
    /// Initializes a new <see cref="BaseMetroDialog"/>.
    /// </summary>
    protected BaseMetroDialog()
        : this(null, new CrystalDialogSettings())
    {
    }

    private static void UpdateLogicalChild(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
      if (dependencyObject is not BaseMetroDialog dialog)
      {
        return;
      }

      if (e.OldValue is FrameworkElement oldChild)
      {
        dialog.RemoveLogicalChild(oldChild);
      }

      if (e.NewValue is FrameworkElement newChild)
      {
        dialog.AddLogicalChild(newChild);
        newChild.DataContext = dialog.DataContext;
      }
    }

    /// <inheritdoc />
    protected override IEnumerator LogicalChildren
    {
      get
      {
        // cheat, make a list with all logical content and return the enumerator
        ArrayList children = new ArrayList();
        if (DialogTop != null)
        {
          children.Add(DialogTop);
        }

        if (Content != null)
        {
          children.Add(Content);
        }

        if (DialogBottom != null)
        {
          children.Add(DialogBottom);
        }

        return children.GetEnumerator();
      }
    }

    /// <summary>
    /// With this method it's possible to return your own settings in a custom dialog.
    /// </summary>
    /// <param name="settings">
    /// Settings from the <see cref="CrystalWindow.MetroDialogOptions"/> or from constructor.
    /// The default is a new created settings.
    /// </param>
    /// <returns></returns>
    protected virtual CrystalDialogSettings ConfigureSettings(CrystalDialogSettings settings)
    {
      return settings;
    }

    private void Initialize(CrystalWindow? owningWindow, CrystalDialogSettings? settings)
    {
      OwningWindow = owningWindow;
      DialogSettings = ConfigureSettings(settings ?? (owningWindow?.MetroDialogOptions ?? new CrystalDialogSettings()));

      if (DialogSettings.CustomResourceDictionary != null)
      {
        Resources.MergedDictionaries.Add(DialogSettings.CustomResourceDictionary);
      }

      HandleThemeChange();

      DataContextChanged += BaseMetroDialogDataContextChanged;
      Loaded += BaseMetroDialogLoaded;
      Unloaded += BaseMetroDialogUnloaded;
    }

    private void BaseMetroDialogDataContextChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
      // Crystal add these content presenter to the dialog with AddLogicalChild method.
      // This has the side effect that the DataContext doesn't update, so do this now here.
      if (DialogTop is FrameworkElement elementTop)
      {
        elementTop.DataContext = DataContext;
      }

      if (DialogBottom is FrameworkElement elementBottom)
      {
        elementBottom.DataContext = DataContext;
      }
    }

    private void BaseMetroDialogLoaded(object? sender, RoutedEventArgs e)
    {
      ThemeManager.Current.ThemeChanged -= HandleThemeManagerThemeChanged;
      ThemeManager.Current.ThemeChanged += HandleThemeManagerThemeChanged;
      OnLoaded();
    }

    private void BaseMetroDialogUnloaded(object? sender, RoutedEventArgs e)
    {
      ThemeManager.Current.ThemeChanged -= HandleThemeManagerThemeChanged;
    }

    private void HandleThemeManagerThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
      this.Invoke(HandleThemeChange);
    }

    private static object? TryGetResource(Theme? theme, string key)
    {
      return theme?.Resources[key];
    }

    internal void HandleThemeChange()
    {
      var theme = DetectTheme(this);

      if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)
          || theme is null)
      {
        return;
      }

      switch (DialogSettings.ColorScheme)
      {
        case CrystalDialogColorScheme.Theme:
          ThemeManager.Current.ChangeTheme(this, Resources, theme);
          SetCurrentValue(BackgroundProperty, TryGetResource(theme, "Crystal.Brushes.Dialog.Background"));
          SetCurrentValue(ForegroundProperty, TryGetResource(theme, "Crystal.Brushes.Dialog.Foreground"));
          break;
        case CrystalDialogColorScheme.Inverted:
          theme = ThemeManager.Current.GetInverseTheme(theme);
          if (theme is null)
          {
            throw new InvalidOperationException("The inverse dialog theme only works if the window theme abides the naming convention. " +
                                                "See ThemeManager.GetInverseAppTheme for more infos");
          }

          ThemeManager.Current.ChangeTheme(this, Resources, theme);
          SetCurrentValue(BackgroundProperty, TryGetResource(theme, "Crystal.Brushes.Dialog.Background"));
          SetCurrentValue(ForegroundProperty, TryGetResource(theme, "Crystal.Brushes.Dialog.Foreground"));
          break;
        case CrystalDialogColorScheme.Accented:
          ThemeManager.Current.ChangeTheme(this, Resources, theme);
          SetCurrentValue(BackgroundProperty, TryGetResource(theme, "Crystal.Brushes.Dialog.Background.Accent"));
          SetCurrentValue(ForegroundProperty, TryGetResource(theme, "Crystal.Brushes.Dialog.Foreground.Accent"));
          break;
      }

      if (ParentDialogWindow != null)
      {
        ParentDialogWindow.SetCurrentValue(BackgroundProperty, Background);
        var glowBrush = TryGetResource(theme, "Crystal.Brushes.Dialog.Glow");
        if (glowBrush != null)
        {
          ParentDialogWindow.SetCurrentValue(CrystalWindow.GlowBrushProperty, glowBrush);
        }
      }
    }

    /// <summary>
    /// This is called in the loaded event.
    /// </summary>
    protected virtual void OnLoaded()
    {
      // nothing here
    }

    private static Theming.Theme? DetectTheme(BaseMetroDialog? dialog)
    {
      if (dialog is null)
      {
        return null;
      }

      // first look for owner
      var window = dialog.OwningWindow ?? dialog.TryFindParent<CrystalWindow>();
      var theme = window != null ? ThemeManager.Current.DetectTheme(window) : null;
      if (theme != null)
      {
        return theme;
      }

      // second try, look for main window and then for current application
      if (Application.Current != null)
      {
        theme = Application.Current.MainWindow is null
            ? ThemeManager.Current.DetectTheme(Application.Current)
            : ThemeManager.Current.DetectTheme(Application.Current.MainWindow);
        if (theme != null)
        {
          return theme;
        }
      }

      return null;
    }

    /// <summary>
    /// Waits for the dialog to become ready for interaction.
    /// </summary>
    /// <returns>A task that represents the operation and it's status.</returns>
    public Task WaitForLoadAsync()
    {
      Dispatcher.VerifyAccess();

      if (IsLoaded)
      {
        return new Task(() => { });
      }

      if (DialogSettings.AnimateShow != true)
      {
        Opacity = 1.0; //skip the animation
      }

      var tcs = new TaskCompletionSource<object>();

      void LoadedHandler(object sender, RoutedEventArgs args)
      {
        Loaded -= LoadedHandler;

        Focus();

        tcs.TrySetResult(null!);
      }

      Loaded += LoadedHandler;

      return tcs.Task;
    }

    /// <summary>
    /// Requests an externally shown Dialog to close. Will throw an exception if the Dialog is inside of a MetroWindow.
    /// </summary>
    public Task RequestCloseAsync()
    {
      if (OnRequestClose())
      {
        // Technically, the Dialog is /always/ inside of a MetroWindow.
        // If the dialog is inside of a user-created MetroWindow, not one created by the external dialog APIs.
        if (ParentDialogWindow is null)
        {
          // this is very bad, or the user called the close event before we can do this
          if (OwningWindow is null)
          {
            Trace.TraceWarning($"{this}: Can not request async closing, because the OwningWindow is already null. This can maybe happen if the dialog was closed manually.");
            return Task.Factory.StartNew(() => { });
          }

          // This is from a user-created MetroWindow
          return OwningWindow.HideMetroDialogAsync(this);
        }

        // This is from a MetroWindow created by the external dialog APIs.
        return WaitForCloseAsync()
                   .ContinueWith(_ => { ParentDialogWindow.Dispatcher.Invoke(() => { ParentDialogWindow.Close(); }); });
      }

      return Task.Factory.StartNew(() => { });
    }

    protected internal virtual void OnShown()
    {
    }

    protected internal virtual void OnClose()
    {
      // this is only set when a dialog is shown (externally) in it's OWN window.
      ParentDialogWindow?.Close();
    }

    /// <summary>
    /// A last chance virtual method for stopping an external dialog from closing.
    /// </summary>
    /// <returns></returns>
    protected internal virtual bool OnRequestClose()
    {
      return true; //allow the dialog to close.
    }

    /// <summary>
    /// Gets the window that owns the current Dialog IF AND ONLY IF the dialog is shown externally.
    /// </summary>
    protected internal Window? ParentDialogWindow { get; internal set; }

    /// <summary>
    /// Gets the window that owns the current Dialog IF AND ONLY IF the dialog is shown inside of a window.
    /// </summary>
    protected internal CrystalWindow? OwningWindow { get; internal set; }

    /// <summary>
    /// Waits until this dialog gets unloaded.
    /// </summary>
    /// <returns></returns>
    public Task WaitUntilUnloadedAsync()
    {
      var tcs = new TaskCompletionSource<object>();

      Unloaded += (_, _) => { tcs.TrySetResult(null!); };

      return tcs.Task;
    }

    public Task WaitForCloseAsync()
    {
      var tcs = new TaskCompletionSource<object>();

      if (DialogSettings.AnimateHide)
      {
        var closingStoryboard = TryFindResource("Crystal.Storyboard.Dialogs.Close") as Storyboard;

        if (closingStoryboard is null)
        {
          throw new InvalidOperationException("Unable to find the dialog closing storyboard. Did you forget to add BaseMetroDialog.xaml to your merged dictionaries?");
        }

        closingStoryboard = closingStoryboard.Clone();

        EventHandler? completedHandler = null;
        completedHandler = (_, _) =>
            {
              closingStoryboard.Completed -= completedHandler;

              tcs.TrySetResult(null!);
            };

        closingStoryboard.Completed += completedHandler;

        closingStoryboard.Begin(this);
      }
      else
      {
        Opacity = 0.0;
        tcs.TrySetResult(null!); //skip the animation
      }

      return tcs.Task;
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
      return new CrystalDialogAutomationPeer(this);
    }
  }
}