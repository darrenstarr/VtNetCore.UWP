namespace VtNetCore.UWP.App
{
    using System.Collections.Generic;

    public class AuthenticationMethod
    {
        public static readonly AuthenticationMethod NoAuthentication = new AuthenticationMethod
        {
            Method = Model.EAuthenticationMethod.NoAuthentication,
            Tag = "NoAuthentication",
            HumanReadable = "No authentication",
        };

        public static readonly AuthenticationMethod UsernameAndPassword = new AuthenticationMethod
        {
            Method = Model.EAuthenticationMethod.UsernamePassword,
            Tag = "UsernamePassword",
            HumanReadable = "Username & Password",
        };

        public static readonly AuthenticationMethod AuthenticationProfile = new AuthenticationMethod
        {
            Method = Model.EAuthenticationMethod.AuthenticationProfile,
            Tag = "AuthenticationProfile",
            HumanReadable = "Authentication Profile",
        };

        public static readonly AuthenticationMethod InheritFromSite = new AuthenticationMethod
        {
            Method = Model.EAuthenticationMethod.InheritFromSite,
            Tag = "InheritFromSite",
            HumanReadable = "{Inherit from site}"
        };

        public static readonly AuthenticationMethod InheritFromTenant = new AuthenticationMethod
        {
            Method = Model.EAuthenticationMethod.InheritFromTenant,
            Tag = "InheritFromTenant",
            HumanReadable = "{Inherit from tenant}"
        };

        public static readonly List<AuthenticationMethod> DeviceAuthenticationMethods = new List<AuthenticationMethod>
        {
            NoAuthentication,
            UsernameAndPassword,
            AuthenticationProfile,
            InheritFromSite,
            InheritFromTenant
        };

        public static readonly List<AuthenticationMethod> ProfileAuthenticationMethods = new List<AuthenticationMethod>
        {
            NoAuthentication,
            UsernameAndPassword,
        };

        public Model.EAuthenticationMethod Method { get; set; }

        public string Tag { get; set; }

        public string HumanReadable { get; set; }

        public override string ToString()
        {
            return HumanReadable;
        }
    }
}
