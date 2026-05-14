using Book.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Book.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly BookDbContext _db;
    public BooksController(BookDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Books.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _db.Books.FindAsync(id);
        return book is null ? NotFound() : Ok(book);
    }

    [HttpPut("{id}/decrease-stock")]
    public async Task<IActionResult> DecreaseStock(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();
        if (book.Stock <= 0) return BadRequest("Hết sách trong kho.");

        book.Stock--;
        await _db.SaveChangesAsync();
        return Ok(book);
    }

    [HttpPut("{id}/increase-stock")]
    public async Task<IActionResult> IncreaseStock(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return NotFound();

        book.Stock++;
        await _db.SaveChangesAsync();
        return Ok(book);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LibraryShared.Book book)
    {
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, book);
    }
}