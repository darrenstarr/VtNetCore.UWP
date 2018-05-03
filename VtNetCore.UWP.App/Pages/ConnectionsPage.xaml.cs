namespace VtNetCore.UWP.App.Pages
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using VtNetCore.UWP.App.Utility.Helpers;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Input;

    public sealed partial class ConnectionsPage : Page
    {
        private ObservableCollection<Model.Tenant> Tenants => Model.Context.Current.Tenants;
        private AdvancedCollectionView Sites { get; } = new AdvancedCollectionView(Model.Context.Current.Sites);
        private AdvancedCollectionView Devices { get; } = new AdvancedCollectionView(Model.Context.Current.Devices);

        public ConnectionsPage()
        {
            InitializeComponent();

            Sites.Filter = x =>
            {
                var tenant = (Model.Tenant)TenantsView.SelectedItem;

                if (tenant == null)
                    return false;

                return ((Model.Site)x).TenantId == tenant.Id;
            };

            Devices.Filter = x =>
            {
                var site = (Model.Site)SitesView.SelectedItem;

                if (site == null)
                    return false;

                return ((Model.Device)x).SiteId == site.Id;
            };
        }

        private void TenantsView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sites.RefreshFilter();
        }

        private void SitesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Devices.RefreshFilter();
        }

        private void DevicesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newlySelectedDevice = (Model.Device)(e.AddedItems.SingleOrDefault());

            DisconnectToDeviceButton.ClearValue(IsEnabledProperty);

            if (newlySelectedDevice == null)
            {
                //ConnectToDeviceButton.Visibility = Visibility.Collapsed;
                //DisconnectToDeviceButton.Visibility = Visibility.Collapsed;

                DisconnectToDeviceButton.IsEnabled = false;
                ConnectToDeviceButton.IsEnabled = false;
                RemoveDeviceButton.IsEnabled = false;
                EditDeviceButton.IsEnabled = false;
                return;
            }

            ConnectToDeviceButton.Visibility = Visibility.Visible;
            DisconnectToDeviceButton.Visibility = Visibility.Visible;

            ConnectToDeviceButton.IsEnabled = !newlySelectedDevice.Connected;
            DisconnectToDeviceButton.IsEnabled = newlySelectedDevice.Connected;

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

        private async void ConnectToDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
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
                password = await device.Password.Unprotect();
            }
            else
            {
                var authenticationProfile = Model.Context.Current.AuthenticationProfiles.Where(x => x.Id == device.AuthenticationProfileId).SingleOrDefault();
                if (authenticationProfile == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to find an authentication profile");
                    return;
                }
                username = authenticationProfile.Username;
                password = await device.Password.Unprotect();
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

            var device = 
                (Model.Device)Devices
                    .Where(
                        x => 
                            (new Uri(((Model.Device)x).Destination)).Equals(terminal.Connection.Destination)
                    )
                    .SingleOrDefault();

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

            SetSelectionEnabled(false);

            DeviceProperties.Operation = Controls.FormOperation.Add;
            DeviceProperties.Device = null;
            DeviceProperties.ClearForm();
            DeviceProperties.SiteId = selectedSite.Id;
            DeviceProperties.Visibility = Visibility.Visible;
            DeviceProperties.SetInitialFocus();
        }

        private void EditDeviceButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedDevice = (Model.Device)DevicesView.SelectedItem;

            SetSelectionEnabled(false);

            DeviceProperties.Operation = Controls.FormOperation.Edit;
            DeviceProperties.Device = selectedDevice ?? throw new Exception("Edit device button should not be active when no device is selected");
            DeviceProperties.Visibility = Visibility.Visible;
            DeviceProperties.SetInitialFocus();
        }

        private async void AddConnectionFlyout_OnDeviceChanged(object sender, Controls.DevicePropertiesForm.DeviceChangedEventArgs e)
        {
            switch (e.Operation)
            {
                case Controls.FormOperation.Add:
                    if (e.Device.AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword)
                        e.Device.Password = await e.Device.Password.Protect();

                    Model.Context.Current.Devices.Add(e.Device);
                    SetSelectionEnabled(true);

                    DevicesView.SelectedItem = e.Device;
                    break;

                case Controls.FormOperation.Edit:
                    if (e.Device.AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword)
                    {
                        if (!e.Device.Password.IsEncryptedPassword())
                            e.Device.Password = await e.Device.Password.Protect();
                    }

                    await Model.Context.Current.SaveChanges(e.Device);

                    SetSelectionEnabled(true);

                    DevicesView.SelectedItem = e.Device;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void AddTenantButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetSelectionEnabled(false);

            TenantProperties.Operation = Controls.FormOperation.Add;
            TenantProperties.Tenant = null;
            TenantProperties.ClearForm();
            TenantProperties.Visibility = Visibility.Visible;
            TenantProperties.SetInitialFocus();
        }

        private void EditTenantButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedTenant = (Model.Tenant)TenantsView.SelectedItem;

            SetSelectionEnabled(false);

            TenantProperties.Operation = Controls.FormOperation.Edit;
            TenantProperties.Tenant = selectedTenant ?? throw new Exception("Edit tenant button should not be active when no tenant is selected");
            TenantProperties.Visibility = Visibility.Visible;
            TenantProperties.SetInitialFocus();
        }

        private async void AddTenantFlyout_OnTenantChanged(object sender, Controls.TenantPropertiesForm.TenantChangedEventArgs e)
        {
            switch (e.Operation)
            {
                case Controls.FormOperation.Add:
                    Model.Context.Current.Tenants.Add(e.Tenant);
                    SetSelectionEnabled(true);

                    DevicesView.SelectedItem = e.Tenant;
                    break;

                case Controls.FormOperation.Edit:
                    await Model.Context.Current.SaveChanges(e.Tenant);
                    SetSelectionEnabled(true);

                    DevicesView.SelectedItem = e.Tenant;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private async void AddSiteFlyout_OnSiteChanged(object sender, Controls.SitePropertiesForm.SiteChangedEventArgs e)
        {
            switch (e.Operation)
            {
                case Controls.FormOperation.Add:
                    Model.Context.Current.Sites.Add(e.Site);
                    SetSelectionEnabled(true);

                    SitesView.SelectedItem = e.Site;

                    break;

                case Controls.FormOperation.Edit:
                    await Model.Context.Current.SaveChanges(e.Site);
                    SetSelectionEnabled(true);

                    SitesView.SelectedItem = e.Site;

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void EditSiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedSite = (Model.Site)SitesView.SelectedItem;

            SetSelectionEnabled(false);

            SiteProperties.Operation = Controls.FormOperation.Edit;
            SiteProperties.Site = selectedSite ?? throw new Exception("Edit site button should not be active when no device is selected");

            SiteProperties.Visibility = Visibility.Visible;
            SiteProperties.SetInitialFocus();
        }

        private void AddSiteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedTenant = TenantsView.SelectedItem as Model.Tenant;
            if (selectedTenant == null)
                throw new Exception("It should not be possible to select add site if no tenant is selected");

            SetSelectionEnabled(false);

            SiteProperties.Operation = Controls.FormOperation.Add;
            SiteProperties.Site = null;
            SiteProperties.ClearForm();
            SiteProperties.TenantId = selectedTenant.Id;
            SiteProperties.Visibility = Visibility.Visible;
            SiteProperties.SetInitialFocus();
        }

        private void SetSelectionEnabled(bool enabled)
        {
            //AddTenantButton.IsEnabled = enabled;
            //RemoveTenantButton.IsEnabled = enabled;
            //EditTenantButton.IsEnabled = enabled;
            TenantsCommandBar.IsEnabled = enabled;
            TenantsView.IsEnabled = enabled;

            //AddSiteButton.IsEnabled = enabled;
            //RemoveSiteButton.IsEnabled = enabled;
            //EditSiteButton.IsEnabled = enabled;
            SitesCommandBar.IsEnabled = enabled;
            SitesView.IsEnabled = enabled;

            //AddDeviceButton.IsEnabled = enabled;
            //RemoveDeviceButton.IsEnabled = enabled;
            //EditDeviceButton.IsEnabled = enabled;
            //ConnectToDeviceButton.IsEnabled = enabled;
            //DisconnectToDeviceButton.IsEnabled = enabled;
            DevicesCommandBar.IsEnabled = enabled;
            DevicesView.IsEnabled = enabled;
        }

        private void FormCancelled(object sender, EventArgs e)
        {
            SetSelectionEnabled(true);
        }
    }
}
