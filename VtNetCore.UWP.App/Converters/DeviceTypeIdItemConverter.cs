namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    public class DeviceTypeIdItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((Guid)value == Guid.Empty)
                return null;

            return Model.Context.Current.DeviceTypes.Single(x => x.Id == (Guid)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((Model.DeviceType)value).Id;
        }
    }
}
