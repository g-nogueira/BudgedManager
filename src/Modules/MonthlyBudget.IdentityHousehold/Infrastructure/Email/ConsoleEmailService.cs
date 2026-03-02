using MonthlyBudget.IdentityHousehold.Application.Ports;
using Microsoft.Extensions.Logging;
namespace MonthlyBudget.IdentityHousehold.Infrastructure.Email;
public sealed class ConsoleEmailService : IEmailService
{
    private readonly ILogger<ConsoleEmailService> _logger;
    public ConsoleEmailService(ILogger<ConsoleEmailService> logger) { _logger = logger; }
    public Task SendInvitationAsync(string toEmail, string householdName, string token, CancellationToken ct = default)
    {
        _logger.LogInformation("[EMAIL] Invitation to {Email} for household '{Household}'. Token: {Token}",
            toEmail, householdName, token);
        return Task.CompletedTask;
    }
}
