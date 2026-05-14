using LibraryShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notification.API.Data;

namespace Notification.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _db;
    public NotificationsController(NotificationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Logs.OrderByDescending(l => l.SentDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] LogEntry entry)
    {
        entry.SentDate = DateTime.UtcNow;
        _db.Logs.Add(entry);
        await _db.SaveChangesAsync();

        Console.WriteLine($"[NOTIFY] {entry.SentDate:yyyy-MM-dd HH:mm:ss} - {entry.Message}");

        return Ok(entry);
    }
}