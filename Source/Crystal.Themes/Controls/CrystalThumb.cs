﻿namespace Crystal.Themes.Controls;

public class CrystalThumb : Thumb, ICrystalThumb
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