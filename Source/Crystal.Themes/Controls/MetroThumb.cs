// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Crystal.Themes.Controls
{
    public class MetroThumb : Thumb, IMetroThumb
    {
        private TouchDevice? currentDevice = null;

        protected override void OnPreviewTouchDown(TouchEventArgs e)
        {
            // Release any previous capture
            ReleaseCurrentDevice();
            // Capture the new touch
            CaptureCurrentDevice(e);
        }

        protected override void OnPreviewTouchUp(TouchEventArgs e)
        {
            ReleaseCurrentDevice();
        }

        protected override void OnLostTouchCapture(TouchEventArgs e)
        {
            // Only re-capture if the reference is not null
            // This way we avoid re-capturing after calling ReleaseCurrentDevice()
            if (currentDevice != null)
            {
                CaptureCurrentDevice(e);
            }
        }

        private void ReleaseCurrentDevice()
        {
            if (currentDevice != null)
            {
                // Set the reference to null so that we don't re-capture in the OnLostTouchCapture() method
                var temp = currentDevice;
                currentDevice = null;
                ReleaseTouchCapture(temp);
            }
        }

        private void CaptureCurrentDevice(TouchEventArgs e)
        {
            bool gotTouch = CaptureTouch(e.TouchDevice);
            if (gotTouch)
            {
                currentDevice = e.TouchDevice;
            }
        }
    }
}