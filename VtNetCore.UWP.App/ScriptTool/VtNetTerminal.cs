namespace VtNetCore.UWP.App.ScriptTool
{
    using NiL.JS.Core;
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public class VtNetTerminal 
    {
        public VirtualTerminalControl Terminal { get; set; }

        public string Name { get; set; }

        public VtNetTerminal(string name)
        {
            Name = name;
            VtNetTerminals.RegisterTerminal(this);
        }

        public VtNetTerminal(VirtualTerminalControl terminal)
        {
            Terminal = terminal;
        }

        public bool connected { get { return Terminal.Connected; } }

        public bool connect(Arguments arguments)
        {
            if (arguments.Length > 0)
                return Terminal.ConnectTo(arguments[0].Value.ToString(), "admin", "Minions12345");

            return Terminal.ConnectTo("ssh://10.100.5.100", "admin", "Minions12345");
        }

        public void disconnect(Arguments arguments)
        {
            Terminal.Disconnect();
        }

        public string rawText
        {
            get
            {
                return Terminal.RawText;
            }
        }

        public Task<string> awaitText(string text)
        {
            return awaitText(text, 10000);
        }

        public async Task<string> awaitText(string text, int timeoutMs)
        {
            var timeoutAt = DateTime.Now.Add(TimeSpan.FromMilliseconds(timeoutMs));

            var ex = new Regex(text);
            while (!ex.IsMatch(rawText))
            {
                if (DateTime.Now > timeoutAt)
                    throw new JSException(new NiL.JS.BaseLibrary.Error("awaitText operation timed out"));

                await Task.Delay(100);
            }

            return rawText;
        }

        public Task wait(int ms)
        {
            return Task.Delay(ms);
        }

        public bool send(string text)
        {
            try
            {
                Terminal.ClearRawText();
                Terminal.SendData(Encoding.UTF8.GetBytes(text));
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void clearRaw()
        {
            Terminal.ClearRawText();
        }
    }
}
