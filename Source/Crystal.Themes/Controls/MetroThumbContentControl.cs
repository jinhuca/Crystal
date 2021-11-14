// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using Crystal.Themes.Automation.Peers;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// The MetroThumbContentControl control can be used for titles or something else and enables basic drag movement functionality.
  /// </summary>
  public class MetroThumbContentControl : ContentControlEx, IMetroThumb
    {
        private TouchDevice? currentDevice = null;
        private Point startDragPoint;
        private Point startDragScreenPoint;
        private Point oldDragScreenPoint;

        static MetroThumbContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MetroThumbContentControl), new FrameworkPropertyMetadata(typeof(MetroThumbContentControl)));
            FocusableProperty.OverrideMetadata(typeof(MetroThumbContentControl), new FrameworkPropertyMetadata(default(bool)));
            EventManager.RegisterClassHandler(typeof(MetroThumbContentControl), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));
        }

        public static readonly RoutedEvent DragStartedEvent
            = EventManager.RegisterRoutedEvent(nameof(DragStarted),
                                               RoutingStrategy.Bubble,
                                               typeof(DragStartedEventHandler),
                                               typeof(MetroThumbContentControl));

        public static readonly RoutedEvent DragDeltaEvent
            = EventManager.RegisterRoutedEvent(nameof(DragDelta),
                                               RoutingStrategy.Bubble,
                                               typeof(DragDeltaEventHandler),
                                               typeof(MetroThumbContentControl));

        public static readonly RoutedEvent DragCompletedEvent
            = EventManager.RegisterRoutedEvent(nameof(DragCompleted),
                                               RoutingStrategy.Bubble,
                                               typeof(DragCompletedEventHandler),
                                               typeof(MetroThumbContentControl));

        /// <summary>
        /// Adds or remove a DragStartedEvent handler
        /// </summary>
        public event DragStartedEventHandler DragStarted
        {
            add => AddHandler(DragStartedEvent, value);
            remove => RemoveHandler(DragStartedEvent, value);
        }

        /// <summary>
        /// Adds or remove a DragDeltaEvent handler
        /// </summary>
        public event DragDeltaEventHandler DragDelta
        {
            add => AddHandler(DragDeltaEvent, value);
            remove => RemoveHandler(DragDeltaEvent, value);
        }

        /// <summary>
        /// Adds or remove a DragCompletedEvent handler
        /// </summary>
        public event DragCompletedEventHandler DragCompleted
        {
            add => AddHandler(DragCompletedEvent, value);
            remove => RemoveHandler(DragCompletedEvent, value);
        }

        private static readonly DependencyPropertyKey IsDraggingPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsDragging),
                                                  typeof(bool),
                                                  typeof(MetroThumbContentControl),
                                                  new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>
        /// DependencyProperty for the IsDragging property.
        /// </summary>
        public static readonly DependencyProperty IsDraggingProperty = IsDraggingPropertyKey.DependencyProperty;

        /// <summary>
        /// Indicates that the left mouse button is pressed and is over the MetroThumbContentControl.
        /// </summary>
        public bool IsDragging
        {
            get => (bool)GetValue(IsDraggingProperty);
            protected set => SetValue(IsDraggingPropertyKey, BooleanBoxes.Box(value));
        }

        public void CancelDragAction()
        {
            if (!IsDragging)
            {
                return;
            }

            if (IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }

            ClearValue(IsDraggingPropertyKey);
            var horizontalChange = oldDragScreenPoint.X - startDragScreenPoint.X;
            var verticalChange = oldDragScreenPoint.Y - startDragScreenPoint.Y;
            RaiseEvent(new MetroThumbContentControlDragCompletedEventArgs(horizontalChange, verticalChange, true));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsDragging)
            {
                e.Handled = true;
                try
                {
                    // focus me
                    Focus();
                    // now capture the mouse for the drag action
                    CaptureMouse();
                    // so now we are in dragging mode
                    SetValue(IsDraggingPropertyKey, BooleanBoxes.TrueBox);
                    // get the mouse points
                    startDragPoint = e.GetPosition(this);
                    oldDragScreenPoint = startDragScreenPoint = PointToScreen(startDragPoint);

                    RaiseEvent(new MetroThumbContentControlDragStartedEventArgs(startDragPoint.X, startDragPoint.Y));
                }
                catch (Exception exception)
                {
                    Trace.TraceError($"{this}: Something went wrong here: {exception} {Environment.NewLine} {exception.StackTrace}");
                    CancelDragAction();
                }
            }

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured && IsDragging)
            {
                e.Handled = true;
                // now we are in normal mode
                ClearValue(IsDraggingPropertyKey);
                // release the captured mouse
                ReleaseMouseCapture();
                // get the current mouse position and call the completed event with the horizontal/vertical change
                Point currentMouseScreenPoint = PointToScreen(e.MouseDevice.GetPosition(this));
                var horizontalChange = currentMouseScreenPoint.X - startDragScreenPoint.X;
                var verticalChange = currentMouseScreenPoint.Y - startDragScreenPoint.Y;
                RaiseEvent(new MetroThumbContentControlDragCompletedEventArgs(horizontalChange, verticalChange, false));
            }

            base.OnMouseLeftButtonUp(e);
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            // Cancel the drag action if we lost capture
            MetroThumbContentControl thumb = (MetroThumbContentControl)sender;
            if (!ReferenceEquals(Mouse.Captured, thumb))
            {
                thumb.CancelDragAction();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!IsDragging)
            {
                return;
            }

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                Point currentDragPoint = e.GetPosition(this);
                // Get client point and convert it to screen point
                Point currentDragScreenPoint = PointToScreen(currentDragPoint);
                if (currentDragScreenPoint != oldDragScreenPoint)
                {
                    oldDragScreenPoint = currentDragScreenPoint;
                    e.Handled = true;
                    var horizontalChange = currentDragPoint.X - startDragPoint.X;
                    var verticalChange = currentDragPoint.Y - startDragPoint.Y;
                    RaiseEvent(new DragDeltaEventArgs(horizontalChange, verticalChange) { RoutedEvent = MetroThumbContentControl.DragDeltaEvent });
                }
            }
            else
            {
                // clear some saved stuff
                if (ReferenceEquals(e.MouseDevice.Captured, this))
                {
                    ReleaseMouseCapture();
                }

                ClearValue(IsDraggingPropertyKey);
                startDragPoint.X = 0;
                startDragPoint.Y = 0;
            }
        }

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            // Release any previous capture
            ReleaseCurrentDevice();
            // Capture the new touch
            CaptureCurrentDevice(e);
        }

        protected override void OnPreviewTouchUp(TouchEventArgs e)
        {
            ReleaseCurrentDevice();
        }

        protected override void OnLostTouchCapture(TouchEventArgs e)
        {
            // Only re-capture if the reference is not null
            // This way we avoid re-capturing after calling ReleaseCurrentDevice()
            if (currentDevice != null)
            {
                CaptureCurrentDevice(e);
            }
        }

        private void ReleaseCurrentDevice()
        {
            if (currentDevice != null)
            {
                // Set the reference to null so that we don't re-capture in the OnLostTouchCapture() method
                var temp = currentDevice;
                currentDevice = null;
                ReleaseTouchCapture(temp);
            }
        }

        private void CaptureCurrentDevice(TouchEventArgs e)
        {
            bool gotTouch = CaptureTouch(e.TouchDevice);
            if (gotTouch)
            {
                currentDevice = e.TouchDevice;
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MetroThumbContentControlAutomationPeer(this);
        }
    }
}