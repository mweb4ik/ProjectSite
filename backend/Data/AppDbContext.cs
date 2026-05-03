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
    public DbSet<CpuSupport> CpuSupports=> Set<CpuSupport>();
    public DbSet<BiosVersion> BiosVersions=> Set<BiosVersion>();
    public DbSet<Motherboard> Motherboards=> Set<Motherboard>();
    public DbSet<OverclockProfile>  OverclockProfiles=> Set<OverclockProfile>();
    public DbSet<QuizQuestion> QuizQuestions=> Set<QuizQuestion>();
    

protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    builder.Entity<QuizQuestion>()
        .Property(q => q.Options)
        .HasConversion(
            v => string.Join(";", v),
            v => v.Split(';', StringSplitOptions.RemoveEmptyEntries)
        );

    builder.Entity<CpuSupport>()
        .HasOne(cs => cs.Cpu)
        .WithMany()
        .HasForeignKey(cs => cs.CpuId);

    builder.Entity<CpuSupport>()
        .HasOne(cs => cs.BiosVersion)
        .WithMany(b => b.SupportedCpus)
        .HasForeignKey(cs => cs.BiosVersionId);

    builder.Entity<BiosVersion>()
        .HasOne(b => b.Motherboard)
        .WithMany()
        .HasForeignKey(b => b.MotherboardId);
}
}