using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class ElectiveController : ControllerBase
{
    private readonly ILogger<ElectiveController> _logger;
    private readonly IElectiveService _electiveService;

    public ElectiveController(IElectiveService electiveService, ILogger<ElectiveController> logger)
    {
        _electiveService = electiveService;
        _logger = logger;
    }

    [HttpGet("days")]
    public async Task<IActionResult> GetPossibleDays()
    {
        return Ok(await _electiveService.GetPossibleDays());
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> GetElectiveLessons([FromQuery] int electiveDayId, [FromQuery] string partialLessonName)
    {
        return Ok(await _electiveService.GetElectiveLessons(electiveDayId, partialLessonName));
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentElectedLessons([FromQuery] long telegramId)
    {
        return Ok(await _electiveService.GetCurrentLessons(telegramId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateNewElectedLesson([FromBody] CreateElectiveLessonDto newLesson)
    {
        await _electiveService.CreateNewElectedLesson(newLesson);
        return Created();
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveElectedLesson([FromQuery] long telegramId, [FromQuery] int lessonId)
    {
        await _electiveService.RemoveElectedLesson(telegramId, lessonId);
        return Ok(true);
    }
}