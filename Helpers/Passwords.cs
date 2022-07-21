using System.Security.Cryptography;
using System.Text;

namespace BookInfo.Helpers;

public static class Password
{
    public static string GeneratePassword(int length) //length of salt    
    {
        const string allowedChars = "abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();
        char[] chars = new char[length];
        for (var i = 0; i <= length - 1; i++)
        {
            chars[i] = allowedChars[Convert.ToInt32((allowedChars.Length) * random.NextDouble())];
        }
        return new string(chars);
    }
    public static string EncodePassword(string pass, string salt) //encrypt password    
    {
        byte[] bytes = Encoding.Unicode.GetBytes(pass);
        byte[] src = Encoding.Unicode.GetBytes(salt);
        byte[] dst = new byte[src.Length + bytes.Length];
        System.Buffer.BlockCopy(src, 0, dst, 0, src.Length);
        System.Buffer.BlockCopy(bytes, 0, dst, src.Length, bytes.Length);
        HashAlgorithm algorithm = HashAlgorithm.Create("SHA256");
        byte[] inArray = algorithm.ComputeHash(dst);
        //return Convert.ToBase64String(inArray);    
        return EncodePasswordMd5(Convert.ToBase64String(inArray));
    }
    public static string EncodePasswordMd5(string pass) //Encrypt using MD5    
    {
        Byte[] originalBytes;
        Byte[] encodedBytes;
        MD5 md5;
        //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)    
        md5 = new MD5CryptoServiceProvider();
        originalBytes = ASCIIEncoding.Default.GetBytes(pass);
        encodedBytes = md5.ComputeHash(originalBytes);
        //Convert encoded bytes back to a 'readable' string    
        return BitConverter.ToString(encodedBytes);
    }
}