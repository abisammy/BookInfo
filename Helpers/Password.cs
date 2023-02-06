using System.Security.Cryptography;

namespace BookInfo.Helpers;

public static class Password
{
    public static string HashString(string input)
    {
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(input);

        SHA256 hasher = SHA256.Create();
        byte[] outputBytes = hasher.ComputeHash(inputBytes);

        // By default this separates bytes with dashes, therefore they need to be removed
        return BitConverter.ToString(outputBytes).Replace("-", "");
    }
}