/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/
using Crystal.Themes.Standard;

namespace Crystal.Themes.Windows.Shell
{
  public enum ResizeGripDirection
    {
        None,
        TopLeft,
        Top,
        TopRight,
        Right,
        BottomRight,
        Bottom,
        BottomLeft,
        Left,
        Caption,
    }

  public static class WindowChrome
    {
        #region Attached Properties

        public static readonly DependencyProperty IsHitTestVisibleInChromeProperty = DependencyProperty.RegisterAttached(
            "IsHitTestVisibleInChrome",
            typeof(bool),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        [Category(DesignerConstants.LibraryName)]
        public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj is null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            return (bool)dobj.GetValue(IsHitTestVisibleInChromeProperty);
        }

        public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj is null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            dobj.SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);
        }

        public static readonly DependencyProperty ResizeGripDirectionProperty = DependencyProperty.RegisterAttached(
            "ResizeGripDirection",
            typeof(ResizeGripDirection),
            typeof(WindowChrome),
            new FrameworkPropertyMetadata(ResizeGripDirection.None, FrameworkPropertyMetadataOptions.Inherits));

        [Category(DesignerConstants.LibraryName)]
        public static ResizeGripDirection GetResizeGripDirection(IInputElement inputElement)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj is null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            return (ResizeGripDirection)dobj.GetValue(ResizeGripDirectionProperty);
        }

        public static void SetResizeGripDirection(IInputElement inputElement, ResizeGripDirection direction)
        {
            Verify.IsNotNull(inputElement, "inputElement");
            var dobj = inputElement as DependencyObject;
            if (dobj is null)
            {
                throw new ArgumentException("The element must be a DependencyObject", nameof(inputElement));
            }

            dobj.SetValue(ResizeGripDirectionProperty, direction);
        }

        #endregion
    }
}