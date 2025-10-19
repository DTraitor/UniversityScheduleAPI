namespace BusinessLogic.Helpers;

public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset GetNextDayOfWeek(this DateTimeOffset start, DayOfWeek targetDay)
    {
        int daysToAdd = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd - 1);
    }
}