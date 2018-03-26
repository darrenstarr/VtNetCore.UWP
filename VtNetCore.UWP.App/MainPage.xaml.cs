namespace VtNetCore.UWP.App
{
    using NiL.JS;
    using NiL.JS.BaseLibrary;
    using System;
    using System.IO;
    using System.Text;
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
        //DispatcherTimer tickTock;

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
        }

        private void OnUrlTapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void ConnectTapped(object sender, TappedRoutedEventArgs e)
        {
            //terminal.ConnectTo(Url.Text, Username.Text, Password.Password);
            //terminal.Focus(FocusState.Programmatic);

            //if (tickTock == null)
            //{
            //    tickTock = new DispatcherTimer
            //    {
            //        Interval = TimeSpan.FromMilliseconds(250)
            //    };
            //    tickTock.Tick += TickTock_Tick;
            //    tickTock.Start();
            //}
        }

        //int OldRawLength = 0;
        private void TickTock_Tick(object sender, object e)
        {
            //if (DateTime.Now.Subtract(terminal.TerminalIdleSince).TotalSeconds > 0.25)
            //{
            //    var text = terminal.RawText;
            //    if (OldRawLength != text.Length)
            //    {
            //        rawView.Text = text;
            //        OldRawLength = text.Length;

            //        var grid = (Grid)Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(rawView, 0);
            //        for (var i = 0; i <= Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            //        {
            //            object obj = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(grid, i);
            //            if (!(obj is ScrollViewer)) continue;
            //            ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
            //            break;
            //        }
            //    }
            //}
        }

        private void DisconnectTapped(object sender, TappedRoutedEventArgs e)
        {
            //terminal.Disconnect();
        }

        private void Editor_KeyDown(Monaco.CodeEditor sender, Monaco.Helpers.WebKeyEventArgs args)
        {

        }

        private class Logger : NiL.JS.BaseLibrary.JSConsole
        {
            private VirtualTerminalControl Terminal { get; set; }

            public class LoggerWriter : TextWriter
            {
                private Logger Parent { get; set; }

                public LoggerWriter(Logger parent)
                {
                    Parent = parent;
                }

                public override Encoding Encoding
                {
                    get
                    {
                        return Encoding.UTF8;
                    }
                }

                public override void Write(char value)
                {
                    Parent.Write(value);
                }
            }

            public void Write(char ch)
            {
                Terminal.PushText(ch.ToString());
            }

            public Logger(VirtualTerminalControl terminal)
            {
                Writer = new LoggerWriter(this);
                Terminal = terminal;
            }

            LoggerWriter Writer { get; set; }

            public override TextWriter GetLogger(LogLevel ll)
            {
                return Writer;
            }
        }

        private Logger nilJsLogger;

        private void ScriptRunTapped(object sender, TappedRoutedEventArgs e)
        {
            if (nilJsLogger == null)
                nilJsLogger = new Logger(javascriptConsole);

            if (ScriptTool.VtNetTerminals.Instance == null)
                ScriptTool.VtNetTerminals.Instance = new ScriptTool.VtNetTerminals(terminals);

            var scriptText = CodeContent;

            Task.Factory.StartNew(() =>
                {
                    try
                    {

                        var context = new NiL.JS.Core.Context();
                        context.DefineConstructor(typeof(ScriptTool.VtNetTerminal), "vtNetTerminal");
                        context.DefineVariable("terminals").Assign(NiL.JS.Core.JSValue.Wrap(ScriptTool.VtNetTerminals.Instance));
                        //context.DefineVariable("terminalZero").Assign(NiL.JS.Core.JSValue.Wrap(new ScriptTool.VtNetTerminal(terminal)));
                        context.DefineVariable("console").Assign(NiL.JS.Core.JSValue.Wrap(nilJsLogger));

                        context.DefineVariable("$").Assign(NiL.JS.Core.JSValue.Wrap(
                        new
                        {
                            sleep = new Action<int>(time => System.Threading.Thread.Sleep(time)),
                            threadId = new Func<int>(() => System.Threading.Thread.CurrentThread.ManagedThreadId),
                            delay = new Func<int, Task>((x) => Task.Delay(x))
                        }));

                        context.Eval(scriptText);
                    }
                    catch (Exception exc)
                    {
                        System.Diagnostics.Debug.WriteLine(exc.Message);
                    }
                }
                ,
                TaskCreationOptions.LongRunning
            );
        }

        private void Context_DebuggerCallback(NiL.JS.Core.Context sender, NiL.JS.Core.DebuggerCallbackEventArgs e)
        {
            throw new NotImplementedException();
        }

        //private void ScriptRunTapped(object sender, TappedRoutedEventArgs e)
        //{
        //}

        private void ScriptStopTapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
