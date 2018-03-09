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
        DispatcherTimer tickTock;

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

            if (tickTock == null)
            {
                tickTock = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(250)
                };
                tickTock.Tick += TickTock_Tick;
                tickTock.Start();
            }
        }

        int OldRawLength = 0;
        private void TickTock_Tick(object sender, object e)
        {
            if (OldRawLength != terminal.RawText.Length)
            {
                rawView.Text = terminal.RawText.ToString();
                OldRawLength = rawView.Text.Length;

                var grid = (Grid)Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(rawView, 0);
                for (var i = 0; i <= Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
                {
                    object obj = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(grid, i);
                    if (!(obj is ScrollViewer)) continue;
                    ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                    break;
                }
            }
        }

        private void DisconnectTapped(object sender, TappedRoutedEventArgs e)
        {
            terminal.Disconnect();
        }
    }
}
