using System;
using System.Diagnostics;

namespace Crystal.Plot2D.Common;

[Conditional(conditionString: "DEBUG")]
[AttributeUsage(validOn: AttributeTargets.Property)]
internal sealed class NotNullAttribute : Attribute {
}
