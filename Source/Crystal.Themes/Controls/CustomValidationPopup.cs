// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using ControlzEx.Native;
using ControlzEx.Standard;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// This custom popup is used by the validation error template.
  /// It provides some additional nice features:
  ///     - repositioning if host-window size or location changed
  ///     - repositioning if host-window gets maximized and vice versa
  ///     - it's only topmost if the host-window is activated
  /// </summary>
  public class CustomValidationPopup : Popup
    {
        private Window? hostWindow;
        private ScrollViewer? scrollViewer;
        private CrystalContentControl? metroContentControl;
        private TransitioningContentControl? transitioningContentControl;
        private Flyout? flyout;

        /// <summary>Identifies the <see cref="CloseOnMouseLeftButtonDown"/> dependency property.</summary>
        public static readonly DependencyProperty CloseOnMouseLeftButtonDownProperty
            = DependencyProperty.Register(nameof(CloseOnMouseLeftButtonDown),
                                          typeof(bool),
                                          typeof(CustomValidationPopup),
                                          new PropertyMetadata(BooleanBoxes.TrueBox));

        /// <summary>
        /// Gets or sets whether if the popup can be closed by left mouse button down.
        /// </summary>
        public bool CloseOnMouseLeftButtonDown
        {
            get => (bool)GetValue(CloseOnMouseLeftButtonDownProperty);
            set => SetValue(CloseOnMouseLeftButtonDownProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="ShowValidationErrorOnMouseOver"/> dependency property.</summary>
        public static readonly DependencyProperty ShowValidationErrorOnMouseOverProperty
            = DependencyProperty.RegisterAttached(nameof(ShowValidationErrorOnMouseOver),
                                                  typeof(bool),
                                                  typeof(CustomValidationPopup),
                                                  new PropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>
        /// Gets or sets whether the validation error text will be shown when hovering the validation triangle.
        /// </summary>
        public bool ShowValidationErrorOnMouseOver
        {
            get => (bool)GetValue(ShowValidationErrorOnMouseOverProperty);
            set => SetValue(ShowValidationErrorOnMouseOverProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="AdornedElement"/> dependency property.</summary>
        public static readonly DependencyProperty AdornedElementProperty
            = DependencyProperty.Register(nameof(AdornedElement),
                                          typeof(UIElement),
                                          typeof(CustomValidationPopup),
                                          new PropertyMetadata(default(UIElement)));

        /// <summary>
        /// Gets or sets the <see cref="T:System.Windows.UIElement" /> that this <see cref="T:System.Windows.Controls.Primitives.Popup" /> object is reserving space for.
        /// </summary>
        public UIElement? AdornedElement
        {
            get => (UIElement?)GetValue(AdornedElementProperty);
            set => SetValue(AdornedElementProperty, value);
        }

        /// <summary>Identifies the <see cref="CanShow"/> dependency property.</summary>
        private static readonly DependencyPropertyKey CanShowPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(CanShow),
                                                  typeof(bool),
                                                  typeof(CustomValidationPopup),
                                                  new PropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>Identifies the <see cref="CanShow"/> dependency property.</summary>
        public static readonly DependencyProperty CanShowProperty = CanShowPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets whether the popup can be shown (useful for transitions).
        /// </summary>
        public bool CanShow
        {
            get => (bool)GetValue(CanShowProperty);
            protected set => SetValue(CanShowPropertyKey, BooleanBoxes.Box(value));
        }

        public CustomValidationPopup()
        {
            Loaded += CustomValidationPopup_Loaded;
            Opened += CustomValidationPopup_Opened;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (CloseOnMouseLeftButtonDown)
            {
                SetCurrentValue(IsOpenProperty, BooleanBoxes.FalseBox);
            }
            else
            {
                var adornedElement = AdornedElement;
                if (adornedElement != null && ValidationHelper.GetCloseOnMouseLeftButtonDown(adornedElement))
                {
                    SetCurrentValue(IsOpenProperty, BooleanBoxes.FalseBox);
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void CustomValidationPopup_Loaded(object? sender, RoutedEventArgs e)
        {
            var adornedElement = AdornedElement;
            if (adornedElement is null)
            {
                return;
            }

            hostWindow = Window.GetWindow(adornedElement);
            if (hostWindow is null)
            {
                return;
            }

            SetValue(CanShowPropertyKey, BooleanBoxes.FalseBox);
            var canShow = true;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }

            scrollViewer = adornedElement.GetVisualAncestor<ScrollViewer>();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            if (metroContentControl != null)
            {
                metroContentControl.TransitionStarted -= OnTransitionStarted;
                metroContentControl.TransitionCompleted -= OnTransitionCompleted;
            }

            metroContentControl = adornedElement.TryFindParent<CrystalContentControl>();
            if (metroContentControl != null)
            {
                canShow = !metroContentControl.TransitionsEnabled || !metroContentControl.IsTransitioning;
                metroContentControl.TransitionStarted += OnTransitionStarted;
                metroContentControl.TransitionCompleted += OnTransitionCompleted;
            }

            if (transitioningContentControl != null)
            {
                transitioningContentControl.TransitionCompleted -= OnTransitionCompleted;
            }

            transitioningContentControl = adornedElement.TryFindParent<TransitioningContentControl>();
            if (transitioningContentControl != null)
            {
                canShow = canShow && (transitioningContentControl.CurrentTransition == null || !transitioningContentControl.IsTransitioning);
                transitioningContentControl.TransitionCompleted += OnTransitionCompleted;
            }

            if (flyout != null)
            {
                flyout.OpeningFinished -= Flyout_OpeningFinished;
                flyout.IsOpenChanged -= Flyout_IsOpenChanged;
                flyout.ClosingFinished -= Flyout_ClosingFinished;
            }

            flyout = adornedElement.TryFindParent<Flyout>();
            if (flyout != null)
            {
                canShow = canShow && (!flyout.AreAnimationsEnabled || flyout.IsShown);
                flyout.OpeningFinished += Flyout_OpeningFinished;
                flyout.IsOpenChanged += Flyout_IsOpenChanged;
                flyout.ClosingFinished += Flyout_ClosingFinished;
            }

            hostWindow.LocationChanged -= OnSizeOrLocationChanged;
            hostWindow.LocationChanged += OnSizeOrLocationChanged;
            hostWindow.SizeChanged -= OnSizeOrLocationChanged;
            hostWindow.SizeChanged += OnSizeOrLocationChanged;
            hostWindow.StateChanged -= OnHostWindowStateChanged;
            hostWindow.StateChanged += OnHostWindowStateChanged;
            hostWindow.Activated -= OnHostWindowActivated;
            hostWindow.Activated += OnHostWindowActivated;
            hostWindow.Deactivated -= OnHostWindowDeactivated;
            hostWindow.Deactivated += OnHostWindowDeactivated;

            if (PlacementTarget is FrameworkElement frameworkElement)
            {
                frameworkElement.SizeChanged -= OnSizeOrLocationChanged;
                frameworkElement.SizeChanged += OnSizeOrLocationChanged;
            }

            RefreshPosition();
            SetValue(CanShowPropertyKey, BooleanBoxes.Box(canShow));

            OnLoaded();

            Unloaded -= CustomValidationPopup_Unloaded;
            Unloaded += CustomValidationPopup_Unloaded;
        }

        private void Flyout_OpeningFinished(object? sender, RoutedEventArgs e)
        {
            RefreshPosition();

            var adornedElement = AdornedElement;
            var isOpen = Validation.GetHasError(adornedElement!) && adornedElement!.IsKeyboardFocusWithin;
            SetCurrentValue(IsOpenProperty, BooleanBoxes.Box(isOpen));

            SetValue(CanShowPropertyKey, BooleanBoxes.TrueBox);
        }

        private void Flyout_IsOpenChanged(object? sender, RoutedEventArgs e)
        {
            RefreshPosition();
            SetValue(CanShowPropertyKey, BooleanBoxes.FalseBox);
        }

        private void Flyout_ClosingFinished(object? sender, RoutedEventArgs e)
        {
            RefreshPosition();
            SetValue(CanShowPropertyKey, BooleanBoxes.FalseBox);
        }

        private void OnTransitionStarted(object? sender, RoutedEventArgs e)
        {
            RefreshPosition();
            SetValue(CanShowPropertyKey, BooleanBoxes.FalseBox);
        }

        private void OnTransitionCompleted(object? sender, RoutedEventArgs e)
        {
            RefreshPosition();

            var adornedElement = AdornedElement;
            var isOpen = Validation.GetHasError(adornedElement!) && adornedElement!.IsKeyboardFocusWithin;
            SetCurrentValue(IsOpenProperty, BooleanBoxes.Box(isOpen));

            SetValue(CanShowPropertyKey, BooleanBoxes.TrueBox);
        }

        private void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
        {
            if (e.VerticalChange is > 0 or < 0 || e.HorizontalChange is > 0 or < 0)
            {
                RefreshPosition();

                if (IsElementVisible(AdornedElement as FrameworkElement, scrollViewer))
                {
                    var adornedElement = AdornedElement;
                    var isOpen = Validation.GetHasError(adornedElement!) && adornedElement!.IsKeyboardFocusWithin;
                    SetCurrentValue(IsOpenProperty, BooleanBoxes.Box(isOpen));
                }
                else
                {
                    SetCurrentValue(IsOpenProperty, BooleanBoxes.FalseBox);
                }
            }
        }

        private static bool IsElementVisible(FrameworkElement? element, FrameworkElement? container)
        {
            if (element is null || container is null || !element.IsVisible)
            {
                return false;
            }

            var bounds = element.TransformToAncestor(container)
                                .TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            var rect = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);
            return rect.IntersectsWith(bounds);
        }

        private void CustomValidationPopup_Opened(object? sender, EventArgs e)
        {
            SetTopmostState(true);
        }

        private void OnHostWindowActivated(object? sender, EventArgs e)
        {
            SetTopmostState(true);
        }

        private void OnHostWindowDeactivated(object? sender, EventArgs e)
        {
            SetTopmostState(false);
        }

        private void CustomValidationPopup_Unloaded(object? sender, RoutedEventArgs e)
        {
            OnUnLoaded();

            if (PlacementTarget is FrameworkElement frameworkElement)
            {
                frameworkElement.SizeChanged -= OnSizeOrLocationChanged;
            }

            if (hostWindow != null)
            {
                hostWindow.LocationChanged -= OnSizeOrLocationChanged;
                hostWindow.SizeChanged -= OnSizeOrLocationChanged;
                hostWindow.StateChanged -= OnHostWindowStateChanged;
                hostWindow.Activated -= OnHostWindowActivated;
                hostWindow.Deactivated -= OnHostWindowDeactivated;
            }

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }

            if (metroContentControl != null)
            {
                metroContentControl.TransitionStarted -= OnTransitionStarted;
                metroContentControl.TransitionCompleted -= OnTransitionCompleted;
            }

            if (transitioningContentControl != null)
            {
                transitioningContentControl.TransitionCompleted -= OnTransitionCompleted;
            }

            if (flyout != null)
            {
                flyout.OpeningFinished -= Flyout_OpeningFinished;
                flyout.IsOpenChanged -= Flyout_IsOpenChanged;
                flyout.ClosingFinished -= Flyout_ClosingFinished;
            }

            Unloaded -= CustomValidationPopup_Unloaded;
            Opened -= CustomValidationPopup_Opened;
            hostWindow = null;
        }

        protected virtual void OnLoaded()
        {
        }

        protected virtual void OnUnLoaded()
        {
        }

        private void OnHostWindowStateChanged(object? sender, EventArgs e)
        {
            if (hostWindow != null && hostWindow.WindowState != WindowState.Minimized)
            {
                var adornedElement = AdornedElement;
                if (adornedElement != null)
                {
                    PopupAnimation = PopupAnimation.None;
                    SetCurrentValue(IsOpenProperty, BooleanBoxes.FalseBox);
                    var errorTemplate = adornedElement.GetValue(Validation.ErrorTemplateProperty);
                    adornedElement.SetValue(Validation.ErrorTemplateProperty, null);
                    adornedElement.SetValue(Validation.ErrorTemplateProperty, errorTemplate);
                }
            }
        }

        private void OnSizeOrLocationChanged(object? sender, EventArgs e)
        {
            RefreshPosition();
        }

        private void RefreshPosition()
        {
            var offset = HorizontalOffset;
            // "bump" the offset to cause the popup to reposition itself on its own
            SetCurrentValue(HorizontalOffsetProperty, offset + 1);
            SetCurrentValue(HorizontalOffsetProperty, offset);
        }

        private bool? appliedTopMost;

        private void SetTopmostState(bool isTop)
        {
            // Dont apply state if its the same as incoming state
            if (appliedTopMost.HasValue && appliedTopMost == isTop)
            {
                return;
            }

            if (Child is null)
            {
                return;
            }

            if (!(PresentationSource.FromVisual(Child) is HwndSource hwndSource))
            {
                return;
            }

            var handle = hwndSource.Handle;

#pragma warning disable 618
            if (!UnsafeNativeMethods.GetWindowRect(handle, out var rect))
            {
                return;
            }
            //Debug.WriteLine("setting z-order " + isTop);

            var left = rect.Left;
            var top = rect.Top;
            var width = rect.Width;
            var height = rect.Height;
            if (isTop)
            {
                NativeMethods.SetWindowPos(handle, Constants.HWND_TOPMOST, left, top, width, height, SWP.TOPMOST);
            }
            else
            {
                // Z-Order would only get refreshed/reflected if clicking the
                // the titlebar (as opposed to other parts of the external
                // window) unless I first set the popup to HWND_BOTTOM
                // then HWND_TOP before HWND_NOTOPMOST
                NativeMethods.SetWindowPos(handle, Constants.HWND_BOTTOM, left, top, width, height, SWP.TOPMOST);
                NativeMethods.SetWindowPos(handle, Constants.HWND_TOP, left, top, width, height, SWP.TOPMOST);
                NativeMethods.SetWindowPos(handle, Constants.HWND_NOTOPMOST, left, top, width, height, SWP.TOPMOST);
            }

            appliedTopMost = isTop;
#pragma warning restore 618
        }
    }
}