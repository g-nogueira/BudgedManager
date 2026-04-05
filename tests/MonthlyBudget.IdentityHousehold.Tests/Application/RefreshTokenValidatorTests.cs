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

    [Fact]
    public void Validate_WhitespaceOnlyToken_FailsValidation()
    {
        var validator = new RefreshTokenValidator();
        var result = validator.Validate(new RefreshTokenCommand("   "));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_EmptyToken_ContainsExpectedErrorMessage()
    {
        var validator = new RefreshTokenValidator();
        var result = validator.Validate(new RefreshTokenCommand(string.Empty));

        Assert.Contains(result.Errors, e => e.ErrorMessage == "Refresh token is required.");
    }
}