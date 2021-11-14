// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls
{
  /// <summary>
  ///     Provides calculated values that can be referenced as TemplatedParent sources when defining templates for a
  ///     <see cref="SplitView" />.
  ///     Not intended for general use.
  /// </summary>
  public sealed class SplitViewTemplateSettings : DependencyObject
    {
        /// <summary>Identifies the <see cref="CompactPaneGridLength"/> dependency property.</summary>
        internal static readonly DependencyProperty CompactPaneGridLengthProperty
            = DependencyProperty.Register(nameof(CompactPaneGridLength),
                                          typeof(GridLength),
                                          typeof(SplitViewTemplateSettings),
                                          new PropertyMetadata(null));

        /// <summary>
        ///     Gets the <see cref="SplitView.CompactPaneLength" /> value as a GridLength.
        /// </summary>
        public GridLength CompactPaneGridLength
        {
            get => (GridLength)GetValue(CompactPaneGridLengthProperty);
            private set => SetValue(CompactPaneGridLengthProperty, value);
        }

        /// <summary>Identifies the <see cref="NegativeOpenPaneLength"/> dependency property.</summary>
        internal static readonly DependencyProperty NegativeOpenPaneLengthProperty
            = DependencyProperty.Register(nameof(NegativeOpenPaneLength),
                                          typeof(double),
                                          typeof(SplitViewTemplateSettings),
                                          new PropertyMetadata(0d));

        /// <summary>
        ///     Gets the negative of the <see cref="SplitView.OpenPaneLength" /> value.
        /// </summary>
        public double NegativeOpenPaneLength
        {
            get => (double)GetValue(NegativeOpenPaneLengthProperty);
            private set => SetValue(NegativeOpenPaneLengthProperty, value);
        }

        /// <summary>Identifies the <see cref="NegativeOpenPaneLengthMinusCompactLength"/> dependency property.</summary>
        internal static readonly DependencyProperty NegativeOpenPaneLengthMinusCompactLengthProperty
            = DependencyProperty.Register(nameof(NegativeOpenPaneLengthMinusCompactLength),
                                          typeof(double),
                                          typeof(SplitViewTemplateSettings),
                                          new PropertyMetadata(0d));

        /// <summary>
        ///     Gets the negative of the value calculated by subtracting the <see cref="SplitView.CompactPaneLength" /> value from
        ///     the <see cref="SplitView.OpenPaneLength" /> value.
        /// </summary>
        public double NegativeOpenPaneLengthMinusCompactLength
        {
            get => (double)GetValue(NegativeOpenPaneLengthMinusCompactLengthProperty);
            set => SetValue(NegativeOpenPaneLengthMinusCompactLengthProperty, value);
        }

        /// <summary>Identifies the <see cref="OpenPaneGridLength"/> dependency property.</summary>
        internal static readonly DependencyProperty OpenPaneGridLengthProperty
            = DependencyProperty.Register(nameof(OpenPaneGridLength),
                                          typeof(GridLength),
                                          typeof(SplitViewTemplateSettings),
                                          new PropertyMetadata(null));

        /// <summary>
        ///     Gets the <see cref="SplitView.OpenPaneLength" /> value as a GridLength.
        /// </summary>
        public GridLength OpenPaneGridLength
        {
            get => (GridLength)GetValue(OpenPaneGridLengthProperty);
            private set => SetValue(OpenPaneGridLengthProperty, value);
        }

        /// <summary>Identifies the <see cref="OpenPaneLength"/> dependency property.</summary>
        internal static readonly DependencyProperty OpenPaneLengthProperty
            = DependencyProperty.Register(nameof(OpenPaneLength),
                                          typeof(double),
                                          typeof(SplitViewTemplateSettings),
                                          new PropertyMetadata(0d));

        /// <summary>
        ///     Gets the <see cref="SplitView.OpenPaneLength" /> value.
        /// </summary>
        public double OpenPaneLength
        {
            get => (double)GetValue(OpenPaneLengthProperty);
            private set => SetValue(OpenPaneLengthProperty, value);
        }

        /// <summary>Identifies the <see cref="OpenPaneLengthMinusCompactLength"/> dependency property.</summary>
        internal static readonly DependencyProperty OpenPaneLengthMinusCompactLengthProperty
            = DependencyProperty.Register(nameof(OpenPaneLengthMinusCompactLength),
                                          typeof(double),
                                          typeof(SplitViewTemplateSettings),
                                          new PropertyMetadata(0d));

        /// <summary>
        ///     Gets a value calculated by subtracting the <see cref="SplitView.CompactPaneLength" /> value from the
        ///     <see cref="SplitView.OpenPaneLength" /> value.
        /// </summary>
        public double OpenPaneLengthMinusCompactLength
        {
            get => (double)GetValue(OpenPaneLengthMinusCompactLengthProperty);
            private set => SetValue(OpenPaneLengthMinusCompactLengthProperty, value);
        }

        internal SplitViewTemplateSettings(SplitView owner)
        {
            Owner = owner;
            Update();
        }

        internal SplitView Owner { get; }

        internal void Update()
        {
            CompactPaneGridLength = new GridLength(Owner.CompactPaneLength, GridUnitType.Pixel);
            OpenPaneGridLength = new GridLength(Owner.OpenPaneLength, GridUnitType.Pixel);

            OpenPaneLength = Owner.OpenPaneLength;
            OpenPaneLengthMinusCompactLength = Owner.OpenPaneLength - Owner.CompactPaneLength;

            NegativeOpenPaneLength = -OpenPaneLength;
            NegativeOpenPaneLengthMinusCompactLength = -OpenPaneLengthMinusCompactLength;
        }
    }
}