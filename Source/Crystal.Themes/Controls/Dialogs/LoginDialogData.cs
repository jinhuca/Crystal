using System.Security;

namespace Crystal.Themes.Controls.Dialogs;

public class LoginDialogData
{
  public string? Username { get; internal set; }

  public string? Password
  {
    [SecurityCritical]
    get
    {
      if (SecurePassword is null)
      {
        return null;
      }

      var ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(SecurePassword);
      try
      {
        return System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
      }
      finally
      {
        System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
      }
    }
  }

  public SecureString? SecurePassword { get; internal set; }

  public bool ShouldRemember { get; internal set; }
}