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

    // endpoint to get levels (bachelor, masters, etc)
    // endpoint to get lessons by partial name and level
    // endpoint to get lesson types by id
    // endpoint to get lesson subgroups

    [HttpGet("days")]
    public async Task<IActionResult> GetPossibleDays([FromQuery] int lessonSourceId)
    {
        try
        {
            return Ok(await _electiveService.GetPossibleDays(lessonSourceId));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> GetElectiveLessons([FromQuery] string partialLessonName)
    {
        try
        {
            return Ok(await _electiveService.GetLessons(partialLessonName));
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("subgroups")]
    public async Task<IActionResult> GetLessonSubgroups([FromQuery] int lessonSourceId, [FromQuery] string lessonType)
    {
        try
        {
            return Ok(await _electiveService.GetPossibleSubgroups(lessonSourceId, lessonType));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetCurrentElectedLessons([FromQuery] long telegramId)
    {
        try
        {
            return Ok(await _electiveService.GetUserLessons(telegramId));
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }

    [HttpPost("source")]
    public async Task<IActionResult> CreateNewElectedLessonEntry([FromQuery] long telegramId, [FromQuery] int lessonSourceId, [FromQuery] string lessonType, [FromQuery] int subgroupNumber)
    {
        try
        {
            await _electiveService.AddSelectedSource(telegramId, lessonSourceId, lessonType, subgroupNumber);
            return Created();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("source")]
    public async Task<IActionResult> RemoveElectedLessonSource([FromQuery] long telegramId, [FromQuery] int lessonId)
    {
        try
        {
            await _electiveService.RemoveSelectedSource(telegramId, lessonId);
            return Ok(true);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost("entry")]
    public async Task<IActionResult> CreateNewElectedLessonEntry([FromQuery] long telegramId, [FromQuery] int lessonSourceId, [FromQuery] int lessonEntry)
    {
        try
        {
            await _electiveService.AddSelectedEntry(telegramId, lessonSourceId, lessonEntry);
            return Created();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete("entry")]
    public async Task<IActionResult> RemoveElectedLessonEntry([FromQuery] long telegramId, [FromQuery] int lessonId)
    {
        try
        {
            await _electiveService.RemoveSelectedEntry(telegramId, lessonId);
            return Ok(true);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }
}