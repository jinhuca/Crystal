// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using Crystal.Themes.Controls;

namespace Crystal.Themes.Actions
{
  public class CloseTabItemAction : CommandTriggerAction
    {
        private TabItem? associatedTabItem;

        private TabItem? AssociatedTabItem => associatedTabItem ??= AssociatedObject.TryFindParent<TabItem>();

        protected override void Invoke(object? parameter)
        {
            if (AssociatedObject is null || (AssociatedObject != null && !AssociatedObject.IsEnabled))
            {
                return;
            }

            var tabControl = AssociatedObject?.TryFindParent<TabControl>();
            var tabItem = AssociatedTabItem;
            if (tabControl is null || tabItem is null)
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

            if (tabControl is CrystalTabControlBase crystalTabControl && tabItem is CrystalTabItem crystalTabItem)
            {
                // run the command handler for the TabControl
                // see #555
                crystalTabControl.BeginInvoke(x => x.CloseThisTabItem(crystalTabItem));
            }
            else
            {
                var closeAction =
                    new Action(
                        () =>
                            {
                                // TODO Raise a closing event to cancel this action

                                if (tabControl.ItemsSource is null)
                                {
                                    // if the list is hard-coded (i.e. has no ItemsSource)
                                    // then we remove the item from the collection
                                    tabItem.ClearStyle();
                                    tabControl.Items.Remove(tabItem);
                                }
                                else
                                {
                                    // if ItemsSource is something we cannot work with, bail out
                                    var collection = tabControl.ItemsSource as IList;
                                    if (collection is null)
                                    {
                                        return;
                                    }

                                    // find the item and kill it (I mean, remove it)
                                    var item2Remove = collection.OfType<object>().FirstOrDefault(item => tabItem == item || tabItem.DataContext == item);
                                    if (item2Remove != null)
                                    {
                                        tabItem.ClearStyle();
                                        collection.Remove(item2Remove);
                                    }
                                }
                            });
                this.BeginInvoke(closeAction);
            }
        }

        protected override object? GetCommandParameter()
        {
            var parameter = CommandParameter;
            if (parameter is null && PassAssociatedObjectToCommand)
            {
                parameter = AssociatedTabItem;
            }

            return parameter;
        }
    }
}