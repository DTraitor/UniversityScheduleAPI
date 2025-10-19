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
    public async Task<IActionResult> GetPossibleDays([FromQuery] int lessonSourceId)
    {
        return Ok(await _electiveService.GetPossibleDays(lessonSourceId));
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> GetElectiveLessons([FromQuery] string partialLessonName)
    {
        return Ok(await _electiveService.GetLessons(partialLessonName));
    }

    [HttpGet("subgroups")]
    public async Task<IActionResult> GetLessonSubgroups([FromQuery] int lessonSourceId, [FromQuery] string lessonType)
    {
        return Ok(await _electiveService.GetPossibleSubgroups(lessonSourceId, lessonType));
    }


    [HttpGet]
    public async Task<IActionResult> GetCurrentElectedLessons([FromQuery] long telegramId)
    {
        return Ok(await _electiveService.GetUserLessons(telegramId));
    }

    [HttpPost("source")]
    public async Task<IActionResult> CreateNewElectedLessonEntry([FromQuery] long telegramId, [FromQuery] int lessonSourceId, [FromQuery] string lessonType, [FromQuery] int subgroupNumber)
    {
        await _electiveService.AddSelectedSource(telegramId, lessonSourceId, lessonType, subgroupNumber);
        return Created();
    }

    [HttpDelete("source")]
    public async Task<IActionResult> RemoveElectedLessonSource([FromQuery] long telegramId, [FromQuery] int lessonId)
    {
        await _electiveService.RemoveSelectedSource(telegramId, lessonId);
        return Ok(true);
    }

    [HttpPost("entry")]
    public async Task<IActionResult> CreateNewElectedLessonEntry([FromQuery] long telegramId, [FromQuery] int lessonSourceId, [FromQuery] int lessonEntry)
    {
        await _electiveService.AddSelectedEntry(telegramId, lessonSourceId, lessonEntry);
        return Created();
    }

    [HttpDelete("entry")]
    public async Task<IActionResult> RemoveElectedLessonEntry([FromQuery] long telegramId, [FromQuery] int lessonId)
    {
        await _electiveService.RemoveSelectedEntry(telegramId, lessonId);
        return Ok(true);
    }
}