using MediatR;
using MonthlyBudget.IdentityHousehold.Application.Ports;
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
    public JoinHouseholdHandler(IInvitationRepository invitations, IHouseholdRepository households, IUserRepository users, ITokenService tokens)
    { _invitations = invitations; _households = households; _users = users; _tokens = tokens; }
    public async Task<JoinHouseholdResult> Handle(JoinHouseholdCommand cmd, CancellationToken ct)
    {
        var invitation = await _invitations.FindByTokenAsync(cmd.Token, ct) ?? throw new InvitationExpiredException();
        invitation.Accept();
        var household = await _households.FindByIdAsync(invitation.HouseholdId, ct) ?? throw new HouseholdNotFoundException(invitation.HouseholdId);
        household.AddMember(cmd.UserId, MemberRole.PARTNER);
        await _households.SaveAsync(household, ct);
        await _invitations.SaveAsync(invitation, ct);
        var user = await _users.FindByIdAsync(cmd.UserId, ct);
        var token = _tokens.GenerateAccessToken(cmd.UserId, user?.Email ?? "", user?.DisplayName ?? "", household.HouseholdId);
        return new JoinHouseholdResult(household.HouseholdId, token);
    }
}
