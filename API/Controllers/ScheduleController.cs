using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Common.Models.Internal;
using Common.Result;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService, ILogger<ScheduleController> logger)
    {
        _scheduleService = scheduleService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetScheduleForDate([FromQuery] DateTimeOffset dateTime, [FromQuery] long userTelegramId)
    {
        var result = await _scheduleService.GetScheduleForDate(dateTime, userTelegramId);

        if (result.IsSuccess)
        {
            return Ok(result.Value.Select(l => new LessonDto
            {
                Title = l.Title,
                LessonType = l.LessonType,
                Teacher = l.Teacher,
                Location = l.Location,
                Cancelled = l.Cancelled,
                BeginTime = l.BeginTime,
                Duration = l.Duration,
                TimeZoneId = l.TimeZoneId,
            }));
        }

        switch (result.Error)
        {
            case ErrorType.UserNotFound:
                return NotFound();
            case ErrorType.TimetableDateOutOfRange:
                return BadRequest(new OutOfRangeResult
                {
                    StartDate = result.ErrorValue.Item1,
                    EndDate = result.ErrorValue.Item2,
                });
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private record OutOfRangeResult
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }

    private record LessonDto
    {
        public string Title { get; set; }
        public string? LessonType { get; set; }
        public IEnumerable<string> Teacher { get; set; } = [];
        public string? Location { get; set; }
        public bool Cancelled { get; set; }

        public TimeSpan BeginTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string TimeZoneId { get; set; }
    }
}