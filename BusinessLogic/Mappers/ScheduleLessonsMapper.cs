using BusinessLogic.Helpers;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace BusinessLogic.Mappers;

public static class ScheduleLessonsMapper
{
    public static IEnumerable<UserLesson> Map(IEnumerable<LessonEntry> lessons, DateTimeOffset begin, DateTimeOffset end, TimeZoneInfo timeZone)
    {
        foreach (var lesson in lessons)
        {
            yield return new UserLesson
            {
                Title = lesson.Title,
                LessonType = lesson.Type,
                Teacher = lesson.Teachers,
                Location = lesson.Location,
                BeginTime = lesson.StartTime,
                Duration = lesson.Length,
                RepeatType = lesson.OneTimeOccurence == null ? RepeatType.Weekly : RepeatType.Never,
                RepeatCount = 2,
                StartTime = lesson.OneTimeOccurence?.ToUniversalTime()
                            ?? begin.GetNextDayOfWeek(lesson.DayOfWeek).AddDays(lesson.Week ? 7 : 0).ToUniversalTime(),
                EndTime = lesson.OneTimeOccurence?.AddDays(1).ToUniversalTime() ?? end.ToUniversalTime(),
                TimeZoneId = timeZone.Id,
                SelectedLessonSourceType = lesson.OneTimeOccurence == null ? SelectedLessonSourceType.None : SelectedLessonSourceType.OneTimeOccurence,
            };
        }
    }
}