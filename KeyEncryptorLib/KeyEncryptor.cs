using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;

namespace KeyEncryptorLib
{
    public static class KeyEncryptor
    {
        [SupportedOSPlatform("windows")]
        public static string Crypt(string text)
        {
            return Convert.ToBase64String(
                ProtectedData.Protect(
                    Encoding.Unicode.GetBytes(text),
                    optionalEntropy: Encoding.Unicode.GetBytes("LabelServiceConnector"),
                    scope: DataProtectionScope.LocalMachine));
        }

        [SupportedOSPlatform("windows")]
        public static string Decrypt(string text)
        {
            return Encoding.Unicode.GetString(
                ProtectedData.Unprotect(                    
                    Convert.FromBase64String(text),
                    optionalEntropy: Encoding.Unicode.GetBytes("LabelServiceConnector"),
                    scope: DataProtectionScope.LocalMachine));
        }
    }
}