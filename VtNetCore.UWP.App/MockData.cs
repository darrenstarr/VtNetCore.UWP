namespace VtNetCore.UWP.App
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    public static class MockData
    {
        private static Model.AuthenticationProfile DefaultAuthenticationProfile = new Model.AuthenticationProfile
        {
            Id = Guid.NewGuid(),
            Name = "MunchkinLAN Admin",
            Username = "admin",
            Password = "Minions12345",
            Notes = "Admin account in MunchkinLAN"
        };

        private static Model.Tennant Local = new Model.Tennant
        {
            Id = Guid.NewGuid(),
            Name = "{Local}",
            Notes = "Not a tennant, but simply locally stored profiles"
        };

        private static Model.Site LocalSite = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = Local.Id,
            Name = "{Local Site}",
            Location = "Local",
            Notes = "These are connections bound to the local user"
        };

        private static Model.Device LocalLinux = new Model.Device {
            Id = Guid.NewGuid(),
            SiteId = LocalSite.Id,
            Name = "Miner1.munchkinlan.local",
            Destination = "ssh://10.100.5.100",
            Notes = "Linux test system for using VtNetCore against multiple verification test",
            DeviceType = "Workstation.Linux.Ubuntu",
            AuthenticationMethod = Model.EAuthenticationMethod.AuthenticationProfile,
            AuthenticationProfileId = DefaultAuthenticationProfile.Id,
        };

        private static Model.Device LocalRouter = new Model.Device {
            Id = Guid.NewGuid(),
            SiteId = LocalSite.Id,
            Name = "Console.munchkinlan.local",
            Destination = "ssh://10.100.5.3",
            Notes = "Terminal server for office rack",
            DeviceType = "Router.Cisco.2811",
            AuthenticationMethod = Model.EAuthenticationMethod.AuthenticationProfile,
            AuthenticationProfileId = DefaultAuthenticationProfile.Id,
        };

        private static Model.Device LocalSwitch = new Model.Device
        {
            Id = Guid.NewGuid(),
            SiteId = LocalSite.Id,
            Name = "TORSW.munchkinlan.local",
            Destination = "ssh://10.100.5.2",
            Notes = "Top of rack switch for office rack",
            DeviceType = "Switch.Cisco.3560-24-TS",
            AuthenticationMethod = Model.EAuthenticationMethod.AuthenticationProfile,
            AuthenticationProfileId = DefaultAuthenticationProfile.Id,
        };

        private static Model.Tennant MunchkinLAN = new Model.Tennant
        {
            Id = Guid.NewGuid(),
            Name = "MunchkinLAN Labs",
            Notes = "Darren's data center"
        };

        private static Model.Site MunchkinLANPod1 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 1",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod2 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 2",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod3 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 3",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod4 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 4",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod5 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 5",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod6 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 6",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod7 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 7",
            Location = "The server",
            Notes = ""
        };

        private static Model.Site MunchkinLANPod8 = new Model.Site
        {
            Id = Guid.NewGuid(),
            TennantId = MunchkinLAN.Id,
            Name = "Pod 8",
            Location = "The server",
            Notes = ""
        };

        private static Model.Device MunchkinLANPod1Isp1 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP1", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32769", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp2 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP2", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32770", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp3 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP3", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32771", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp4 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP4", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32772", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp5 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP5", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32773", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp6 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP6", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32774", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp7 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP7", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32775", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp8 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP8", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32776", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp9 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP9", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32777", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp10 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP10", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32778", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp11 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP11", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32779", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp12 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP12", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32770", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp13 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP13", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32781", DeviceType = "Router" };
        private static Model.Device MunchkinLANPod1Isp14 = new Model.Device { Id = Guid.NewGuid(), SiteId = MunchkinLANPod1.Id, Name = "ISP14", AuthenticationMethod = Model.EAuthenticationMethod.NoAuthentication, Destination = "telnet://10.100.11.151:32782", DeviceType = "Router" };

        public static List<Model.Tennant> MockTennants =
            new List<Model.Tennant>
            {
                Local,
                new Model.Tennant
                {
                    Id = Guid.Parse("7c182cfb-7756-43c3-84a6-de947664152e"),
                    Name = "Telenor Inpli",
                    Notes = "Those who pay the bills"
                },
                new Model.Tennant
                {
                    Id = Guid.Parse("183e3710-68a0-40ed-8d08-34f28e7939d2"),
                    Name = "Monster's Inc.",
                    Notes = "Don't look in the closet or under the bed."
                },
                MunchkinLAN,
            };

        public static List<Model.Site> MockSites =
            new List<Model.Site>
            {
                LocalSite,
                MunchkinLANPod1,
                MunchkinLANPod2,
                MunchkinLANPod3,
                MunchkinLANPod4,
                MunchkinLANPod5,
                MunchkinLANPod6,
                MunchkinLANPod7,
                MunchkinLANPod8,
            };

        public static List<Model.AuthenticationProfile> MockAuthenticationProfiles =
            new List<Model.AuthenticationProfile>
            {
                DefaultAuthenticationProfile,
            };

        public static List<Model.Device> MockDevices =
            new List<Model.Device> {
                LocalLinux,
                LocalRouter,
                LocalSwitch,
                MunchkinLANPod1Isp1,
                MunchkinLANPod1Isp2,
                MunchkinLANPod1Isp3,
                MunchkinLANPod1Isp4,
                MunchkinLANPod1Isp5,
                MunchkinLANPod1Isp6,
                MunchkinLANPod1Isp7,
                MunchkinLANPod1Isp8,
                MunchkinLANPod1Isp9,
                MunchkinLANPod1Isp10,
                MunchkinLANPod1Isp11,
                MunchkinLANPod1Isp12,
                MunchkinLANPod1Isp13,
                MunchkinLANPod1Isp14,
            };
    }
}
