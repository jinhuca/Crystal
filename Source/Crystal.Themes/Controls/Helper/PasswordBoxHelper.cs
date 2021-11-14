﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Crystal.Themes.Controls
{
  public class PasswordBoxHelper
    {
        public static readonly DependencyProperty CapsLockIconProperty
            = DependencyProperty.RegisterAttached(
                "CapsLockIcon",
                typeof(object),
                typeof(PasswordBoxHelper),
                new PropertyMetadata("!", OnCapsLockIconPropertyChanged));

        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static object GetCapsLockIcon(PasswordBox element)
        {
            return element.GetValue(CapsLockIconProperty);
        }

        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static void SetCapsLockIcon(PasswordBox element, object value)
        {
            element.SetValue(CapsLockIconProperty, value);
        }

        private static void OnCapsLockIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                PasswordBox pb = (PasswordBox)d;

                pb.KeyDown -= RefreshCapsLockStatus;
                pb.GotFocus -= RefreshCapsLockStatus;
                pb.PreviewGotKeyboardFocus -= RefreshCapsLockStatus;
                pb.LostFocus -= HandlePasswordBoxLostFocus;

                if (e.NewValue != null)
                {
                    pb.KeyDown += RefreshCapsLockStatus;
                    pb.GotFocus += RefreshCapsLockStatus;
                    pb.PreviewGotKeyboardFocus += RefreshCapsLockStatus;
                    pb.LostFocus += HandlePasswordBoxLostFocus;
                }
            }
        }

        private static void RefreshCapsLockStatus(object sender, RoutedEventArgs e)
        {
            var fe = FindCapsLockIndicator(sender as Control);
            if (fe != null)
            {
                fe.Visibility = Keyboard.IsKeyToggled(Key.CapsLock) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void HandlePasswordBoxLostFocus(object sender, RoutedEventArgs e)
        {
            var fe = FindCapsLockIndicator(sender as Control);
            if (fe != null)
            {
                fe.Visibility = Visibility.Collapsed;
            }
        }

        private static FrameworkElement? FindCapsLockIndicator(Control? pb)
        {
            return pb?.Template?.FindName("PART_CapsLockIndicator", pb) as FrameworkElement;
        }

        public static readonly DependencyProperty CapsLockWarningToolTipProperty
            = DependencyProperty.RegisterAttached(
                "CapsLockWarningToolTip",
                typeof(object),
                typeof(PasswordBoxHelper),
                new PropertyMetadata("Caps lock is on"));

        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static object GetCapsLockWarningToolTip(PasswordBox element)
        {
            return element.GetValue(CapsLockWarningToolTipProperty);
        }

        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static void SetCapsLockWarningToolTip(PasswordBox element, object value)
        {
            element.SetValue(CapsLockWarningToolTipProperty, value);
        }

        public static readonly DependencyProperty RevealButtonContentProperty
            = DependencyProperty.RegisterAttached(
                "RevealButtonContent",
                typeof(object),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets the content of the RevealButton.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static object? GetRevealButtonContent(DependencyObject d)
        {
            return (object?)d.GetValue(RevealButtonContentProperty);
        }

        /// <summary>
        /// Sets the content of the RevealButton.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static void SetRevealButtonContent(DependencyObject obj, object? value)
        {
            obj.SetValue(RevealButtonContentProperty, value);
        }

        public static readonly DependencyProperty RevealButtonContentTemplateProperty
            = DependencyProperty.RegisterAttached(
                "RevealButtonContentTemplate",
                typeof(DataTemplate),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(null));

        /// <summary> 
        /// Gets the data template used to display the content of the RevealButton.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static DataTemplate? GetRevealButtonContentTemplate(DependencyObject d)
        {
            return (DataTemplate?)d.GetValue(RevealButtonContentTemplateProperty);
        }

        /// <summary> 
        /// Sets the data template used to display the content of the RevealButton.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        [AttachedPropertyBrowsableForType(typeof(PasswordBox))]
        public static void SetRevealButtonContentTemplate(DependencyObject obj, DataTemplate? value)
        {
            obj.SetValue(RevealButtonContentTemplateProperty, value);
        }
    }
}