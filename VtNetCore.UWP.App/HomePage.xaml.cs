namespace VtNetCore.UWP.App
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        private static List<Model.Tennant> MockTennants =
            new List<Model.Tennant>
            {
                new Model.Tennant
                {
                    Id = Guid.Parse("d592a4d2-dc5b-4e83-b37a-ccf90dae1e0b"),
                    Name = "{Local}",
                    Notes = "Not a tennant, but simply locally stored profiles"
                }
            };

        private static List<Model.Site> MockSites =
            new List<Model.Site>
            {
                new Model.Site
                {
                    Id = Guid.Parse("d1df98b4-0bd6-4813-b115-1b2e63aae17b"),
                    TennantId = Guid.Parse("d592a4d2-dc5b-4e83-b37a-ccf90dae1e0b"),
                    Name = "{Local Site}",
                    Location = "Local",
                    Notes = "These are connections bound to the local user"
                }
            };

        private static List<Model.AuthenticationProfile> MockAuthenticationProfiles =
            new List<Model.AuthenticationProfile>
            {
                new Model.AuthenticationProfile
                {
                    Id = Guid.Parse("220ec5da-f4c1-4273-a3b6-c45aca443cd8"),
                    Name = "MunchkinLAN Admin",
                    Username = "admin",
                    Password = "Minions12345",
                    Notes = "Admin account in MunchkinLAN"
                },
            };

        private static List<Model.Device> MockDevices =
            new List<Model.Device> {
                new Model.Device {
                    Id = Guid.NewGuid(),
                    SiteId = Guid.Parse("d1df98b4-0bd6-4813-b115-1b2e63aae17b"),
                    Name = "Miner1.munchkinlan.local",
                    Destination = "ssh://10.100.5.100",
                    Notes = "Linux test system for using VtNetCore against multiple verification test",
                    IconName = "Assets/DeviceIcons/view1/workstation.png",
                    AuthenticationProfileId = Guid.Parse("220ec5da-f4c1-4273-a3b6-c45aca443cd8"),
                },
                new Model.Device {
                    Id = Guid.NewGuid(),
                    SiteId = Guid.Parse("d1df98b4-0bd6-4813-b115-1b2e63aae17b"),
                    Name = "Console.munchkinlan.local",
                    Destination = "ssh://10.100.5.3",
                    Notes = "Terminal server for office rack",
                    IconName = "Assets/DeviceIcons/view1/router.png",
                    AuthenticationProfileId = Guid.Parse("220ec5da-f4c1-4273-a3b6-c45aca443cd8"),
                },
                new Model.Device {
                    Id = Guid.NewGuid(),
                    SiteId = Guid.Parse("d1df98b4-0bd6-4813-b115-1b2e63aae17b"),
                    Name = "TORSW.munchkinlan.local",
                    Destination = "ssh://10.100.5.2",
                    Notes = "Top of rack switch for office rack",
                    IconName = "Assets/DeviceIcons/view1/mls.png",
                    AuthenticationProfileId = Guid.Parse("220ec5da-f4c1-4273-a3b6-c45aca443cd8"),
                },
            };

        public Model.Device NewConnection { get; set; } =
            new Model.Device
            {
                AuthenticationMethod = Model.EAuthenticationMethod.InheritFromSite
            };

        public List<Model.Device> Devices
        {
            get
            {
                return MockDevices;
            }
        }

        public List<Model.AuthenticationProfile> AuthenticationProfiles
        {
            get
            {
                return MockAuthenticationProfiles;
            }
        }

        public List<Model.Tennant> Tennants
        {
            get
            {
                return MockTennants;
            }
        }

        public List<Model.Site> Sites
        {
            get
            {
                return MockSites;
            }
        }

        public HomePage()
        {
            this.InitializeComponent();
        }

        private void TennantsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Model.Device)e.ClickedItem;

            var t = Terminals.Instance;

            var terminalInstance = t.Where(x => x.Connection.Destination.ToString() == item.Destination).SingleOrDefault();
            if(terminalInstance == null)
            {
                var authenticationProfile = AuthenticationProfiles.Where(x => x.Id == item.AuthenticationProfileId).SingleOrDefault();
                if(authenticationProfile == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to find an authentication profile");
                    return;
                }

                terminalInstance = new TerminalInstance();
                if(!terminalInstance.ConnectTo(new Uri(item.Destination), authenticationProfile.Username, authenticationProfile.Password))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to connect to destination");
                    return;
                }

                t.Add(terminalInstance);
            }
        }

        private void AddConnectionDoneClicked(object sender, RoutedEventArgs e)
        {
        }

        private void NewConnectionButtonTapped(object sender, TappedRoutedEventArgs e)
        {
            NewConnection = new Model.Device
            {
                Id = Guid.NewGuid(),
                AuthenticationMethod = Model.EAuthenticationMethod.InheritFromSite,
            };

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("NewConnection"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
