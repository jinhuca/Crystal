namespace Crystal.Themes.Behaviors;

public class StylizedBehaviorCollection : FreezableCollection<Behavior>
{
  protected override Freezable CreateInstanceCore()
  {
    return new StylizedBehaviorCollection();
  }
}