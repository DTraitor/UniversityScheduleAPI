using BusinessLogic.Services.Interfaces;
using Common.Enums;
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

    [HttpGet("levels")]
    public async Task<IActionResult> GetLevels()
    {
        var result = await _electiveService.GetPossibleLevelsAsync();

        return Ok(result.Select(x => new LevelReturn
        {
            Name = x.Name,
            Id = x.Id,
        }));
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> GetLessons(string lessonName, int sourceId)
    {
        var result = await _electiveService.GetLessonsByNameAsync(lessonName, sourceId);

        if (result.IsSuccess)
            return Ok(result.Value);

        switch (result.Error)
        {
            case ErrorType.TooManyElements:
                return BadRequest("Too many elements to return. Try more precise name.");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("types")]
    public async Task<IActionResult> GetLessonTypes(string lessonName, int sourceId)
    {
        var result = await _electiveService.GetLessonTypesAsync(lessonName, sourceId);

        if (result.IsSuccess)
            return Ok(result.Value);

        switch (result.Error)
        {
            case ErrorType.NotFound:
                return NotFound();
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("subgroups")]
    public async Task<IActionResult> GetSubgroups(int lessonSourceId, string lessonName, string lessonType)
    {
        var result = await _electiveService.GetLessonSubgroupsAsync(lessonSourceId, lessonName, lessonType);

        if (result.IsSuccess)
            return Ok(result.Value);

        switch (result.Error)
        {
            case ErrorType.NotFound:
                return NotFound();
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private record LevelReturn
    {
        public string Name { get; init; }
        public int Id { get; init; }
    }
}
