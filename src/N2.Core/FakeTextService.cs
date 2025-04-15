namespace N2.Core;

public class FakeTextService : ITextService
{
    public string GetGlobalText(string key) => key;
    public string GetText(string pageContext, string key) => key;
}
