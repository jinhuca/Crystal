using System.Collections.Specialized;

namespace Crystal;
#pragma warning disable CS1574 // XML comment has cref attribute 'Add(object,string,bool)' that could not be resolved
/// <summary>
/// Behavior that monitors a <see cref="IRegion"/> object and 
/// changes the value for the <see cref="IActiveAware.IsActive"/> property when
/// an object that implements <see cref="IActiveAware"/> gets added or removed 
/// from the collection.
/// </summary>
/// <remarks>
/// This class can also sync the active state for any scoped regions directly on the view based on the <see cref="SyncActiveStateAttribute"/>.
/// If you use the <see cref="Crystal.Regions.Region.Add(object,string,bool)" /> method with the createRegionManagerScope option, the scoped manager will be attached to the view.
/// </remarks>
public class RegionActiveAwareBehavior : IRegionBehavior
#pragma warning restore CS1574 // XML comment has cref attribute 'Add(object,string,bool)' that could not be resolved
{
  /// <summary>
  /// Name that identifies the <see cref="RegionActiveAwareBehavior"/> behavior in a collection of <see cref="IRegionBehavior"/>.
  /// </summary>
  public const string BehaviorKey = "ActiveAware";

  /// <summary>
  /// The region that this behavior is extending
  /// </summary>
  public IRegion Region { get; set; }

  /// <summary>
  /// Attaches the behavior to the specified region
  /// </summary>
  public void Attach()
  {
    INotifyCollectionChanged collection = GetCollection();
    if (collection != null)
    {
      collection.CollectionChanged += OnCollectionChanged;
    }
  }

  /// <summary>
  /// Detaches the behavior from the <see cref="INotifyCollectionChanged"/>.
  /// </summary>
  public void Detach()
  {
    INotifyCollectionChanged collection = GetCollection();
    if (collection != null)
    {
      collection.CollectionChanged -= OnCollectionChanged;
    }
  }

  private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
  {
    if (e.Action == NotifyCollectionChangedAction.Add)
    {
      foreach (object item in e.NewItems)
      {
        void Invocation(IActiveAware activeAware) => activeAware.IsActive = true;
        MvvmHelpers.ViewAndViewModelAction(item, (Action<IActiveAware>) Invocation);
        InvokeOnSynchronizedActiveAwareChildren(item, Invocation);
      }
    }
    else if (e.Action == NotifyCollectionChangedAction.Remove)
    {
      foreach (object item in e.OldItems)
      {
        void Invocation(IActiveAware activeAware) => activeAware.IsActive = false;
        MvvmHelpers.ViewAndViewModelAction(item, (Action<IActiveAware>) Invocation);
        InvokeOnSynchronizedActiveAwareChildren(item, Invocation);
      }
    }

    // May need to handle other action values (reset, replace). Currently the ViewsCollection class does not raise CollectionChanged with these values.
  }

  private void InvokeOnSynchronizedActiveAwareChildren(object item, Action<IActiveAware> invocation)
  {
    var dependencyObjectView = (DependencyObject)item;

    if (dependencyObjectView == null) return;

    // We are assuming that any scoped region managers are attached directly to the view.
    var regionManager = RegionManager.GetRegionManager(dependencyObjectView);

    // If the view's RegionManager attached property is different from the region's RegionManager,
    // then the view's region manager is a scoped region manager.
    if (regionManager == null || regionManager == Region.RManager)
    {
      return;
    }

    var activeViews = regionManager.Regions.SelectMany(e => e.ActiveViews);
    var syncActiveViews = activeViews.Where(ShouldSyncActiveState);

    foreach (var syncActiveView in syncActiveViews)
    {
      MvvmHelpers.ViewAndViewModelAction(syncActiveView, invocation);
    }
  }

  private bool ShouldSyncActiveState(object view)
  {
    if (Attribute.IsDefined(view.GetType(), typeof(SyncActiveStateAttribute)))
    {
      return true;
    }

    if (view is FrameworkElement viewAsFrameworkElement)
    {
      var viewModel = viewAsFrameworkElement.DataContext;
      return viewModel != null && Attribute.IsDefined(viewModel.GetType(), typeof(SyncActiveStateAttribute));
    }

    return false;
  }

  private INotifyCollectionChanged GetCollection()
  {
    return Region.ActiveViews;
  }
}