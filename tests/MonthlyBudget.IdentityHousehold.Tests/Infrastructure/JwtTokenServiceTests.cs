using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using MonthlyBudget.IdentityHousehold.Infrastructure.Auth;

namespace MonthlyBudget.IdentityHousehold.Tests.Infrastructure;

public sealed class JwtTokenServiceTests
{
    private static JwtTokenService BuildService() =>
        new(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "super-secret-key-that-is-long-enough-for-hmac-sha256",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience",
                ["Jwt:AccessTokenExpiryMinutes"] = "15"
            })
            .Build());

    [Fact]
    public void HashToken_ValidInput_ReturnsSha256HexString()
    {
        var sut = BuildService();

        var hash = sut.HashToken("my-refresh-token");

        var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("my-refresh-token"))).ToLowerInvariant();
        Assert.Equal(expected, hash);
    }

    [Fact]
    public void HashToken_SameInput_ProducesDeterministicOutput()
    {
        var sut = BuildService();

        var hash1 = sut.HashToken("deterministic-token");
        var hash2 = sut.HashToken("deterministic-token");

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashToken_DifferentInputs_ProduceDifferentHashes()
    {
        var sut = BuildService();

        var hash1 = sut.HashToken("token-a");
        var hash2 = sut.HashToken("token-b");

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashToken_EmptyString_ThrowsArgumentException()
    {
        var sut = BuildService();

        Assert.Throws<ArgumentException>(() => sut.HashToken(string.Empty));
    }

    [Fact]
    public void HashToken_WhitespaceOnly_ThrowsArgumentException()
    {
        var sut = BuildService();

        Assert.Throws<ArgumentException>(() => sut.HashToken("   "));
    }

    [Fact]
    public void HashToken_Output_IsLowercaseHex()
    {
        var sut = BuildService();

        var hash = sut.HashToken("some-token");

        Assert.Equal(hash, hash.ToLowerInvariant());
        Assert.Matches("^[0-9a-f]+$", hash);
    }

    [Fact]
    public void HashToken_Output_Is64Characters()
    {
        var sut = BuildService();

        // SHA256 produces 32 bytes → 64 hex chars
        var hash = sut.HashToken("some-token");

        Assert.Equal(64, hash.Length);
    }
}