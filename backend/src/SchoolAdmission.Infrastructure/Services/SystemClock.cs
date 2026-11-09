using SchoolAdmission.Application.Abstractions;

namespace SchoolAdmission.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;

    public SystemClock(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();
}

