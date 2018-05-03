namespace VtNetCore.UWP.App.Utility.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CollectionTrySingle
    {
        // From https://github.com/dotnet/corefx/blob/master/src/System.Linq/src/System/Linq/Errors.cs
        internal static Exception ArgumentNull(string s) => new ArgumentNullException(s);

        public static bool TrySingle<TSource>(this IEnumerable<TSource> source, out TSource result)
        {
            if (source == null)
            {
                throw ArgumentNull(nameof(source));
            }

            result = source.SingleOrDefault();

            return result != null;
        }

        public static bool TrySingle<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, out TSource result)
        {
            if (source == null)
            {
                throw ArgumentNull(nameof(source));
            }

            result = source.SingleOrDefault(predicate);

            return result != null;
        }
    }
}
