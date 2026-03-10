using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MonthlyBudget.IdentityHousehold.Application.Features.AuthenticateUser;
using MonthlyBudget.IdentityHousehold.Application.Features.CreateHousehold;
using MonthlyBudget.IdentityHousehold.Application.Features.GetHousehold;
using MonthlyBudget.IdentityHousehold.Application.Features.InviteMember;
using MonthlyBudget.IdentityHousehold.Application.Features.JoinHousehold;
using MonthlyBudget.IdentityHousehold.Application.Features.RegisterUser;
using MonthlyBudget.IdentityHousehold.Infrastructure.Dto;
using System.Security.Claims;
namespace MonthlyBudget.IdentityHousehold.Infrastructure.Controllers;
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) { _mediator = mediator; }
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new RegisterUserCommand(req.Email, req.DisplayName, req.Password), ct);
        return Created($"/api/v1/users/{result.UserId}", result);
    }
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AuthenticateUserCommand(req.Email, req.Password), ct);
        return Ok(result);
    }
}
[ApiController]
[Route("api/v1/households")]
[Authorize]
public sealed class HouseholdController : ControllerBase
{
    private readonly IMediator _mediator;
    public HouseholdController(IMediator mediator) { _mediator = mediator; }
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("User ID claim not found"));
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHouseholdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateHouseholdCommand(UserId, req.Name), ct);
        return CreatedAtAction(nameof(GetById), new { householdId = result.HouseholdId }, result);
    }
    [HttpGet("{householdId:guid}")]
    public async Task<IActionResult> GetById(Guid householdId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetHouseholdQuery(householdId, UserId), ct);
        if (result == null) return NotFound();
        return Ok(result);
    }
    [HttpPost("{householdId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid householdId, [FromBody] InviteRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new InviteMemberCommand(householdId, UserId, req.Email), ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }
    // NOTE: /join requires authentication. The invited user must register first via
    // POST /api/v1/auth/register, then authenticate, and finally call this endpoint
    // with their JWT token. This is intentional — UserId is derived from JWT claims.
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new JoinHouseholdCommand(UserId, req.Token), ct);
        return Ok(result);
    }
}
