namespace VtNetCore.UWP.App.Model
{
    using System;

    public class Site
    {
        public Guid Id { get; set; }
        public Guid TennantId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
    }
}
