namespace TaskManagement.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTimeOffset UtcNowOffset { get; }
    DateOnly TodayUtc { get; }
}
