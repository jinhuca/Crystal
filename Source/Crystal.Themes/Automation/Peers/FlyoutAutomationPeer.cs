﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Automation.Peers;
using Crystal.Themes.Controls;

namespace Crystal.Themes.Automation.Peers
{
    public class FlyoutAutomationPeer : FrameworkElementAutomationPeer
    {
        public FlyoutAutomationPeer(Flyout owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return "Flyout";
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        protected override string GetNameCore()
        {
            string? nameCore = base.GetNameCore();
            if (string.IsNullOrEmpty(nameCore))
            {
                nameCore = ((Flyout)Owner).Header as string;
            }

            if (string.IsNullOrEmpty(nameCore))
            {
                nameCore = ((Flyout)Owner).Name;
            }

            if (string.IsNullOrEmpty(nameCore))
            {
                nameCore = GetClassNameCore();
            }

            return nameCore!;
        }
    }
}