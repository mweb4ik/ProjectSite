using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Models;

namespace PcComponentsApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<PCBuild> PCBuilds => Set<PCBuild>();
    public DbSet<Component> Components => Set<Component>();
    public DbSet<QuizResult> QuizResults=> Set<QuizResult>();
}
