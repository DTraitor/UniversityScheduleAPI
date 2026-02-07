using BusinessLogic.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
public class UserAlertsController : ControllerBase
{
    private readonly ILogger<UserAlertsController> _logger;
    private readonly IUserAlertService _userAlertService;

    public UserAlertsController(IUserAlertService userAlertService, ILogger<UserAlertsController> logger)
    {
        _userAlertService = userAlertService;
        _logger = logger;
    }

    // endpoint to get alerts
    // endpoint to remove processed alerts

    [HttpGet]
    public async Task<IActionResult> GetScheduleForDate([FromQuery] int batchSize)
    {
        var alerts = await _userAlertService.GetAlerts(batchSize);
        await _userAlertService.RemoveProcessedAlerts(alerts.Select(a => a.Id));
        return Ok(alerts);
    }
}