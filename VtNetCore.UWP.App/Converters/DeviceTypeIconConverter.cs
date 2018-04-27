namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media.Imaging;

    public class DeviceTypeIconConverter : IValueConverter
    {
        private static BitmapImage IconOfLastResort = new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/QuestionMark.png"));

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var deviceType = Model.Context.Current.DeviceTypes.SingleOrDefault(x => x.Id == (Guid)value);

            var deviceClassId = Guid.Empty;
            if (deviceType != null)
                deviceClassId = deviceType.DeviceClassId;

            if (DeviceClassIconItemConverter.Bitmaps.TryGetValue(deviceClassId, out BitmapImage result))
                return result;

            return IconOfLastResort;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
