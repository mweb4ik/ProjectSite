using Microsoft.EntityFrameworkCore;
using PcComponentsApi.Models;

namespace PcComponentsApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Component> Components { get; set; } = null!;
        public DbSet<PCBuild> PCBuilds { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizResult> QuizResults { get; set; } = null!;
        public DbSet<BIOSVersion> BIOSVersions { get; set; } = null!;
        public DbSet<OverclockingProfile> OverclockingProfiles { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlite("Data Source=PcComponents.db");
        }
    }
}
