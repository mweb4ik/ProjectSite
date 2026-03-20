using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Models;

namespace PcComponentsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
}
