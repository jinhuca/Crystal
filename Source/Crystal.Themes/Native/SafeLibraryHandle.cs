using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;

namespace Crystal.Themes.Native;

[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
[Obsolete(DesignerConstants.Win32ElementWarning)]
public sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
  private SafeLibraryHandle()
    : base(true)
  {
  }

  protected override bool ReleaseHandle()
  {
    return UnsafeNativeMethods.FreeLibrary(handle);
  }
}