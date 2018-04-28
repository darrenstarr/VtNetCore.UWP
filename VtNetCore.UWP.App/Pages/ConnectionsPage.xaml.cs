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

    public sealed partial class ConnectionsPage : Page
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

        public ObservableCollection<Model.Tenant> Tenants
        {
            get => Model.Context.Current.Tenants;
        }

        public ObservableCollection<Model.Site> Sites
        {
            get => Model.Context.Current.Sites;
        }

        public ObservableCollection<Model.DeviceType> DeviceTypes
        {
            get => Model.Context.Current.DeviceTypes;
        }

        private ObservableCollection<Model.Site> SitesForSelectedTenant { get; set; } = new ObservableCollection<Model.Site>();
        private ObservableCollection<Model.Device> DevicesForSelectedSite { get; set; } = new ObservableCollection<Model.Device>();

        public ConnectionsPage()
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
            if (TenantsView.SelectedItem == null)
                return;

            var tenant = TenantsView.SelectedItem as Model.Tenant;

            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (var addition in e.NewItems.Cast<Model.Site>().Where(x => x.TenantId == tenant.Id))
                        SitesForSelectedTenant.Add(addition);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (var removal in e.OldItems.Cast<Model.Site>().Where(x => x.TenantId == tenant.Id))
                        SitesForSelectedTenant.Remove(removal);
                    break;

                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    SitesForSelectedTenant.Clear();
                    break;
            }
        }

        private void TenantsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SitesForSelectedTenant.Clear();
            DevicesForSelectedSite.Clear();

            if ((e.ClickedItem as Model.Tenant) != null)
            {
                var sites = Sites.Where(x => x.TenantId == (e.ClickedItem as Model.Tenant).Id);
                foreach (var site in sites)
                    SitesForSelectedTenant.Add(site);
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
                EditDeviceButton.IsEnabled = false;
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
            EditDeviceButton.IsEnabled = true;
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

        private void AddSiteDone_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Sites.Add(
                new Model.Site
                {
                    Id = Guid.NewGuid(),
                    TenantId = (TenantsView.SelectedItem as Model.Tenant).Id,
                    Name = SiteNameField.Text,
                    Location = SiteLocationField.Text,
                    Notes = SiteNotesField.Text
                }
                );
        }

        private async void RemoveTenantButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedTenant = (Model.Tenant)TenantsView.SelectedItem;

            var removeTenantDialog = new ContentDialog
            {
                Title = "Delete tenant?",
                Content = "The selected tenant '" + selectedTenant.Name + "' is about to be removed. Continue?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Remove"
            };

            var result = await removeTenantDialog.ShowAsync();
            if (result == ContentDialogResult.None)
                return;

            Model.Context.Current.RemoveTenant(selectedTenant);
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

        private void AddDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedSite = SitesView.SelectedItem as Model.Site;
            if (selectedSite == null)
                throw new Exception("It should not be possible to select add device if no site is selected");

            AddConnectionFlyout.Operation = Controls.FormOperation.Add;
            AddConnectionFlyout.Device = null;
            AddConnectionFlyout.ClearForm();
            AddConnectionFlyout.SiteId = selectedSite.Id;
        }

        private void EditDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedDevice = (Model.Device)DevicesView.SelectedItem;

            AddConnectionFlyout.Operation = Controls.FormOperation.Edit;
            AddConnectionFlyout.Device = selectedDevice ?? throw new Exception("Edit device button should not be active when no device is selected");
        }

        private async void AddConnectionFlyout_OnDeviceChanged(object sender, Controls.DevicePropertiesForm.DeviceChangedEventArgs e)
        {
            switch(e.Operation)
            {
                case Controls.FormOperation.Add:
                    Model.Context.Current.Devices.Add(e.Device);
                    DevicesView.SelectedItem = e.Device;
                    break;

                case Controls.FormOperation.Edit:
                    await Model.Context.Current.SaveChanges(e.Device);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void AddTenantButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AddTenantFlyout.Operation = Controls.FormOperation.Add;
            AddTenantFlyout.Tenant = null;
            AddTenantFlyout.ClearForm();
        }

        private void EditTenantButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedTenant = (Model.Tenant)TenantsView.SelectedItem;

            AddTenantFlyout.Operation = Controls.FormOperation.Edit;
            AddTenantFlyout.Tenant = selectedTenant ?? throw new Exception("Edit tenant button should not be active when no tenant is selected");
        }

        private async void AddTenantFlyout_OnTenantChanged(object sender, Controls.TenantPropertiesForm.TenantChangedEventArgs e)
        {
            switch (e.Operation)
            {
                case Controls.FormOperation.Add:
                    Model.Context.Current.Tenants.Add(e.Tenant);
                    DevicesView.SelectedItem = e.Tenant;
                    break;

                case Controls.FormOperation.Edit:
                    await Model.Context.Current.SaveChanges(e.Tenant);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
