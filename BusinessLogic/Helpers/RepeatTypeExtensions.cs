using Common.Enums;

namespace BusinessLogic.Helpers;

public static class RepeatTypeExtensions
{
    public static DateTime? GetNextOccurrence(this RepeatType repeatType, DateTime dateTime, int count)
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