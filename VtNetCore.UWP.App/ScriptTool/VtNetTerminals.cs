using NiL.JS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace VtNetCore.UWP.App.ScriptTool
{
    public class VtNetTerminals
    {
        public List<VtNetTerminal> Terminals = new List<VtNetTerminal>();

        public Pivot TerminalContainer { get; set; }

        public static VtNetTerminals Instance { get; set; }

        internal static void RegisterTerminal(VtNetTerminal vtNetTerminal)
        {
            Instance.EmbedTerminalInObject(vtNetTerminal);
        }

        internal void EmbedTerminalInObject(VtNetTerminal vtNetTerminal)
        {
            ManualResetEvent reset = new ManualResetEvent(false);

            var t = Task.Factory.StartNew(() =>
                TerminalContainer.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    //UI code here
                    var newTerminal = new VirtualTerminalControl();
                    var newPivotItem = new PivotItem { Content = newTerminal };
                    var headingBinding = new Binding { Source = newTerminal, Path = new Windows.UI.Xaml.PropertyPath("WindowTitle") };
                    newPivotItem.SetBinding(PivotItem.HeaderProperty, headingBinding);

                    TerminalContainer.Items.Add(newPivotItem);

                    var result = new VtNetTerminal(newTerminal);

                    vtNetTerminal.Terminal = newTerminal;
                    lock (Terminals)
                    {
                        Terminals.Add(vtNetTerminal);
                    }

                    reset.Set();
                })
            );

            t.Wait();
            reset.WaitOne();
            return;
        }

        public VtNetTerminals(Pivot terminalContainer)
        {
            if (Instance != null)
                throw new Exception("Application can only have a single VtNetTerminals instance");

            Instance = this;

            TerminalContainer = terminalContainer;
        }

        public VtNetTerminal getByName(string key)
        {
            lock (Terminals)
            {
                return Terminals.SingleOrDefault(x => x.Name == key);
            }
        }
    }
}
