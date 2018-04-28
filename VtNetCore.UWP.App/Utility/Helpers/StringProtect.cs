namespace VtNetCore.UWP.App.Utility.Helpers
{
    using System;
    using System.Threading.Tasks;
    using Windows.Security.Cryptography;
    using Windows.Security.Cryptography.DataProtection;
    using Windows.Storage.Streams;

    public static class StringProtect
    {
        private static async Task<IBuffer> ProtectAsync(
            String strMsg,
            String strDescriptor,
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            var provider = new DataProtectionProvider(strDescriptor);

            // Encode the plaintext input message to a buffer.
            encoding = BinaryStringEncoding.Utf8;
            var buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Encrypt the message.
            var protectedBuffer = await provider.ProtectAsync(buffMsg);

            // Execution of the SampleProtectAsync function resumes here
            // after the awaited task (Provider.ProtectAsync) completes.
            return protectedBuffer;
        }

        private static async Task<String> UnprotectAsync(
            IBuffer buffProtected,
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8)
        {
            // Create a DataProtectionProvider object.
            var provider = new DataProtectionProvider();

            // Decrypt the protected message specified on input.
            var unprotectedBuffer = await provider.UnprotectAsync(buffProtected);

            // Execution of the SampleUnprotectData method resumes here
            // after the awaited task (Provider.UnprotectAsync) completes
            // Convert the unprotected message from an IBuffer object to a string.
            return CryptographicBuffer.ConvertBinaryToString(encoding, unprotectedBuffer);
        }

        public static async Task<string> Protect(this string source)
        {
            var resultAsBuffer = await ProtectAsync(source, "LOCAL=user");

            return CryptographicBuffer.EncodeToBase64String(resultAsBuffer);
        }

        public static async Task<string> Unprotect(this string source)
        {
            var buffer = CryptographicBuffer.DecodeFromBase64String(source);

            return await UnprotectAsync(buffer);
        }
    }
}
