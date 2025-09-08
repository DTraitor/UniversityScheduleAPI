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

    [HttpGet("exists")]
    public async Task<IActionResult> Exists([FromQuery] string groupName)
    {
        return Ok(await _groupService.GroupExists(groupName));
    }

    [HttpGet("faculties")]
    public async Task<IActionResult> GetFaculties()
    {
        return Ok(await _groupService.GetFacultiesAsync());
    }

    [HttpGet("degree")]
    public async Task<IActionResult> GetGroupByDegree([FromQuery] string facultyName, [FromQuery] bool bachelor)
    {
        return Ok(await _groupService.GetGroupByDegreeAsync(facultyName, bachelor));
    }
}