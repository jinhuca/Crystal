namespace Crystal.Themes.Exceptions
{
  [Serializable]
  public class CrystalThemesException : Exception
  {
    public CrystalThemesException()
    {
    }

    public CrystalThemesException(string message) : base(message)
    {
    }

    public CrystalThemesException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    protected CrystalThemesException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }
}