namespace VtNetCore.UWP.App.Pages
{
    using System;
    using System.Collections.ObjectModel;
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

        private ObservableCollection<Model.Tenant> Tenants { get => Model.Context.Current.Tenants; }

        public AuthenticationProfilesPage()
        {
            InitializeComponent();
        }

        private void AddProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetSelectionEnabled(false);

            AuthenticationProfileForm.Operation = Controls.FormOperation.Add;
            AuthenticationProfileForm.AuthenticationProfile = null;
            AuthenticationProfileForm.ClearForm();
            AuthenticationProfileForm.Visibility = Visibility.Visible;
            AuthenticationProfileForm.SetInitialFocus();
        }

        private void EditProfileButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedProfile = (Model.AuthenticationProfile)AuthenticationProfilesView.SelectedItem;

            SetSelectionEnabled(false);

            AuthenticationProfileForm.Operation = Controls.FormOperation.Edit;
            AuthenticationProfileForm.AuthenticationProfile = selectedProfile ?? throw new Exception("Edit authentication profile button should not be active when no profile is selected");
            AuthenticationProfileForm.Visibility = Visibility.Visible;
            AuthenticationProfileForm.SetInitialFocus();
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
                    EditProfileButton.IsEnabled = false;
                    RemoveProfileButton.IsEnabled = false;
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
                        e.AuthenticationProfile.Password = await e.AuthenticationProfile.Password.Protect();

                    Model.Context.Current.AuthenticationProfiles.Add(e.AuthenticationProfile);

                    SetSelectionEnabled(true);
                    AuthenticationProfilesView.SelectedItem = e.AuthenticationProfile;
                    break;

                case Controls.FormOperation.Edit:
                    if (e.AuthenticationProfile.AuthenticationMethod == Model.EAuthenticationMethod.UsernamePassword)
                    {
                        if (!e.AuthenticationProfile.Password.IsEncryptedPassword())
                            e.AuthenticationProfile.Password = await e.AuthenticationProfile.Password.Protect();
                    }

                    await Model.Context.Current.SaveChanges(e.AuthenticationProfile);
                    AuthenticationProfiles.SaveChanges(e.AuthenticationProfile);

                    SetSelectionEnabled(true);
                    AuthenticationProfilesView.SelectedItem = e.AuthenticationProfile;

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void FormCancelled(object sender, EventArgs e)
        {
            SetSelectionEnabled(true);
        }

        private void SetSelectionEnabled(bool enabled)
        {
            ProfilesCommandBar.IsEnabled = enabled;
            AuthenticationProfilesView.IsEnabled = enabled;
        }

        private void AuthenticationProfilesView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems == null)
            {
                EditProfileButton.IsEnabled = false;
                RemoveProfileButton.IsEnabled = false;
            }
            else
            {
                EditProfileButton.IsEnabled = true;
                RemoveProfileButton.IsEnabled = true;
            }
        }
    }
}
    