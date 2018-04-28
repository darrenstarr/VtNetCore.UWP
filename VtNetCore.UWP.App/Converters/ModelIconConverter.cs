namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media.Imaging;

    public class ModelIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var parentId = (Guid)value;

            if(parentId == Guid.Empty)
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/Tenant.png"));

            if (Model.Context.Current.Tenants.SingleOrDefault(x => x.Id == parentId) != null)
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/Tenant.png"));

            if (Model.Context.Current.Sites.SingleOrDefault(x => x.Id == parentId) != null)
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/Site.png"));

            if (Model.Context.Current.Devices.SingleOrDefault(x => x.Id == parentId) != null)
                return new BitmapImage(new Uri("ms-appx://VtNetCore.UWP.App/Assets/DeviceIcons/view2/QuestionMark.png"));

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
