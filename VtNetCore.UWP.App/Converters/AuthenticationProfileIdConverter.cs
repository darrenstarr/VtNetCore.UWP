namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    public class AuthenticationProfileIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Model.Context.Current.AuthenticationProfiles.SingleOrDefault(x => x.Id == (Guid)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return Guid.Empty;

            return (value as Model.AuthenticationProfile).Id;
        }
    }
}
