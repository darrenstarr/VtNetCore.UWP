namespace VtNetCore.UWP.App
{
    using Microsoft.Toolkit.Uwp.UI;
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
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
        private Model.Tennant NewProfileTennant { get; set; }
        private Model.Site NewProfileSite { get; set; }
        private ObservableCollection<Model.Tennant> Tennants { get => Model.Context.Current.Tennants; }
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
                ProfileTennantsLabel.Visibility = Visibility.Collapsed;
                ProfileTennantsField.Visibility = Visibility.Collapsed;
                ProfileSitesLabel.Visibility = Visibility.Collapsed;
                ProfileSitesField.Visibility = Visibility.Collapsed;
            }
            else if (ScopeTennant.IsChecked.Value)
            {
                ProfileTennantsLabel.Visibility = Visibility.Visible;
                ProfileTennantsField.Visibility = Visibility.Visible;
                ProfileSitesLabel.Visibility = Visibility.Collapsed;
                ProfileSitesField.Visibility = Visibility.Collapsed;
            }
            else if (ScopeSite.IsChecked.Value)
            {
                ProfileTennantsLabel.Visibility = Visibility.Visible;
                ProfileTennantsField.Visibility = Visibility.Visible;
                ProfileSitesLabel.Visibility = Visibility.Visible;
                ProfileSitesField.Visibility = Visibility.Visible;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            IsLoaded = true;
            Sites.Filter = x =>
            {
                if (NewProfileTennant == null)
                    return false;

                return ((Model.Site)x).TennantId == NewProfileTennant.Id;
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

        private void TennantsField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Sites.RefreshFilter();
        }

        private void AddProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProfileFlyoutMode = EProfileFlyoutMode.Add;
            ScopeGlobal.IsEnabled = true;
            ScopeTennant.IsEnabled = true;
            ScopeSite.IsEnabled = true;
            ProfileTennantsField.IsEnabled = true;
            ProfileSitesField.IsEnabled = true;

            ProfileNameField.Text = "";
            ProfileAuthenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
            ProfileUsernameField.Text = "";
            ProfilePasswordField.Password = "";
            ProfileTennantsField.SelectedIndex = -1;
            ScopeGlobal.IsChecked = true;
            Sites.RefreshFilter();
            ProfileNotesField.Text = "";
        }

        private void EditProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ProfileFlyoutMode = EProfileFlyoutMode.Edit;
            ScopeGlobal.IsEnabled = false;
            ScopeTennant.IsEnabled = false;
            ScopeSite.IsEnabled = false;
            ProfileTennantsField.IsEnabled = false;
            ProfileSitesField.IsEnabled = false;

            var selectedItem = (Model.AuthenticationProfile) AuthenticationProfilesView.SelectedItem;
            if (selectedItem == null)
                throw new Exception("Edit profile button should not be accessible if there is no selected profile");

            ProfileNameField.Text = selectedItem.Name;
            ProfileAuthenticationMethod = selectedItem.AuthenticationMethod;
            ProfileUsernameField.Text = string.IsNullOrWhiteSpace(selectedItem.Username) ? "" : selectedItem.Username;
            ProfilePasswordField.Password = string.IsNullOrWhiteSpace(selectedItem.Password) ? "" : selectedItem.Password;
            ProfileTennantsField.SelectedIndex = -1;
            ProfileNotesField.Text = string.IsNullOrWhiteSpace(selectedItem.Notes) ? "" : selectedItem.Notes;

            if(selectedItem.ParentId == Guid.Empty)
            {
                ScopeGlobal.IsChecked = true;
                ProfileTennantsField.SelectedIndex = -1;
                Sites.RefreshFilter();

                return;
            }

            var parentTennant = Model.Context.Current.Tennants.SingleOrDefault(x => x.Id == selectedItem.ParentId);
            if (parentTennant != null)
            {
                ScopeTennant.IsChecked = true;
                ProfileTennantsField.SelectedItem = parentTennant;
                Sites.RefreshFilter();

                return;
            }

            var parentSite = Model.Context.Current.Sites.SingleOrDefault(x => x.Id == selectedItem.ParentId);
            if(parentSite != null)
            {
                parentTennant = Model.Context.Current.Tennants.Single(x => x.Id == parentSite.TennantId);

                ScopeSite.IsChecked = true;
                ProfileTennantsField.SelectedItem = parentTennant;
                Sites.RefreshFilter();
                ProfileSitesField.SelectedItem = parentSite;

                return;
            }
        }

        private void ProfileCancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ProfileDoneButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var authenticationMethod = ((AuthenticationMethod)ProfileAuthenticationMethodField.SelectedItem).Method;

            Guid parentId = Guid.Empty;
            if (ScopeTennant.IsChecked.Value)
                parentId = ((Model.Tennant)ProfileTennantsField.SelectedItem).Id;
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
                        Password = authenticationMethod == Model.EAuthenticationMethod.UsernamePassword ? ProfilePasswordField.Password : null,
                        Notes = ProfileNotesField.Text
                    }
                    );

                ProfileNameField.Text = "";
                ProfileAuthenticationMethod = Model.EAuthenticationMethod.UsernamePassword;
                ProfileUsernameField.Text = "";
                ProfilePasswordField.Password = "";
                ProfileTennantsField.SelectedIndex = -1;
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
                selectedItem.Password = authenticationMethod == Model.EAuthenticationMethod.UsernamePassword ? ProfilePasswordField.Password : null;
                selectedItem.Notes = ProfileNotesField.Text;

                return;
            }

            throw new Exception("Unhandled flyout mode");
        }
    }
}
