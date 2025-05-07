using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Models;

namespace Restaurant.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
}