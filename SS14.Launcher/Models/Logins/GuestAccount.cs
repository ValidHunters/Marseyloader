using System;
using SS14.Launcher.Models.Data;

namespace SS14.Launcher.Models.Logins;

public class GuestAccount : LoggedInAccount
{
    public GuestAccount() : base(new LoginInfo())
    {
        LoginInfo.Username = "Guest";
        LoginInfo.UserId = Guid.Empty;
        LoginInfo.Token = new LoginToken("marsey", DateTimeOffset.MaxValue);
    }

    public override AccountLoginStatus Status => AccountLoginStatus.Guest;
}

