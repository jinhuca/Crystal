// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.Themes.Controls
{
  [TemplatePart(Name = "PART_SaturationValueBox", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ColorEyeDropper", Type = typeof(ColorEyeDropper))]
    public class ColorCanvas : ColorPickerBase
    {
        private FrameworkElement? saturationValueBox;

        static ColorCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorCanvas), new FrameworkPropertyMetadata(typeof(ColorCanvas)));
        }

        private void PART_SaturationValueBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            saturationValueBox!.ReleaseMouseCapture();
            saturationValueBox.MouseMove -= PART_SaturationValueBox_MouseMove;
        }

        private void PART_SaturationValueBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(saturationValueBox);
            saturationValueBox!.MouseMove += PART_SaturationValueBox_MouseMove;

            UpdateValues(e.GetPosition(saturationValueBox));
        }

        private void PART_SaturationValueBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateValues(e.GetPosition(saturationValueBox));
            }
        }

        private void UpdateValues(Point position)
        {
            if (saturationValueBox!.ActualWidth < 1 || saturationValueBox.ActualHeight < 1)
            {
                return;
            }

            var s = position.X / saturationValueBox.ActualWidth;
            var v = 1 - (position.Y / saturationValueBox.ActualHeight);

            if (s > 1)
            {
                s = 1;
            }

            if (v > 1)
            {
                v = 1;
            }

            if (s < 0)
            {
                s = 0;
            }

            if (v < 0)
            {
                v = 0;
            }

            SetCurrentValue(SaturationProperty, s);
            SetCurrentValue(ValueProperty, v);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            saturationValueBox = (FrameworkElement?)GetTemplateChild("PART_SaturationValueBox");
            if (saturationValueBox != null)
            {
                saturationValueBox.MouseLeftButtonDown += PART_SaturationValueBox_MouseLeftButtonDown;
                saturationValueBox.MouseLeftButtonUp += PART_SaturationValueBox_MouseLeftButtonUp;
            }
        }
    }
}