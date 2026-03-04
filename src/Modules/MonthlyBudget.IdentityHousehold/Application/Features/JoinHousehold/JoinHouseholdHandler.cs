using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
using MonthlyBudget.IdentityHousehold.Domain.Events;
using MonthlyBudget.IdentityHousehold.Domain.Exceptions;
using MonthlyBudget.IdentityHousehold.Domain.Entities;
using MonthlyBudget.IdentityHousehold.Domain.Repositories;
namespace MonthlyBudget.IdentityHousehold.Application.Features.JoinHousehold;
public sealed class JoinHouseholdHandler : IRequestHandler<JoinHouseholdCommand, JoinHouseholdResult>
{
    private readonly IInvitationRepository _invitations;
    private readonly IHouseholdRepository _households;
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;
    private readonly IHouseholdEventPublisher _events;
    public JoinHouseholdHandler(IInvitationRepository invitations, IHouseholdRepository households, IUserRepository users, ITokenService tokens, IHouseholdEventPublisher events)
    { _invitations = invitations; _households = households; _users = users; _tokens = tokens; _events = events; }
    public async Task<JoinHouseholdResult> Handle(JoinHouseholdCommand cmd, CancellationToken ct)
    {
        // Not found → 404; expired → 410 (via InvitationExpiredException inside Accept())
        var invitation = await _invitations.FindByTokenAsync(cmd.Token, ct) ?? throw new InvitationNotFoundException();
        invitation.Accept(); // throws InvitationExpiredException if expired
        var household = await _households.FindByIdAsync(invitation.HouseholdId, ct) ?? throw new HouseholdNotFoundException(invitation.HouseholdId);
        household.AddMember(cmd.UserId, MemberRole.PARTNER);
        await _households.SaveAsync(household, ct);
        await _invitations.SaveAsync(invitation, ct);
        var user = await _users.FindByIdAsync(cmd.UserId, ct);
        var token = _tokens.GenerateAccessToken(cmd.UserId, user?.Email ?? "", user?.DisplayName ?? "", household.HouseholdId);
        await _events.PublishAsync(new MemberJoined(household.HouseholdId, cmd.UserId, MemberRole.PARTNER.ToString()), ct);
        return new JoinHouseholdResult(household.HouseholdId, token);
    }
}
