using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.Helpers;

public static class Hashing
{
    public static string ComputeHash(string content)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha.ComputeHash(bytes);
        return Convert.ToHexString(hashBytes);
    }
}