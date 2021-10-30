namespace Crystal
{
	/// <summary>
	/// Provides a way for objects involved in navigation to opt-out of being added to the IRegionNavigationJournal back stack.
	/// </summary>
	public interface IJournalAware
	{
		/// <summary>
		/// Determines if the current object is going to be added to the navigation journal's back stack.
		/// </summary>
		/// <returns>True, add to back stack. False, remove from back stack.</returns>
		bool PersistInHistory();
	}
}
