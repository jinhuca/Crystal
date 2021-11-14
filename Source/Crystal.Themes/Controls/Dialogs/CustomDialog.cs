namespace Crystal.Themes.Controls.Dialogs
{
  /// <summary>
  /// An implementation of BaseMetroDialog allowing arbitrary content.
  /// </summary>
  public class CustomDialog : BaseMetroDialog
  {
    public CustomDialog()
        : this(null, null)
    {
    }

    public CustomDialog(CrystalWindow? parentWindow)
        : this(parentWindow, null)
    {
    }

    public CustomDialog(MetroDialogSettings? settings)
        : this(null, settings)
    {
    }

    public CustomDialog(CrystalWindow? parentWindow, MetroDialogSettings? settings)
        : base(parentWindow, settings)
    {
    }
  }
}