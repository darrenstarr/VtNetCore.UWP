namespace VtNetCore.UWP.App.Converters
{
    using System;
    using Windows.UI.Xaml.Data;

    public class SiteItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as Model.Site;
        }
    }
}
