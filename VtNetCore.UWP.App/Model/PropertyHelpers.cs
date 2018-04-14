namespace VtNetCore.UWP.App.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;

    public static class PropertyHelpers
    {
        public static bool ChangeAndNotify<T>
            (
                this PropertyChangedEventHandler handler,
                ref T field,
                T value,
                Expression<Func<T>> memberExpression
            )
        {
            if (memberExpression == null)
                throw new ArgumentNullException("memberExpression");

            var body = memberExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("Lambda must return a property.");

            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;

            if (body.Expression is ConstantExpression vmExpression)
            {
                LambdaExpression lambda = Expression.Lambda(vmExpression);
                Delegate vmFunc = lambda.Compile();
                object sender = vmFunc.DynamicInvoke();

                handler?.Invoke(sender, new PropertyChangedEventArgs(body.Member.Name));
            }

            return true;
        }
    }
}
