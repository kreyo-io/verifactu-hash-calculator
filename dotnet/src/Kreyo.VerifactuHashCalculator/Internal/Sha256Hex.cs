using System;
using System.Security.Cryptography;
using System.Text;

#if NETSTANDARD2_0
using System.Linq;
#endif

namespace Kreyo.VerifactuHashCalculator.Internal;

internal static class Sha256Hex
{
    public static string Compute(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        
#if NETSTANDARD2_0
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(bytes);
        return string.Concat(hash.Select(b => b.ToString("X2")));
#else
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
#endif
    }
}
