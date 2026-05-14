using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Data;
using SharedModels;

namespace Ordering.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly OrderingDbContext _db;
    private readonly IConfiguration _config;

    public OrdersController(IHttpClientFactory clientFactory, OrderingDbContext db, IConfiguration config)
    {
        _clientFactory = clientFactory;
        _db = db;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Orders.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromQuery] int userId, [FromQuery] int courseId)
    {
        var client = _clientFactory.CreateClient();
        var identityUrl = _config["Services:Identity"];
        var catalogUrl = _config["Services:Catalog"];

        // 1. Check User
        var userRes = await client.GetAsync($"{identityUrl}/api/users/{userId}");
        if (!userRes.IsSuccessStatusCode)
            return BadRequest($"User {userId} không tồn tại.");

        // 2. Check Course
        var courseRes = await client.GetAsync($"{catalogUrl}/api/courses/{courseId}");
        if (!courseRes.IsSuccessStatusCode)
            return BadRequest($"Course {courseId} không tồn tại.");

        // 3. Save order
        var order = new Order
        {
            UserId = userId,
            CourseId = courseId,
            OrderDate = DateTime.UtcNow
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Đăng ký khóa học thành công!", OrderId = order.Id });
    }
}