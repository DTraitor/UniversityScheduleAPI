namespace BusinessLogic.Helpers;

public static class DateTimeOffsetExtensions
{
    public static DateTimeOffset GetNextDayOfWeek(this DateTimeOffset start, DayOfWeek targetDay)
    {
        int daysToAdd = ((int)targetDay - (int)start.DayOfWeek + 7) % 7;
        if (daysToAdd == 0) daysToAdd = 7; // ensure strictly after
        return start.AddDays(daysToAdd);
    }
}