namespace VtNetCore.UWP.App.Pages
{
    //using NiL.JS;
    //using NiL.JS.BaseLibrary;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Windows.ApplicationModel.Core;
    using Windows.System;
    using Windows.UI.Core;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Controls.Primitives;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Media.Animation;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //DispatcherTimer tickTock;

        TerminalPage terminalPage = new TerminalPage();

        public string CodeContent
        {
            get { return (string)GetValue(CodeContentProperty); }
            set { SetValue(CodeContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeContentProperty =
            DependencyProperty.Register("CodeContent", typeof(string), typeof(MainPage), new PropertyMetadata(""));

        public MainPage()
        {
            this.InitializeComponent();
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
        }

        private void NavView_Loaded(object sender, RoutedEventArgs e)
        {
            // set the initial SelectedItem 
            foreach (NavigationViewItemBase item in NavView.MenuItems)
            {
                if (item is NavigationViewItem && item.Tag.ToString() == "home")
                {
                    NavView.SelectedItem = item;
                    break;
                }
            }

            ContentFrame.Navigated += On_Navigated;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            if (ContentFrame.SourcePageType == typeof(SettingsPage))
            {
                NavView.SelectedItem = NavView.SettingsItem as NavigationViewItem;
            }
            else
            {
                Dictionary<Type, string> lookup = new Dictionary<Type, string>()
                {
                    {typeof(HomePage), "home"},
                    {typeof(TerminalPage), "terminals"},
                    {typeof(AuthenticationProfilesPage), "authenticationProfiles"},
                    {typeof(DeviceTypesPage), "deviceTypes" },
                    { typeof(SettingsPage), "settings"},
        };

                string stringTag = lookup[ContentFrame.SourcePageType];

                // set the new SelectedItem  
                foreach (NavigationViewItemBase item in NavView.MenuItems)
                {
                    if (item is NavigationViewItem && item.Tag.Equals(stringTag))
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void NavView_Navigate(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "home":
                    ContentFrame.Navigate(typeof(HomePage));
                    break;

                case "terminals":
                    ContentFrame.Navigate(typeof(TerminalPage));
                    break;

                case "authenticationProfiles":
                    ContentFrame.Navigate(typeof(AuthenticationProfilesPage));
                    break;

                case "deviceTypes":
                    ContentFrame.Navigate(typeof(DeviceTypesPage));
                    break;

                case "settings":
                    ContentFrame.Navigate(typeof(SettingsPage));
                    break;
            }
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                var item = sender.MenuItems.OfType<NavigationViewItem>().First(x => (string)x.Content == (string)args.InvokedItem);
                NavView_Navigate(item as NavigationViewItem);
            }
        }


        //private class Logger : NiL.JS.BaseLibrary.JSConsole
        //{
        //    private VirtualTerminalControl Terminal { get; set; }

        //    public class LoggerWriter : TextWriter
        //    {
        //        private Logger Parent { get; set; }

        //        public LoggerWriter(Logger parent)
        //        {
        //            Parent = parent;
        //        }

        //        public override Encoding Encoding
        //        {
        //            get
        //            {
        //                return Encoding.UTF8;
        //            }
        //        }

        //        public override void Write(char value)
        //        {
        //            Parent.Write(value);
        //        }
        //    }

        //    public void Write(char ch)
        //    {
        //        Terminal.PushText(ch.ToString());
        //    }

        //    public Logger(VirtualTerminalControl terminal)
        //    {
        //        Writer = new LoggerWriter(this);
        //        Terminal = terminal;
        //    }

        //    LoggerWriter Writer { get; set; }

        //    public override TextWriter GetLogger(LogLevel ll)
        //    {
        //        return Writer;
        //    }
        //}

        //private Logger nilJsLogger;

        //private void ScriptRunTapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (nilJsLogger == null)
        //        nilJsLogger = new Logger(javascriptConsole);

        //    if (ScriptTool.VtNetTerminals.Instance == null)
        //        ScriptTool.VtNetTerminals.Instance = new ScriptTool.VtNetTerminals(terminals);

        //    var scriptText = CodeContent;

        //    Task.Factory.StartNew(() =>
        //        {
        //            try
        //            {

        //                var context = new NiL.JS.Core.Context();
        //                context.DefineConstructor(typeof(ScriptTool.VtNetTerminal), "vtNetTerminal");
        //                context.DefineVariable("terminals").Assign(NiL.JS.Core.JSValue.Wrap(ScriptTool.VtNetTerminals.Instance));
        //                //context.DefineVariable("terminalZero").Assign(NiL.JS.Core.JSValue.Wrap(new ScriptTool.VtNetTerminal(terminal)));
        //                context.DefineVariable("console").Assign(NiL.JS.Core.JSValue.Wrap(nilJsLogger));

        //                context.DefineVariable("$").Assign(NiL.JS.Core.JSValue.Wrap(
        //                new
        //                {
        //                    sleep = new Action<int>(time => System.Threading.Thread.Sleep(time)),
        //                    threadId = new Func<int>(() => System.Threading.Thread.CurrentThread.ManagedThreadId),
        //                    delay = new Func<int, Task>((x) => Task.Delay(x))
        //                }));

        //                context.Eval(scriptText);
        //            }
        //            catch (Exception exc)
        //            {
        //                System.Diagnostics.Debug.WriteLine(exc.Message);
        //            }
        //        }
        //        ,
        //        TaskCreationOptions.LongRunning
        //    );
        //}

        //private void Context_DebuggerCallback(NiL.JS.Core.Context sender, NiL.JS.Core.DebuggerCallbackEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //private void ScriptRunTapped(object sender, TappedRoutedEventArgs e)
        //{
        //}

        private void ScriptStopTapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
