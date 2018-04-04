namespace VtNetCore.UWP.App
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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


        public Model.Device NewConnection { get; set; } =
            new Model.Device
            {
                AuthenticationMethod = Model.EAuthenticationMethod.InheritFromSite
            };

        public List<Model.Device> Devices
        {
            get
            {
                return MockData.MockDevices;
            }
        }

        public List<Model.AuthenticationProfile> AuthenticationProfiles
        {
            get
            {
                return MockData.MockAuthenticationProfiles;
            }
        }

        public List<Model.Tennant> Tennants
        {
            get
            {
                return MockData.MockTennants;
            }
        }

        public List<Model.Site> Sites
        {
            get
            {
                return MockData.MockSites;
            }
        }

        private ObservableCollection<Model.Site> SitesForSelectedTennant = new ObservableCollection<Model.Site>();
        private ObservableCollection<Model.Device> DevicesForSelectedSite = new ObservableCollection<Model.Device>();

        public HomePage()
        {
            this.InitializeComponent();
        }

        private void TennantsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SitesForSelectedTennant.Clear();
            DevicesForSelectedSite.Clear();

            if ((e.ClickedItem as Model.Tennant) != null)
            {
                var sites = Sites.Where(x => x.TennantId == (e.ClickedItem as Model.Tennant).Id);
                foreach (var site in sites)
                    SitesForSelectedTennant.Add(site);
            }
        }

        private void SitesView_ItemClick(object sender, ItemClickEventArgs e)
        {
            DevicesForSelectedSite.Clear();

            if ((e.ClickedItem as Model.Site) != null)
            {
                var devices = Devices.Where(x => x.SiteId == (e.ClickedItem as Model.Site).Id);
                foreach (var device in devices)
                    DevicesForSelectedSite.Add(device);
            }
        }


        private void DevicesView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Model.Device)e.ClickedItem;

            var t = Terminals.Instance;

            var terminalInstance = t.Where(x => x.Connection.Destination.ToString() == item.Destination).SingleOrDefault();
            if(terminalInstance == null)
            {
                var username = string.Empty;
                var password = string.Empty;

                if (item.AuthenticationMethod == Model.EAuthenticationMethod.NoAuthentication)
                {

                }
                else
                {
                    var authenticationProfile = AuthenticationProfiles.Where(x => x.Id == item.AuthenticationProfileId).SingleOrDefault();
                    if (authenticationProfile == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to find an authentication profile");
                        return;
                    }
                    username = authenticationProfile.Username;
                    password = authenticationProfile.Password;
                }

                terminalInstance = new TerminalInstance();
                if(!terminalInstance.ConnectTo(new Uri(item.Destination), username, password))
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
