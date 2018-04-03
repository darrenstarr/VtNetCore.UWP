namespace VtNetCore.UWP.App.Model
{
    using System;

    public class Device
    {
        public Guid Id { get; set; }
        public Guid SiteId { get; set; }
        public string Name { get; set; }
        public string Destination { get; set; }
        public EAuthenticationMethod AuthenticationMethod { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Guid AuthenticationProfileId { get; set; }
        public string Notes { get; set; }
        public string IconName { get; set; }
    }
}
