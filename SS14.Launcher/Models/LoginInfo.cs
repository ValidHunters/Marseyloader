using System;

namespace SS14.Launcher.Models
{
    public class LoginInfo
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
