namespace VtNetCore.UWP.App
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using VtConnect;
    using VtNetCore.VirtualTerminal;
    using VtNetCore.XTermParser;

    public class TerminalInstance
    {
        public VirtualTerminalController Terminal { get; private set; }
        public DataConsumer Consumer { get; set; }
        public Connection Connection { get; set; }

        public EventHandler ContentChanged;

        public string Title
        {
            get;
            set;
        }

        public TerminalInstance()
        {
            Terminal = new VirtualTerminalController();
            Consumer = new DataConsumer(Terminal);
            Title = "New";

            Terminal.SendData += OnSendData;
            Terminal.SizeChanged += OnSizeChanged;
        }

        public bool ConnectTo(Uri uri, string userName, string password)
        {
            if (Connection != null)
                Connection.Disconnect();

            var credentials = new UsernamePasswordCredentials
            {
                Username = userName,
                Password = password
            };

            Connection = Connection.CreateConnection(uri);
            Connection.DataReceived += OnDataReceived;

            return Connection.Connect(uri, credentials);
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            bool changed = false;
            lock(Terminal)
            {
                Consumer.Push(e.Data);
                changed = Terminal.Changed;
            }

            if(changed)
            {
                if (ContentChanged != null)
                    ContentChanged.Invoke(this, new EventArgs());

                Terminal.ClearChanges();
            }
        }

        private void OnSizeChanged(object sender, SizeEventArgs e)
        {
            if (Connection != null && Connection.IsConnected)
            {
                Task.Run(() =>
                {
                    // TODO : Make queuing work. There is a change this could cause problems
                    Connection.SetTerminalWindowSize(e.Width, e.Height, 800, 600);
                });
            }
        }

        private void OnSendData(object sender, SendDataEventArgs e)
        {
            if(Connection != null && Connection.IsConnected)
            {
                Task.Run(() =>
                {
                    // TODO : Make queuing work. There is a change this could cause problems
                    Connection.SendData(e.Data);
                });
            }
        }
    }

    public class Terminals : ObservableCollection<TerminalInstance>
    {
        private static Terminals _instance;
        public static Terminals Instance
        {
            get
            {
                if(_instance == null)
                    _instance = new Terminals();

                return _instance;
            }
        }

        private Terminals()
        {
        }
    }
}
