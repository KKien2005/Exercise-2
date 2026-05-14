using LibraryShared;
using Microsoft.EntityFrameworkCore;

namespace Borrowing.API.Data;

public class BorrowingDbContext : DbContext
{
    public BorrowingDbContext(DbContextOptions<BorrowingDbContext> options) : base(options) { }

    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BorrowRecord>().ToTable("BorrowRecords");
    }
}