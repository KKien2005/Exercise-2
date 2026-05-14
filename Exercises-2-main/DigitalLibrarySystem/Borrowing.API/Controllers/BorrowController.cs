using Borrowing.API.Data;
using LibraryShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;

namespace Borrowing.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BorrowController : ControllerBase
{
    private readonly IHttpClientFactory _factory;
    private readonly BorrowingDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<BorrowController> _logger;

    public BorrowController(
        IHttpClientFactory factory,
        BorrowingDbContext db,
        IConfiguration config,
        ILogger<BorrowController> logger)
    {
        _factory = factory;
        _db = db;
        _config = config;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.BorrowRecords.OrderByDescending(b => b.BorrowDate).ToListAsync());

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(int userId) =>
        Ok(await _db.BorrowRecords.Where(b => b.UserId == userId).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Borrow([FromBody] BorrowRequest req)
    {
        var client = _factory.CreateClient("default");
        var identityUrl = _config["Services:Identity"];
        var bookUrl = _config["Services:Book"];
        var notifyUrl = _config["Services:Notification"];

        
        CanBorrowResponse? rankInfo;
        try
        {
            var canBorrowRes = await client.GetAsync($"{identityUrl}/api/users/{req.UserId}/can-borrow");
            if (!canBorrowRes.IsSuccessStatusCode)
                return BadRequest($"User {req.UserId} không tồn tại.");

            rankInfo = await canBorrowRes.Content.ReadFromJsonAsync<CanBorrowResponse>();
            if (rankInfo is null) return BadRequest("Không lấy được thông tin rank.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Identity Service không phản hồi");
            return StatusCode(503, $"Identity Service không khả dụng: {ex.Message}");
        }

       
        var currentBorrowed = await _db.BorrowRecords
            .CountAsync(b => b.UserId == req.UserId && b.ReturnDate == null);

        if (currentBorrowed >= rankInfo.MaxBooks)
            return BadRequest($"Bạn đã đạt giới hạn mượn ({rankInfo.MaxBooks} cuốn) cho hạng {rankInfo.Rank}.");

        
        LibraryShared.Book? book;
        try
        {
            var bookRes = await client.GetAsync($"{bookUrl}/api/books/{req.BookId}");
            if (!bookRes.IsSuccessStatusCode)
                return BadRequest($"Sách {req.BookId} không tồn tại.");

            book = await bookRes.Content.ReadFromJsonAsync<LibraryShared.Book>();
            if (book is null || book.Stock <= 0)
                return BadRequest("Sách đã hết trong kho.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Book Service không phản hồi");
            return StatusCode(503, $"Book Service không khả dụng: {ex.Message}");
        }

        
        var record = new BorrowRecord
        {
            UserId = req.UserId,
            BookId = req.BookId,
            BorrowDate = DateTime.UtcNow
        };
        _db.BorrowRecords.Add(record);
        await _db.SaveChangesAsync();

       
        try
        {
            var stockRes = await client.PutAsync($"{bookUrl}/api/books/{req.BookId}/decrease-stock", null);
            if (!stockRes.IsSuccessStatusCode)
            {
                _db.BorrowRecords.Remove(record);
                await _db.SaveChangesAsync();
                return StatusCode(503, "Không cập nhật được stock, đã hủy giao dịch.");
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Lỗi update stock - rollback");
            _db.BorrowRecords.Remove(record);
            await _db.SaveChangesAsync();
            return StatusCode(503, $"Không kết nối được Book Service để update stock: {ex.Message}");
        }

   
        try
        {
            var log = new LogEntry
            {
                Message = $"User {req.UserId} đã mượn sách '{book.Title}' (BookId={req.BookId}) thành công."
            };
            await client.PostAsJsonAsync($"{notifyUrl}/api/notifications", log);
        }
        catch (Exception ex)
        {
           
            _logger.LogWarning(ex, "Không gửi được notification, nghiệp vụ chính vẫn thành công.");
        }

        return Ok(new
        {
            Message = "Mượn sách thành công!",
            RecordId = record.Id,
            BookTitle = book.Title
        });
    }

    [HttpPut("{id}/return")]
    public async Task<IActionResult> ReturnBook(int id)
    {
        var record = await _db.BorrowRecords.FindAsync(id);
        if (record is null) return NotFound();
        if (record.ReturnDate is not null) return BadRequest("Sách đã được trả.");

        record.ReturnDate = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var client = _factory.CreateClient("default");
        var bookUrl = _config["Services:Book"];
        try
        {
            await client.PutAsync($"{bookUrl}/api/books/{record.BookId}/increase-stock", null);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Không tăng được stock khi trả sách");
        }

        return Ok(new { Message = "Trả sách thành công.", RecordId = id });
    }
}