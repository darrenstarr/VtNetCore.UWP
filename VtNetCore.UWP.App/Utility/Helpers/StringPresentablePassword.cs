namespace VtNetCore.UWP.App.Utility.Helpers
{
    public static class StringPresentablePassword
    {
        public static string PresentablePassword(this string value)
        {
            var result = value.BlankIfNull();

            if (result.Length > 10 && result[0] == '\u00FF')
                return result.Substring(0, 10);

            return result;
        }
    }
}
