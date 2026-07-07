using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Domain.Interfaces;

namespace Tasks.DAL;

/// <summary>Registers the data-access layer (DbContext + repository).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<TasksDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<ITaskRepository, TaskRepository>();
        return services;
    }
}
