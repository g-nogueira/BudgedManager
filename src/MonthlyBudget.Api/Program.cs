using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonthlyBudget.BudgetManagement.Domain.Exceptions;
using MonthlyBudget.ForecastEngine.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.Infrastructure;
using MonthlyBudget.Infrastructure.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ─── Infrastructure & Domain Services ────────────────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── JWT Authentication ───────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// ─── Controllers — discover from all modules ─────────────────────────────────
builder.Services.AddControllers()
    .AddApplicationPart(typeof(MonthlyBudget.BudgetManagement.Infrastructure.Controllers.BudgetController).Assembly)
    .AddApplicationPart(typeof(MonthlyBudget.IdentityHousehold.Infrastructure.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(MonthlyBudget.ForecastEngine.Infrastructure.Controllers.ForecastController).Assembly);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Global Exception Handler ─────────────────────────────────────────────────
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = error switch
        {
            // ── Identity-specific codes (must precede base IdentityDomainException) ──
            InvalidCredentialsException   => StatusCodes.Status401Unauthorized,
            DuplicateEmailException       => StatusCodes.Status409Conflict,
            UserAlreadyInHouseholdException => StatusCodes.Status409Conflict,
            HouseholdFullException        => StatusCodes.Status409Conflict,
            PendingInvitationExistsException => StatusCodes.Status409Conflict,
            InsufficientRoleException     => StatusCodes.Status403Forbidden,
            InvitationExpiredException    => StatusCodes.Status410Gone,
            InvitationNotFoundException   => StatusCodes.Status404NotFound,
            HouseholdNotFoundException    => StatusCodes.Status404NotFound,
            // ── Generic domain exceptions ──────────────────────────────────────────
            DomainException               => StatusCodes.Status400BadRequest,
            ForecastDomainException       => StatusCodes.Status400BadRequest,
            IdentityDomainException       => StatusCodes.Status400BadRequest,
            FluentValidation.ValidationException => StatusCodes.Status422UnprocessableEntity,
            UnauthorizedAccessException   => StatusCodes.Status401Unauthorized,
            _                             => StatusCodes.Status500InternalServerError
        };
        await context.Response.WriteAsJsonAsync(new
        {
            error = error?.Message ?? "An unexpected error occurred.",
            type = error?.GetType().Name
        });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<HouseholdScopeMiddleware>();
app.UseAuthorization();
app.MapControllers();

// ─── Auto-apply EF Core migrations on startup ─────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MonthlyBudget.Infrastructure.Database.AppDbContext>();
    db.Database.Migrate();
}

app.Run();

// Partial class for integration test access
public partial class Program { }
