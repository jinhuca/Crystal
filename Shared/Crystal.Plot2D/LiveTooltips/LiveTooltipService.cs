using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Crystal.Plot2D.LiveTooltips;

public static class LiveToolTipService {

  #region Properties

  public static object GetToolTip(DependencyObject obj) {
    return (object)obj.GetValue(dp: ToolTipProperty);
  }

  public static void SetToolTip(DependencyObject obj, object value) {
    obj.SetValue(dp: ToolTipProperty, value: value);
  }

  public static readonly DependencyProperty ToolTipProperty = DependencyProperty.RegisterAttached(
    name: "ToolTip",
    propertyType: typeof(object),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: null, propertyChangedCallback: OnToolTipChanged));

  private static LiveToolTip GetliveToolTip(DependencyObject obj) {
    return (LiveToolTip)obj.GetValue(dp: liveToolTipProperty);
  }

  private static void SetliveToolTip(DependencyObject obj, LiveToolTip value) {
    obj.SetValue(dp: liveToolTipProperty, value: value);
  }

  private static readonly DependencyProperty liveToolTipProperty = DependencyProperty.RegisterAttached(
    name: "liveToolTip",
    propertyType: typeof(LiveToolTip),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(propertyChangedCallback: null));

  #region Opacity

  public static double GetTooltipOpacity(DependencyObject obj) {
    return (double)obj.GetValue(dp: TooltipOpacityProperty);
  }

  public static void SetTooltipOpacity(DependencyObject obj, double value) {
    obj.SetValue(dp: TooltipOpacityProperty, value: value);
  }

  public static readonly DependencyProperty TooltipOpacityProperty = DependencyProperty.RegisterAttached(
    name: "TooltipOpacity",
    propertyType: typeof(double),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: 1.0, propertyChangedCallback: OnTooltipOpacityChanged));

  private static void OnTooltipOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var liveTooltip_ = GetliveToolTip(obj: d);
    if(liveTooltip_ != null) {
      liveTooltip_.Opacity = (double)e.NewValue;
    }
  }

  #endregion // end of Opacity

  #region IsPropertyProxy property

  public static bool GetIsPropertyProxy(DependencyObject obj) {
    return (bool)obj.GetValue(dp: IsPropertyProxyProperty);
  }

  public static void SetIsPropertyProxy(DependencyObject obj, bool value) {
    obj.SetValue(dp: IsPropertyProxyProperty, value: value);
  }

  public static readonly DependencyProperty IsPropertyProxyProperty = DependencyProperty.RegisterAttached(
    name: "IsPropertyProxy",
    propertyType: typeof(bool),
    ownerType: typeof(LiveToolTipService),
    defaultMetadata: new FrameworkPropertyMetadata(defaultValue: false));

  #endregion // end of IsPropertyProxy property

  #endregion

  private static void OnToolTipChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
    var source_ = (FrameworkElement)d;

    if(e.NewValue == null) {
      source_.Loaded -= source_Loaded;
      source_.ClearValue(dp: liveToolTipProperty);
    }

    if(GetIsPropertyProxy(obj: source_)) {
      return;
    }

    var content_ = e.NewValue;

    if(content_ is DataTemplate template_) {
      content_ = template_.LoadContent();
    }

    LiveToolTip tooltip_;
    if(e.NewValue is LiveToolTip) {
      tooltip_ = e.NewValue as LiveToolTip;
    }
    else {
      tooltip_ = new LiveToolTip { Content = content_ };
    }

    if(tooltip_ == null && e.OldValue == null) {
      tooltip_ = new LiveToolTip { Content = content_ };
    }

    if(tooltip_ != null) {
      SetliveToolTip(obj: source_, value: tooltip_);
      if(!source_.IsLoaded) {
        source_.Loaded += source_Loaded;
      }
      else {
        AddTooltip(source: source_);
      }
    }
  }

  private static void AddTooltipForElement(FrameworkElement source, LiveToolTip tooltip) {
    var layer_ = AdornerLayer.GetAdornerLayer(visual: source);

    LiveToolTipAdorner adorner_ = new(adornedElement: source, tooltip: tooltip);
    layer_.Add(adorner: adorner_);
  }

  private static void source_Loaded(object sender, RoutedEventArgs e) {
    var source_ = (FrameworkElement)sender;

    if(source_.IsLoaded) {
      AddTooltip(source: source_);
    }
  }

  private static void AddTooltip(FrameworkElement source) {
    if(DesignerProperties.GetIsInDesignMode(element: source)) {
      return;
    }

    var tooltip_ = GetliveToolTip(obj: source);

    var window_ = Window.GetWindow(dependencyObject: source);
    var child_ = source;
    FrameworkElement parent_ = null;
    if(window_ != null) {
      while(parent_ != window_) {
        parent_ = (FrameworkElement)VisualTreeHelper.GetParent(reference: child_);
        child_ = parent_;
        var nameScope_ = NameScope.GetNameScope(dependencyObject: parent_ ?? throw new InvalidOperationException());
        if(nameScope_ != null) {
          var nameScopeName_ = nameScope_.ToString();
          if(nameScopeName_ != "System.Windows.TemplateNameScope") {
            NameScope.SetNameScope(dependencyObject: tooltip_, value: nameScope_);
            break;
          }
        }
      }
    }

    var binding_ = BindingOperations.GetBinding(target: tooltip_, dp: ContentControl.ContentProperty);
    if(binding_ != null) {
      BindingOperations.ClearBinding(target: tooltip_, dp: ContentControl.ContentProperty);
      BindingOperations.SetBinding(target: tooltip_, dp: ContentControl.ContentProperty, binding: binding_);
    }

    Binding dataContextBinding_ = new() { Path = new PropertyPath(path: "DataContext"), Source = source };
    tooltip_.SetBinding(dp: FrameworkElement.DataContextProperty, binding: dataContextBinding_);

    tooltip_.Owner = source;
    if(GetTooltipOpacity(obj: source) != (double)TooltipOpacityProperty.DefaultMetadata.DefaultValue) {
      tooltip_.Opacity = GetTooltipOpacity(obj: source);
    }

    AddTooltipForElement(source: source, tooltip: tooltip_);
  }
}
