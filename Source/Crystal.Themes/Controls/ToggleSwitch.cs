// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Automation.Peers;
using Crystal.Themes.Automation.Peers;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Controls
{
  /// <summary>
  /// A control that allows the user to toggle between two states: One represents true; The other represents false.
  /// </summary>
  [System.Diagnostics.CodeAnalysis.SuppressMessage("WpfAnalyzers.TemplatePart", "WPF0132:Use PART prefix.", Justification = "<Pending>")]
    [ContentProperty(nameof(Content))]
    [TemplatePart(Name = nameof(HeaderContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(ContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(OffContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(OnContentPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(SwitchKnobBounds), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(SwitchKnob), Type = typeof(FrameworkElement))]
    [TemplatePart(Name = nameof(KnobTranslateTransform), Type = typeof(TranslateTransform))]
    [TemplatePart(Name = nameof(SwitchThumb), Type = typeof(Thumb))]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateNormal)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateMouseOver)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StatePressed)]
    [TemplateVisualState(GroupName = VisualStates.GroupCommon, Name = VisualStates.StateDisabled)]
    [TemplateVisualState(GroupName = ContentStatesGroup, Name = OffContentState)]
    [TemplateVisualState(GroupName = ContentStatesGroup, Name = OnContentState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = DraggingState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = OffState)]
    [TemplateVisualState(GroupName = ToggleStatesGroup, Name = OnState)]
    public class ToggleSwitch : HeaderedContentControl, ICommandSource
    {
        private const string ContentStatesGroup = "ContentStates";
        private const string OffContentState = "OffContent";
        private const string OnContentState = "OnContent";
        private const string ToggleStatesGroup = "ToggleStates";
        private const string DraggingState = "Dragging";
        private const string OffState = "Off";
        private const string OnState = "On";

        private double onTranslation;
        private double startTranslation;
        private bool wasDragged;

        private ContentPresenter? HeaderContentPresenter { get; set; }

        private ContentPresenter? ContentPresenter { get; set; }

        private ContentPresenter? OffContentPresenter { get; set; }

        private ContentPresenter? OnContentPresenter { get; set; }

        private FrameworkElement? SwitchKnobBounds { get; set; }

        private FrameworkElement? SwitchKnob { get; set; }

        private TranslateTransform? KnobTranslateTransform { get; set; }

        private Thumb? SwitchThumb { get; set; }

        /// <summary>Identifies the <see cref="ContentDirection"/> dependency property.</summary>
        public static readonly DependencyProperty ContentDirectionProperty
            = DependencyProperty.Register(nameof(ContentDirection),
                                          typeof(FlowDirection),
                                          typeof(ToggleSwitch),
                                          new PropertyMetadata(FlowDirection.LeftToRight));

        /// <summary>
        /// Gets or sets the flow direction of the switch and content.
        /// </summary>
        /// <remarks>
        /// LeftToRight means content left and button right and RightToLeft vise versa.
        /// </remarks>
        [Bindable(true)]
        [Category(AppName.CrystalThemes)]
        public FlowDirection ContentDirection
        {
            get => (FlowDirection)GetValue(ContentDirectionProperty);
            set => SetValue(ContentDirectionProperty, value);
        }

        /// <summary>Identifies the <see cref="ContentPadding"/> dependency property.</summary>
        public static readonly DependencyProperty ContentPaddingProperty
            = DependencyProperty.Register(nameof(ContentPadding),
                                          typeof(Thickness),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));

        /// <summary>
        /// Gets or sets the padding of the inner content.
        /// </summary>
        [Bindable(true)]
        [Category(AppName.CrystalThemes)]
        public Thickness ContentPadding
        {
            get => (Thickness)GetValue(ContentPaddingProperty);
            set => SetValue(ContentPaddingProperty, value);
        }

        /// <summary>Identifies the <see cref="IsOn"/> dependency property.</summary>
        public static readonly DependencyProperty IsOnProperty
            = DependencyProperty.Register(nameof(IsOn),
                                          typeof(bool),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata(
                                              BooleanBoxes.FalseBox,
                                              FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                                              OnIsOnChanged));

        private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ToggleSwitch toggleSwitch && e.NewValue != e.OldValue && e.NewValue is bool newValue && e.OldValue is bool oldValue)
            {
                // doing soft casting here because the peer can be that of RadioButton and it is not derived from
                // ToggleButtonAutomationPeer - specifically to avoid implementing TogglePattern
                if (UIElementAutomationPeer.FromElement(toggleSwitch) is ToggleSwitchAutomationPeer peer)
                {
                    peer.RaiseToggleStatePropertyChangedEvent(oldValue, newValue);
                }

                toggleSwitch.OnToggled();
                toggleSwitch.UpdateVisualStates(true);
            }
        }

        /// <summary>
        /// Gets or sets a value that declares whether the state of the ToggleSwitch is "On".
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public bool IsOn
        {
            get => (bool)GetValue(IsOnProperty);
            set => SetValue(IsOnProperty, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="OnContent"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentProperty
            = DependencyProperty.Register(nameof(OnContent),
                                          typeof(object),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata("On", OnOnContentChanged));

        private static void OnOnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).OnOnContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnOnContentChanged(object oldContent, object newContent)
        {
        }

        /// <summary>
        /// Provides the object content that should be displayed using the OnContentTemplate when this ToggleSwitch has state of "On".
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public object OnContent
        {
            get => GetValue(OnContentProperty);
            set => SetValue(OnContentProperty, value);
        }

        /// <summary>Identifies the <see cref="OnContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentTemplateProperty
            = DependencyProperty.Register(nameof(OnContentTemplate),
                                          typeof(DataTemplate),
                                          typeof(ToggleSwitch));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's content while in "On" state.
        /// </summary>
        public DataTemplate? OnContentTemplate
        {
            get => (DataTemplate?)GetValue(OnContentTemplateProperty);
            set => SetValue(OnContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="OnContentTemplateSelector"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentTemplateSelectorProperty
            = DependencyProperty.Register(nameof(OnContentTemplateSelector),
                                          typeof(DataTemplateSelector),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a template selector for OnContent property that enables an application writer to provide custom template-selection logic .
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OnContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category(AppName.CrystalThemes)]
        public DataTemplateSelector? OnContentTemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(OnContentTemplateSelectorProperty);
            set => SetValue(OnContentTemplateSelectorProperty, value);
        }

        /// <summary>Identifies the <see cref="OnContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty OnContentStringFormatProperty
            = DependencyProperty.Register(nameof(OnContentStringFormat),
                                          typeof(string),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the OnContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OnContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category(AppName.CrystalThemes)]
        public string? OnContentStringFormat
        {
            get => (string?)GetValue(OnContentStringFormatProperty);
            set => SetValue(OnContentStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContent"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentProperty
            = DependencyProperty.Register(nameof(OffContent),
                                          typeof(object),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata("Off", OnOffContentChanged));

        private static void OnOffContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).OnOffContentChanged(e.OldValue, e.NewValue);
        }

        protected virtual void OnOffContentChanged(object oldContent, object newContent)
        {
        }

        /// <summary>
        /// Provides the object content that should be displayed using the OffContentTemplate when this ToggleSwitch has state of "Off".
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public object OffContent
        {
            get => GetValue(OffContentProperty);
            set => SetValue(OffContentProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentTemplateProperty
            = DependencyProperty.Register(nameof(OffContentTemplate),
                                          typeof(DataTemplate),
                                          typeof(ToggleSwitch));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the control's content while in "Off" state.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public DataTemplate? OffContentTemplate
        {
            get => (DataTemplate?)GetValue(OffContentTemplateProperty);
            set => SetValue(OffContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContentTemplateSelector"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentTemplateSelectorProperty
            = DependencyProperty.Register(nameof(OffContentTemplateSelector),
                                          typeof(DataTemplateSelector),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a template selector for OffContent property that enables an application writer to provide custom template-selection logic .
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OffContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category(AppName.CrystalThemes)]
        public DataTemplateSelector? OffContentTemplateSelector
        {
            get => (DataTemplateSelector?)GetValue(OffContentTemplateSelectorProperty);
            set => SetValue(OffContentTemplateSelectorProperty, value);
        }

        /// <summary>Identifies the <see cref="OffContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty OffContentStringFormatProperty
            = DependencyProperty.Register(nameof(OffContentStringFormat),
                                          typeof(string),
                                          typeof(ToggleSwitch),
                                          new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the OffContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="OffContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        [Category(AppName.CrystalThemes)]
        public string? OffContentStringFormat
        {
            get => (string?)GetValue(OffContentStringFormatProperty);
            set => SetValue(OffContentStringFormatProperty, value);
        }

        private static readonly DependencyPropertyKey IsPressedPropertyKey
            = DependencyProperty.RegisterReadOnly(nameof(IsPressed),
                                                  typeof(bool),
                                                  typeof(ToggleSwitch),
                                                  new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));

        /// <summary>Identifies the <see cref="IsPressed"/> dependency property.</summary>
        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

        [Browsable(false)]
        [ReadOnly(true)]
        [Category(AppName.CrystalThemes)]
        public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            protected set => SetValue(IsPressedPropertyKey, BooleanBoxes.Box(value));
        }

        /// <summary>Identifies the <see cref="Command"/> dependency property.</summary>
        public static readonly DependencyProperty CommandProperty
            = DependencyProperty.Register(nameof(Command),
                                          typeof(ICommand),
                                          typeof(ToggleSwitch),
                                          new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets a command which will be executed when the <see cref="IsOnProperty"/> changes.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>Identifies the <see cref="OnCommand"/> dependency property.</summary>
        public static readonly DependencyProperty OnCommandProperty
            = DependencyProperty.Register(nameof(OnCommand),
                                          typeof(ICommand),
                                          typeof(ToggleSwitch),
                                          new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets a command which will be executed when the <see cref="IsOnProperty"/> changes.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public ICommand? OnCommand
        {
            get => (ICommand?)GetValue(OnCommandProperty);
            set => SetValue(OnCommandProperty, value);
        }

        /// <summary>Identifies the <see cref="OffCommand"/> dependency property.</summary>
        public static readonly DependencyProperty OffCommandProperty
            = DependencyProperty.Register(nameof(OffCommand),
                                          typeof(ICommand),
                                          typeof(ToggleSwitch),
                                          new PropertyMetadata(null, OnCommandChanged));

        /// <summary>
        /// Gets or sets a command which will be executed when the <see cref="IsOnProperty"/> changes.
        /// </summary>
        [Category(AppName.CrystalThemes)]
        public ICommand? OffCommand
        {
            get => (ICommand?)GetValue(OffCommandProperty);
            set => SetValue(OffCommandProperty, value);
        }

        /// <summary>Identifies the <see cref="CommandParameter"/> dependency property.</summary>
        public static readonly DependencyProperty CommandParameterProperty
            = DependencyProperty.Register(nameof(CommandParameter),
                                          typeof(object),
                                          typeof(ToggleSwitch),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command parameter which will be passed by the Command.
        /// </summary>
        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>Identifies the <see cref="CommandTarget"/> dependency property.</summary>
        public static readonly DependencyProperty CommandTargetProperty
            = DependencyProperty.Register(nameof(CommandTarget),
                                          typeof(IInputElement),
                                          typeof(ToggleSwitch),
                                          new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the element on which to raise the specified Command.
        /// </summary>
        /// <returns>
        /// Element on which to raise the Command.
        /// </returns>
        public IInputElement? CommandTarget
        {
            get => (IInputElement?)GetValue(CommandTargetProperty);
            set => SetValue(CommandTargetProperty, value);
        }

        /// <summary>
        /// Occurs when "On"/"Off" state changes for this ToggleSwitch.
        /// </summary>
        public event RoutedEventHandler? Toggled;

        /// <summary>This method is invoked when the <see cref="IsOnProperty"/> changes.</summary>
        protected virtual void OnToggled()
        {
            Toggled?.Invoke(this, new RoutedEventArgs());
        }

        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
            EventManager.RegisterClassHandler(typeof(ToggleSwitch), MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnMouseLeftButtonDown), true);
        }

        public ToggleSwitch()
        {
            IsEnabledChanged += OnIsEnabledChanged;
        }

        public override void OnApplyTemplate()
        {
            if (SwitchKnobBounds != null && SwitchKnob != null && KnobTranslateTransform != null && SwitchThumb != null)
            {
                SwitchThumb.DragStarted -= OnSwitchThumbDragStarted;
                SwitchThumb.DragDelta -= OnSwitchThumbDragDelta;
                SwitchThumb.DragCompleted -= OnSwitchThumbDragCompleted;
            }

            base.OnApplyTemplate();

            HeaderContentPresenter = GetTemplateChild(nameof(HeaderContentPresenter)) as ContentPresenter;
            ContentPresenter = GetTemplateChild(nameof(ContentPresenter)) as ContentPresenter;
            OffContentPresenter = GetTemplateChild(nameof(OffContentPresenter)) as ContentPresenter;
            OnContentPresenter = GetTemplateChild(nameof(OnContentPresenter)) as ContentPresenter;
            SwitchKnobBounds = GetTemplateChild(nameof(SwitchKnobBounds)) as FrameworkElement;
            SwitchKnob = GetTemplateChild(nameof(SwitchKnob)) as FrameworkElement;
            KnobTranslateTransform = GetTemplateChild(nameof(KnobTranslateTransform)) as TranslateTransform;
            SwitchThumb = GetTemplateChild(nameof(SwitchThumb)) as Thumb;

            if (SwitchKnobBounds != null && SwitchKnob != null && KnobTranslateTransform != null && SwitchThumb != null)
            {
                SwitchThumb.DragStarted += OnSwitchThumbDragStarted;
                SwitchThumb.DragDelta += OnSwitchThumbDragDelta;
                SwitchThumb.DragCompleted += OnSwitchThumbDragCompleted;
            }

            UpdateHeaderContentPresenterVisibility();
            UpdateContentPresenterVisibility();

            UpdateVisualStates(false);
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateVisualStates(true);
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ToggleSwitch toggle && !toggle.IsKeyboardFocusWithin)
            {
                e.Handled = toggle.Focus() || e.Handled;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                Toggle();
            }

            base.OnKeyUp(e);
        }

        protected override void OnHeaderChanged(object oldHeader, object newHeader)
        {
            base.OnHeaderChanged(oldHeader, newHeader);

            UpdateHeaderContentPresenterVisibility();
        }

        private void UpdateHeaderContentPresenterVisibility()
        {
            if (HeaderContentPresenter == null)
            {
                return;
            }

            bool showHeader = (Header is string s && !string.IsNullOrEmpty(s)) || Header != null;
            HeaderContentPresenter.Visibility = showHeader ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            UpdateContentPresenterVisibility();
        }

        private void UpdateContentPresenterVisibility()
        {
            if (ContentPresenter is null
                || OffContentPresenter is null
                || OnContentPresenter is null)
            {
                return;
            }

            bool showContent = (Content is string s && !string.IsNullOrEmpty(s)) || Content != null;
            ContentPresenter.Visibility = showContent ? Visibility.Visible : Visibility.Collapsed;
            OffContentPresenter.Visibility = !showContent ? Visibility.Visible : Visibility.Collapsed;
            OnContentPresenter.Visibility = !showContent ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsMouseOverProperty)
            {
                UpdateVisualStates(true);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (SwitchKnobBounds != null && SwitchKnob != null)
            {
                onTranslation = SwitchKnobBounds.ActualWidth - SwitchKnob.ActualWidth - SwitchKnob.Margin.Left - SwitchKnob.Margin.Right;
            }
        }

        private void OnSwitchThumbDragStarted(object sender, DragStartedEventArgs e)
        {
            e.Handled = true;
            IsPressed = true;
            wasDragged = false;
            startTranslation = KnobTranslateTransform!.X;
            UpdateVisualStates(true);
            KnobTranslateTransform.X = startTranslation;
        }

        private void OnSwitchThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            e.Handled = true;
            if (e.HorizontalChange != 0)
            {
                wasDragged = Math.Abs(e.HorizontalChange) >= SystemParameters.MinimumHorizontalDragDistance;
                double dragTranslation = startTranslation + e.HorizontalChange;
                KnobTranslateTransform!.X = Math.Max(0, Math.Min(onTranslation, dragTranslation));
            }
        }

        private void OnSwitchThumbDragCompleted(object sender, DragCompletedEventArgs e)
        {
            e.Handled = true;
            IsPressed = false;
            if (wasDragged)
            {
                if (!IsOn && KnobTranslateTransform!.X + SwitchKnob!.ActualWidth / 2 >= SwitchKnobBounds!.ActualWidth / 2)
                {
                    Toggle();
                }
                else if (IsOn && KnobTranslateTransform!.X + SwitchKnob!.ActualWidth / 2 <= SwitchKnobBounds!.ActualWidth / 2)
                {
                    Toggle();
                }
                else
                {
                    UpdateVisualStates(true);
                }
            }
            else
            {
                Toggle();
            }

            wasDragged = false;
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            string stateName;

            if (!IsEnabled)
            {
                stateName = VisualStates.StateDisabled;
            }
            else if (IsPressed)
            {
                stateName = VisualStates.StatePressed;
            }
            else if (IsMouseOver)
            {
                stateName = VisualStates.StateMouseOver;
            }
            else
            {
                stateName = VisualStates.StateNormal;
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);

            if (SwitchThumb != null && SwitchThumb.IsDragging)
            {
                stateName = DraggingState;
            }
            else
            {
                stateName = IsOn ? OnState : OffState;
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);

            VisualStateManager.GoToState(this, IsOn ? OnContentState : OffContentState, useTransitions);
        }

        private void Toggle()
        {
            var newValue = !IsOn;
            SetCurrentValue(IsOnProperty, BooleanBoxes.Box(newValue));

            CommandHelpers.ExecuteCommandSource(this);
            CommandHelpers.ExecuteCommandSource(this, newValue ? OnCommand : OffCommand);
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSwitch)d).OnCommandChanged((ICommand?)e.OldValue, (ICommand?)e.NewValue);
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

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToggleSwitchAutomationPeer(this);
        }

        internal void AutomationPeerToggle()
        {
            Toggle();
        }
    }
}