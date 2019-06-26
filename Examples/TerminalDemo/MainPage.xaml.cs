using System;
using System.Threading.Tasks;
using VtConnect;
using VtNetCore.VirtualTerminal;
using VtNetCore.XTermParser;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TerminalDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DataConsumer terminalStream;
        private readonly VirtualTerminalController terminalController = new VirtualTerminalController();
        private Connection connection;

        public MainPage()
        {
            InitializeComponent();

            terminal.Terminal = terminalController;
            terminalController.SendData += SendDataEvent;
            terminalController.SizeChanged += TerminalSizeChanged;
            terminalController.WindowTitleChanged += OnWindowTitleChanged;

            terminalController.OnLog += OnLog;

            terminalStream = new DataConsumer(terminal.Terminal);
        }

        private void OnLog(object sender, TextEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.Text);
        }

        private void OnWindowTitleChanged(object sender, TextEventArgs e)
        {
            Task.Factory.StartNew(async () =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    () =>
                    {
                        windowTitle.Text = e.Text;
                    }
                );
            });
        }

        private void TerminalSizeChanged(object sender, SizeEventArgs e)
        {
            connection?.SetTerminalWindowSize(e.Width, e.Height, 800, 600);
        }

        private void SendDataEvent(object sender, SendDataEventArgs e)
        {
            try
            {
                connection.SendData(e.Data);
            }
            catch
            {
                OnConnectionClosed();
            }
        }

        private void OnDataReceived(object sender, VtConnect.DataReceivedEventArgs e)
        {
            terminalStream.Push(e.Data);
        }

        private void ConnectButton_Click(object sender, TappedRoutedEventArgs e)
        {
            var uri = new Uri($"ssh://{server.Text}");
            connection = Connection.CreateConnection(uri);
            connection.SetTerminalWindowSize(terminal.Terminal.VisibleColumns, terminal.Terminal.VisibleRows, 800, 600);
            connection.DataReceived += OnDataReceived;
            connection.PropertyChanged += Connection_PropertyChanged;

            if (connection.Connect(uri, new UsernamePasswordCredentials { Username = username.Text, Password = password.Password }))
            {
                server.IsEnabled = false;
                username.IsEnabled = false;
                password.IsEnabled = false;

                connectButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                connection = null;
            }
        }

        private void Connection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!connection.IsConnected)
                OnConnectionClosed();
        }

        private void OnConnectionClosed()
        {
            connection = null;
            windowTitle.Text = "Disconnected";

            server.IsEnabled = true;
            username.IsEnabled = true;
            password.IsEnabled = true;

            connectButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void DisconnectButton_Click(object sender, TappedRoutedEventArgs e)
        {
            connection.Disconnect();
            connection = null;
            windowTitle.Text = "";

            server.IsEnabled = true;
            username.IsEnabled = true;
            password.IsEnabled = true;

            connectButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
            disconnectButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void Logging_Checked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            terminalController.Debugging = logging.IsChecked.Value;
        }
    }
}
