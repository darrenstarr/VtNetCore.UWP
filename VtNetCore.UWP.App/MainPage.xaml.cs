namespace VtNetCore.UWP.App
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Input;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            
        }

        private void OnUrlTapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ConnectTapped(object sender, TappedRoutedEventArgs e)
        {
            terminal.ConnectTo(Url.Text, Username.Text, Password.Password);
            terminal.Focus(FocusState.Programmatic);
        }

        private void DisconnectTapped(object sender, TappedRoutedEventArgs e)
        {
            terminal.Disconnect();
        }
    }
}
