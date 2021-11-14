using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class Md5
{
    public static string HashFile(string path)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(path))
        {
            return ByteArrayToString(md5.ComputeHash(stream));
        }
    }

    public static string TryHashFile(string path)
    {
        try
        {
            return HashFile(path);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string ByteArrayToString(byte[] bytes)
    {
        var result = new StringBuilder(bytes.Length * 2);
        var hexAlphabet = "0123456789ABCDEF";
        foreach (byte b in bytes)
        {
            result.Append(hexAlphabet[b >> 4]);
            result.Append(hexAlphabet[b & 0xF]);
        }
        return result.ToString();
    }
}