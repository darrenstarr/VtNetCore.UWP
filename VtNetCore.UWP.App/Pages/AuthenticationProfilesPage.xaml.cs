namespace VtNetCore.UWP.App.Pages
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using VtNetCore.UWP.App.Utility.Helpers;
    using VtNetCore.UWP.App.ViewModel.AuthenticationProfilesViewModel;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;

    public sealed partial class AuthenticationProfilesPage : Page, INotifyPropertyChanged
    {
        public AuthenticationProfilesViewModel AuthenticationProfiles { get; set; } = new AuthenticationProfilesViewModel();

        private enum EProfileFlyoutMode
        {
            Add,
            Edit
        }

        private EProfileFlyoutMode ProfileFlyoutMode { get; set; }

        private Model.EAuthenticationMethod _profileAuthenticationMethod = Model.EAuthenticationMethod.UsernamePassword;

        public event PropertyChangedEventHandler PropertyChanged;

        private Model.EAuthenticationMethod ProfileAuthenticationMethod
        {
            get => _profileAuthenticationMethod;
            set
            {
                if (_profileAuthenticationMethod == value)
                    return;

                _profileAuthenticationMethod = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ProfileAuthenticationMethod"));
            }
        }

        private bool IsLoaded { get; set; }
        private Model.Tenant NewProfileTenant { get; set; }
        private Model.Site NewProfileSite { get; set; }
        private ObservableCollection<Model.Tenant> Tenants { get => Model.Context.Current.Tenants; }
        private AdvancedCollectionView Sites { get; } = new AdvancedCollectionView(Model.Context.Current.Sites, true);

        public AuthenticationProfilesPage()
        {
            InitializeComponent();
            PropertyChanged += AuthenticationProfilesPage_PropertyChanged;
        }

        private void AuthenticationProfilesPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "ProfileAuthenticationMethod")
            {
                switch(ProfileAuthenticationMethod)
                {
                    case Model.EAuthenticationMethod.NoAuthentication:
                        ProfileUsernameLabel.Visibility = Visibility.Collapsed;
                        ProfileUsernameField.Visibility = Visibility.Collapsed;
                        ProfilePasswordLabel.Visibility = Visibility.Collapsed;
                        ProfilePasswordField.Visibility = Visibility.Collapsed;
                        break;

                    case Model.EAuthenticationMethod.UsernamePassword:
                        ProfileUsernameLabel.Visibility = Visibility.Visible;
                        ProfileUsernameField.Visibility = Visibility.Visible;
                        ProfilePasswordLabel.Visibility = Visibility.Visible;
                        ProfilePasswordField.Visibility = Visibility.Visible;
                        break;

                    default:
                        throw new Exception("Unhandled case for authentication method");
                }
            }
        }

        private void ScopeRadioChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
                return;

            if (ScopeGlobal.IsChecked.Value)
            {
                ProfileTenantsLabel.Visibility = Visibility.Collapsed;
                ProfileTenantsField.Visibility = Visibility.Collapsed;
                ProfileSitesLabel.Visibility = Visibility.Collapsed;
                ProfileSitesField.Visibility = Visibility.Collapsed;
            }
            else if (ScopeTenant.IsChecked.Value)
            {
                ProfileTenantsLabel.Visibility = Visibility.Visible;
                ProfileTenantsField.Visibility = Visibility.Visible;
                ProfileSitesLabel.Visibility = Visibility.Collapsed;
                ProfileSitesField.Visibility = Visibility.Collapsed;
            }
            else if (ScopeSite.IsChecked.Value)
            {
                ProfileTenantsLabel.Visibility = Visibility.Visible;
                ProfileTenantsField.Visibility = Visibility.Visible;
                ProfileSitesLabel.Visibility = Visibility.Visible;
                ProfileSitesField.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = true;
            Sites.Filter = x =>
            {
                if (NewProfileTenant == null)
                    return false;

                return ((Model.Site)x).TenantId == NewProfileTenant.Id;
            };

            AuthenticationProfiles.CollectionChanged += (source, ev) =>
            {
                System.Diagnostics.Debug.WriteLine("Collection changed");
            };
        }

        private void AppButton_Loaded(object sender, RoutedEventArgs e)
        {
            bool allowFocusOnInteractionAvailable =
                Windows.Foundation.Metadata.ApiInformation.IsPropertyPresent(
                    "Windows.UI.Xaml.FrameworkElement",
                    "AllowFocusOnInteraction"
                    );

            if (allowFocusOnInteractionAvailable)
            {
                if (sender is FrameworkElement s)
                    s.AllowFocusOnInteraction = true;
            }
        }

        private void TenantsField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sites.RefreshFilter();
        }

        private void AddProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProfileFlyoutMode = EProfileFlyoutMode.Add;
            ScopeGlobal.IsEnabled = true;
            ScopeTenant.IsEnabled = true;
            ScopeSite.IsEnabled = true;
            ProfileTenantsField.IsEnabled = true;
            ProfileSitesField.IsEnabled = true;

            ProfileNameField.Text = "";
            ProfileAuthenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
            ProfileUsernameField.Text = "";
            ProfilePasswordField.Password = "";
            ProfileTenantsField.SelectedIndex = -1;
            ScopeGlobal.IsChecked = true;
            Sites.RefreshFilter();
            ProfileNotesField.Text = "";
        }

        private void EditProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProfileFlyoutMode = EProfileFlyoutMode.Edit;
            ScopeGlobal.IsEnabled = false;
            ScopeTenant.IsEnabled = false;
            ScopeSite.IsEnabled = false;
            ProfileTenantsField.IsEnabled = false;
            ProfileSitesField.IsEnabled = false;

            var selectedItem = (Model.AuthenticationProfile) AuthenticationProfilesView.SelectedItem;
            if (selectedItem == null)
                throw new Exception("Edit profile button should not be accessible if there is no selected profile");

            ProfileNameField.Text = selectedItem.Name;
            ProfileAuthenticationMethod = selectedItem.AuthenticationMethod;
            ProfileUsernameField.Text = string.IsNullOrWhiteSpace(selectedItem.Username) ? "" : selectedItem.Username;
            ProfilePasswordField.Password = string.IsNullOrWhiteSpace(selectedItem.Password) ? "" : selectedItem.Password;
            ProfileTenantsField.SelectedIndex = -1;
            ProfileNotesField.Text = string.IsNullOrWhiteSpace(selectedItem.Notes) ? "" : selectedItem.Notes;

            if(selectedItem.ParentId == Guid.Empty)
            {
                ScopeGlobal.IsChecked = true;
                ProfileTenantsField.SelectedIndex = -1;
                Sites.RefreshFilter();

                return;
            }

            var parentTenant = Model.Context.Current.Tenants.SingleOrDefault(x => x.Id == selectedItem.ParentId);
            if (parentTenant != null)
            {
                ScopeTenant.IsChecked = true;
                ProfileTenantsField.SelectedItem = parentTenant;
                Sites.RefreshFilter();

                return;
            }

            var parentSite = Model.Context.Current.Sites.SingleOrDefault(x => x.Id == selectedItem.ParentId);
            if(parentSite != null)
            {
                parentTenant = Model.Context.Current.Tenants.Single(x => x.Id == parentSite.TenantId);

                ScopeSite.IsChecked = true;
                ProfileTenantsField.SelectedItem = parentTenant;
                Sites.RefreshFilter();
                ProfileSitesField.SelectedItem = parentSite;

                return;
            }
        }

        private void ProfileCancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private string ProtectedPasswordValueField
        {
            get
            {
                var authenticationMethod = ((AuthenticationMethod)ProfileAuthenticationMethodField.SelectedItem).Method;

                if (authenticationMethod != Model.EAuthenticationMethod.UsernamePassword)
                    return null;

                var result = ProfilePasswordField.Password;

                if (result == null || result.StartsWith("\u00FF"))
                    return result;

                var protectTask = Task.Run(async () => await result.Protect());
                protectTask.Wait();

                return "\u00FF" + protectTask.Result;
            }
        }

        private void ProfileDoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var authenticationMethod = ((AuthenticationMethod)ProfileAuthenticationMethodField.SelectedItem).Method;

            Guid parentId = Guid.Empty;
            if (ScopeTenant.IsChecked.Value)
                parentId = ((Model.Tenant)ProfileTenantsField.SelectedItem).Id;
            else if (ScopeSite.IsChecked.Value)
                parentId = ((Model.Site)ProfileSitesField.SelectedItem).Id;

            if (ProfileFlyoutMode == EProfileFlyoutMode.Add)
            {
                Model.Context.Current.AuthenticationProfiles.Add(
                    new Model.AuthenticationProfile
                    {
                        Id = Guid.NewGuid(),
                        ParentId = parentId,
                        Name = ProfileNameField.Text,
                        AuthenticationMethod = authenticationMethod,
                        Username = authenticationMethod == Model.EAuthenticationMethod.UsernamePassword ? ProfileUsernameField.Text : null,
                        Password = ProtectedPasswordValueField,
                        Notes = ProfileNotesField.Text
                    }
                    );

                ProfileNameField.Text = "";
                ProfileAuthenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
                ProfileUsernameField.Text = "";
                ProfilePasswordField.Password = "";
                ProfileTenantsField.SelectedIndex = -1;
                ScopeGlobal.IsChecked = true;
                Sites.RefreshFilter();
                ProfileNotesField.Text = "";

                return;
            }

            if (ProfileFlyoutMode == EProfileFlyoutMode.Edit)
            {
                var selectedItem = (Model.AuthenticationProfile)AuthenticationProfilesView.SelectedItem;
                if (selectedItem == null)
                    throw new Exception("Edit profile button should not be accessible if there is no selected profile");

                selectedItem.AuthenticationMethod = ProfileAuthenticationMethod;
                selectedItem.Username = authenticationMethod == Model.EAuthenticationMethod.UsernamePassword ? ProfileUsernameField.Text : null;
                selectedItem.Password = ProtectedPasswordValueField;
                selectedItem.Notes = ProfileNotesField.Text;

                Model.Context.Current.SaveChanges(selectedItem);

                EditProfileButton.Flyout.Hide();

                return;
            }

            throw new Exception("Unhandled flyout mode");
        }

        private async void RemoveProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var currentAuthenticationProfile = (Model.AuthenticationProfile)AuthenticationProfilesView.SelectedItem;

            if (currentAuthenticationProfile == null)
                throw new InvalidOperationException("The remove authentication profile button should not be enabled when there is none selected");

            var confirmDeletion = new ContentDialog
            {
                Title = "Confirm deletion?",
                Content = "Are you sure you wish to delete this authentication profile from the system?",
                SecondaryButtonText = "Confirm",
                PrimaryButtonText = "Cancel"
            };

            var result = await confirmDeletion.ShowAsync();
            if (result == ContentDialogResult.Secondary)
            {
                try
                {
                    Model.Context.Current.RemoveAuthenticationProfile(currentAuthenticationProfile);
                }
                catch (InvalidOperationException ex)
                {
                    // TODO : Replace this with a "Record in use" in use type exception

                    var itemInUseDialog = new ContentDialog
                    {
                        Title = "Invalid operation",
                        Content = ex.Message,
                        PrimaryButtonText = "Close"
                    };

                    await itemInUseDialog.ShowAsync();

                    return;
                }
            }
        }
    }
}
    