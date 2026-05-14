using Microsoft.EntityFrameworkCore;
using SharedModels;

namespace Catalog.API.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }
    public DbSet<Course> Courses => Set<Course>();
}