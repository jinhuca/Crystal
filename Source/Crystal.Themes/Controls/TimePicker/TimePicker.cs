// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace Crystal.Themes.Controls
{
  /// <summary>
  ///     Represents a control that allows the user to select a time.
  /// </summary>
  public class TimePicker : TimePickerBase
    {
        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
        }

        public TimePicker()
        {
            IsDatePickerVisible = false;
        }

        /// <inheritdoc />
        protected override void FocusElementAfterIsDropDownOpenChanged()
        {
            if (hourInput is null)
            {
                return;
            }

            // When the popup is opened set focus to the hour input.
            // Do this asynchronously because the IsDropDownOpen could
            // have been set even before the template for the DatePicker is
            // applied. And this would mean that the visuals wouldn't be available yet.

            Dispatcher.BeginInvoke(DispatcherPriority.Input, (Action)delegate
                {
                    // setting the focus to the calendar will focus the correct date.
                    hourInput.Focus();
                });
        }

        /// <inheritdoc />
        protected override void SetSelectedDateTime()
        {
            if (textBox is null)
            {
                return;
            }

            const DateTimeStyles dateTimeParseStyle = DateTimeStyles.AllowWhiteSpaces
                                                      & DateTimeStyles.AssumeLocal
                                                      & DateTimeStyles.NoCurrentDateDefault;

            if (DateTime.TryParse(textBox.Text, SpecificCultureInfo, dateTimeParseStyle, out var timeSpan))
            {
                SetCurrentValue(SelectedDateTimeProperty, SelectedDateTime.GetValueOrDefault().Date + timeSpan.TimeOfDay);
            }
            else
            {
                SetCurrentValue(SelectedDateTimeProperty, null);
                if (SelectedDateTime == null)
                {
                    // if already null, overwrite wrong data in TextBox
                    WriteValueToTextBox();
                }
            }
        }
    }
}