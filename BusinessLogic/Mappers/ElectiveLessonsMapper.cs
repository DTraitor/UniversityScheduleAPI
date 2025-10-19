using BusinessLogic.Helpers;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace BusinessLogic.Mappers;

public static class ElectiveLessonsMapper
{
    public static IEnumerable<UserLesson> Map(IEnumerable<ElectiveLesson> lessons, ElectiveLessonDay day, DateTimeOffset begin, DateTimeOffset end, TimeZoneInfo timeZone)
    {
        foreach (var lesson in lessons)
        {
            yield return new UserLesson
            {
                Title = lesson.Title,
                LessonType = lesson.Type,
                Teacher = lesson.Teacher,
                Location = lesson.Location,
                BeginTime = lesson.StartTime,
                Duration = lesson.Length,
                RepeatType = RepeatType.Weekly,
                RepeatCount = 2,
                StartTime = begin.AddDays(day.DayId-1),
                EndTime = end,
                TimeZoneId = timeZone.Id,
                SelectedLessonSourceType = SelectedLessonSourceType.Elective,
                LessonSourceId = lesson.ElectiveLessonDayId,
            };
        }
    }
}