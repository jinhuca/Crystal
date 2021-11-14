// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using System.Windows.Media;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  public class ColorPalette : ListBox
    {
        static ColorPalette()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPalette), new FrameworkPropertyMetadata(typeof(ColorPalette)));
        }

        /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(nameof(Header),
                                          typeof(object),
                                          typeof(ColorPalette),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Header of this Control
        /// </summary>
        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>Identifies the <see cref="ColorNamesDictionary"/> dependency property.</summary>
        public static readonly DependencyProperty ColorNamesDictionaryProperty
            = DependencyProperty.Register(nameof(ColorNamesDictionary),
                                          typeof(Dictionary<Color, string>),
                                          typeof(ColorPalette),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the HeaderTemplate of this Control
        /// </summary>
        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty HeaderTemplateProperty
            = DependencyProperty.Register(nameof(HeaderTemplate),
                                          typeof(DataTemplate),
                                          typeof(ColorPalette),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a custom dictionary for color to name. If null, the default dictionary will be used.
        /// </summary>
        public Dictionary<Color, string>? ColorNamesDictionary
        {
            get => (Dictionary<Color, string>?)GetValue(ColorNamesDictionaryProperty);
            set => SetValue(ColorNamesDictionaryProperty, value);
        }

        /// <summary>Identifies the <see cref="IsAlphaChannelVisible"/> dependency property.</summary>
        public static readonly DependencyProperty IsAlphaChannelVisibleProperty
            = DependencyProperty.Register(nameof(IsAlphaChannelVisible),
                                          typeof(bool),
                                          typeof(ColorPalette),
                                          new PropertyMetadata(BooleanBoxes.TrueBox));

        /// <summary>
        /// Gets or sets whether the Alpha-Channel is visible
        /// </summary>
        public bool IsAlphaChannelVisible
        {
            get => (bool)GetValue(IsAlphaChannelVisibleProperty);
            set => SetValue(IsAlphaChannelVisibleProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="ColorHelper"/> dependency property.</summary>
        public static readonly DependencyProperty ColorHelperProperty
            = DependencyProperty.Register(nameof(ColorHelper),
                                          typeof(ColorHelper),
                                          typeof(ColorPalette),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or Sets the helper class which is used to convert the color from or to string
        /// </summary>
        public ColorHelper? ColorHelper
        {
            get => (ColorHelper?)GetValue(ColorHelperProperty);
            set => SetValue(ColorHelperProperty, value);
        }

        internal bool FocusSelectedItem()
        {
            ListBoxItem? listBoxItem = null;

            if (SelectedIndex >= 0)
            {
                listBoxItem = ItemContainerGenerator.ContainerFromIndex(SelectedIndex) as ListBoxItem;
            }
            else if (Items.Count > 0)
            {
                listBoxItem = ItemContainerGenerator.ContainerFromItem(Items[0]) as ListBoxItem;
            }

            return listBoxItem is not null && listBoxItem.Focus();
        }
    }
}