using BusinessLogic.Services.Interfaces;
using Common.Enums;
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
    [HttpGet]
    public async Task<IActionResult> GetAlerts([FromQuery] int batchSize)
    {
        var alerts = await _userAlertService.GetAlerts(batchSize);

        return Ok(alerts.Select(x => new UserAlertDto
        {
            Id = x.Item1.Id,
            UserTelegramId = x.Item2.TelegramId,
            AlertType = x.Item1.AlertType,
            Options = x.Item1.Options,
        }));
    }

    // endpoint to remove processed alerts
    [HttpDelete]
    public async Task<IActionResult> DeleteProcessedAlerts([FromBody] ICollection<int> alertIds)
    {
        await _userAlertService.RemoveProcessedAlerts(alertIds);
        return Ok();
    }

    private record UserAlertDto
    {
        public int Id { get; init; }
        public long UserTelegramId { get; init; }
        public UserAlertType AlertType { get; init; }
        public Dictionary<string, string> Options { get; init; }
    }
}