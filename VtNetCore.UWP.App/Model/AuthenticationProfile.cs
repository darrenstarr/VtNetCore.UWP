namespace VtNetCore.UWP.App.Model
{
    using System;

    public class AuthenticationProfile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Notes { get; set; }
    }
}
