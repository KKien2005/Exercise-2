using Identity.API.Data;
using LibraryShared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IdentityDbContext _db;
    public UsersController(IdentityDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Users.ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpGet("{id}/can-borrow")]
    public async Task<IActionResult> CanBorrow(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();

        int maxBooks = user.Rank switch
        {
            "Gold" => 10,
            "Silver" => 3,
            _ => 1
        };

        return Ok(new CanBorrowResponse
        {
            Id = user.Id,
            Rank = user.Rank,
            MaxBooks = maxBooks
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
}