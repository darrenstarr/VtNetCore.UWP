namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;

    public class TerminalConnectedColorConverter : IValueConverter
    {
        public static Brush RedBrush = new SolidColorBrush(Colors.Red);
        public static Brush GreenBrush = new SolidColorBrush(Colors.Green);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is bool boolValue)
            {
                return boolValue ? GreenBrush : RedBrush;
            }

            var id = (Guid)value;

            var device = Model.Context.Current.Devices
                .Where(x => x.Id == id)
                .Single();

            var destinationUri = new Uri(device.Destination);

            var terminal = Terminals.Instance
                .Where(x => x.Connection.Destination.Equals(destinationUri))
                .SingleOrDefault();

            if (terminal == null)
                return RedBrush;

            return terminal.IsConnected ? GreenBrush : RedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
