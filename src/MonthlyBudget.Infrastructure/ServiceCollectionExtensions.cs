using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonthlyBudget.BudgetManagement.Application.Ports;
using MonthlyBudget.BudgetManagement.Domain.Repositories;
using MonthlyBudget.BudgetManagement.Infrastructure.Events;
using MonthlyBudget.ForecastEngine.Application.Ports;
using MonthlyBudget.ForecastEngine.Domain.Repositories;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
using MonthlyBudget.IdentityHousehold.Infrastructure.Auth;
using MonthlyBudget.IdentityHousehold.Infrastructure.Email;
using MonthlyBudget.IdentityHousehold.Infrastructure.Events;
using MonthlyBudget.ForecastEngine.Infrastructure.Events;
using MonthlyBudget.ForecastEngine.Application.Features.Reforecast;
using MonthlyBudget.Infrastructure.Acl;
using MonthlyBudget.Infrastructure.Database;
using MonthlyBudget.Infrastructure.Repositories;

namespace MonthlyBudget.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // ── EF Core with PostgreSQL ──────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly("MonthlyBudget.Infrastructure")));

        // ── BudgetManagement adapters ────────────────────────────────────────────
        services.AddScoped<IBudgetRepository, PostgresBudgetRepository>();
        services.AddScoped<IBudgetEventPublisher, MediatRBudgetEventPublisher>();

        // ── ForecastEngine adapters ──────────────────────────────────────────────
        services.AddScoped<IForecastRepository, PostgresForecastRepository>();
        services.AddScoped<IForecastEventPublisher, MediatRForecastEventPublisher>();
        services.AddScoped<IBudgetDataPort, BudgetManagementAcl>();

        // ── IdentityHousehold adapters ───────────────────────────────────────────
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<IHouseholdRepository, PostgresHouseholdRepository>();
        services.AddScoped<IInvitationRepository, PostgresInvitationRepository>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IEmailService, ConsoleEmailService>();
        services.AddScoped<IHouseholdEventPublisher, MediatRHouseholdEventPublisher>();

        // ── MediatR — all module assemblies ─────────────────────────────────────
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(
                typeof(BudgetManagement.Application.Features.CreateBudget.CreateBudgetHandler).Assembly);
            cfg.RegisterServicesFromAssembly(
                typeof(ForecastEngine.Application.Features.GenerateForecast.GenerateForecastHandler).Assembly);
            cfg.RegisterServicesFromAssembly(
                typeof(IdentityHousehold.Application.Features.RegisterUser.RegisterUserHandler).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // ── FluentValidation — all module assemblies ─────────────────────────────
        services.AddValidatorsFromAssemblyContaining<
            BudgetManagement.Application.Features.CreateBudget.CreateBudgetValidator>();
        services.AddValidatorsFromAssemblyContaining<
            IdentityHousehold.Application.Features.RegisterUser.RegisterUserValidator>();
        services.AddValidatorsFromAssemblyContaining<ReforecastValidator>();

        // ── Background Services ───────────────────────────────────────────────
        services.AddHostedService<ExpireStaleInvitationsService>();

        return services;
    }
}
