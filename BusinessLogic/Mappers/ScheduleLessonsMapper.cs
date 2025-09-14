using BusinessLogic.Helpers;
using DataAccess.Enums;
using DataAccess.Models;
using DataAccess.Models.Internal;

namespace BusinessLogic.Mappers;

public static class ScheduleLessonsMapper
{
    public static IEnumerable<UserLesson> Map(IEnumerable<GroupLesson> lessons, DateTimeOffset begin, DateTimeOffset end)
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
                StartTime = begin.GetNextDayOfWeek(lesson.DayOfWeek).AddDays(lesson.Week ? 7 : 0),
                EndTime = end,
                LessonSourceType = LessonSourceTypeEnum.Group,
            };
        }
    }
}