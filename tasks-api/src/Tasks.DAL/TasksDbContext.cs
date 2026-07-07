using Microsoft.EntityFrameworkCore;
using Tasks.Domain.Entities;

namespace Tasks.DAL;

/// <summary>EF Core context backing the SQLite database.</summary>
public class TasksDbContext : DbContext
{
    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).IsRequired().HasMaxLength(120);
            entity.Property(t => t.Description).HasMaxLength(1000);
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20); // readable in the DB
            entity.Property(t => t.OwnerUserId).IsRequired();
            entity.HasIndex(t => t.OwnerUserId); // tasks are always queried per owner
        });
    }
}
