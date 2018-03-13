namespace VtNetCore.UWP.App.ScriptTool
{
    using Newtonsoft.Json;
    //using NiL.JS;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class ScriptTool
    {
        public class ConnectionProfile
        {
            public string Name { get; set; }
            public string HostAddress { get; set; }
            public string AuthenticationProfile { get; set; }

            public ConnectionProfile Clone()
            {
                return new ConnectionProfile
                {
                    Name = Name,
                    HostAddress = HostAddress,
                    AuthenticationProfile = AuthenticationProfile
                };
            }
        }

        private List<ConnectionProfile> _connectionProfiles = new List<ConnectionProfile>();
        public List<ConnectionProfile> ConnectionProfiles
        {
            get
            {
                lock (_connectionProfiles)
                {
                    return _connectionProfiles.Select(x => x.Clone()).ToList();
                }
            }
        }

        public async Task<bool> LoadConnectionProfiles()
        {
            var uri = new Uri("ms-appx://Sample/connectionProfiles.json");
            var sampleFile = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            var text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

            var profiles = JsonConvert.DeserializeObject(text);

            return true;
        }

    }
}
