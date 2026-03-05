namespace MonthlyBudget.IdentityHousehold.Infrastructure.Dto;
public record RegisterRequest(string Email, string DisplayName, string Password);
public record LoginRequest(string Email, string Password);
public record CreateHouseholdRequest(string Name);
public record InviteRequest(string Email);
public record JoinRequest(string Token);
