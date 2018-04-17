namespace VtNetCore.UWP.App
{
    using VtNetCore.UWP.App.ViewModel.AuthenticationProfilesViewModel;
    using Windows.UI.Xaml.Controls;

    public sealed partial class AuthenticationProfilesPage : Page
    {
        public AuthenticationProfilesViewModel AuthenticationProfiles { get; set; } = new AuthenticationProfilesViewModel();

        public AuthenticationProfilesPage()
        {
            this.InitializeComponent();
        }
    }
}
