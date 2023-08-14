// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Crystal.Themes.Standard;
using System.Windows.Interop;

namespace Crystal.Themes.Controls;

[TemplatePart(Name = PART_TextBox, Type = typeof(TextBox))]
public class HotKeyBox : Control
{
  private const string PART_TextBox = "PART_TextBox";

  /// <summary>Identifies the <see cref="HotKey"/> dependency property.</summary>
  public static readonly DependencyProperty HotKeyProperty
    = DependencyProperty.Register(nameof(HotKey),
      typeof(HotKey),
      typeof(HotKeyBox),
      new FrameworkPropertyMetadata(default(HotKey), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHotKeyPropertyChanged));

  private static void OnHotKeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    (d as HotKeyBox)?.UpdateText();
  }

  /// <summary>
  /// Gets or sets the <see cref="HotKey"/> for this <see cref="HotKeyBox"/>.
  /// </summary>
  public HotKey? HotKey
  {
    get => (HotKey?)GetValue(HotKeyProperty);
    set => SetValue(HotKeyProperty, value);
  }

  /// <summary>Identifies the <see cref="AreModifierKeysRequired"/> dependency property.</summary>
  public static readonly DependencyProperty AreModifierKeysRequiredProperty
    = DependencyProperty.Register(nameof(AreModifierKeysRequired),
      typeof(bool),
      typeof(HotKeyBox),
      new PropertyMetadata(BooleanBoxes.FalseBox));

  /// <summary>
  /// Gets or sets if the modifier keys are required.
  /// </summary>
  public bool AreModifierKeysRequired
  {
    get => (bool)GetValue(AreModifierKeysRequiredProperty);
    set => SetValue(AreModifierKeysRequiredProperty, BooleanBoxes.Box(value));
  }

  private static readonly DependencyPropertyKey TextPropertyKey
    = DependencyProperty.RegisterReadOnly(nameof(Text),
      typeof(string),
      typeof(HotKeyBox),
      new PropertyMetadata(default(string)));

  /// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
  public static readonly DependencyProperty TextProperty = TextPropertyKey.DependencyProperty;

  /// <summary>
  /// Gets the text of the <see cref="HotKey"/>.
  /// </summary>
  public string? Text
  {
    get => (string?)GetValue(TextProperty);
    protected set => SetValue(TextPropertyKey, value);
  }

  private TextBox? _textBox;

  static HotKeyBox()
  {
    DefaultStyleKeyProperty.OverrideMetadata(typeof(HotKeyBox), new FrameworkPropertyMetadata(typeof(HotKeyBox)));
    EventManager.RegisterClassHandler(typeof(HotKeyBox), GotFocusEvent, new RoutedEventHandler(OnGotFocus));
  }

  private static void OnGotFocus(object sender, RoutedEventArgs e)
  {
    // If we're an editable HotKeyBox, forward focus to the TextBox or previous element
    if (!e.Handled)
    {
      HotKeyBox hotKeyBox = (HotKeyBox)sender;
      if (hotKeyBox.Focusable && e.OriginalSource == hotKeyBox)
      {
        // MoveFocus takes a TraversalRequest as its argument.
        var request = new TraversalRequest((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
        // Gets the element with keyboard focus.
        // Change keyboard focus.
        if (Keyboard.FocusedElement is UIElement elementWithFocus)
        {
          elementWithFocus.MoveFocus(request);
        }
        else
        {
          hotKeyBox.Focus();
        }

        e.Handled = true;
      }
    }
  }

  public override void OnApplyTemplate()
  {
    if (_textBox != null)
    {
      _textBox.PreviewKeyDown -= TextBoxOnPreviewKeyDown2;
      _textBox.GotFocus -= TextBoxOnGotFocus;
      _textBox.LostFocus -= TextBoxOnLostFocus;
      _textBox.TextChanged -= TextBoxOnTextChanged;
    }

    base.OnApplyTemplate();

    _textBox = Template.FindName(PART_TextBox, this) as TextBox;
    if (_textBox != null)
    {
      _textBox.PreviewKeyDown += TextBoxOnPreviewKeyDown2;
      _textBox.GotFocus += TextBoxOnGotFocus;
      _textBox.LostFocus += TextBoxOnLostFocus;
      _textBox.TextChanged += TextBoxOnTextChanged;
    }

    UpdateText();
  }

  private void TextBoxOnTextChanged(object sender, TextChangedEventArgs args)
  {
    _textBox!.SelectionStart = _textBox.Text.Length;
  }

  private void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
  {
    ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcherOnThreadPreprocessMessage;
  }

#pragma warning disable 618
  private void ComponentDispatcherOnThreadPreprocessMessage(ref MSG msg, ref bool handled)
  {
    if (msg.message == (int)WM.HOTKEY)
    {
      // swallow all hotkeys, so our control can catch the key strokes
      handled = true;
    }
  }
#pragma warning restore 618

  private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
  {
    ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcherOnThreadPreprocessMessage;
  }

  private void TextBoxOnPreviewKeyDown2(object sender, KeyEventArgs e)
  {
    var key = e.Key == Key.System ? e.SystemKey : e.Key;
    switch (key)
    {
      case Key.Tab:
      case Key.LeftShift:
      case Key.RightShift:
      case Key.LeftCtrl:
      case Key.RightCtrl:
      case Key.LeftAlt:
      case Key.RightAlt:
      case Key.RWin:
      case Key.LWin:
        return;
    }

    e.Handled = true;

    var currentModifierKeys = GetCurrentModifierKeys();
    if (currentModifierKeys == ModifierKeys.None && key == Key.Back)
    {
      SetCurrentValue(HotKeyProperty, null);
    }
    else if (currentModifierKeys != ModifierKeys.None || !AreModifierKeysRequired)
    {
      SetCurrentValue(HotKeyProperty, new HotKey(key, currentModifierKeys));
    }

    UpdateText();
  }

  private static ModifierKeys GetCurrentModifierKeys()
  {
    var modifier = ModifierKeys.None;
    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
    {
      modifier |= ModifierKeys.Control;
    }

    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
    {
      modifier |= ModifierKeys.Alt;
    }

    if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
    {
      modifier |= ModifierKeys.Shift;
    }

    if (Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin))
    {
      modifier |= ModifierKeys.Windows;
    }

    return modifier;
  }

  private void UpdateText()
  {
    var hotkey = HotKey;
    Text = hotkey is null || hotkey.Key == Key.None ? string.Empty : hotkey.ToString();
  }
}