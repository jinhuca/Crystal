using System.Collections.Specialized;

namespace Crystal;

/// <summary>
/// Defines a behavior that forwards the <see cref="RegionManager.RegionContextProperty"/> 
/// to the views in the region.
/// </summary>
public class BindRegionContextToDependencyObjectBehavior : IRegionBehavior
{
  /// <summary>
  /// The key of this behavior.
  /// </summary>
  public const string BehaviorKey = "ContextToDependencyObject";

  /// <summary>
  /// Behavior's attached region.
  /// </summary>
  public IRegion Region { get; set; }

  /// <summary>
  /// Attaches the behavior to the specified region.
  /// </summary>
  public void Attach()
  {
    Region.Views.CollectionChanged += Views_CollectionChanged;
    Region.PropertyChanged += Region_PropertyChanged;
    SetContextToViews(Region.Views, Region.Context);
    AttachNotifyChangeEvent(Region.Views);
  }

  private static void SetContextToViews(IEnumerable views, object context)
  {
    foreach (var view in views)
    {
      if (view is not DependencyObject dependencyObjectView) continue;
      var contextWrapper = RegionContext.GetObservableContext(dependencyObjectView);
      contextWrapper.Value = context;
    }
  }

  private void AttachNotifyChangeEvent(IEnumerable views)
  {
    foreach (var view in views)
    {
      if (view is not DependencyObject dependencyObject) continue;
      var viewRegionContext = RegionContext.GetObservableContext(dependencyObject);
      viewRegionContext.PropertyChanged += ViewRegionContext_OnPropertyChangedEvent;
    }
  }

  private void DetachNotifyChangeEvent(IEnumerable views)
  {
    foreach (var view in views)
    {
      if (view is not DependencyObject dependencyObject) continue;
      var viewRegionContext = RegionContext.GetObservableContext(dependencyObject);
      viewRegionContext.PropertyChanged -= ViewRegionContext_OnPropertyChangedEvent;
    }
  }

  private void ViewRegionContext_OnPropertyChangedEvent(object sender, PropertyChangedEventArgs args)
  {
    if (args.PropertyName != "Value") return;
    var context = (ObservableObject<object>)sender;
    Region.Context = context.Value;
  }

  private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.Action == NotifyCollectionChangedAction.Add)
    {
      SetContextToViews(e.NewItems, Region.Context);
      AttachNotifyChangeEvent(e.NewItems);
    }
    else if (e.Action == NotifyCollectionChangedAction.Remove && Region.Context != null)
    {
      DetachNotifyChangeEvent(e.OldItems);
      SetContextToViews(e.OldItems, null);

    }
  }

  private void Region_PropertyChanged(object sender, PropertyChangedEventArgs e)
  {
    if (e.PropertyName == "Context")
    {
      SetContextToViews(Region.Views, Region.Context);
    }
  }
}