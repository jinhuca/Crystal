using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  ///     Represents a control that allows the user to select a date and a time.
  /// </summary>
  [TemplatePart(Name = PART_PopupContainer, Type = typeof(StackPanel))]
  [TemplatePart(Name = PART_Calendar, Type = typeof(ContentPresenter))]
  [StyleTypedProperty(Property = nameof(CalendarStyle), StyleTargetType = typeof(Calendar))]
  public class DateTimePicker : TimePickerBase
  {
    private const string PART_PopupContainer = "PART_PopupContainer";
    private const string PART_Calendar = "PART_Calendar";

    private FrameworkElement? popupContainer;
    private ContentPresenter? popupCalendarPresenter;
    private Calendar? calendar;

    /// <summary>Identifies the <see cref="DisplayDateEnd"/> dependency property.</summary>
    public static readonly DependencyProperty DisplayDateEndProperty = DatePicker.DisplayDateEndProperty.AddOwner(typeof(DateTimePicker));

    /// <summary>
    ///     Gets or sets the last date to be displayed.
    /// </summary>
    /// <returns>The last date to display.</returns>
    public DateTime? DisplayDateEnd
    {
      get => (DateTime?)GetValue(DisplayDateEndProperty);
      set => SetValue(DisplayDateEndProperty, value);
    }

    /// <summary>Identifies the <see cref="DisplayDate"/> dependency property.</summary>
    public static readonly DependencyProperty DisplayDateProperty = DatePicker.DisplayDateProperty.AddOwner(typeof(DateTimePicker));

    /// <summary>
    ///     Gets or sets the date to display
    /// </summary>
    /// <returns>
    ///     The date to display. The default is <see cref="DateTime.Today" />.
    /// </returns>
    public DateTime DisplayDate
    {
      get => (DateTime)GetValue(DisplayDateProperty);
      set => SetValue(DisplayDateProperty, value);
    }

    /// <summary>Identifies the <see cref="DisplayDateStart"/> dependency property.</summary>
    public static readonly DependencyProperty DisplayDateStartProperty = DatePicker.DisplayDateStartProperty.AddOwner(typeof(DateTimePicker));

    /// <summary>
    ///     Gets or sets the first date to be displayed.
    /// </summary>
    /// <returns>The first date to display.</returns>
    public DateTime? DisplayDateStart
    {
      get => (DateTime?)GetValue(DisplayDateStartProperty);
      set => SetValue(DisplayDateStartProperty, value);
    }

    /// <summary>Identifies the <see cref="FirstDayOfWeek"/> dependency property.</summary>
    public static readonly DependencyProperty FirstDayOfWeekProperty = DatePicker.FirstDayOfWeekProperty.AddOwner(typeof(DateTimePicker));

    /// <summary>
    ///     Gets or sets the day that is considered the beginning of the week.
    /// </summary>
    /// <returns>
    ///     A <see cref="DayOfWeek" /> that represents the beginning of the week. The default is the
    ///     <see cref="System.Globalization.DateTimeFormatInfo.FirstDayOfWeek" /> that is determined by the current culture.
    /// </returns>
    public DayOfWeek FirstDayOfWeek
    {
      get => (DayOfWeek)GetValue(FirstDayOfWeekProperty);
      set => SetValue(FirstDayOfWeekProperty, value);
    }

    /// <summary>Identifies the <see cref="IsTodayHighlighted"/> dependency property.</summary>
    public static readonly DependencyProperty IsTodayHighlightedProperty = DatePicker.IsTodayHighlightedProperty.AddOwner(typeof(DateTimePicker));

    /// <summary>
    ///     Gets or sets a value that indicates whether the current date will be highlighted.
    /// </summary>
    /// <returns>true if the current date is highlighted; otherwise, false. The default is true. </returns>
    public bool IsTodayHighlighted
    {
      get => (bool)GetValue(IsTodayHighlightedProperty);
      set => SetValue(IsTodayHighlightedProperty, BooleanBoxes.Box(value));
    }

    /// <summary>Identifies the <see cref="SelectedDateFormat"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedDateFormatProperty = DatePicker.SelectedDateFormatProperty.AddOwner(typeof(DateTimePicker), new FrameworkPropertyMetadata(DatePickerFormat.Short, OnSelectedDateFormatChanged));

    private static void OnSelectedDateFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is DateTimePicker dateTimePicker)
      {
        dateTimePicker.WriteValueToTextBox();
      }
    }

    /// <summary>
    /// Gets or sets the format that is used to display the selected date.
    /// </summary>
    [Category("Appearance")]
    [DefaultValue(DatePickerFormat.Short)]
    public DatePickerFormat SelectedDateFormat
    {
      get => (DatePickerFormat)GetValue(SelectedDateFormatProperty);
      set => SetValue(SelectedDateFormatProperty, value);
    }

    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty
        = DependencyProperty.Register(nameof(Orientation),
                                      typeof(Orientation),
                                      typeof(DateTimePicker),
                                      new PropertyMetadata(Orientation.Horizontal, null, CoerceOrientation));

    [MustUseReturnValue]
    private static object? CoerceOrientation(DependencyObject d, object? basevalue)
    {
      if (((DateTimePicker)d).IsClockVisible)
      {
        return basevalue;
      }

      return Orientation.Vertical;
    }

    /// <summary>
    ///     Gets or sets a value that indicates the dimension by which calendar and clock are stacked.
    /// </summary>
    /// <returns>
    ///     The <see cref="System.Windows.Controls.Orientation" /> of the calendar and clock. The default is
    ///     <see cref="System.Windows.Controls.Orientation.Horizontal" />.
    /// </returns>
    [Category("Layout")]
    public Orientation Orientation
    {
      get => (Orientation)GetValue(OrientationProperty);
      set => SetValue(OrientationProperty, value);
    }

    /// <summary>Identifies the <see cref="CalendarStyle"/> dependency property.</summary>
    public static readonly DependencyProperty CalendarStyleProperty
        = DependencyProperty.Register(nameof(CalendarStyle),
                                      typeof(Style),
                                      typeof(DateTimePicker));

    /// <summary>
    /// Gets or sets the style that is used when rendering the calendar.
    /// </summary>
    public Style? CalendarStyle
    {
      get => (Style?)GetValue(CalendarStyleProperty);
      set => SetValue(CalendarStyleProperty, value);
    }

    /// <summary>
    /// Gets the days that are not selectable.
    /// </summary>
    public CalendarBlackoutDatesCollection? BlackoutDates => calendar?.BlackoutDates;

    static DateTimePicker()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(DateTimePicker), new FrameworkPropertyMetadata(typeof(DateTimePicker)));
      IsClockVisibleProperty.OverrideMetadata(typeof(DateTimePicker), new PropertyMetadata(OnClockVisibilityChanged));
    }

    public DateTimePicker()
    {
      InitializeCalendar();

      SetCurrentValue(DisplayDateProperty, DateTime.Today);
    }

    /// <inheritdoc />
    protected override void FocusElementAfterIsDropDownOpenChanged()
    {
      if (calendar is null)
      {
        return;
      }

      // When the popup is opened set focus to the DisplayDate button.
      // Do this asynchronously because the IsDropDownOpen could
      // have been set even before the template for the DatePicker is
      // applied. And this would mean that the visuals wouldn't be available yet.

      Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)delegate
          {
                  // setting the focus to the calendar will focus the correct date.
                  calendar.Focus();
          });
    }

    private void InitializeCalendar()
    {
      calendar = new Calendar();

      calendar.PreviewKeyDown += CalendarPreviewKeyDown;
      calendar.DisplayDateChanged += CalendarDisplayDateChanged;
      calendar.SelectedDatesChanged += CalendarSelectedDateChanged;
      calendar.PreviewMouseUp += CalendarPreviewMouseUp;

      calendar.HorizontalAlignment = HorizontalAlignment.Left;
      calendar.VerticalAlignment = VerticalAlignment.Top;

      calendar.SelectionMode = CalendarSelectionMode.SingleDate;
      calendar.SetBinding(ForegroundProperty, GetBinding(ForegroundProperty));
      calendar.SetBinding(StyleProperty, GetBinding(CalendarStyleProperty));
      calendar.SetBinding(Calendar.IsTodayHighlightedProperty, GetBinding(IsTodayHighlightedProperty));
      calendar.SetBinding(Calendar.FirstDayOfWeekProperty, GetBinding(FirstDayOfWeekProperty));
      calendar.SetBinding(Calendar.SelectedDateProperty, GetBinding(SelectedDateTimeProperty, BindingMode.OneWay));
      calendar.SetBinding(Calendar.DisplayDateProperty, GetBinding(DisplayDateProperty));
      calendar.SetBinding(Calendar.DisplayDateStartProperty, GetBinding(DisplayDateStartProperty));
      calendar.SetBinding(Calendar.DisplayDateEndProperty, GetBinding(DisplayDateEndProperty));
      calendar.SetBinding(FontFamilyProperty, GetBinding(FontFamilyProperty));
      calendar.SetBinding(FontSizeProperty, GetBinding(FontSizeProperty));
      calendar.SetBinding(FlowDirectionProperty, GetBinding(FlowDirectionProperty));

      RenderOptions.SetClearTypeHint(calendar, ClearTypeHint.Enabled);
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      if (popupCalendarPresenter is not null)
      {
        popupCalendarPresenter.Content = null;
      }

      base.OnApplyTemplate();

      popupContainer = GetTemplateChild(PART_PopupContainer) as StackPanel;
      popupContainer?.SetBinding(StackPanel.OrientationProperty, GetBinding(OrientationProperty));

      popupCalendarPresenter = GetTemplateChild(PART_Calendar) as ContentPresenter;
      if (popupCalendarPresenter is not null)
      {
        popupCalendarPresenter.Content = calendar;
      }
    }

    private static void CalendarPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.Captured is CalendarItem)
      {
        Mouse.Capture(null);
      }
    }

    private void CalendarDisplayDateChanged(object? sender, CalendarDateChangedEventArgs e)
    {
      if (e.AddedDate.HasValue && e.AddedDate.Value != DisplayDate)
      {
        SetCurrentValue(DisplayDateProperty, e.AddedDate.Value);
      }
    }

    private void CalendarPreviewKeyDown(object? sender, RoutedEventArgs e)
    {
      var keyEventArgs = (KeyEventArgs)e;

      Debug.Assert(keyEventArgs != null);

      if (keyEventArgs is not null
          && calendar is not null
          && (keyEventArgs.Key == Key.Escape || ((keyEventArgs.Key == Key.Enter || keyEventArgs.Key == Key.Space) && calendar.DisplayMode == CalendarMode.Month)))
      {
        SetCurrentValue(IsDropDownOpenProperty, BooleanBoxes.FalseBox);
        if (keyEventArgs.Key == Key.Escape)
        {
          SetCurrentValue(SelectedDateTimeProperty, originalSelectedDateTime);
        }
      }
    }

    /// <inheritdoc />
    protected override void OnPopUpOpened()
    {
      if (calendar != null)
      {
        calendar.DisplayMode = CalendarMode.Month;
        calendar.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
      }
    }

    /// <inheritdoc />
    protected override void OnPopUpClosed()
    {
      if (calendar?.IsKeyboardFocusWithin == true)
      {
        MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
      }
    }

    /// <inheritdoc />
    protected override void ApplyCulture()
    {
      base.ApplyCulture();

      SetCurrentValue(FirstDayOfWeekProperty, SpecificCultureInfo.DateTimeFormat.FirstDayOfWeek);
    }

    /// <inheritdoc />
    protected override string? GetValueForTextBox()
    {
      var formatInfo = SpecificCultureInfo.DateTimeFormat;
      var timeFormat = SelectedTimeFormat == TimePickerFormat.Long ? formatInfo.LongTimePattern : formatInfo.ShortTimePattern;
      var dateFormat = SelectedDateFormat == DatePickerFormat.Long ? formatInfo.LongDatePattern : formatInfo.ShortDatePattern;

      var dateTimeFormat = string.Intern($"{dateFormat} {timeFormat}");

      var selectedDateTimeFromGui = GetSelectedDateTimeFromGUI();
      var valueForTextBox = selectedDateTimeFromGui?.ToString(dateTimeFormat, SpecificCultureInfo);
      return valueForTextBox;
    }

    /// <inheritdoc />
    protected override void SetSelectedDateTime()
    {
      if (textBox is null)
      {
        return;
      }

      if (DateTime.TryParse(textBox.Text, SpecificCultureInfo, System.Globalization.DateTimeStyles.None, out var dateTime))
      {
        SetCurrentValue(SelectedDateTimeProperty, dateTime);
        SetCurrentValue(DisplayDateProperty, dateTime);
      }
      else
      {
        SetCurrentValue(SelectedDateTimeProperty, null);
        if (SelectedDateTime == null)
        {
          // if already null, overwrite wrong data in textbox
          WriteValueToTextBox();
        }
      }
    }

    private static void OnClockVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      d.CoerceValue(OrientationProperty);
    }

    private void CalendarSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count > 0 && SelectedDateTime.HasValue && DateTime.Compare((DateTime?)e.AddedItems[0] ?? default, SelectedDateTime.Value) != 0)
      {
        SetCurrentValue(SelectedDateTimeProperty, (DateTime?)e.AddedItems[0] + GetSelectedTimeFromGUI());
      }
      else
      {
        if (e.AddedItems.Count == 0)
        {
          SetCurrentValue(SelectedDateTimeProperty, (DateTime?)null);
          return;
        }

        if (!SelectedDateTime.HasValue)
        {
          if (e.AddedItems.Count > 0)
          {
            SetCurrentValue(SelectedDateTimeProperty, (DateTime?)e.AddedItems[0] + GetSelectedTimeFromGUI());
          }
        }
      }
    }

    /// <inheritdoc />
    protected override void OnSelectedDateTimeChanged(DateTime? oldValue, DateTime? newValue)
    {
      calendar?.SetCurrentValue(Calendar.SelectedDateProperty, newValue);

      base.OnSelectedDateTimeChanged(oldValue, newValue);
    }

    private DateTime? GetSelectedDateTimeFromGUI()
    {
      // Because Calendar.SelectedDate is bound to this.SelectedDate return this.SelectedDate
      var selectedDate = SelectedDateTime;
      if (selectedDate != null)
      {
        return selectedDate.Value.Date + GetSelectedTimeFromGUI().GetValueOrDefault();
      }

      return null;
    }
  }
}