using BusinessLogic.Services.Interfaces;
using Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private readonly ILogger<GroupController> _logger;
    private readonly IGroupService _groupService;

    public GroupController(IGroupService groupService, ILogger<GroupController> logger)
    {
        _groupService = groupService;
        _logger = logger;
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserGroups([FromQuery] long telegramId)
    {
        return Ok(await _groupService.GetUserGroups(telegramId));
    }

    [HttpGet("exist")]
    public async Task<IActionResult> Exists([FromQuery] string groupName)
    {
        return Ok(await _groupService.GroupExists(groupName));
    }

    [HttpGet("subgroups")]
    public async Task<IActionResult> GetSubgroups([FromQuery] long telegramId)
    {
        var result  = await _groupService.GetSubgroups(telegramId);

        if (result.IsSuccess)
            return Ok(result.Value);

        switch (result.Error)
        {
            case ErrorType.UserNotFound:
                return NotFound("User does not exist.");
            case ErrorType.GroupNotFound:
                return NotFound("User has no group.");
            default:
                return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}