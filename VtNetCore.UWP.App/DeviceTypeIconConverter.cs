namespace VtNetCore.UWP.App
{
    using System;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media.Imaging;

    public class DeviceTypeIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var deviceType = value.ToString().ToLowerInvariant();

            if(deviceType.StartsWith("router"))
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/router.png"));

            if (deviceType.StartsWith("switch"))
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/mls.png"));

            if (deviceType.StartsWith("workstation"))
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/workstation.png"));

            return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/QuestionMark.png"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
