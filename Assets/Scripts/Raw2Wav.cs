using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class Raw2Wav
{
    public static void Convert(Stream from, Stream to, int bitrate)
    {
        using (BinaryWriter writer = new BinaryWriter(to))
        {
            writer.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
            writer.Write(36); //5
            writer.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);

            writer.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
            writer.Write(16);
            writer.Write((short) 3);
            writer.Write((short) 2);
            writer.Write(bitrate);
            writer.Write(bitrate*4*2);
            writer.Write((short) 8);
            writer.Write((short) 32);

            writer.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
            writer.Write(0); //40
            var length = CopyStream(from, writer);

            writer.Seek(4, SeekOrigin.Begin);
            writer.Write(36 + length);
            writer.Seek(40, SeekOrigin.Begin);
            writer.Write(length);
        }
    }

    public static void Convert(string from, string to, int bitrate)
    {
        using (var source = File.OpenRead(from))
        using (var dest = File.OpenWrite(to))
        {
            Convert(source, dest, bitrate);
        }
    }

    public static int CopyStream(Stream input, BinaryWriter output)
    {
        var total = 0;
        byte[] buffer = new byte[0x8000];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            total += read;
            output.Write(buffer, 0, read);
        }
        return total;
    }
}