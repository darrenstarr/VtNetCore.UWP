namespace VtNetCore.UWP.App.Pages
{
    using System.Collections.ObjectModel;
    using Windows.UI.Xaml.Controls;

    public sealed partial class TerminalPage : Page
    {
        private ObservableCollection<TerminalInstance> Terminals
        {
            get { return UWP.App.Terminals.Instance; }
        }

        public TerminalPage()
        {
            InitializeComponent();
        }
    }
}
