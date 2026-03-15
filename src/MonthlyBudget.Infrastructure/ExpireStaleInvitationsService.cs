using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;

namespace MonthlyBudget.Infrastructure;

/// <summary>
/// Background service that runs daily and transitions PENDING invitations
/// past their <c>ExpiresAt</c> date to EXPIRED status (INV-H5 corrective policy).
/// </summary>
public sealed class ExpireStaleInvitationsService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpireStaleInvitationsService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public ExpireStaleInvitationsService(IServiceScopeFactory scopeFactory, ILogger<ExpireStaleInvitationsService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpireStaleInvitationsService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExpireInvitationsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error expiring stale invitations.");
            }
            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ExpireInvitationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IInvitationRepository>();

        var expired = await repo.FindAllExpiredPendingAsync(ct);
        if (expired.Count == 0) return;

        foreach (var invitation in expired)
            invitation.Expire();

        await repo.SaveAllAsync(expired, ct);
        _logger.LogInformation("Expired {Count} stale invitation(s).", expired.Count);
    }
}

