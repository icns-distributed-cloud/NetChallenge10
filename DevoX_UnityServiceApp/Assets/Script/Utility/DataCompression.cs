using System.IO;
using System.IO.Compression;

//This scirpt is Data compression. Can compress byte array data and decompress.
public static class DataCompression
{
    public static byte[] Decompress(byte[] data)
    {
        try
        {
            if (data == null)
            {
                return null;
            }
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }
        catch
        {
            return null;
        }
    
        return null;
    }

    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }
}
