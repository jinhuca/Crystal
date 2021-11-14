// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Crystal.Themes.Controls;
using Crystal.Themes.ValueBoxes;

namespace Crystal.Themes.Actions
{
    public class CloseFlyoutAction : CommandTriggerAction
    {
        private Flyout? associatedFlyout;

        private Flyout? AssociatedFlyout => associatedFlyout ??= AssociatedObject.TryFindParent<Flyout>();

        protected override void Invoke(object? parameter)
        {
            if (AssociatedObject is null || (AssociatedObject != null && !AssociatedObject.IsEnabled))
            {
                return;
            }

            var command = Command;
            if (command != null)
            {
                var commandParameter = GetCommandParameter();
                if (command.CanExecute(commandParameter))
                {
                    command.Execute(commandParameter);
                }
            }
            else
            {
                AssociatedFlyout?.SetCurrentValue(Flyout.IsOpenProperty, BooleanBoxes.FalseBox);
            }
        }

        protected override object? GetCommandParameter()
        {
            var parameter = CommandParameter;
            if (parameter is null && PassAssociatedObjectToCommand)
            {
                parameter = AssociatedFlyout;
            }

            return parameter;
        }
    }
}