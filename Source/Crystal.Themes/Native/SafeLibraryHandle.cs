using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

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