namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    public class TenantIdItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is Guid))
                return null;

            var tenantId = (Guid)value;

            return Model.Context.Current.Tenants.SingleOrDefault(x => x.Id == tenantId);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (value == null) ? Guid.Empty : (value as Model.Tenant).Id;
        }
    }
}
