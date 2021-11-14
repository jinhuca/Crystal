// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Crystal.Themes.Controls
{
  [TemplatePart(Name = "PART_Scroll", Type = typeof(ScrollViewer))]
    [TemplatePart(Name = "PART_Headers", Type = typeof(ListView))]
    [TemplatePart(Name = "PART_Mediator", Type = typeof(ScrollViewerOffsetMediator))]
    public class Pivot : ItemsControl
    {
        private ScrollViewer? scrollViewer;
        private ListView? headers;
        private PivotItem? selectedItem;
        private ScrollViewerOffsetMediator? mediator;
        internal int internalIndex;

        /// <summary>Identifies the <see cref="SelectionChanged"/> routed event.</summary>
        public static readonly RoutedEvent SelectionChangedEvent
            = EventManager.RegisterRoutedEvent(nameof(SelectionChanged),
                                               RoutingStrategy.Bubble,
                                               typeof(RoutedEventHandler),
                                               typeof(Pivot));

        public event RoutedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        /// <summary>Identifies the <see cref="Header"/> dependency property.</summary>
        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register(nameof(Header),
                                          typeof(object),
                                          typeof(Pivot),
                                          new PropertyMetadata(default(string)));

        /// <summary>
        /// Gets or sets the Header of the <see cref="Pivot"/>.
        /// </summary>
        public object? Header
        {
            get => (object?)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        /// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty HeaderTemplateProperty
            = DependencyProperty.Register(nameof(HeaderTemplate),
                                          typeof(DataTemplate),
                                          typeof(Pivot));

        /// <summary>
        /// Gets or sets the HeaderTemplate of the <see cref="Pivot"/>.
        /// </summary>
        public DataTemplate? HeaderTemplate
        {
            get => (DataTemplate?)GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="SelectedIndex"/> dependency property.</summary>
        public static readonly DependencyProperty SelectedIndexProperty
            = DependencyProperty.Register(nameof(SelectedIndex),
                                          typeof(int),
                                          typeof(Pivot),
                                          new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexPropertyChanged));

        private static void OnSelectedIndexPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue is int newSelectedIndex)
            {
                var pivot = (Pivot)dependencyObject;
                if (pivot.internalIndex != pivot.SelectedIndex && newSelectedIndex >= 0 && newSelectedIndex < pivot.Items.Count)
                {
                    var pivotItem = (PivotItem)pivot.Items[newSelectedIndex];
                    // set headers selected item too
                    if (pivot.headers is not null)
                    {
                        pivot.headers.SelectedItem = pivotItem;
                    }

                    pivot.GoToItem(pivotItem);
                }
            }
        }

        public int SelectedIndex

        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public void GoToItem(PivotItem? item)
        {
            if (item is null || item == selectedItem)
            {
                return;
            }

            var widthToScroll = 0.0;
            int index;
            for (index = 0; index < Items.Count; index++)
            {
                if (ReferenceEquals(Items[index], item))
                {
                    internalIndex = index;
                    break;
                }

                widthToScroll += ((PivotItem)Items[index]).ActualWidth;
            }

            if (mediator is not null
                && scrollViewer is not null)
            {
                mediator.HorizontalOffset = scrollViewer.HorizontalOffset;
                var sb = mediator.Resources["Storyboard1"] as Storyboard;
                var frame = (EasingDoubleKeyFrame)mediator.FindName("edkf");
                frame.Value = widthToScroll;

                if (sb is not null)
                {
                    sb.Completed -= OnStoryboardCompleted;
                    sb.Completed += OnStoryboardCompleted;
                    sb.Begin();
                }
            }

            RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
        }

        private void OnStoryboardCompleted(object? sender, EventArgs e)
        {
            SetCurrentValue(SelectedIndexProperty, internalIndex);
        }

        static Pivot()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Pivot), new FrameworkPropertyMetadata(typeof(Pivot)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scrollViewer = GetTemplateChild("PART_Scroll") as ScrollViewer;
            headers = GetTemplateChild("PART_Headers") as ListView;
            mediator = GetTemplateChild("PART_Mediator") as ScrollViewerOffsetMediator;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewerScrollChanged;
                scrollViewer.PreviewMouseWheel += ScrollViewerMouseWheel;
            }

            if (headers != null)
            {
                headers.SelectionChanged += headers_SelectionChanged;
            }
        }

        private void ScrollViewerMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            scrollViewer!.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + -e.Delta);
        }

        private void headers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GoToItem((PivotItem)headers!.SelectedItem);
        }

        private void ScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var position = 0.0;
            for (var i = 0; i < Items.Count; i++)
            {
                var pivotItem = ((PivotItem)Items[i]);
                var widthOfItem = pivotItem.ActualWidth;
                if (e.HorizontalOffset <= (position + widthOfItem - 1))
                {
                    selectedItem = pivotItem;
                    if (headers is not null && headers.SelectedItem != selectedItem)
                    {
                        headers.SelectedItem = selectedItem;
                        internalIndex = i;
                        SelectedIndex = i;

                        RaiseEvent(new RoutedEventArgs(SelectionChangedEvent));
                    }

                    break;
                }

                position += widthOfItem;
            }
        }
    }
}