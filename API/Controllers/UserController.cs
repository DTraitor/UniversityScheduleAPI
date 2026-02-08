using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserService _userService;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // endpoint to create new user
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromQuery] long telegramId)
    {
        await _userService.CreateUserAsync(telegramId);

        return Created();
    }

    // endpoint to change group
    [HttpPut("group")]
    public async Task<IActionResult> ChangeGroup([FromQuery] long telegramId, [FromQuery] string groupName)
    {
        var result = await _userService.ChangeGroupAsync(telegramId, groupName);

        if (result.IsSuccess)
            return Ok();

        switch (result.Error)
        {
            case ErrorType.UserNotFound:
                return NotFound("User does not exist.");
            case ErrorType.GroupNotFound:
                return NotFound("Group does not exist.");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // endpoint to get users elective lessons
    [HttpGet("elective")]
    public async Task<IActionResult> GetUsersElective([FromQuery] long telegramId)
    {
        var result = await _userService.GetUserElectiveLessonAsync(telegramId);

        if (result.IsSuccess)
            return Ok(result.Value.Select(x => new SelectedElectiveLessonInputOutput
            {
                Id = x.Id,
                LessonName = x.LessonName,
                LessonType = x.LessonType,
                SubgroupNumber = x.SubgroupNumber
            }));

        switch (result.Error)
        {
            case ErrorType.UserNotFound:
                return NotFound("User does not exist.");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // endpoint to select users elective lesson
    [HttpPost("elective")]
    public async Task<IActionResult> AddUsersElective([FromQuery] long telegramId, [FromBody] SelectedElectiveLessonInputOutput electiveParams)
    {
        var result = await _userService.AddUserElectiveLessonAsync(
            telegramId,
            electiveParams.Id,
            electiveParams.LessonName,
            electiveParams.LessonType,
            electiveParams.SubgroupNumber
            );

        if (result.IsSuccess)
            return Ok();

        switch (result.Error)
        {
            case ErrorType.UserNotFound:
                return NotFound("User does not exist.");
            case ErrorType.NotFound:
                return NotFound("Elective lesson does not exist.");
            case ErrorType.ElectiveLessonExistsAlready:
                return Conflict("Elective lesson already exists.");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    // endpoint to remove users elective lesson
    [HttpDelete("elective")]
    public async Task<IActionResult> RemoveUsersElective([FromQuery] long telegramId, [FromQuery] int electiveId)
    {
        await _userService.RemoveUserElectiveLessonAsync(telegramId, electiveId);
        return Ok();
    }

    public record SelectedElectiveLessonInputOutput
    {
        // LessonId for output, SourceId for input
        public int Id { get; set; }
        public string LessonName { get; set; }
        public string? LessonType { get; set; }
        public int SubgroupNumber { get; set; }
    }
}