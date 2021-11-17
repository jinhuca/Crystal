namespace Crystal.Themes.Native
{
    using System;
    using System.Security.Permissions;
    using Crystal.Themes.Controls;
    using Microsoft.Win32.SafeHandles;

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
}