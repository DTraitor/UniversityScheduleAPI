using BusinessLogic.DTO;
using BusinessLogic.Services.Interfaces;
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

    [HttpGet("exists")]
    public async Task<IActionResult> UserExists([FromQuery] int telegramId)
    {
        return Ok(await _userService.UserExists(telegramId));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDtoInput newUser)
    {
        try
        {
            return CreatedAtAction(nameof(UserExists), await _userService.CreateUser(newUser));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("group")]
    public async Task<IActionResult> ChangeGroup([FromBody] UserDtoInput editUser)
    {
        try
        {
            return Ok(await _userService.ChangeGroup(editUser));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}