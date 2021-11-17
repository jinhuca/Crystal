// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Crystal.Themes.Controls
{
  public class LayoutInvalidationCatcher : Decorator
    {
        private Planerator? PlaParent => Parent as Planerator;

        protected override Size MeasureOverride(Size constraint)
        {
            PlaParent?.InvalidateMeasure();
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            PlaParent?.InvalidateArrange();
            return base.ArrangeOverride(arrangeSize);
        }
    }
}