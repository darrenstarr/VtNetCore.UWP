namespace VtNetCore.UWP.App.Utility.Helpers
{
    public static class StringIsEncryptedPassword
    {
        public static bool IsEncryptedPassword(this string value)
        {
            return value.BlankIfNull().Length > 0 && value[0] == '\u00FF';
        }
    }
}
