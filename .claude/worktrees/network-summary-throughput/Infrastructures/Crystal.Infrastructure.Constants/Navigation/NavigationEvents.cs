using Prism.Events;

namespace Crystal.Infrastructure.Constants.Navigation;

/// <summary>
/// Raised by a module's summary tile to request that the shell replace the dashboard
/// with a full-scale detail view. The payload is the navigation name the detail view
/// was registered under (see <see cref="DetailViewNames"/>).
/// </summary>
public sealed class ShowDetailEvent : PubSubEvent<string> { }

/// <summary>
/// Raised by a detail view's "Back" affordance to request that the shell return to the
/// dashboard of summary tiles.
/// </summary>
public sealed class ShowDashboardEvent : PubSubEvent { }
