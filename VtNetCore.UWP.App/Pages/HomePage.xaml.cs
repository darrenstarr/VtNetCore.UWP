namespace VtNetCore.UWP.App.Pages
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;

    public sealed partial class HomePage : Page, INotifyPropertyChanged
    {
        public Model.Device NewConnection { get; set; } =
            new Model.Device
            {
                AuthenticationMethod = Model.EAuthenticationMethod.InheritFromSite
            };

        public ObservableCollection<Model.Device> Devices
        {
            get => Model.Context.Current.Devices;
        }

        public ObservableCollection<Model.AuthenticationProfile> AuthenticationProfiles
        {
            get => Model.Context.Current.AuthenticationProfiles;
        }

        public ObservableCollection<Model.Tennant> Tennants
        {
            get => Model.Context.Current.Tennants;
        }

        public ObservableCollection<Model.Site> Sites
        {
            get => Model.Context.Current.Sites;
        }

        public ObservableCollection<Model.DeviceType> DeviceTypes
        {
            get => Model.Context.Current.DeviceTypes;
        }

        private ObservableCollection<Model.Site> SitesForSelectedTennant { get; set; } = new ObservableCollection<Model.Site>();
        private ObservableCollection<Model.Device> DevicesForSelectedSite { get; set; } = new ObservableCollection<Model.Device>();

        public HomePage()
        {
            InitializeComponent();
            Sites.CollectionChanged += Sites_CollectionChanged;
            Devices.CollectionChanged += Devices_CollectionChanged;
        }

        private void Devices_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (SitesView.SelectedItem == null)
                return;

            var site = SitesView.SelectedItem as Model.Site;

            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var addition in e.NewItems.Cast<Model.Device>().Where(x => x.SiteId == site.Id))
                        DevicesForSelectedSite.Add(addition);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var removal in e.OldItems.Cast<Model.Device>().Where(x => x.SiteId == site.Id))
                        DevicesForSelectedSite.Remove(removal);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    DevicesForSelectedSite.Clear();
                    break;
            }
        }

        private void Sites_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (TennantsView.SelectedItem == null)
                return;

            var tennant = TennantsView.SelectedItem as Model.Tennant;

            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var addition in e.NewItems.Cast<Model.Site>().Where(x => x.TennantId == tennant.Id))
                        SitesForSelectedTennant.Add(addition);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var removal in e.OldItems.Cast<Model.Site>().Where(x => x.TennantId == tennant.Id))
                        SitesForSelectedTennant.Remove(removal);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    SitesForSelectedTennant.Clear();
                    break;
            }
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

        public bool AlwaysFalseAnchorValue { get; } = false;

        private void DevicesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newlySelectedDevice = (Model.Device)(e.AddedItems.SingleOrDefault());

            DisconnectToDeviceButton.ClearValue(IsEnabledProperty);

            if (newlySelectedDevice == null)
            {
                ConnectToDeviceButton.Visibility = Visibility.Collapsed;
                DisconnectToDeviceButton.Visibility = Visibility.Collapsed;

                DisconnectToDeviceButton.IsEnabled = false;
                RemoveDeviceButton.IsEnabled = false;
                return;
            }

            ConnectToDeviceButton.Visibility = Visibility.Visible;
            DisconnectToDeviceButton.Visibility = Visibility.Visible;

            var newBinding = new Binding
            {
                Source = newlySelectedDevice,
                Path = new PropertyPath("Connected"),
                Mode = BindingMode.TwoWay
            };

            DisconnectToDeviceButton.SetBinding(IsEnabledProperty, newBinding);

            RemoveDeviceButton.IsEnabled = true;
        }

        private void ConnectToDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var device = (Model.Device)DevicesView.SelectedItem;
            if (device == null)
                return;

            var terminals = Terminals.Instance;

            var destinationUri = new Uri(device.Destination);
            var terminalInstance = terminals.Where(x => x.Connection.Destination.Equals(destinationUri)).SingleOrDefault();

            if (terminalInstance == null)
            {
                terminalInstance = new TerminalInstance();
                terminalInstance.PropertyChanged += TerminalInstance_PropertyChanged;

                terminals.Add(terminalInstance);
            }

            var username = string.Empty;
            var password = string.Empty;

            if (device.AuthenticationMethod == Model.EAuthenticationMethod.NoAuthentication)
            {

            }
            else if(device.AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword)
            {
                username = device.Username;
                password = device.Password;
            }
            else
            {
                var authenticationProfile = AuthenticationProfiles.Where(x => x.Id == device.AuthenticationProfileId).SingleOrDefault();
                if (authenticationProfile == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to find an authentication profile");
                    return;
                }
                username = authenticationProfile.Username;
                password = authenticationProfile.Password;
            }

            DisconnectToDeviceButton.ClearValue(IsEnabledProperty);

            var newBinding = new Binding
            {
                Source = device,
                Path = new PropertyPath("Connected"),
                Mode = BindingMode.TwoWay
            };

            DisconnectToDeviceButton.SetBinding(IsEnabledProperty, newBinding);

            if (!terminalInstance.ConnectTo(new Uri(device.Destination), username, password))
            {
                System.Diagnostics.Debug.WriteLine("Failed to connect to destination");
                return;
            }
        }

        private void TerminalInstance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var terminal = (TerminalInstance)sender;

            var device = Devices.Where(x => (new Uri(x.Destination)).Equals(terminal.Connection.Destination)).SingleOrDefault();

            if (device == null)
                return;

            device.Connected = terminal.IsConnected;
        }

        private void DisconnectToDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedDevice = DevicesView.SelectedItem as Model.Device;

            if (!selectedDevice.Connected)
                throw new Exception("Invalid state, disconnect clicked on a device which isn't connected");

            var destinationUri = new Uri(selectedDevice.Destination);
            var terminalInstance = Terminals.Instance.Where(x => x.Connection.Destination.Equals(destinationUri)).SingleOrDefault();

            if (terminalInstance == null)
                throw new Exception("Failed to find a reference to the connected device");

            terminalInstance.Connection.Disconnect();
        }

        private void AddTennantDone_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Tennants.Add(
                new Model.Tennant
                {
                    Id = Guid.NewGuid(),
                    Name = TennantNameField.Text,
                    Notes = TennantNotesField.Text
                }
                );
        }

        private void AddSiteDone_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Sites.Add(
                new Model.Site
                {
                    Id = Guid.NewGuid(),
                    TennantId = (TennantsView.SelectedItem as Model.Tennant).Id,
                    Name = SiteNameField.Text,
                    Location = SiteLocationField.Text,
                    Notes = SiteNotesField.Text
                }
                );
        }

        private void AddConnectionDoneClicked(object sender, RoutedEventArgs e)
        {
            Devices.Add(
                new Model.Device
                {
                    Id = Guid.NewGuid(),
                    SiteId = (SitesView.SelectedItem as Model.Site).Id,
                    Name = ConnectionNameField.Text,
                    Destination = ConnectionDestinationField.Text,
                    AuthenticationMethod = (ConnectionAuthenticationMethodField.SelectedItem as AuthenticationMethod).Method,
                    AuthenticationProfileId =
                        ConnectionAuthenticationProfileField.SelectedItem == null ?
                            Guid.Empty :
                            (ConnectionAuthenticationProfileField.SelectedItem as Model.AuthenticationProfile).Id,
                    Username = ConnectionUsernameField.Text,
                    Password = ConnectionPasswordField.Password,
                    Notes = ConnectionNotesField.Text,
                    DeviceTypeId = NewConnection.Id,
                }
                );
        }

        private async void RemoveTennantButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedTennant = (Model.Tennant)TennantsView.SelectedItem;

            var removeTennantDialog = new ContentDialog
            {
                Title = "Delete tennant?",
                Content = "The selected tennant '" + selectedTennant.Name + "' is about to be removed. Continue?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Remove"
            };

            var result = await removeTennantDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            Model.Context.Current.RemoveTennant(selectedTennant);
        }

        private async void RemoveSiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedSite = (Model.Site)SitesView.SelectedItem;

            var remoteSiteDialog = new ContentDialog
            {
                Title = "Delete Site?",
                Content = "The selected site '" + selectedSite.Name + "' is about to be removed. Continue?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Remove"
            };

            var result = await remoteSiteDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            Model.Context.Current.RemoveSite(selectedSite);
        }

        private async void RemoveDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedDevice = (Model.Device)DevicesView.SelectedItem;

            var removeDeviceDialog = new ContentDialog
            {
                Title = "Delete device?",
                Content = "The selected device '" + selectedDevice.Name + "' is about to be removed. Continue?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Remove"
            };

            var result = await removeDeviceDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            Model.Context.Current.RemoveDevice(selectedDevice);
        }
    }
}
