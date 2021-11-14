// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  [ContentProperty(nameof(ItemsSource))]
    [TemplatePart(Name = "PART_Button", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_Menu", Type = typeof(ContextMenu))]
    [StyleTypedProperty(Property = nameof(ButtonStyle), StyleTargetType = typeof(Button))]
    [StyleTypedProperty(Property = nameof(MenuStyle), StyleTargetType = typeof(ContextMenu))]
    public class DropDownButton : ItemsControl, ICommandSource
    {
        public static readonly RoutedEvent ClickEvent
            = EventManager.RegisterRoutedEvent(nameof(Click),
                                               RoutingStrategy.Bubble,
                                               typeof(RoutedEventHandler),
                                               typeof(DropDownButton));

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        /// <summary>Identifies the <see cref="IsExpanded"/> dependency property.</summary>
        public static readonly DependencyProperty IsExpandedProperty
            = DependencyProperty.Register(nameof(IsExpanded),
                                          typeof(bool),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(BooleanBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsExpandedPropertyChangedCallback));

        private static void OnIsExpandedPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyObject is DropDownButton dropDownButton
                && dropDownButton.contextMenu is not null)
            {
                dropDownButton.SetContextMenuPlacementTarget(dropDownButton.contextMenu);
            }
        }

        protected virtual void SetContextMenuPlacementTarget(ContextMenu contextMenu)
        {
            if (button != null)
            {
                contextMenu.PlacementTarget = button;
            }
        }

        /// <summary> 
        /// Whether or not the "popup" menu for this control is currently open
        /// </summary>
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set => SetValue(IsExpandedProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="ExtraTag"/> dependency property.</summary>
        public static readonly DependencyProperty ExtraTagProperty
            = DependencyProperty.Register(nameof(ExtraTag),
                                          typeof(object),
                                          typeof(DropDownButton));

        /// <summary>
        /// Gets or sets an extra tag.
        /// </summary>
        public object? ExtraTag
        {
            get => GetValue(ExtraTagProperty);
            set => SetValue(ExtraTagProperty, value);
        }

        /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
        public static readonly DependencyProperty OrientationProperty
            = DependencyProperty.Register(nameof(Orientation),
                                          typeof(Orientation),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the orientation of children stacking.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>Identifies the <see cref="Icon"/> dependency property.</summary>
        public static readonly DependencyProperty IconProperty
            = DependencyProperty.Register(nameof(Icon),
                                          typeof(object),
                                          typeof(DropDownButton));

        /// <summary>
        /// Gets or sets the content for the icon part.
        /// </summary>
        [Bindable(true)]
        public object? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>Identifies the <see cref="IconTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty IconTemplateProperty
            = DependencyProperty.Register(nameof(IconTemplate),
                                          typeof(DataTemplate),
                                          typeof(DropDownButton));

        /// <summary> 
        /// Gets or sets the DataTemplate for the icon part.
        /// </summary>
        [Bindable(true)]
        public DataTemplate? IconTemplate
        {
            get => (DataTemplate?)GetValue(IconTemplateProperty);
            set => SetValue(IconTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.Register(nameof(Command),
                                          typeof(ICommand),
                                          typeof(DropDownButton),
                                          new PropertyMetadata(null, OnCommandPropertyChangedCallback));

        private static void OnCommandPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            (dependencyObject as DropDownButton)?.OnCommandChanged((ICommand?)e.OldValue, (ICommand?)e.NewValue);
        }

        /// <summary>
        /// Gets or sets the command to invoke when the content button is pressed.
        /// </summary>
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>Identifies the <see cref="CommandTarget"/> dependency property.</summary>
        public static readonly DependencyProperty CommandTargetProperty
            = DependencyProperty.Register(nameof(CommandTarget),
                                          typeof(IInputElement),
                                          typeof(DropDownButton),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the element on which to raise the specified command.
        /// </summary>
        public IInputElement? CommandTarget
        {
            get => (IInputElement?)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
        public static readonly DependencyProperty CommandParameterProperty
            = DependencyProperty.Register(nameof(CommandParameter),
                                          typeof(object),
                                          typeof(DropDownButton),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the parameter to pass to the command property.
        /// </summary>
        public object? CommandParameter
        {
            get => (object?)GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>Identifies the <see cref="Content"/> dependency property.</summary>
        public static readonly DependencyProperty ContentProperty
            = DependencyProperty.Register(nameof(Content),
                                          typeof(object),
                                          typeof(DropDownButton));

        /// <summary>
        /// Gets or sets the content of this control.
        /// </summary>
        public object? Content
        {
            get => (object?)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>Identifies the <see cref="ContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty ContentTemplateProperty
            = DependencyProperty.Register(nameof(ContentTemplate),
                                          typeof(DataTemplate),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(null));

        /// <summary> 
        /// Gets or sets the data template used to display the content of the DropDownButton.
        /// </summary>
        [Bindable(true)]
        public DataTemplate? ContentTemplate
        {
            get => (DataTemplate?)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="ContentTemplateSelector"/> dependency property.</summary>
        public static readonly DependencyProperty ContentTemplateSelectorProperty
            = DependencyProperty.Register(nameof(ContentTemplateSelector),
                                          typeof(DataTemplateSelector),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a template selector that enables an application writer to provide custom template-selection logic.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="ContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public DataTemplateSelector? ContentTemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(ContentTemplateSelectorProperty);
            set => SetValue(ContentTemplateSelectorProperty, value);
        }

        /// <summary>Identifies the <see cref="ContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty ContentStringFormatProperty
            = DependencyProperty.Register(nameof(ContentStringFormat),
                                          typeof(string),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the content property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="ContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public string? ContentStringFormat
        {
            get => (string?)GetValue(ContentStringFormatProperty);
            set => SetValue(ContentStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonStyle"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonStyleProperty
            = DependencyProperty.Register(nameof(ButtonStyle),
                                          typeof(Style),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the button content style.
        /// </summary>
        public Style? ButtonStyle
        {
            get => (Style?)GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        /// <summary>Identifies the <see cref="MenuStyle"/> dependency property.</summary>
        public static readonly DependencyProperty MenuStyleProperty
            = DependencyProperty.Register(nameof(MenuStyle),
                                          typeof(Style),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the "popup" menu style.
        /// </summary>
        public Style? MenuStyle
        {
            get => (Style?)GetValue(MenuStyleProperty);
            set => SetValue(MenuStyleProperty, value);
        }

        /// <summary>Identifies the <see cref="ArrowBrush"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowBrushProperty
            = DependencyProperty.Register(nameof(ArrowBrush),
                                          typeof(Brush),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the foreground brush for the button arrow icon.
        /// </summary>
        public Brush? ArrowBrush
        {
            get => (Brush?)GetValue(ArrowBrushProperty);
            set => SetValue(ArrowBrushProperty, value);
        }

        /// <summary>Identifies the <see cref="ArrowMouseOverBrush"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowMouseOverBrushProperty
            = DependencyProperty.Register(nameof(ArrowMouseOverBrush),
                                          typeof(Brush),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the foreground brush of the button arrow icon if the mouse is over the drop down button.
        /// </summary>
        public Brush? ArrowMouseOverBrush
        {
            get => (Brush?)GetValue(ArrowMouseOverBrushProperty);
            set => SetValue(ArrowMouseOverBrushProperty, value);
        }

        /// <summary>Identifies the <see cref="ArrowPressedBrush"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowPressedBrushProperty
            = DependencyProperty.Register(nameof(ArrowPressedBrush),
                                          typeof(Brush),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the foreground brush of the button arrow icon if the arrow button is pressed.
        /// </summary>
        public Brush? ArrowPressedBrush
        {
            get => (Brush?)GetValue(ArrowPressedBrushProperty);
            set => SetValue(ArrowPressedBrushProperty, value);
        }

        /// <summary>Identifies the <see cref="ArrowVisibility"/> dependency property.</summary>
        public static readonly DependencyProperty ArrowVisibilityProperty
            = DependencyProperty.Register(nameof(ArrowVisibility),
                                          typeof(Visibility),
                                          typeof(DropDownButton),
                                          new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the visibility of the button arrow icon.
        /// </summary>
        public Visibility ArrowVisibility
        {
            get => (Visibility)GetValue(ArrowVisibilityProperty);
            set => SetValue(ArrowVisibilityProperty, value);
        }

        static DropDownButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton), new FrameworkPropertyMetadata(typeof(DropDownButton)));
        }

        private void OnCommandChanged(ICommand? oldCommand, ICommand? newCommand)
        {
            if (oldCommand != null)
            {
                UnhookCommand(oldCommand);
            }

            if (newCommand != null)
            {
                HookCommand(newCommand);
            }
        }

        private void UnhookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.RemoveHandler(command, OnCanExecuteChanged);
            UpdateCanExecute();
        }

        private void HookCommand(ICommand command)
        {
            CanExecuteChangedEventManager.AddHandler(command, OnCanExecuteChanged);
            UpdateCanExecute();
        }

        private void OnCanExecuteChanged(object? sender, EventArgs e)
        {
            UpdateCanExecute();
        }

        private void UpdateCanExecute()
        {
            CanExecute = Command == null || CommandHelpers.CanExecuteCommandSource(this);
        }

        /// <inheritdoc />
        protected override bool IsEnabledCore => base.IsEnabledCore && CanExecute;

        private bool canExecute = true;

        private bool CanExecute
        {
            get => canExecute;
            set
            {
                if (value == canExecute)
                {
                    return;
                }

                canExecute = value;
                CoerceValue(IsEnabledProperty);
            }
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            CommandHelpers.ExecuteCommandSource(this);

            if (contextMenu?.HasItems == true)
            {
                SetCurrentValue(IsExpandedProperty, BooleanBoxes.TrueBox);
            }

            e.RoutedEvent = ClickEvent;
            RaiseEvent(e);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (button != null)
            {
                button.Click -= ButtonClick;
            }

            button = GetTemplateChild("PART_Button") as Button;
            if (button != null)
            {
                button.Click += ButtonClick;
            }

            GroupStyle.CollectionChanged -= OnGroupStyleCollectionChanged;

            contextMenu = GetTemplateChild("PART_Menu") as ContextMenu;
            if (contextMenu != null)
            {
                foreach (var groupStyle in GroupStyle)
                {
                    contextMenu.GroupStyle.Add(groupStyle);
                }

                GroupStyle.CollectionChanged += OnGroupStyleCollectionChanged;

                if (Items != null && ItemsSource == null)
                {
                    foreach (var newItem in Items)
                    {
                        TryRemoveVisualFromOldTree(newItem);
                        contextMenu.Items.Add(newItem);
                    }
                }
            }
        }

#if NET5_0_OR_GREATER
        private void OnGroupStyleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
#else
        private void OnGroupStyleCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
#endif
        {
            if (e.OldItems != null)
            {
                foreach (var groupStyle in e.OldItems.OfType<GroupStyle>())
                {
                    contextMenu?.GroupStyle.Remove(groupStyle);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var groupStyle in e.NewItems.OfType<GroupStyle>())
                {
                    contextMenu?.GroupStyle.Add(groupStyle);
                }
            }
        }

        /// <inheritdoc />
        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonUp(e);
            e.Handled = true;
        }

        private void TryRemoveVisualFromOldTree(object? item)
        {
            if (item is Visual visual)
            {
                var parent = LogicalTreeHelper.GetParent(visual) as FrameworkElement ?? VisualTreeHelper.GetParent(visual) as FrameworkElement;
                if (Equals(this, parent))
                {
                    RemoveLogicalChild(visual);
                    RemoveVisualChild(visual);
                }
            }
        }

        /// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> property changes.</summary>
        /// <param name="e">Information about the change.</param>
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            if (contextMenu == null || ItemsSource != null || contextMenu.ItemsSource != null)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        foreach (var newItem in e.NewItems)
                        {
                            TryRemoveVisualFromOldTree(newItem);
                            contextMenu.Items.Add(newItem);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (var oldItem in e.OldItems)
                        {
                            contextMenu.Items.Remove(oldItem);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (var oldItem in e.OldItems)
                        {
                            contextMenu.Items.Remove(oldItem);
                        }
                    }

                    if (e.NewItems != null)
                    {
                        foreach (var newItem in e.NewItems)
                        {
                            TryRemoveVisualFromOldTree(newItem);
                            contextMenu.Items.Add(newItem);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (Items != null)
                    {
                        contextMenu.Items.Clear();
                        foreach (var newItem in Items)
                        {
                            TryRemoveVisualFromOldTree(newItem);
                            contextMenu.Items.Add(newItem);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Button? button;
        private ContextMenu? contextMenu;
    }
}