using MonthlyBudget.IdentityHousehold.Application.Features.RefreshToken;

namespace MonthlyBudget.IdentityHousehold.Tests.Application;

public sealed class RefreshTokenValidatorTests
{
    [Fact]
    public void Validate_EmptyToken_FailsValidation()
    {
        var validator = new RefreshTokenValidator();
        var result = validator.Validate(new RefreshTokenCommand(string.Empty));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_ValidToken_PassesValidation()
    {
        var validator = new RefreshTokenValidator();
        var result = validator.Validate(new RefreshTokenCommand("valid-token"));

        Assert.True(result.IsValid);
    }
}