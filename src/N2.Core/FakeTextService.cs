namespace N2.Core;

public class FakeTextService : ITranslator
{
    public string Language { get; } = "en-US";
    public string GT(string pageContext, string key) => key;
    public string GT(string key) => key;
    public string Translate(string language, string pageContext, string key) => key;
    public string Translate(string language, string key) => key;
}
