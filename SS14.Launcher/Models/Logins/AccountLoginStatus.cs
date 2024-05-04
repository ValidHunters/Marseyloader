namespace SS14.Launcher.Models.Logins;

public enum AccountLoginStatus
{
    Unsure = 0,

    /// <summary>
    ///     Last we checked, the login token was still valid.
    /// </summary>
    Available,

    /// <summary>
    ///     The login token expired and we need the user to log in again.
    /// </summary>
    Expired,

    /// <summary>
    ///     This is a local account, and has no token
    /// </summary>
    Guest
}
