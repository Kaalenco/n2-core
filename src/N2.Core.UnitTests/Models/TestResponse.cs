using N2.Core.Commands;

namespace N2.Core.UnitTests.Models;

public class TestResponse : CommandResponse
{
    public TestResponse(Guid handle, int status, long elapsedMilliseconds) :
        base(status, "Test completed", handle, elapsedMilliseconds)
    {
    }
}