using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
using MonthlyBudget.BudgetManagement.Infrastructure.Events;
using MonthlyBudget.Infrastructure.Database;
using MonthlyBudget.Infrastructure.Repositories;
namespace MonthlyBudget.Infrastructure;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core with PostgreSQL
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly("MonthlyBudget.Infrastructure")));
        // BudgetManagement adapters
        services.AddScoped<IBudgetRepository, PostgresBudgetRepository>();
        services.AddScoped<IBudgetEventPublisher, MediatRBudgetEventPublisher>();
        // MediatR � register all handlers from all modules
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(BudgetManagement.Application.Features.CreateBudget.CreateBudgetHandler).Assembly);
        });
        // FluentValidation � auto-discover all validators
        services.AddValidatorsFromAssemblyContaining<BudgetManagement.Application.Features.CreateBudget.CreateBudgetValidator>();
        return services;
    }
}
