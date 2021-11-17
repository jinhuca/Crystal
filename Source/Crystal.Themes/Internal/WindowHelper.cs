using Crystal.Themes.Standard;

namespace Crystal.Themes.Internal
{
  internal static class WindowHelper
    {
        public static bool IsWindowHandleValid(IntPtr windowHandle)
        {
            return windowHandle != IntPtr.Zero
#pragma warning disable 618
                   && NativeMethods.IsWindow(windowHandle);
#pragma warning restore 618
        }
    }
}