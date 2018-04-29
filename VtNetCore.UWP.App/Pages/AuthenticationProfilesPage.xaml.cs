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

    public sealed partial class AuthenticationProfilesPage : Page
    {
        public AuthenticationProfilesViewModel AuthenticationProfiles { get; set; } = new AuthenticationProfilesViewModel();

        private enum EProfileFlyoutMode
        {
            Add,
            Edit
        }

        private bool IsLoaded { get; set; }
        private Model.Tenant NewProfileTenant { get; set; }
        private Model.Site NewProfileSite { get; set; }
        private ObservableCollection<Model.Tenant> Tenants { get => Model.Context.Current.Tenants; }
        private AdvancedCollectionView Sites { get; } = new AdvancedCollectionView(Model.Context.Current.Sites, true);

        public AuthenticationProfilesPage()
        {
            InitializeComponent();
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

        private void AddProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AuthenticationProfileForm.Operation = Controls.FormOperation.Add;
            AuthenticationProfileForm.ClearForm();
            AuthenticationProfileForm.Visibility = Visibility.Visible;
        }

        private void EditProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedProfile = (Model.AuthenticationProfile)AuthenticationProfilesView.SelectedItem;

            AuthenticationProfileForm.Operation = Controls.FormOperation.Edit;
            AuthenticationProfileForm.AuthenticationProfile = selectedProfile ?? throw new Exception("Edit authentication profile button should not be active when no profile is selected");
            AuthenticationProfileForm.Visibility = Visibility.Visible;
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

        private async void AuthenticationProfileForm_OnAuthenticationProfileChanged(object sender, Controls.AuthenticationProfilePropertiesForm.AuthenticationProfileChangedEventArgs e)
        {
            switch (e.Operation)
            {
                case Controls.FormOperation.Add:
                    if (e.AuthenticationProfile.AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword)
                        e.AuthenticationProfile.Password = "\u00FF" + await e.AuthenticationProfile.Password.Protect();

                    Model.Context.Current.AuthenticationProfiles.Add(e.AuthenticationProfile);

                    AuthenticationProfilesView.SelectedItem = e.AuthenticationProfile;
                    break;

                case Controls.FormOperation.Edit:
                    if (e.AuthenticationProfile.AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword)
                    {
                        if (!e.AuthenticationProfile.Password.StartsWith("\u00FF"))
                            e.AuthenticationProfile.Password = "\u00FF" + await e.AuthenticationProfile.Password.Protect();
                    }

                    await Model.Context.Current.SaveChanges(e.AuthenticationProfile);
                    AuthenticationProfiles.SaveChanges(e.AuthenticationProfile);

                    AuthenticationProfilesView.SelectedItem = e.AuthenticationProfile;

                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
    