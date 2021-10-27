using Crystal;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace C0302_RegionInfra
{
	public class StackPanelRegionAdapter : RegionAdapterBase<StackPanel>
	{
		public StackPanelRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory) : base(regionBehaviorFactory)
		{
		}

		protected override void Adapt(IRegion region, StackPanel regionTarget)
		{
			region.Views.CollectionChanged += (s, e) =>
			{
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					foreach (FrameworkElement element in e.NewItems)
					{
						regionTarget.Children.Add(element);
					}
				}

				// handle remove
			};
		}

		protected override IRegion CreateRegion()
		{
			return new AllActiveRegion();
		}
	}
}
