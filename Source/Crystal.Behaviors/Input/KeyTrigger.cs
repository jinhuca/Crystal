// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
namespace Crystal.Behaviors.Input
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Crystal.Behaviors;

    public enum KeyTriggerFiredOn
    {
        KeyDown,
        KeyUp
    }

    /// <summary>
    /// A Trigger that is triggered by a keyboard event.  If the target Key and Modifiers are detected, it fires.
    /// </summary>
    public class KeyTrigger : EventTriggerBase<UIElement>
    {
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(Key), typeof(KeyTrigger));

        public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register("Modifiers", typeof(ModifierKeys), typeof(KeyTrigger));

        public static readonly DependencyProperty ActiveOnFocusProperty = DependencyProperty.Register("ActiveOnFocus", typeof(bool), typeof(KeyTrigger));

        public static readonly DependencyProperty FiredOnProperty = DependencyProperty.Register("FiredOn", typeof(KeyTriggerFiredOn), typeof(KeyTrigger));

        private UIElement targetElement;

        /// <summary>
        /// The key that must be pressed for the trigger to fire.
        /// </summary>
        public Key Key
        {
            get { return (Key)GetValue(KeyTrigger.KeyProperty); }
            set { SetValue(KeyTrigger.KeyProperty, value); }
        }

        /// <summary>
        /// The modifiers that must be active for the trigger to fire (the default is no modifiers pressed).
        /// </summary>
        public ModifierKeys Modifiers
        {
            get { return (ModifierKeys)GetValue(KeyTrigger.ModifiersProperty); }
            set { SetValue(KeyTrigger.ModifiersProperty, value); }
        }

        /// <summary>
        /// If true, the Trigger only listens to its trigger Source object, which means that element must have focus for the trigger to fire.
        /// If false, the Trigger listens at the root, so any unhandled KeyDown/Up messages will be caught.
        /// </summary>
        public bool ActiveOnFocus
        {
            get { return (bool)GetValue(KeyTrigger.ActiveOnFocusProperty); }
            set { SetValue(KeyTrigger.ActiveOnFocusProperty, value); }
        }

        /// <summary>
        /// Determines whether or not to listen to the KeyDown or KeyUp event.
        /// </summary>
        public KeyTriggerFiredOn FiredOn
        {
            get { return (KeyTriggerFiredOn)GetValue(KeyTrigger.FiredOnProperty); }
            set { SetValue(KeyTrigger.FiredOnProperty, value); }
        }

        protected override string GetEventName()
        {
            return "Loaded";
        }

        private void OnKeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key &&
                Keyboard.Modifiers == GetActualModifiers(e.Key, Modifiers))
            {
                InvokeActions(e);
            }
        }

        private static ModifierKeys GetActualModifiers(Key key, ModifierKeys modifiers)
        {
            if (key == Key.LeftCtrl || key == Key.RightCtrl)
            {
                modifiers |= ModifierKeys.Control;
            }
            else if (key == Key.LeftAlt || key == Key.RightAlt || key == Key.System)
            {
                modifiers |= ModifierKeys.Alt;
            }
            else if (key == Key.LeftShift || key == Key.RightShift)
            {
                modifiers |= ModifierKeys.Shift;
            }
            return modifiers;
        }

        protected override void OnEvent(EventArgs eventArgs)
        {
            // Listen to keyboard events.
            if (ActiveOnFocus)
            {
                targetElement = Source;
            }
            else
            {
                targetElement = KeyTrigger.GetRoot(Source);
            }

            if (FiredOn == KeyTriggerFiredOn.KeyDown)
            {
                targetElement.KeyDown += OnKeyPress;
            }
            else
            {
                targetElement.KeyUp += OnKeyPress;
            }
        }

        protected override void OnDetaching()
        {
            if (targetElement != null)
            {
                if (FiredOn == KeyTriggerFiredOn.KeyDown)
                {
                    targetElement.KeyDown -= OnKeyPress;
                }
                else
                {
                    targetElement.KeyUp -= OnKeyPress;
                }
            }

            base.OnDetaching();
        }

        private static UIElement GetRoot(DependencyObject current)
        {
            UIElement last = null;

            while (current != null)
            {
                last = current as UIElement;
                current = VisualTreeHelper.GetParent(current);
            }

            return last;
        }
    }
}
