#pragma warning disable CA1028, CA1815
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
#pragma warning disable SA1602 // Enumeration items should be documented
namespace Crystal.Themes.Standard
{
    using Crystal.Themes.Controls;
    using System;
    using System.Runtime.InteropServices;

    [Obsolete(DesignerConstants.Win32ElementWarning)]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [Obsolete(DesignerConstants.Win32ElementWarning)]
    [CLSCompliant(false)]
    public enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }
}