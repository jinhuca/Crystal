using System.Windows;

namespace Crystal.Behaviors
{
  /// <summary>
  /// Represents one ternary condition.
  /// </summary>
  public class ComparisonCondition : Freezable
  {
    public static readonly DependencyProperty LeftOperandProperty = DependencyProperty.Register(
      nameof(LeftOperand),
      typeof(object),
      typeof(ComparisonCondition),
      new PropertyMetadata(null));

    public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register(
      nameof(Operator),
      typeof(ComparisonConditionType),
      typeof(ComparisonCondition),
      new PropertyMetadata(ComparisonConditionType.Equal));

    public static readonly DependencyProperty RightOperandProperty = DependencyProperty.Register(
      nameof(RightOperand),
      typeof(object),
      typeof(ComparisonCondition),
      new PropertyMetadata(null));

    protected override Freezable CreateInstanceCore() => new ComparisonCondition();

    /// <summary>
    /// Gets or sets the left operand.
    /// </summary>
    public object LeftOperand
    {
      get => GetValue(LeftOperandProperty);
      set => SetValue(LeftOperandProperty, value);
    }
    /// <summary>
    /// Gets or sets the right operand.
    /// </summary>
    public object RightOperand
    {
      get => GetValue(RightOperandProperty);
      set => SetValue(RightOperandProperty, value);
    }
    /// <summary>
    /// Gets or sets the comparison operator. 
    /// </summary>
    public ComparisonConditionType Operator
    {
      get => (ComparisonConditionType)GetValue(OperatorProperty);
      set => SetValue(OperatorProperty, value);
    }

    /// <summary>
    /// Method that evaluates the condition. Note that this method can throw ArgumentException if the operator is
    /// incompatible with the type. For instance, operators LessThan, LessThanOrEqual, GreaterThan, and GreaterThanOrEqual
    /// require both operators to implement IComparable. 
    /// </summary>
    /// <returns>Returns true if the condition has been met; otherwise, returns false.</returns>
    public bool Evaluate()
    {
      EnsureBindingUpToDate();
      return ComparisonLogic.EvaluateImpl(LeftOperand, Operator, RightOperand);
    }

    /// <summary>
    /// Ensure that any binding on DP operands are up-to-date.  
    /// </summary>
    private void EnsureBindingUpToDate()
    {
      DataBindingHelper.EnsureBindingUpToDate(this, LeftOperandProperty);
      DataBindingHelper.EnsureBindingUpToDate(this, OperatorProperty);
      DataBindingHelper.EnsureBindingUpToDate(this, RightOperandProperty);
    }
  }
}