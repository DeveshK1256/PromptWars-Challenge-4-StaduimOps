using StadiumOps.Infrastructure.Security;

namespace StadiumOps.UnitTests.Application;

public sealed class TokenHashTests
{
    [Fact]
    public void HashToken_IsStableAndDoesNotExposeRawToken()
    {
        const string token = "refresh-token-value";

        var first = TokenService.HashToken(token);
        var second = TokenService.HashToken(token);

        Assert.Equal(first, second);
        Assert.NotEqual(token, first);
        Assert.Equal(64, first.Length);
    }
}
