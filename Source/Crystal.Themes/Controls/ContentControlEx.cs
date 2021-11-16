// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using System.Windows.Data;

using Crystal.Themes.ValueBoxes;
using Crystal.Themes.Windows.Shell;

namespace Crystal.Themes.Controls
{
  public class ContentControlEx : ContentControl
    {
        /// <summary>Identifies the <see cref="ContentCharacterCasing"/> dependency property.</summary>
        public static readonly DependencyProperty ContentCharacterCasingProperty
            = DependencyProperty.Register(nameof(ContentCharacterCasing),
                                          typeof(CharacterCasing),
                                          typeof(ContentControlEx),
                                          new FrameworkPropertyMetadata(CharacterCasing.Normal, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure),
                                          value => CharacterCasing.Normal <= (CharacterCasing)value && (CharacterCasing)value <= CharacterCasing.Upper);

        /// <summary> 
        /// Gets or sets the character casing of the Content.
        /// </summary> 
        public CharacterCasing ContentCharacterCasing
        {
            get => (CharacterCasing)GetValue(ContentCharacterCasingProperty);
            set => SetValue(ContentCharacterCasingProperty, value);
        }

        /// <summary>Identifies the <see cref="RecognizesAccessKey"/> dependency property.</summary>
        public static readonly DependencyProperty RecognizesAccessKeyProperty
            = DependencyProperty.Register(nameof(RecognizesAccessKey),
                                          typeof(bool),
                                          typeof(ContentControlEx),
                                          new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary> 
        /// Determine if the inner ContentPresenter should use AccessText in its style
        /// </summary> 
        public bool RecognizesAccessKey
        {
            get => (bool)GetValue(RecognizesAccessKeyProperty);
            set => SetValue(RecognizesAccessKeyProperty, BooleanBoxes.Box(value));
        }

        static ContentControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ContentControlEx), new FrameworkPropertyMetadata(typeof(ContentControlEx)));
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (oldContent is IInputElement && oldContent is DependencyObject oldInputElement)
            {
                BindingOperations.ClearBinding(oldInputElement, WindowChrome.IsHitTestVisibleInChromeProperty);
            }

            base.OnContentChanged(oldContent, newContent);

            if (newContent is IInputElement && newContent is DependencyObject newInputElement)
            {
                BindingOperations.SetBinding(newInputElement, WindowChrome.IsHitTestVisibleInChromeProperty, new Binding { Path = new PropertyPath(WindowChrome.IsHitTestVisibleInChromeProperty), Source = this });
            }
        }
    }
}