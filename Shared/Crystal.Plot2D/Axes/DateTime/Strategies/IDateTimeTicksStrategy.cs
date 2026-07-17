namespace Crystal.Plot2D.Axes;

public interface IDateTimeTicksStrategy {
  DifferenceIn GetDifference(System.TimeSpan span);
  bool TryGetLowerDiff(DifferenceIn diff, out DifferenceIn lowerDiff);
  bool TryGetBiggerDiff(DifferenceIn diff, out DifferenceIn biggerDiff);
}
