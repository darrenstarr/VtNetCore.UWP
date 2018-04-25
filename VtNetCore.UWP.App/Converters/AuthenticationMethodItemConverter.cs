﻿namespace VtNetCore.UWP.App.Converters
{
    using System;
    using System.Linq;
    using Windows.UI.Xaml.Data;

    public class AuthenticationMethodItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Model.EAuthenticationMethod)
            {
                var result = AuthenticationMethod.DeviceAuthenticationMethods.SingleOrDefault(x => x.Method == ((Model.EAuthenticationMethod)value));
                if (result == null)
                    throw new ArgumentOutOfRangeException("value", "Unknown authentication method");

                return result;
            }

            throw new ArgumentException("value", "Value must be of type EAuthenticationMethod");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return null;
            
            return (value as AuthenticationMethod).Method;
        }
    }
}