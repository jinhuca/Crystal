namespace Crystal.Themes.Controls.Dialogs
{
  /// <summary>
  /// An implementation of CrystalDialogBase allowing arbitrary content.
  /// </summary>
  public class CustomDialog : CrystalDialogBase
  {
    public CustomDialog()
        : this(null, null)
    {
    }

    public CustomDialog(CrystalWindow? parentWindow)
        : this(parentWindow, null)
    {
    }

    public CustomDialog(CrystalDialogSettings? settings)
        : this(null, settings)
    {
    }

    public CustomDialog(CrystalWindow? parentWindow, CrystalDialogSettings? settings)
        : base(parentWindow, settings)
    {
    }
  }
}