using SchoolAdmission.Infrastructure.Services;
using Xunit;

namespace SchoolAdmission.UnitTests;

public class ClockTests
{
    [Fact]
    public void SystemClock_UtcNow_ReturnsCurrentUtcTime()
    {
        var clock = new SystemClock();
        var before = DateTimeOffset.UtcNow;
        var now = clock.UtcNow;
        var after = DateTimeOffset.UtcNow;

        Assert.True(now >= before);
        Assert.True(now <= after);
    }
}

