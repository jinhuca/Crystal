// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls
{
  public class ColorEyeDropper : Button
    {
        private DispatcherOperation? currentTask;
        internal ColorEyePreviewData previewData = new ColorEyePreviewData();
        private ToolTip? previewToolTip;

        static ColorEyeDropper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorEyeDropper), new FrameworkPropertyMetadata(typeof(ColorEyeDropper)));
        }

        /// <summary>Identifies the <see cref="SelectedColor"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedColorProperty
            = DependencyProperty.Register(nameof(SelectedColor),
                                          typeof(Color?),
                                          typeof(ColorEyeDropper),
                                          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorPropertyChanged));

        /// <summary>
        /// Gets or sets the selected <see cref="Color"/>.
        /// </summary>
        public Color? SelectedColor
        {
            get => (Color?)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        /// <summary>Identifies the <see cref="PreviewImageOuterPixelCount"/> dependency property.</summary>
        public static readonly DependencyProperty PreviewImageOuterPixelCountProperty
            = DependencyProperty.Register(nameof(PreviewImageOuterPixelCount),
                                          typeof(int),
                                          typeof(ColorEyeDropper),
                                          new PropertyMetadata(2));

        private static void OnSelectedColorPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is ColorEyeDropper eyeDropper)
            {
                eyeDropper.RaiseEvent(new RoutedPropertyChangedEventArgs<Color?>((Color?)e.OldValue, (Color?)e.NewValue, SelectedColorChangedEvent));
            }
        }

        /// <summary>
        /// Gets or sets the number of additional pixel in the preview image.
        /// </summary>
        public int PreviewImageOuterPixelCount
        {
            get => (int)GetValue(PreviewImageOuterPixelCountProperty);
            set => SetValue(PreviewImageOuterPixelCountProperty, value);
        }

        /// <summary>Identifies the <see cref="EyeDropperCursor"/> dependency property.</summary>
        public static readonly DependencyProperty EyeDropperCursorProperty
            = DependencyProperty.Register(nameof(EyeDropperCursor),
                                          typeof(Cursor),
                                          typeof(ColorEyeDropper),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Cursor for Selecting Color Mode
        /// </summary>
        public Cursor? EyeDropperCursor
        {
            get => (Cursor?)GetValue(EyeDropperCursorProperty);
            set => SetValue(EyeDropperCursorProperty, value);
        }

        /// <summary>Identifies the <see cref="PreviewContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty PreviewContentTemplateProperty
            = DependencyProperty.Register(nameof(PreviewContentTemplate),
                                          typeof(DataTemplate),
                                          typeof(ColorEyeDropper),
                                          new PropertyMetadata(default(DataTemplate)));

        /// <summary>
        /// Gets or sets the ContentControl.ContentTemplate for the preview.
        /// </summary>
        public DataTemplate? PreviewContentTemplate
        {
            get => (DataTemplate?)GetValue(PreviewContentTemplateProperty);
            set => SetValue(PreviewContentTemplateProperty, value);
        }

        private void SetPreview(Point mousePos)
        {
            previewToolTip?.Move(mousePos, new Point(16, 16));

            if (currentTask?.Status == DispatcherOperationStatus.Executing || currentTask?.Status == DispatcherOperationStatus.Pending)
            {
                currentTask.Abort();
            }

            var action = new Action(() =>
                {
                    mousePos = PointToScreen(mousePos);
                    var outerPixelCount = PreviewImageOuterPixelCount;
                    var posX = (int)Math.Round(mousePos.X - outerPixelCount);
                    var posY = (int)Math.Round(mousePos.Y - outerPixelCount);
                    var region = new Int32Rect(posX, posY, 2 * outerPixelCount + 1, 2 * outerPixelCount + 1);
                    var previewImage = EyeDropperHelper.CaptureRegion(region);
                    var previewBrush = new SolidColorBrush(EyeDropperHelper.GetPixelColor(mousePos));
                    previewBrush.Freeze();

                    previewData.SetValue(ColorEyePreviewData.PreviewImagePropertyKey, previewImage);
                    previewData.SetValue(ColorEyePreviewData.PreviewBrushPropertyKey, previewBrush);
                });

            currentTask = Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);

            Mouse.Capture(this);

            previewToolTip ??= ColorEyePreview.GetPreviewToolTip(this);

            previewToolTip.Show();

            Cursor = EyeDropperCursor;

            SetPreview(e.GetPosition(this));
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);

            Mouse.Capture(null);

            previewToolTip?.Hide();

            Cursor = Cursors.Arrow;

            SetCurrentValue(SelectedColorProperty, EyeDropperHelper.GetPixelColor(PointToScreen(e.GetPosition(this))));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SetPreview(e.GetPosition(this));
            }
        }

        /// <summary>Identifies the <see cref="SelectedColorChanged"/> routed event.</summary>
        public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedColorChanged),
            RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<Color?>),
            typeof(ColorEyeDropper));

        /// <summary>
        ///     Occurs when the <see cref="SelectedColor"/> property is changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Color?> SelectedColorChanged
        {
            add => AddHandler(SelectedColorChangedEvent, value);
            remove => RemoveHandler(SelectedColorChangedEvent, value);
        }
    }
}