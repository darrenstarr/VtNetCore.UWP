namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    public class DeviceClassGuidItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Model.DeviceClass.Classes.SingleOrDefault(x => x.Id == (Guid)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Guid.Empty;

            return ((Model.DeviceClass)value).Id;
        }
    }
}

