using System.Security.Cryptography;

namespace CCXT.Simple.Core.Extensions
{
    public static class SHA256Extensions
    {
        public static byte[] HashDataCompat(byte[] data)
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
#else
        return SHA256.HashData(data);
#endif
        }
    }
}