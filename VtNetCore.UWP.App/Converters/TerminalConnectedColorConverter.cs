namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using VtNetCore.UWP.App.Utility.Helpers;
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

            if(!Terminals.Instance
                .Where(x => x.Connection.Destination.Equals(destinationUri))
                .TrySingle(out var terminal))
            {
                return terminal.IsConnected ? GreenBrush : RedBrush;
            }

            return RedBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
