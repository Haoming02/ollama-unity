using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// A dumb way to not save in plain text <br/>
/// You should probably replace these if security is critical...
/// </summary>
public static class IO
{
    private const byte shift = 1;

    public static string Encrypt(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)((bytes[i] + shift) % 256);
        return Convert.ToBase64String(bytes);
    }

    public static string Decrypt(string input)
    {
        byte[] bytes = Convert.FromBase64String(input);
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = (byte)((bytes[i] - shift + 256) % 256);
        return Encoding.UTF8.GetString(bytes);
    }

    public static string Hash(string input)
    {
        using MD5 md5 = MD5.Create();
        return string.Concat(md5.ComputeHash(Encoding.UTF8.GetBytes(input)).Select(x => x.ToString("X2")));
    }
}
