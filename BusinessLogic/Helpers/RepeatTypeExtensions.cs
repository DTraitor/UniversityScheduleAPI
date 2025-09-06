using DataAccess.Enums;

namespace BusinessLogic.Helpers;

public static class RepeatTypeExtensions
{
    public static DateTimeOffset? GetNextOccurrence(this RepeatType repeatType, DateTimeOffset dateTime, int count)
    {
        switch (repeatType)
        {
            case RepeatType.Daily:
                return dateTime.AddDays(count);
            case RepeatType.Weekly:
                return dateTime.AddDays(count * 7);
            case RepeatType.Monthly:
                return dateTime.AddMonths(count);
            case RepeatType.Yearly:
                return dateTime.AddYears(count);
            case RepeatType.Never:
            default:
                return null;
        }
    }
}