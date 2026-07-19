using System;
using StadiumOps.Infrastructure.Security;
using Xunit;

namespace StadiumOps.UnitTests.Security;

public sealed class SecurityTests
{
    [Fact]
    public void ResolveSigningKey_WithDefaultKeyInProduction_ThrowsException()
    {
        var options = new JwtOptions
        {
            SigningKey = "development-only-change-this-key-before-production-32chars-minimum"
        };

        // Assert that resolving the signing key in Production with the default development key throws InvalidOperationException
        var exception = Assert.Throws<InvalidOperationException>(() => options.ResolveSigningKey(isProduction: true));
        Assert.Contains("cannot be set to the default development key", exception.Message);
    }

    [Fact]
    public void ResolveSigningKey_WithEmptyKeyInProduction_ThrowsException()
    {
        var options = new JwtOptions
        {
            SigningKey = ""
        };

        // Assert that resolving an empty signing key in Production throws InvalidOperationException
        var exception = Assert.Throws<InvalidOperationException>(() => options.ResolveSigningKey(isProduction: true));
        Assert.Contains("SigningKey is required in Production", exception.Message);
    }

    [Fact]
    public void ResolveSigningKey_WithShortKey_ThrowsException()
    {
        var options = new JwtOptions
        {
            SigningKey = "too-short"
        };

        // Assert that a key under 32 characters throws InvalidOperationException
        var exception = Assert.Throws<InvalidOperationException>(() => options.ResolveSigningKey(isProduction: false));
        Assert.Contains("must be at least 32 characters", exception.Message);
    }
}
