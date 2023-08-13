using System.Windows;

namespace Crystal.Behaviors;

/// <summary>
/// An action that will change the value of a property from a data store object.
/// This class is identical to ChangePropertyAction. The only difference is that the data store picker is loaded 
/// for this action.
/// </summary>
[DefaultTrigger(typeof(UIElement), typeof(Behaviors.EventTrigger), "Loaded")]
public class SetDataStoreValueAction : ChangePropertyAction
{
}