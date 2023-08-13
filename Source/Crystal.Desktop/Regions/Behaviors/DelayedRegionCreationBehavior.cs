using Crystal.Constants;

namespace Crystal;

/// <summary>
/// Behavior that creates a new <see cref="IRegion"/>, when the control that will host the <see cref="IRegion"/> (see <see cref="TargetElement"/>)
/// is added to the VisualTree. This behavior will use the <see cref="RegionAdapterMappings"/> class to find the right type of adapter to create
/// the region. After the region is created, this behavior will detach.
/// </summary>
/// <remarks>
/// Attached property value inheritance is not available in Silverlight, so the current approach walks up the visual tree when requesting a region from a region manager.
/// The <see cref="RegionManagerRegistrationBehavior"/> is now responsible for walking up the Tree.
/// </remarks>
public class DelayedRegionCreationBehavior
{
  private readonly RegionAdapterMappings regionAdapterMappings;
  private WeakReference elementWeakReference;
  private bool regionCreated;
  private static readonly ICollection<DelayedRegionCreationBehavior> _instanceTracker = new Collection<DelayedRegionCreationBehavior>();
  private readonly object _trackerLock = new();

  /// <summary>
  /// Initializes a new instance of the <see cref="DelayedRegionCreationBehavior"/> class.
  /// </summary>
  /// <param name="regionAdapterMappings">
  /// The region adapter mappings, that are used to find the correct adapter for
  /// a given control type. The control type is determined by the <see name="TargetElement"/> value.
  /// </param>
  public DelayedRegionCreationBehavior(RegionAdapterMappings regionAdapterMappings)
  {
    this.regionAdapterMappings = regionAdapterMappings;
    RegionManagerAccessor = new DefaultRegionManagerAccessor();
  }

  /// <summary>
  /// Sets a class that interfaces between the <see cref="RegionManager"/> 's static properties/events and this behavior,
  /// so this behavior can be tested in isolation.
  /// </summary>
  /// <value>The region manager accessor.</value>
  public IRegionManagerAccessor RegionManagerAccessor { get; set; }

  /// <summary>
  /// The element that will host the Region.
  /// </summary>
  /// <value>The target element.</value>
  public DependencyObject TargetElement
  {
    get => elementWeakReference.Target as DependencyObject;
    set => elementWeakReference = new WeakReference(value);
  }

  /// <summary>
  /// Start monitoring the <see cref="RegionManager"/> and the <see cref="TargetElement"/> to detect when the <see cref="TargetElement"/> becomes
  /// part of the Visual Tree. When that happens, the Region will be created and the behavior will <see cref="Detach"/>.
  /// </summary>
  public void Attach()
  {
    RegionManagerAccessor.UpdatingRegions += OnUpdatingRegions;
    WireUpTargetElement();
  }

  /// <summary>
  /// Stop monitoring the <see cref="RegionManager"/> and the  <see cref="TargetElement"/>, so that this behavior can be garbage collected.
  /// </summary>
  public void Detach()
  {
    RegionManagerAccessor.UpdatingRegions -= OnUpdatingRegions;
    UnWireTargetElement();
  }

  /// <summary>
  /// Called when the <see cref="RegionManager"/> is updating it's <see cref="RegionManager.Regions"/> collection.
  /// </summary>
  /// <remarks>
  /// This method has to be public, because it has to be callable using weak references in silverlight and other partial trust environments.
  /// </remarks>
  /// <param name="sender">The <see cref="RegionManager"/>. </param>
  /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
  public void OnUpdatingRegions(object sender, EventArgs e)
  {
    TryCreateRegion();
  }

  private void TryCreateRegion()
  {
    DependencyObject targetElement = TargetElement;
    if (targetElement == null)
    {
      Detach();
      return;
    }

    if (targetElement.CheckAccess())
    {
      Detach();

      if (!regionCreated)
      {
        string regionName = RegionManagerAccessor.GetRegionName(targetElement);
        CreateRegion(targetElement, regionName);
        regionCreated = true;
      }
    }
  }

  /// <summary>
  /// Method that will create the region, by calling the right <see cref="IRegionAdapter"/>.
  /// </summary>
  /// <param name="targetElement">The target element that will host the <see cref="IRegion"/>.</param>
  /// <param name="regionName">Name of the region.</param>
  /// <returns>The created <see cref="IRegion"/></returns>
  protected virtual IRegion CreateRegion(DependencyObject targetElement, string regionName)
  {
    if (targetElement == null)
    {
      throw new ArgumentNullException(nameof(targetElement));
    }

    try
    {
      // Build the region
      IRegionAdapter regionAdapter = regionAdapterMappings.GetMapping(targetElement.GetType());
      IRegion region = regionAdapter.Initialize(targetElement, regionName);

      return region;
    }
    catch (Exception ex)
    {
      throw new RegionCreationException(Format(StringConstants.RegionCreationExceptionMsg, regionName, ex), ex);
    }
  }

  private void ElementLoaded(object sender, RoutedEventArgs e)
  {
    UnWireTargetElement();
    TryCreateRegion();
  }

  private void WireUpTargetElement()
  {
    if (TargetElement is not FrameworkElement element)
    {
      var fcElement = (FrameworkContentElement)TargetElement;
      if (fcElement != null)
      {
        fcElement.Loaded += ElementLoaded;
        return;
      }

      //if the element is a dependency object, and not a FrameworkElement, nothing is holding onto the reference after the DelayedRegionCreationBehavior
      //is instantiated inside RegionManager.CreateRegion(DependencyObject element). If the GC runs before RegionManager.UpdateRegions is called, the region will
      //never get registered because it is gone from the updatingRegionsListeners list inside RegionManager. So we need to hold on to it. This should be rare.
      var depObj = TargetElement;
      if (depObj == null) return;
      Track();
    }
    else
    {
      element.Loaded += ElementLoaded;
    }
  }

  private void UnWireTargetElement()
  {
    var element = (FrameworkElement)TargetElement;
    if (element != null)
    {
      element.Loaded -= ElementLoaded;
      return;
    }

    if (TargetElement is FrameworkContentElement fcElement)
    {
      fcElement.Loaded -= ElementLoaded;
      return;
    }

    var depObj = TargetElement;
    if (depObj == null) return;
    Untrack();
  }


  /// <summary>
  /// Add the instance of this class to <see cref="_instanceTracker"/> to keep it alive
  /// </summary>
  private void Track()
  {
    lock (_trackerLock)
    {
      if (!_instanceTracker.Contains(this))
      {
        _instanceTracker.Add(this);
      }
    }
  }

  /// <summary>
  /// Remove the instance of this class from <see cref="_instanceTracker"/>
  /// so it can eventually be garbage collected
  /// </summary>
  private void Untrack()
  {
    lock (_trackerLock)
    {
      _instanceTracker.Remove(this);
    }
  }
}