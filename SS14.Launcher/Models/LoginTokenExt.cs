using System;

namespace SS14.Launcher.Models;

public static class LoginTokenExt
{
    public static bool IsTimeExpired(this LoginToken token)
    {
        return token.ExpireTime <= DateTimeOffset.UtcNow;
    }

    public static bool ShouldRefresh(this LoginToken token)
    {
        return token.ExpireTime <= DateTimeOffset.UtcNow + ConfigConstants.TokenRefreshThreshold;
    }
}