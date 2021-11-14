using System.Windows.Controls;
using ControlzEx.Theming;
using Crystal.Themes.ValueBoxes;
using JetBrains.Annotations;

namespace Crystal.Themes.Controls.Dialogs
{
  public static class DialogManager
  {
    /// <summary>
    /// Creates a LoginDialog inside of the current window.
    /// </summary>
    /// <param name="window">The window that is the parent of the dialog.</param>
    /// <param name="title">The title of the LoginDialog.</param>
    /// <param name="message">The message contained within the LoginDialog.</param>
    /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
    /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
    public static Task<LoginDialogData?> ShowLoginAsync(this CrystalWindow window, string title, string message, LoginDialogSettings? settings = null)
    {
      window.Dispatcher.VerifyAccess();

      settings ??= new LoginDialogSettings();

      return HandleOverlayOnShow(settings, window).ContinueWith(z =>
          {
            return (Task<LoginDialogData?>)window.Dispatcher.Invoke(new Func<Task<LoginDialogData?>>(() =>
                      {
                          //create the dialog control
                          LoginDialog dialog = new LoginDialog(window, settings)
                    {
                      Title = title,
                      Message = message
                    };

                    SetDialogFontSizes(settings, dialog);

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                              {
                            if (DialogOpened != null)
                            {
                              window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                            }

                            return dialog.WaitForButtonPressAsync().ContinueWith(y =>
                                      {
                                          //once a button as been clicked, begin removing the dialog.

                                          dialog.OnClose();

                                    if (DialogClosed != null)
                                    {
                                      window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                                    }

                                    Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog.WaitForCloseAsync()));
                                    return closingTask.ContinueWith(a =>
                                              {
                                            return ((Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                                      {
                                                    window.SizeChanged -= sizeHandler;

                                                    window.RemoveDialog(dialog);

                                                    return HandleOverlayOnHide(settings, window);
                                                  }))).ContinueWith(y3 => y).Unwrap();
                                          });
                                  }).Unwrap();
                          }).Unwrap().Unwrap();
                  }));
          }).Unwrap();
    }

    /// <summary>
    /// Creates a InputDialog inside of the current window.
    /// </summary>
    /// <param name="window">The MetroWindow</param>
    /// <param name="title">The title of the MessageDialog.</param>
    /// <param name="message">The message contained within the MessageDialog.</param>
    /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
    /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
    public static Task<string?> ShowInputAsync(this CrystalWindow window, string title, string message, MetroDialogSettings? settings = null)
    {
      window.Dispatcher.VerifyAccess();

      settings ??= window.MetroDialogOptions;

      return HandleOverlayOnShow(settings, window).ContinueWith(z =>
          {
            return (Task<string?>)window.Dispatcher.Invoke(new Func<Task<string?>>(() =>
                      {
                          //create the dialog control
                          var dialog = new InputDialog(window, settings)
                    {
                      Title = title,
                      Message = message,
                      Input = settings?.DefaultText,
                    };

                    SetDialogFontSizes(settings, dialog);

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                              {
                            if (DialogOpened != null)
                            {
                              window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                            }

                            return dialog.WaitForButtonPressAsync().ContinueWith(y =>
                                      {
                                          //once a button as been clicked, begin removing the dialog.

                                          dialog.OnClose();

                                    if (DialogClosed != null)
                                    {
                                      window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                                    }

                                    Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog.WaitForCloseAsync()));
                                    return closingTask.ContinueWith(a =>
                                              {
                                            return ((Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                                      {
                                                    window.SizeChanged -= sizeHandler;

                                                    window.RemoveDialog(dialog);

                                                    return HandleOverlayOnHide(settings, window);
                                                  }))).ContinueWith(y3 => y).Unwrap();
                                          });
                                  }).Unwrap();
                          }).Unwrap().Unwrap();
                  }));
          }).Unwrap();
    }

    /// <summary>
    /// Creates a MessageDialog inside of the current window.
    /// </summary>
    /// <param name="window">The MetroWindow</param>
    /// <param name="title">The title of the MessageDialog.</param>
    /// <param name="message">The message contained within the MessageDialog.</param>
    /// <param name="style">The type of buttons to use.</param>
    /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
    /// <returns>A task promising the result of which button was pressed.</returns>
    public static Task<MessageDialogResult> ShowMessageAsync(this CrystalWindow window, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
    {
      window.Dispatcher.VerifyAccess();

      settings ??= window.MetroDialogOptions;

      return HandleOverlayOnShow(settings, window).ContinueWith(z =>
          {
            return (Task<MessageDialogResult>)window.Dispatcher.Invoke(new Func<Task<MessageDialogResult>>(() =>
                      {
                          //create the dialog control
                          var dialog = new MessageDialog(window, settings)
                    {
                      Message = message,
                      Title = title,
                      ButtonStyle = style
                    };

                    SetDialogFontSizes(settings, dialog);

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                              {
                            if (DialogOpened != null)
                            {
                              window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                            }

                            return dialog.WaitForButtonPressAsync().ContinueWith(y =>
                                      {
                                          //once a button as been clicked, begin removing the dialog.

                                          dialog.OnClose();

                                    if (DialogClosed != null)
                                    {
                                      window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                                    }

                                    Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog.WaitForCloseAsync()));
                                    return closingTask.ContinueWith(a =>
                                              {
                                            return ((Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                                      {
                                                    window.SizeChanged -= sizeHandler;

                                                    window.RemoveDialog(dialog);

                                                    return HandleOverlayOnHide(settings, window);
                                                  }))).ContinueWith(y3 => y).Unwrap();
                                          });
                                  }).Unwrap();
                          }).Unwrap().Unwrap();
                  }));
          }).Unwrap();
    }

    /// <summary>
    /// Creates a ProgressDialog inside of the current window.
    /// </summary>
    /// <param name="window">The MetroWindow</param>
    /// <param name="title">The title of the ProgressDialog.</param>
    /// <param name="message">The message within the ProgressDialog.</param>
    /// <param name="isCancelable">Determines if the cancel button is visible.</param>
    /// <param name="settings">Optional Settings that override the global metro dialog settings.</param>
    /// <returns>A task promising the instance of ProgressDialogController for this operation.</returns>
    public static Task<ProgressDialogController> ShowProgressAsync(this CrystalWindow window, string title, string message, bool isCancelable = false, MetroDialogSettings? settings = null)
    {
      window.Dispatcher.VerifyAccess();

      settings ??= window.MetroDialogOptions;

      return HandleOverlayOnShow(settings, window).ContinueWith(z =>
          {
            return ((Task<ProgressDialogController>)window.Dispatcher.Invoke(new Func<Task<ProgressDialogController>>(() =>
                      {
                          //create the dialog control
                          var dialog = new ProgressDialog(window, settings)
                    {
                      Title = title,
                      Message = message,
                      IsCancelable = isCancelable
                    };

                    SetDialogFontSizes(settings, dialog);

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                              {
                            if (DialogOpened != null)
                            {
                              window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                            }

                            return new ProgressDialogController(dialog, () =>
                                      {
                                    dialog.OnClose();

                                    if (DialogClosed != null)
                                    {
                                      window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
                                    }

                                    Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(() => dialog.WaitForCloseAsync()));
                                    return closingTask.ContinueWith(a =>
                                              {
                                            return (Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                                                      {
                                                    window.SizeChanged -= sizeHandler;

                                                    window.RemoveDialog(dialog);

                                                    return HandleOverlayOnHide(settings, window);
                                                  }));
                                          }).Unwrap();
                                  });
                          });
                  })));
          }).Unwrap();
    }

    private static Task HandleOverlayOnHide(MetroDialogSettings? settings, CrystalWindow window)
    {
      if (window.metroActiveDialogContainer is null)
      {
        throw new InvalidOperationException("Active dialog container could not be found.");
      }

      Task? result = null;
      if (!window.metroActiveDialogContainer.Children.OfType<BaseMetroDialog>().Any())
      {
        result = (settings is null || settings.AnimateHide ? window.HideOverlayAsync() : Task.Factory.StartNew(() => window.Dispatcher.Invoke(new Action(window.HideOverlay))));
      }
      else
      {
        var tcs = new TaskCompletionSource<object>();
        tcs.SetResult(null!);
        result = tcs.Task;
      }

      result.ContinueWith(task =>
          {
            window.Invoke(() =>
                      {
                    if (window.metroActiveDialogContainer.Children.Count == 0)
                    {
                      window.SetValue(CrystalWindow.IsCloseButtonEnabledWithDialogPropertyKey, BooleanBoxes.TrueBox);
                      window.RestoreFocus();
                    }
                    else
                    {
                      var onTopShownDialogSettings = window.metroActiveDialogContainer.Children.OfType<BaseMetroDialog>().LastOrDefault()?.DialogSettings;
                      var isCloseButtonEnabled = window.ShowDialogsOverTitleBar || onTopShownDialogSettings is null || onTopShownDialogSettings.OwnerCanCloseWithDialog;
                      window.SetValue(CrystalWindow.IsCloseButtonEnabledWithDialogPropertyKey, BooleanBoxes.Box(isCloseButtonEnabled));
                    }
                  });
          });

      return result;
    }

    private static Task HandleOverlayOnShow(MetroDialogSettings? settings, CrystalWindow window)
    {
      return Task.Factory.StartNew(() =>
                     {
                       window.Invoke(() =>
                                 {
                               var isCloseButtonEnabled = window.ShowDialogsOverTitleBar || settings is null || settings.OwnerCanCloseWithDialog;
                               window.SetValue(CrystalWindow.IsCloseButtonEnabledWithDialogPropertyKey, BooleanBoxes.Box(isCloseButtonEnabled));
                             });
                     })
                 .ContinueWith(task =>
                     {
                       return window.Invoke(() =>
                                 {
                               if (window.metroActiveDialogContainer is null)
                               {
                                 throw new InvalidOperationException("Active dialog container could not be found.");
                               }

                               if (!window.metroActiveDialogContainer.Children.OfType<BaseMetroDialog>().Any())
                               {
                                 return (settings is null || settings.AnimateShow ? window.ShowOverlayAsync() : Task.Factory.StartNew(() => window.Dispatcher.Invoke(new Action(window.ShowOverlay))));
                               }
                               else
                               {
                                 var tcs = new TaskCompletionSource<object>();
                                 tcs.SetResult(null!);
                                 return tcs.Task;
                               }
                             });
                     })
                 .Unwrap();
    }

    /// <summary>
    /// Adds a Metro Dialog instance to the specified window and makes it visible asynchronously.
    /// If you want to wait until the user has closed the dialog, use <see cref="BaseMetroDialog.WaitUntilUnloadedAsync"/>
    /// <para>You have to close the resulting dialog yourself with <see cref="HideMetroDialogAsync"/>.</para>
    /// </summary>
    /// <param name="window">The owning window of the dialog.</param>
    /// <param name="dialog">The dialog instance itself.</param>
    /// <param name="settings">An optional pre-defined settings instance.</param>
    /// <returns>A task representing the operation.</returns>
    /// <exception cref="InvalidOperationException">The <paramref name="dialog"/> is already visible in the window.</exception>
    public static Task ShowMetroDialogAsync(this CrystalWindow window, BaseMetroDialog dialog, MetroDialogSettings? settings = null)
    {
      if (window is null)
      {
        throw new ArgumentNullException(nameof(window));
      }

      window.Dispatcher.VerifyAccess();

      if (dialog is null)
      {
        throw new ArgumentNullException(nameof(dialog));
      }

      if (window.metroActiveDialogContainer is null)
      {
        throw new InvalidOperationException("Active dialog container could not be found.");
      }

      if (window.metroInactiveDialogContainer is null)
      {
        throw new InvalidOperationException("Inactive dialog container could not be found.");
      }

      if (window.metroActiveDialogContainer.Children.Contains(dialog) || window.metroInactiveDialogContainer.Children.Contains(dialog))
      {
        throw new InvalidOperationException("The provided dialog is already visible in the specified window.");
      }

      settings ??= (dialog.DialogSettings ?? window.MetroDialogOptions);

      return HandleOverlayOnShow(settings, window).ContinueWith(z =>
          {
            return (Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                      {
                    SetDialogFontSizes(settings, dialog);

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                              {
                            dialog.OnShown();

                            if (DialogOpened != null)
                            {
                              window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                            }
                          });
                  }));
          }).Unwrap();
    }

    /// <summary>
    /// Adds a Metro Dialog instance of the given type to the specified window and makes it visible asynchronously.
    /// If you want to wait until the user has closed the dialog, use <see cref="BaseMetroDialog.WaitUntilUnloadedAsync"/>
    /// <para>You have to close the resulting dialog yourself with <see cref="HideMetroDialogAsync"/>.</para>
    /// </summary>
    /// <param name="window">The owning window of the dialog.</param>
    /// <param name="settings">An optional pre-defined settings instance.</param>
    /// <returns>A task with the dialog representing the operation.</returns>
    public static Task<TDialog> ShowMetroDialogAsync<TDialog>([NotNull] this CrystalWindow window, MetroDialogSettings? settings = null)
        where TDialog : BaseMetroDialog
    {
      if (window is null)
      {
        throw new ArgumentNullException(nameof(window));
      }

      window.Dispatcher.VerifyAccess();

      var dialog = (TDialog)Activator.CreateInstance(typeof(TDialog), window, settings)!;

      return HandleOverlayOnShow(dialog.DialogSettings, window).ContinueWith(z =>
          {
            return (Task<TDialog>)window.Dispatcher.Invoke(new Func<Task<TDialog>>(() =>
                      {
                    SetDialogFontSizes(dialog.DialogSettings, dialog);

                    SizeChangedEventHandler sizeHandler = SetupAndOpenDialog(window, dialog);
                    dialog.SizeChangedHandler = sizeHandler;

                    return dialog.WaitForLoadAsync().ContinueWith(x =>
                              {
                            dialog.OnShown();

                            if (DialogOpened != null)
                            {
                              window.Dispatcher.BeginInvoke(new Action(() => DialogOpened(window, new DialogStateChangedEventArgs())));
                            }
                          }).ContinueWith(x => dialog);
                  }));
          }).Unwrap();
    }

    /// <summary>
    /// Hides a visible Metro Dialog instance.
    /// </summary>
    /// <param name="window">The window with the dialog that is visible.</param>
    /// <param name="dialog">The dialog instance to hide.</param>
    /// <param name="settings">An optional pre-defined settings instance.</param>
    /// <returns>A task representing the operation.</returns>
    /// <exception cref="InvalidOperationException">
    /// The <paramref name="dialog"/> is not visible in the window.
    /// This happens if <see cref="ShowMetroDialogAsync"/> hasn't been called before.
    /// </exception>
    public static Task HideMetroDialogAsync(this CrystalWindow window, BaseMetroDialog dialog, MetroDialogSettings? settings = null)
    {
      window.Dispatcher.VerifyAccess();

      if (window.metroActiveDialogContainer is null)
      {
        throw new InvalidOperationException("Active dialog container could not be found.");
      }

      if (window.metroInactiveDialogContainer is null)
      {
        throw new InvalidOperationException("Inactive dialog container could not be found.");
      }

      if (!window.metroActiveDialogContainer.Children.Contains(dialog) && !window.metroInactiveDialogContainer.Children.Contains(dialog))
      {
        throw new InvalidOperationException("The provided dialog is not visible in the specified window.");
      }

      window.SizeChanged -= dialog.SizeChangedHandler;

      dialog.OnClose();

      Task closingTask = (Task)window.Dispatcher.Invoke(new Func<Task>(dialog.WaitForCloseAsync));
      return closingTask.ContinueWith(a =>
          {
            if (DialogClosed != null)
            {
              window.Dispatcher.BeginInvoke(new Action(() => DialogClosed(window, new DialogStateChangedEventArgs())));
            }

            return (Task)window.Dispatcher.Invoke(new Func<Task>(() =>
                      {
                    window.RemoveDialog(dialog);

                    settings ??= (dialog.DialogSettings ?? window.MetroDialogOptions);
                    return HandleOverlayOnHide(settings, window);
                  }));
          }).Unwrap();
    }

    /// <summary>
    /// Gets the current shown dialog in async way.
    /// </summary>
    /// <param name="window">The dialog owner.</param>
    public static Task<TDialog?> GetCurrentDialogAsync<TDialog>(this CrystalWindow window)
        where TDialog : BaseMetroDialog
    {
      window.Dispatcher.VerifyAccess();
      var t = new TaskCompletionSource<TDialog?>();
      window.Dispatcher.Invoke((Action)(() =>
          {
            var dialog = window.metroActiveDialogContainer?.Children.OfType<TDialog>().LastOrDefault();
            t.TrySetResult(dialog);
          }));
      return t.Task;
    }

    private static SizeChangedEventHandler SetupAndOpenDialog(CrystalWindow window, BaseMetroDialog dialog)
    {
      dialog.SetValue(Panel.ZIndexProperty, (int)(window.overlayBox?.GetValue(Panel.ZIndexProperty) ?? 0) + 1);

      var fixedMinHeight = dialog.MinHeight > 0;
      var fixedMaxHeight = dialog.MaxHeight is not double.PositiveInfinity && dialog.MaxHeight > 0;

      void CalculateMinAndMaxHeight()
      {
        if (!fixedMinHeight)
        {
          dialog.SetCurrentValue(FrameworkElement.MinHeightProperty, window.ActualHeight / 4.0);
        }

        if (!fixedMaxHeight)
        {
          dialog.SetCurrentValue(FrameworkElement.MaxHeightProperty, window.ActualHeight);
        }
        else
        {
          dialog.SetCurrentValue(FrameworkElement.MinHeightProperty, Math.Min(dialog.MinHeight, dialog.MaxHeight));
        }
      }

      CalculateMinAndMaxHeight();

      void OnWindowSizeChanged(object sender, SizeChangedEventArgs args)
      {
        CalculateMinAndMaxHeight();
      }

      window.SizeChanged += OnWindowSizeChanged;

      window.AddDialog(dialog);

      dialog.OnShown();

      return OnWindowSizeChanged;
    }

    private static void AddDialog(this CrystalWindow window, BaseMetroDialog dialog)
    {
      if (window.metroActiveDialogContainer is null)
      {
        throw new InvalidOperationException("Active dialog container could not be found.");
      }

      if (window.metroInactiveDialogContainer is null)
      {
        throw new InvalidOperationException("Inactive dialog container could not be found.");
      }

      window.StoreFocus();

      // if there's already an active dialog, move to the background
      var activeDialog = window.metroActiveDialogContainer.Children.OfType<BaseMetroDialog>().SingleOrDefault();
      if (activeDialog != null)
      {
        window.metroActiveDialogContainer.Children.Remove(activeDialog);
        window.metroInactiveDialogContainer.Children.Add(activeDialog);
      }

      window.metroActiveDialogContainer.Children.Add(dialog); //add the dialog to the container}

      window.SetValue(CrystalWindow.IsAnyDialogOpenPropertyKey, BooleanBoxes.TrueBox);
    }

    private static void RemoveDialog(this CrystalWindow window, BaseMetroDialog dialog)
    {
      if (window.metroActiveDialogContainer is null)
      {
        throw new InvalidOperationException("Active dialog container could not be found.");
      }

      if (window.metroInactiveDialogContainer is null)
      {
        throw new InvalidOperationException("Inactive dialog container could not be found.");
      }

      if (window.metroActiveDialogContainer.Children.Contains(dialog))
      {
        window.metroActiveDialogContainer.Children.Remove(dialog); //remove the dialog from the container

        // if there's an inactive dialog, bring it to the front
        var dlg = window.metroInactiveDialogContainer.Children.OfType<BaseMetroDialog>().LastOrDefault();
        if (dlg != null)
        {
          window.metroInactiveDialogContainer.Children.Remove(dlg);
          window.metroActiveDialogContainer.Children.Add(dlg);
        }
      }
      else
      {
        window.metroInactiveDialogContainer.Children.Remove(dialog);
      }

      window.SetValue(CrystalWindow.IsAnyDialogOpenPropertyKey, BooleanBoxes.Box(window.metroActiveDialogContainer.Children.Count > 0));
    }

    /// <summary>
    /// Create and show an external dialog.
    /// </summary>
    /// <param name="dialog">The dialog which will be shown externally.</param>
    /// <param name="windowOwner">The owner for the external window. If it's null the main window will be use.</param>
    /// <param name="handleExternalDialogWindow">The delegate for customizing dialog window. It can be null.</param>
    /// <returns>The given dialog.</returns>
    public static TDialog ShowDialogExternally<TDialog>(this TDialog dialog, Window? windowOwner = null, Action<Window>? handleExternalDialogWindow = null)
        where TDialog : BaseMetroDialog
    {
      var win = SetupExternalDialogWindow(dialog, windowOwner);

      handleExternalDialogWindow?.Invoke(win);

      dialog.OnShown();
      win.Show();

      return dialog;
    }

    /// <summary>
    /// Create and show an external modal dialog.
    /// </summary>
    /// <param name="dialog">The dialog which will be shown externally.</param>
    /// <param name="windowOwner">The owner for the external window. If it's null the main window will be use.</param>
    /// <param name="handleExternalDialogWindow">The delegate for customizing dialog window. It can be null.</param>
    /// <returns>The given dialog.</returns>
    public static TDialog ShowModalDialogExternally<TDialog>(this TDialog dialog, Window? windowOwner = null, Action<Window>? handleExternalDialogWindow = null)
        where TDialog : BaseMetroDialog
    {
      var win = SetupExternalDialogWindow(dialog, windowOwner);

      handleExternalDialogWindow?.Invoke(win);

      dialog.OnShown();
      win.ShowDialog();

      return dialog;
    }

    private static CrystalWindow CreateExternalWindow(Window? windowOwner = null)
    {
      var window = new CrystalWindow
      {
        ShowInTaskbar = false,
        ShowActivated = true,
        Topmost = true,
        ResizeMode = ResizeMode.NoResize,
        WindowStyle = WindowStyle.None,
        WindowStartupLocation = WindowStartupLocation.CenterScreen,
        ShowTitleBar = false,
        ShowCloseButton = false,
        WindowTransitionsEnabled = false,
        Owner = windowOwner
      };

      // If there is no Application then we need to add our default resources
      if (Application.Current is null)
      {
        window.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Crystal.Themes;component/Styles/Controls.xaml", UriKind.RelativeOrAbsolute) });
        window.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/Crystal.Themes;component/Styles/Fonts.xaml", UriKind.RelativeOrAbsolute) });

        if (windowOwner is not null)
        {
          var theme = ThemeManager.Current.DetectTheme(windowOwner);
          if (theme != null)
          {
            ThemeManager.Current.ChangeTheme(window, theme);
          }
        }
      }

      return window;
    }

    private static CrystalWindow SetupExternalDialogWindow(BaseMetroDialog dialog, Window? windowOwner = null)
    {
      var win = CreateExternalWindow(windowOwner ?? Application.Current?.MainWindow);

      // Remove the border on left and right side
      win.BeginInvoke(window =>
                          {
                            window.SetCurrentValue(Control.BorderThicknessProperty, new Thickness(0, window.BorderThickness.Top, 0, window.BorderThickness.Bottom));
                            window.SetCurrentValue(CrystalWindow.ResizeBorderThicknessProperty, new Thickness(0, window.ResizeBorderThickness.Top, 0, window.ResizeBorderThickness.Bottom));
                          },
                      DispatcherPriority.Loaded);

      // Get the monitor working area
      var monitorWorkingArea = win.Owner.GetMonitorWorkSize();
      if (monitorWorkingArea != default)
      {
        win.Width = monitorWorkingArea.Width;
        win.MinHeight = monitorWorkingArea.Height / 4.0;
        win.MaxHeight = monitorWorkingArea.Height;
      }
      else
      {
        win.Width = SystemParameters.PrimaryScreenWidth;
        win.MinHeight = SystemParameters.PrimaryScreenHeight / 4.0;
        win.MaxHeight = SystemParameters.PrimaryScreenHeight;
      }

      dialog.ParentDialogWindow = win; //THIS IS ONLY, I REPEAT, ONLY SET FOR EXTERNAL DIALOGS!

      win.Content = dialog;

      dialog.HandleThemeChange();

      EventHandler? closedHandler = null;
      closedHandler = (_, _) =>
          {
            win.Closed -= closedHandler;
            dialog.ParentDialogWindow = null;
            win.Content = null;
          };

      win.Closed += closedHandler;

      win.SizeToContent = SizeToContent.Height;

      return win;
    }

    private static CrystalWindow CreateModalExternalWindow(CrystalWindow windowOwner)
    {
      var win = CreateExternalWindow(windowOwner);
      win.Topmost = false; // It is not necessary here because the owner is set
      win.WindowStartupLocation = WindowStartupLocation.CenterOwner; // WindowStartupLocation should be CenterOwner

      // Set Width and Height maximum according Owner
      if (windowOwner.WindowState != WindowState.Maximized)
      {
        win.Width = windowOwner.ActualWidth;
        win.MaxHeight = windowOwner.ActualHeight;
      }
      else
      {
        // Remove the border on left and right side
        win.BeginInvoke(window =>
                            {
                              window.SetCurrentValue(Control.BorderThicknessProperty, new Thickness(0, window.BorderThickness.Top, 0, window.BorderThickness.Bottom));
                              window.SetCurrentValue(CrystalWindow.ResizeBorderThicknessProperty, new Thickness(0, window.ResizeBorderThickness.Top, 0, window.ResizeBorderThickness.Bottom));
                            },
                        DispatcherPriority.Loaded);

        // Get the monitor working area
        var monitorWorkingArea = windowOwner.GetMonitorWorkSize();
        if (monitorWorkingArea != default)
        {
          win.Width = monitorWorkingArea.Width;
          win.MaxHeight = monitorWorkingArea.Height;
        }
        else
        {
          win.Width = windowOwner.ActualWidth;
          win.MaxHeight = windowOwner.ActualHeight;
        }
      }

      win.SizeToContent = SizeToContent.Height;

      return win;
    }

    /// <summary>
    /// Creates a LoginDialog outside of the current window.
    /// </summary>
    /// <param name="window">The window that is the parent of the dialog.</param>
    /// <param name="title">The title of the LoginDialog.</param>
    /// <param name="message">The message contained within the LoginDialog.</param>
    /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
    /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
    public static LoginDialogData? ShowModalLoginExternal(this CrystalWindow window, string title, string message, LoginDialogSettings? settings = null)
    {
      var win = CreateModalExternalWindow(window);

      settings ??= new LoginDialogSettings();

      //create the dialog control
      LoginDialog dialog = new LoginDialog(win, settings)
      {
        Title = title,
        Message = message
      };

      SetDialogFontSizes(settings, dialog);

      win.Content = dialog;

      LoginDialogData? result = null;
      dialog.WaitForButtonPressAsync()
            .ContinueWith(task =>
                {
                  result = task.Result;
                  win.Invoke(win.Close);
                });

      HandleOverlayOnShow(settings, window);
      win.ShowDialog();
      HandleOverlayOnHide(settings, window);
      return result;
    }

    /// <summary>
    /// Creates a InputDialog outside of the current window.
    /// </summary>
    /// <param name="window">The MetroWindow</param>
    /// <param name="title">The title of the MessageDialog.</param>
    /// <param name="message">The message contained within the MessageDialog.</param>
    /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
    /// <returns>The text that was entered or null (Nothing in Visual Basic) if the user cancelled the operation.</returns>
    public static string? ShowModalInputExternal(this CrystalWindow window, string title, string message, MetroDialogSettings? settings = null)
    {
      var win = CreateModalExternalWindow(window);

      settings ??= window.MetroDialogOptions;

      //create the dialog control
      var dialog = new InputDialog(win, settings)
      {
        Message = message,
        Title = title,
        Input = settings?.DefaultText
      };

      SetDialogFontSizes(settings, dialog);

      win.Content = dialog;

      string? result = null;
      dialog.WaitForButtonPressAsync()
            .ContinueWith(task =>
                {
                  result = task.Result;
                  win.Invoke(win.Close);
                });

      HandleOverlayOnShow(settings, window);
      win.ShowDialog();
      HandleOverlayOnHide(settings, window);
      return result;
    }

    /// <summary>
    /// Creates a MessageDialog outside of the current window.
    /// </summary>
    /// <param name="window">The MetroWindow</param>
    /// <param name="title">The title of the MessageDialog.</param>
    /// <param name="message">The message contained within the MessageDialog.</param>
    /// <param name="style">The type of buttons to use.</param>
    /// <param name="settings">Optional settings that override the global metro dialog settings.</param>
    /// <returns>A task promising the result of which button was pressed.</returns>
    public static MessageDialogResult ShowModalMessageExternal(this CrystalWindow window, string title, string message, MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings? settings = null)
    {
      var win = CreateModalExternalWindow(window);

      settings ??= window.MetroDialogOptions;

      //create the dialog control
      var dialog = new MessageDialog(win, settings)
      {
        Message = message,
        Title = title,
        ButtonStyle = style
      };

      SetDialogFontSizes(settings, dialog);

      win.Content = dialog;

      MessageDialogResult result = MessageDialogResult.Affirmative;
      dialog.WaitForButtonPressAsync()
            .ContinueWith(task =>
                {
                  result = task.Result;
                  win.Invoke(win.Close);
                });

      HandleOverlayOnShow(settings, window);
      win.ShowDialog();
      HandleOverlayOnHide(settings, window);
      return result;
    }

    private static void SetDialogFontSizes(MetroDialogSettings? settings, BaseMetroDialog dialog)
    {
      if (settings is null)
      {
        return;
      }

      if (!double.IsNaN(settings.DialogTitleFontSize))
      {
        dialog.DialogTitleFontSize = settings.DialogTitleFontSize;
      }

      if (!double.IsNaN(settings.DialogMessageFontSize))
      {
        dialog.DialogMessageFontSize = settings.DialogMessageFontSize;
      }

      if (!double.IsNaN(settings.DialogButtonFontSize))
      {
        dialog.DialogButtonFontSize = settings.DialogButtonFontSize;
      }
    }

    public static event EventHandler<DialogStateChangedEventArgs>? DialogOpened;

    public static event EventHandler<DialogStateChangedEventArgs>? DialogClosed;
  }
}