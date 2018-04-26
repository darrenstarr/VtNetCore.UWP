namespace VtNetCore.UWP.App.Utility.Helpers
{
    public static class StringBlankIfNull
    {
        public static string BlankIfNull(this string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value;
        }
    }
}
