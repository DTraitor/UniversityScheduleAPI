using BusinessLogic.Services.Interfaces;
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

    //Endpoint to get a group of a user
    //Endpoint to get groups by name
    //Endpoint to get subgroups of a group

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
        try
        {
            return Ok(await _groupService.GetSubgroups(telegramId));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}