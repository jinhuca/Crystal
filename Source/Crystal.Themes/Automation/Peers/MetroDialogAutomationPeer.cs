// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Automation.Peers;
using Crystal.Themes.Controls.Dialogs;

namespace Crystal.Themes.Automation.Peers
{
    public class MetroDialogAutomationPeer : FrameworkElementAutomationPeer
    {
        public MetroDialogAutomationPeer(BaseMetroDialog owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return Owner.GetType().Name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override string GetNameCore()
        {
            var nameCore = base.GetNameCore();
            if (string.IsNullOrEmpty(nameCore))
            {
                nameCore = ((BaseMetroDialog)Owner).Title;
            }

            if (string.IsNullOrEmpty(nameCore))
            {
                nameCore = ((BaseMetroDialog)Owner).Name;
            }

            if (string.IsNullOrEmpty(nameCore))
            {
                nameCore = GetClassNameCore();
            }

            return nameCore!;
        }
    }
}