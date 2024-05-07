using System;
using System.Text;

public static partial class Ollama
{
    /// <summary>
    /// A dumb way to not save in plain text.
    /// You should probably replace these if you're serious about security...
    /// </summary>
    private static class IO
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
    }
}
