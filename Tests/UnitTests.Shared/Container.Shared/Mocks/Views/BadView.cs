using System;

namespace Crystal.Mocks.Views
{
    public class BadView : ViewBase
    {
        public BadView()
        {
            throw new XamlParseException("You write bad XAML");
        }
    }

    public class XamlParseException : Exception
    {
        public XamlParseException(string message)
            : base(message)
        {
        }
    }
}
