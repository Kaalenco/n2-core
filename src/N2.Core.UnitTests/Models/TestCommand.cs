
using N2.Core.Commands;

namespace N2.Core.UnitTests.Models;

public class TestCommand : CommandRequest
{
    public int WaitTime { get; }
    public TestCommand() : base(Guid.NewGuid())
    {
    }
    public TestCommand(Guid handle) : base(handle)
    {
    }

    public TestCommand(Guid handle, int waitTime) : base(handle)
    {
        WaitTime = waitTime;
    }
}
