using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tasks.DAL;

/// <summary>Lets EF Core CLI tooling create the context at design time (for migrations).</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TasksDbContext>
{
    public TasksDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<TasksDbContext>()
            .UseSqlite("Data Source=tasks.db")
            .Options;
        return new TasksDbContext(options);
    }
}
