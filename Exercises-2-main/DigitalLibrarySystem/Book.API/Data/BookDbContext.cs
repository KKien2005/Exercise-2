using LibraryShared;
using Microsoft.EntityFrameworkCore;

namespace Book.API.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

    public DbSet<LibraryShared.Book> Books => Set<LibraryShared.Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LibraryShared.Book>().ToTable("Books");
    }
}