using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using static System.String;
using static Crystal.Constants.StringConstants;

namespace Crystal
{
	/// <summary>
	/// This class is responsible for maintaining a collection of regions and attaching regions to controls.
	/// </summary>
	/// <remarks>
	/// This class supplies the attached properties that can be used for simple region creation from XAML.
	/// </remarks>
	public class RegionManager : IRegionManager
	{
		#region Static members (for XAML support)

		private static readonly WeakDelegatesManager updatingRegionsListeners = new();

		/// <summary>
		/// Identifies the RegionName attached property.
		/// </summary>
		/// <remarks>
		/// When a control has both the <see cref="RegionNameProperty"/> and
		/// <see cref="RegionManagerProperty"/> attached properties set to
		/// a value different than <see langword="null" /> and there is a
		/// <see cref="IRegionAdapter"/> mapping registered for the control, it
		/// will create and adapt a new region for that control, and register it
		/// in the <see cref="IRegionManager"/> with the specified region name.
		/// </remarks>
		public static readonly DependencyProperty RegionNameProperty = DependencyProperty.RegisterAttached(
				"RegionName",
				typeof(string),
				typeof(RegionManager),
				new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnSetRegionNameCallback));

		/// <summary>
		/// Sets the <see cref="RegionNameProperty"/> attached property.
		/// </summary>
		/// <param name="regionTarget">The object to adapt. This is typically a container (i.e a control).</param>
		/// <param name="regionName">The name of the region to register.</param>
		public static void SetRegionName(DependencyObject regionTarget, string regionName)
		{
			if (regionTarget == null)
			{
				throw new ArgumentNullException(nameof(regionTarget));
			}

			regionTarget.SetValue(RegionNameProperty, regionName);
		}

		/// <summary>
		/// Gets the value for the <see cref="RegionNameProperty"/> attached property.
		/// </summary>
		/// <param name="regionTarget">The object to adapt. This is typically a container (i.e a control).</param>
		/// <returns>The name of the region that should be created when
		/// <see cref="RegionManagerProperty"/> is also set in this element.</returns>
		public static string GetRegionName(DependencyObject regionTarget)
		{
			return regionTarget == null
				? throw new ArgumentNullException(nameof(regionTarget))
				: regionTarget.GetValue(RegionNameProperty) as string;
		}

		private static readonly DependencyProperty ObservableRegionProperty = DependencyProperty.RegisterAttached(
			"ObservableRegion",
			typeof(ObservableObject<IRegion>),
			typeof(RegionManager),
			null);


