namespace VtNetCore.UWP.App
{
    using System;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media.Animation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer tickTock;
        ScriptTool.ScriptTool scriptTool = new ScriptTool.ScriptTool();

        public MainPage()
        {
            this.InitializeComponent();

            Task.Run(async () =>
                {
                    await scriptTool.LoadConnectionProfiles();
                }
            );
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
            if (DateTime.Now.Subtract(terminal.TerminalIdleSince).TotalSeconds > 0.25)
            {
                var text = terminal.RawText;
                if (OldRawLength != text.Length)
                {
                    rawView.Text = text;
                    OldRawLength = text.Length;

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
        }

        private void DisconnectTapped(object sender, TappedRoutedEventArgs e)
        {
            terminal.Disconnect();
        }

        private void ToggleRawText(object sender, TappedRoutedEventArgs e)
        {
            if (HideRawView.GetCurrentState() == ClockState.Active || ShowRawView.GetCurrentState() == ClockState.Active)
                return;

            if (rawView.Visibility == Visibility.Visible)
                HideRawView.Begin();
            else
                ShowRawView.Begin();
        }

        private void Page_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            var controlPressed = (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down));
            if (e.Key == Windows.System.VirtualKey.F9 && controlPressed)
            {
                if (HideRawView.GetCurrentState() == ClockState.Active || ShowRawView.GetCurrentState() == ClockState.Active)
                    return;

                if (rawView.Visibility == Visibility.Visible)
                    HideRawView.Begin();
                else
                    ShowRawView.Begin();
            }
        }
    }
}
