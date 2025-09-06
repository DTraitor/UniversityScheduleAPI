using BusinessLogic.Services.Interfaces;
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
    public async Task<IActionResult> GetScheduleForDate([FromQuery] DateTimeOffset dateTime, [FromQuery] int userId)
    {
        return Ok(await _scheduleService.GetScheduleForDate(dateTime, userId));
    }
}