using N2.Core.SystemAbstractions;

namespace Kaalenco.Common.SystemAbstractions;

public class TimeSystem : ITimeSystem
{
    public DateTime UtcNow => DateTimeOffset.UtcNow.DateTime;
    public DateTimeOffset LocalNow => DateTimeOffset.Now;
    public DateTime LocalToday => DateTimeOffset.Now.Date;
}