		/// <summary>
		/// Returns an <see cref="ObservableObject{T}"/> wrapper that can hold an <see cref="IRegion"/>. Using this wrapper
		/// you can detect when an <see cref="IRegion"/> has been created by the <see cref="RegionAdapterBase{T}"/>.
		///
		/// If the <see cref="ObservableObject{T}"/> wrapper does not yet exist, a new wrapper will be created. When the region
		/// gets created and assigned to the wrapper, you can use the <see cref="ObservableObject{T}.PropertyChanged"/> event
		/// to get notified of that change.
		/// </summary>
		/// <param name="view">The view that will host the region. </param>
		/// <returns>Wrapper that can hold an <see cref="IRegion"/> value and can notify when the <see cref="IRegion"/> value changes. </returns>
		public static ObservableObject<IRegion> GetObservableRegion(DependencyObject view)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}

			if (view.GetValue(ObservableRegionProperty) is not ObservableObject<IRegion> regionWrapper)
			{
				regionWrapper = new ObservableObject<IRegion>();
				view.SetValue(ObservableRegionProperty, regionWrapper);
			}

			return regionWrapper;
		}

		private static void OnSetRegionNameCallback(DependencyObject element, DependencyPropertyChangedEventArgs args)
		{
			if (!IsInDesignMode(element))
			{
				CreateRegion(element);
			}
		}

		private static void CreateRegion(DependencyObject element)
		{
			var container = ContainerLocator.Container;
			DelayedRegionCreationBehavior regionCreationBehavior = container.Resolve<DelayedRegionCreationBehavior>();
			regionCreationBehavior.TargetElement = element;
			regionCreationBehavior.Attach();
		}

		/// <summary>
		/// Identifies the RegionManager attached property.
		/// </summary>
		/// <remarks>
		/// When a control has both the <see cref="RegionNameProperty"/> and
		/// <see cref="RegionManagerProperty"/> attached properties set to
		/// a value different than <see langword="null" /> and there is a
		/// <see cref="IRegionAdapter"/> mapping registered for the control, it
		/// will create and adapt a new region for that control, and register it
		/// in the <see cref="IRegionManager"/> with the specified region name.
		/// </remarks>
		public static readonly DependencyProperty RegionManagerProperty = DependencyProperty.RegisterAttached(
			"RegionManager",
			typeof(IRegionManager),
			typeof(RegionManager),
			null);

		/// <summary>
		/// Gets the value of the <see cref="RegionNameProperty"/> attached property.
		/// </summary>
		/// <param name="target">The target element.</param>
		/// <returns>The <see cref="IRegionManager"/> attached to the <paramref name="target"/> element.</returns>
		public static IRegionManager GetRegionManager(DependencyObject target)
		{
			return target == null
				? throw new ArgumentNullException(nameof(target))
				: (IRegionManager)target.GetValue(RegionManagerProperty);
		}

		/// <summary>
		/// Sets the <see cref="RegionManagerProperty"/> attached property.
		/// </summary>
		/// <param name="target">The target element.</param>
		/// <param name="value">The value.</param>
		public static void SetRegionManager(DependencyObject target, IRegionManager value)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			target.SetValue(RegionManagerProperty, value);
		}

		/// <summary>
		/// Identifies the RegionContext attached property.
		/// </summary>
		public static readonly DependencyProperty RegionContextProperty = DependencyProperty.RegisterAttached(
			"RegionContext",
			typeof(object),
			typeof(RegionManager),
			new PropertyMetadata(defaultValue: null, propertyChangedCallback: OnRegionContextChanged));

		private static void OnRegionContextChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
		{
			if (RegionContext.GetObservableContext(depObj).Value != e.NewValue)
			{
				RegionContext.GetObservableContext(depObj).Value = e.NewValue;
			}
		}

		/// <summary>
		/// Gets the value of the <see cref="RegionContextProperty"/> attached property.
		/// </summary>
		/// <param name="target">The target element.</param>
		/// <returns>The region context to pass to the contained views.</returns>
		public static object GetRegionContext(DependencyObject target)
		{
			return target == null
				? throw new ArgumentNullException(nameof(target))
				: target.GetValue(RegionContextProperty);
		}

		/// <summary>
		/// Sets the <see cref="RegionContextProperty"/> attached property.
		/// </summary>
		/// <param name="target">The target element.</param>
		/// <param name="value">The value.</param>
		public static void SetRegionContext(DependencyObject target, object value)
		{
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}

			target.SetValue(RegionContextProperty, value);
		}

		/// <summary>
		/// Notification used by attached behaviors to update the region managers appropriately if needed to.
		/// </summary>
		/// <remarks>This event uses weak references to the event handler to prevent this static event of keeping the
		/// target element longer than expected.</remarks>
		public static event EventHandler UpdatingRegions
		{
			add => updatingRegionsListeners.AddListener(value);
			remove => updatingRegionsListeners.RemoveListener(value);
		}

		/// <summary>
		/// Notifies attached behaviors to update the region managers appropriately if needed to.
		/// </summary>
		/// <remarks>
		/// This method is normally called internally, and there is usually no need to call this from user code.
		/// </remarks>
		public static void UpdateRegions()
		{
			try
			{
				updatingRegionsListeners.Raise(null, EventArgs.Empty);
			}
			catch (TargetInvocationException ex)
			{
				Exception rootException = ex.GetRootException();
				throw new UpdateRegionsException(Format(UpdateRegionException, rootException), ex.InnerException);
			}
		}

		private static bool IsInDesignMode(DependencyObject element) => DesignerProperties.GetIsInDesignMode(element);

		#endregion

		private readonly RegionCollection regionCollection;

		/// <summary>
		/// Initializes a new instance of <see cref="RegionManager"/>.
		/// </summary>
		public RegionManager()
		{
			regionCollection = new RegionCollection(this);
		}

		/// <summary>
		/// Gets a collection of <see cref="IRegion"/> that identify each region by name. You can use this collection to add or remove regions to the current region manager.
		/// </summary>
		/// <value>A <see cref="IRegionCollection"/> with all the registered regions.</value>
		public IRegionCollection Regions => regionCollection;

		/// <summary>
		/// Creates a new region manager.
		/// </summary>
		/// <returns>A new region manager that can be used as a different scope from the current region manager.</returns>
		public IRegionManager CreateRegionManager() => new RegionManager();

		/// <summary>
		///     Add a view to the Views collection of a Region. Note that the region must already exist in this RegionManager.
		/// </summary>
		/// <param name="regionName">The name of the region to add a view to</param>
		/// <param name="view">The view to add to the views collection</param>
		/// <returns>The RegionManager, to easily add several views. </returns>
		public IRegionManager AddToRegion(string regionName, object view)
		{
			return !Regions.ContainsRegionWithName(regionName)
				? throw new ArgumentException(Format(Thread.CurrentThread.CurrentCulture, RegionNotFound, regionName), nameof(regionName))
				: Regions[regionName].Add(view);
		}

		/// <summary>
		/// Associate a view with a region, by registering a type. When the region get's displayed
		/// this type will be resolved using the ServiceLocator into a concrete instance. The instance
		/// will be added to the Views collection of the region
		/// </summary>
		/// <param name="regionName">The name of the region to associate the view with.</param>
		/// <typeparam name="T">The type of the view to register</typeparam>
		/// <returns>The RegionManager, for adding several views easily</returns>
		public IRegionManager RegisterViewWithRegion<T>(string regionName) => RegisterViewWithRegion(regionName, typeof(T));

		/// <summary>
		/// Associate a view with a region, by registering a type. When the region get's displayed
		/// this type will be resolved using the ServiceLocator into a concrete instance. The instance
		/// will be added to the Views collection of the region
		/// </summary>
		/// <param name="regionName">The name of the region to associate the view with.</param>
		/// <param name="viewType">The type of the view to register with the </param>
		/// <returns>The RegionManager, for adding several views easily</returns>
		public IRegionManager RegisterViewWithRegion(string regionName, Type viewType)
		{
			var regionViewRegistry = ContainerLocator.Container.Resolve<IRegionViewRegistry>();
			regionViewRegistry.RegisterViewWithRegion(regionName, viewType);
			return this;
		}

		/// <summary>
		/// Associate a view with a region, using a delegate to resolve a concrete instance of the view.
		/// When the region get's displayed, this delegate will be called and the result will be added to the
		/// views collection of the region.
		/// </summary>
		/// <param name="regionName">The name of the region to associate the view with.</param>
		/// <param name="getContentDelegate">The delegate used to resolve a concrete instance of the view.</param>
		/// <returns>The RegionManager, for adding several views easily</returns>
		public IRegionManager RegisterViewWithRegion(string regionName, Func<object> getContentDelegate)
		{
			var regionViewRegistry = ContainerLocator.Container.Resolve<IRegionViewRegistry>();
			regionViewRegistry.RegisterViewWithRegion(regionName, getContentDelegate);
			return this;
		}

		/// <summary>
		/// Navigates the specified region manager.
		/// </summary>
		/// <param name="regionName">The name of the region to call Navigate on.</param>
		/// <param name="source">The URI of the content to display.</param>
		/// <param name="navigationCallback">The navigation callback.</param>
		public void RequestNavigate(string regionName, Uri source, Action<NavigationResult> navigationCallback)
		{
			if (navigationCallback == null)
			{
				throw new ArgumentNullException(nameof(navigationCallback));
			}

			if (Regions.ContainsRegionWithName(regionName))
			{
				Regions[regionName].RequestNavigate(source, navigationCallback);
			}
			else
			{
				navigationCallback(new NavigationResult(new NavigationContext(null, source), false));
			}
		}

		/// <summary>
		/// Navigates the specified region manager.
		/// </summary>
		/// <param name="regionName">The name of the region to call Navigate on.</param>
		/// <param name="source">The URI of the content to display.</param>
		public void RequestNavigate(string regionName, Uri source) => RequestNavigate(regionName, source, _ => { });

		/// <summary>
		/// Navigates the specified region manager.
		/// </summary>
		/// <param name="regionName">The name of the region to call Navigate on.</param>
		/// <param name="source">The URI of the content to display.</param>
		/// <param name="navigationCallback">The navigation callback.</param>
		public void RequestNavigate(string regionName, string source, Action<NavigationResult> navigationCallback)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			RequestNavigate(regionName, new Uri(source, UriKind.RelativeOrAbsolute), navigationCallback);
		}

		/// <summary>
		/// Navigates the specified region manager.
		/// </summary>
		/// <param name="regionName">The name of the region to call Navigate on.</param>
		/// <param name="source">The URI of the content to display.</param>
		public void RequestNavigate(string regionName, string source) => RequestNavigate(regionName, source, _ => { });

		/// <summary>
		/// This method allows an IRegionManager to locate a specified region and navigate in it to the specified target Uri, passing a navigation callback and an instance of NavigationParameters, which holds a collection of object parameters.
		/// </summary>
		/// <param name="regionName">The name of the region where the navigation will occur.</param>
		/// <param name="target">A Uri that represents the target where the region will navigate.</param>
		/// <param name="navigationCallback">The navigation callback that will be executed after the navigation is completed.</param>
		/// <param name="navigationParameters">An instance of NavigationParameters, which holds a collection of object parameters.</param>
		public void RequestNavigate(string regionName, Uri target, Action<NavigationResult> navigationCallback, NavigationParameters navigationParameters)
		{
			if (navigationCallback == null)
			{
				throw new ArgumentNullException(nameof(navigationCallback));
			}

			if (Regions.ContainsRegionWithName(regionName))
			{
				Regions[regionName].RequestNavigate(target, navigationCallback, navigationParameters);
			}
			else
			{
				navigationCallback(new NavigationResult(new NavigationContext(null, target, navigationParameters), false));
			}
		}

		/// <summary>
		/// This method allows an IRegionManager to locate a specified region and navigate in it to the specified target string, passing a navigation callback and an instance of NavigationParameters, which holds a collection of object parameters.
		/// </summary>
		/// <param name="regionName">The name of the region where the navigation will occur.</param>
		/// <param name="target">A string that represents the target where the region will navigate.</param>
		/// <param name="navigationCallback">The navigation callback that will be executed after the navigation is completed.</param>
		/// <param name="navigationParameters">An instance of NavigationParameters, which holds a collection of object parameters.</param>
		public void RequestNavigate(string regionName, string target, Action<NavigationResult> navigationCallback, NavigationParameters navigationParameters)
			=> RequestNavigate(regionName, new Uri(target, UriKind.RelativeOrAbsolute), navigationCallback, navigationParameters);

		/// <summary>
		/// This method allows an IRegionManager to locate a specified region and navigate in it to the specified target Uri, passing an instance of NavigationParameters, which holds a collection of object parameters.
		/// </summary>
		/// <param name="regionName">The name of the region where the navigation will occur.</param>
		/// <param name="target">A Uri that represents the target where the region will navigate.</param>
		/// <param name="navigationParameters">An instance of NavigationParameters, which holds a collection of object parameters.</param>
		public void RequestNavigate(string regionName, Uri target, NavigationParameters navigationParameters)
			=> RequestNavigate(regionName, target, _ => { }, navigationParameters);

		/// <summary>
		/// This method allows an IRegionManager to locate a specified region and navigate in it to the specified target string, passing an instance of NavigationParameters, which holds a collection of object parameters.
		/// </summary>
		/// <param name="regionName">The name of the region where the navigation will occur.</param>
		/// <param name="target">A string that represents the target where the region will navigate.</param>
		/// <param name="navigationParameters">An instance of NavigationParameters, which holds a collection of object parameters.</param>
		public void RequestNavigate(string regionName, string target, NavigationParameters navigationParameters)
			=> RequestNavigate(regionName, new Uri(target, UriKind.RelativeOrAbsolute), _ => { }, navigationParameters);

		private class RegionCollection : IRegionCollection
		{
			private readonly IRegionManager regionManager;
			private readonly List<IRegion> regions;

			public RegionCollection(IRegionManager regionManager)
			{
				this.regionManager = regionManager;
				regions = new List<IRegion>();
			}

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			public IEnumerator<IRegion> GetEnumerator()
			{
				UpdateRegions();
				return regions.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

			public IRegion this[string regionName]
			{
				get
				{
					UpdateRegions();

					IRegion region = GetRegionByName(regionName);
					if (region == null)
					{
						throw new KeyNotFoundException(Format(RegionNotInRegionManagerException, regionName));
					}

					return region;
				}
			}

			public void Add(IRegion region)
			{
				if (region == null)
				{
					throw new ArgumentNullException(nameof(region));
				}

				UpdateRegions();

				if (region.Name == null)
				{
					throw new InvalidOperationException(RegionNameCannotBeEmptyExceptionMsg);
				}

				if (GetRegionByName(region.Name) != null)
				{
					throw new ArgumentException(Format(RegionNameExistsException, region.Name));
				}

				regions.Add(region);
				region.RManager = regionManager;

				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, region, 0));
			}

			public bool Remove(string regionName)
			{
				UpdateRegions();

				bool removed = false;

				IRegion region = GetRegionByName(regionName);
				if (region != null)
				{
					removed = true;
					regions.Remove(region);
					region.RManager = null;

					OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, region, 0));
				}

				return removed;
			}

			public bool ContainsRegionWithName(string regionName)
			{
				UpdateRegions();
				return GetRegionByName(regionName) != null;
			}

			/// <summary>
			/// Adds a region to the RegionManager with the name received as argument.
			/// </summary>
			/// <param name="regionName">The name to be given to the region.</param>
			/// <param name="region">The region to be added to the RegionManager.</param>
			/// <exception cref="ArgumentNullException">Thrown if <paramref name="region"/> is <see langword="null"/>.</exception>
			/// <exception cref="ArgumentException">Thrown if <paramref name="regionName"/> and <paramref name="region"/>'s name do not match and the <paramref name="region"/> <see cref="IRegion.Name"/> is not <see langword="null"/>.</exception>
			public void Add(string regionName, IRegion region)
			{
				if (region == null)
				{
					throw new ArgumentNullException(nameof(region));
				}
				if (region.Name != null && region.Name != regionName)
				{
					throw new ArgumentException(Format(RegionManagerWithDifferentNameException, region.Name, regionName), nameof(regionName));
				}
				region.Name ??= regionName;
				Add(region);
			}

			private IRegion GetRegionByName(string regionName) => regions.FirstOrDefault(r => r.Name == regionName);

			private void OnCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
				=> CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
		}
	}
}
