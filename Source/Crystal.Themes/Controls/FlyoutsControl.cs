// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using System.Windows.Input;
using ControlzEx;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// A FlyoutsControl is for displaying flyouts in a <see cref="CrystalWindow"/>.
  /// </summary>
  [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(Flyout))]
    public class FlyoutsControl : ItemsControl
    {
        /// <summary>Identifies the <see cref="OverrideExternalCloseButton"/> dependency property.</summary>
        public static readonly DependencyProperty OverrideExternalCloseButtonProperty
            = DependencyProperty.Register(nameof(OverrideExternalCloseButton),
                                          typeof(MouseButton?),
                                          typeof(FlyoutsControl),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets whether <see cref="Flyout.ExternalCloseButton"/> is ignored and all flyouts behave as if it was set to the value of this property.
        /// </summary>
        public MouseButton? OverrideExternalCloseButton
        {
            get => (MouseButton?)GetValue(OverrideExternalCloseButtonProperty);
            set => SetValue(OverrideExternalCloseButtonProperty, value);
        }

        /// <summary>Identifies the <see cref="OverrideIsPinned"/> dependency property.</summary>
        public static readonly DependencyProperty OverrideIsPinnedProperty
            = DependencyProperty.Register(nameof(OverrideIsPinned),
                                          typeof(bool),
                                          typeof(FlyoutsControl),
                                          new PropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>
        /// Gets or sets whether <see cref="Flyout.IsPinned"/> is ignored and all flyouts behave as if it was set false.
        /// </summary>
        public bool OverrideIsPinned
        {
            get => (bool)GetValue(OverrideIsPinnedProperty);
            set => SetValue(OverrideIsPinnedProperty, BooleanBoxes.Box(value));
        }

        static FlyoutsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlyoutsControl), new FrameworkPropertyMetadata(typeof(FlyoutsControl)));
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new Flyout();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is Flyout;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            var flyout = element as Flyout;
            var headerTemplate = flyout?.HeaderTemplate;
            var headerTemplateSelector = flyout?.HeaderTemplateSelector;
            var headerStringFormat = flyout?.HeaderStringFormat;

            base.PrepareContainerForItemOverride(element, item);

            if (flyout != null)
            {
                if (headerTemplate != null)
                {
                    flyout.SetValue(HeaderedContentControl.HeaderTemplateProperty, headerTemplate);
                }

                if (headerTemplateSelector != null)
                {
                    flyout.SetValue(HeaderedContentControl.HeaderTemplateSelectorProperty, headerTemplateSelector);
                }

                if (headerStringFormat != null)
                {
                    flyout.SetValue(HeaderedContentControl.HeaderStringFormatProperty, headerStringFormat);
                }

                if (ItemTemplate != null && null == flyout.ContentTemplate)
                {
                    flyout.SetValue(ContentControl.ContentTemplateProperty, ItemTemplate);
                }

                if (ItemTemplateSelector != null && null == flyout.ContentTemplateSelector)
                {
                    flyout.SetValue(ContentControl.ContentTemplateSelectorProperty, ItemTemplateSelector);
                }

                if (ItemStringFormat != null && null == flyout.ContentStringFormat)
                {
                    flyout.SetValue(ContentControl.ContentStringFormatProperty, ItemStringFormat);
                }
            }

            AttachHandlers((Flyout)element);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            (element as Flyout)?.CleanUp();
            base.ClearContainerForItemOverride(element, item);
        }

        private void AttachHandlers(Flyout flyout)
        {
            var isOpenNotifier = new PropertyChangeNotifier(flyout, Flyout.IsOpenProperty);
            isOpenNotifier.ValueChanged += FlyoutStatusChanged;
            flyout.IsOpenPropertyChangeNotifier = isOpenNotifier;

            var themeNotifier = new PropertyChangeNotifier(flyout, Flyout.ThemeProperty);
            themeNotifier.ValueChanged += FlyoutStatusChanged;
            flyout.ThemePropertyChangeNotifier = themeNotifier;
        }

        private void FlyoutStatusChanged(object? sender, EventArgs e)
        {
            var flyout = GetFlyout(sender); //Get the flyout that raised the handler.

            HandleFlyoutStatusChange(flyout, this.TryFindParent<CrystalWindow>());
        }

        internal void HandleFlyoutStatusChange(Flyout? flyout, CrystalWindow? parentWindow)
        {
            if (flyout is null || parentWindow is null)
            {
                return;
            }

            ReorderZIndices(flyout);

            var visibleFlyouts = GetFlyouts()
                                     .Where(i => i.IsOpen)
                                     .OrderBy(Panel.GetZIndex)
                                     .ToList();
            parentWindow.HandleFlyoutStatusChange(flyout, visibleFlyouts);
        }

        private Flyout? GetFlyout(object? item)
        {
            if (item is Flyout flyout)
            {
                return flyout;
            }

            return ItemContainerGenerator.ContainerFromItem(item) as Flyout;
        }

        internal IEnumerable<Flyout> GetFlyouts()
        {
            foreach (var item in Items)
            {
                var flyout = GetFlyout(item);
                if (flyout is not null)
                {
                    yield return flyout;
                }
            }
        }

        private void ReorderZIndices(Flyout lastChanged)
        {
            var openFlyouts = GetFlyouts()
                                  .Where(f => f.IsOpen && f != lastChanged)
                                  .OrderBy(Panel.GetZIndex)
                                  .ToList();

            var index = 0;
            foreach (var openFlyout in openFlyouts)
            {
                Panel.SetZIndex(openFlyout, index);
                index++;
            }

            if (lastChanged.IsOpen)
            {
                Panel.SetZIndex(lastChanged, index);
            }
        }
    }
}