namespace Crystal.Themes.Exceptions
{
  [Serializable]
  public class MissingRequiredTemplatePartException : CrystalThemesException
  {
    public MissingRequiredTemplatePartException(FrameworkElement target, string templatePart)
        : base($"Template part \"{templatePart}\" in template for \"{target.GetType().FullName}\" is missing.")
    {
    }

    protected MissingRequiredTemplatePartException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
  }
}