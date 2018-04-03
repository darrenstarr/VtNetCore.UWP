using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace VtNetCore.UWP.App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TerminalPage : Page
    {
        private ObservableCollection<TerminalInstance> Terminals
        {
            get { return UWP.App.Terminals.Instance; }
        }

        public TerminalPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //foreach (var item in Terminals)
            //{
            //    var newPivotItem = new PivotItem();
            //    pivot.Items.Add(newPivotItem);

            //    var control = new VirtualTerminalControl();
            //    control.Consumer = item.Consumer;
            //    control.Terminal = item.Terminal;

            //    newPivotItem.Header = control.WindowTitle ?? item.Connection.Destination.ToString();

            //    newPivotItem.Content = control;
            //    control.PropertyChanged +=
            //        (s, args) =>
            //        {
            //            if (args.PropertyName == "WindowTitle")
            //                newPivotItem.Header = control.WindowTitle ?? item.Connection.Destination.ToString();
            //        };
            //}
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
