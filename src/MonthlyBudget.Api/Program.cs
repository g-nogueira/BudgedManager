using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    .AddApplicationPart(typeof(MonthlyBudget.BudgetManagement.Infrastructure.Controllers.BudgetController).Assembly);

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
            DomainException       => StatusCodes.Status400BadRequest,
            ForecastDomainException => StatusCodes.Status400BadRequest,
            IdentityDomainException => StatusCodes.Status400BadRequest,
            FluentValidation.ValidationException => StatusCodes.Status422UnprocessableEntity,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _                     => StatusCodes.Status500InternalServerError
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

app.Run();

// Partial class for integration test access
public partial class Program { }
