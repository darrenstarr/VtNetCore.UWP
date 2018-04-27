namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Collections.Generic;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media.Imaging;

    public class DeviceClassIconItemConverter : IValueConverter
    {
        internal static Dictionary<Guid, BitmapImage> Bitmaps = new Dictionary<Guid, BitmapImage>
        {
            {
                Model.DeviceClass.Router,
                new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/router.png"))
            },
            {
                Model.DeviceClass.Layer2Switch,
                new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/l2switch.png"))
            },
            {
                Model.DeviceClass.MultiLayerSwitch,
                new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/mls.png"))
            },
            {
                Model.DeviceClass.Workstation,
                new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view1/workstation.png"))
            },
            {
                Guid.Empty,
                new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/QuestionMark.png"))
            },
        };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Guid id = Guid.Empty;

            if (value is Guid)
                id = (Guid)value;
            else if (value is Model.DeviceClass)
                id = ((Model.DeviceClass)value).Id;
            else if (value is Model.DeviceType)
                id = ((Model.DeviceType)value).DeviceClassId;

            if (Bitmaps.TryGetValue(id, out BitmapImage result))
                return result;

            return Bitmaps[Guid.Empty];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

