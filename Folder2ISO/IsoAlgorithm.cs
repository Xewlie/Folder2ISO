using System.Text;

namespace Folder2ISO;

internal static class IsoAlgorithm
{
    // Class containing various ISO algorithm utility functions.
    
    public static uint SectorSize => 2048u;
    public static byte DefaultDirectoryRecordLength => 34;
    public static byte AsciiBlank => 32;
    private static byte[] UnicodeBlank => new byte[] { 0, AsciiBlank };
    public static int SystemIdLength => 32;
    public static int VolumeIdLength => 32;
    public static int VolumeSetIdLength => 128;
    public static int PublisherIdLength => 128;
    public static int PreparerIdLength => 128;
    public static int ApplicationIdLength => 128;
    public static int CopyrightFileIdLength => 37;
    public static int AbstractFileIdLength => 37;
    public static int BibliographicFileIdLength => 37;
    public static DateTime NoDate => new(1900, 1, 1, 0, 0, 0, 0);
    public static int FileNameMaxLength => 101;

    // Convert single endian value to both endian by replicating bytes at MSB.
    public static ulong BothEndian(uint value)
    {
        const ulong mask0 = 4278190080uL;
        const ulong mask1 = 16711680uL;
        const ulong mask2 = 65280uL;
        const ulong mask3 = 255uL;
        return value | ((value & mask0) << 8) | ((value & mask1) << 24) | ((value & mask2) << 40) | ((value & mask3) << 56);
    }
    
    public static byte[] MemSet(int count, byte value)
    {
        var array = new byte[count];
        for (var i = 0; i < count; i++) array[i] = value;
        return array;
    }
    
    private static byte[] MemSet(int count, byte[] value)
    {
        var array = new byte[count * value.Length];
        for (var i = 0; i < count; i++)
        {
            value.CopyTo(array, i * value.Length);
        }
        
        return array;
    }
    
    private static byte[] AsciiToUnicode(string asciiText)
    {
        // Convert ASCII strings to Big Endian Unicode byte arrays.
        var memoryStream = new MemoryStream();
        var binaryWriter = new BinaryWriter(memoryStream, Encoding.BigEndianUnicode);
        binaryWriter.Write(asciiText);
        binaryWriter.Close();
        var buffer = memoryStream.GetBuffer();
        var array = new byte[asciiText.Length * 2];
        for (var i = 0; i < array.Length && i + 1 < buffer.Length; i++)
        {
            array[i] = buffer[i + 1];
        }
        
        return array;
    }

    // Convert ASCII strings to Big Endian Unicode byte arrays with a specified size.
    public static byte[] AsciiToUnicode(string asciiText, int size)
    {
        var array = AsciiToUnicode(asciiText);
        var array2 = MemSet(size / 2, UnicodeBlank);
        
        if (size % 2 == 1)
        {
            Array.Resize(ref array2, array2.Length + 1);
        }
        
        Array.Copy(array, array2, Math.Min(size, array.Length));
        
        if (array.Length < size - 2)
        {
            array2[array.Length] = array2[array.Length + 1] = 0;
        }
        
        return array2;
    }

    // Convert byte arrays representing ASCII text to Big Endian Unicode byte arrays.
    public static byte[] AsciiToUnicode(byte[]? asciiText)
    {
        var memoryStream = new MemoryStream();
        var binaryWriter = new BinaryWriter(memoryStream, Encoding.BigEndianUnicode);
        
        foreach (var t in asciiText!)
        {
            binaryWriter.Write((char)t);
        }

        binaryWriter.Close();
        var buffer = memoryStream.GetBuffer();
        var array = new byte[asciiText.Length * 2];
        Array.Copy(buffer, array, Math.Min(array.Length, buffer.Length));
        
        return array;
    }

    // Convert byte arrays representing ASCII text to Big Endian Unicode byte arrays with a specified size.
    public static byte[] AsciiToUnicode(byte[]? asciiText, int size)
    {
        var array = AsciiToUnicode(asciiText);
        var array2 = MemSet(size / 2, UnicodeBlank);
        if (size % 2 == 1)
        {
            Array.Resize(ref array2, array2.Length + 1);
        }
        Array.Copy(array, array2, Math.Min(array2.Length, array.Length));
        return array2;
    }

    // Convert byte arrays representing Big Endian Unicode text to ASCII byte arrays.
    public static byte[] UnicodeToAscii(byte[]? unicodeText)
    {
        var array = new byte[unicodeText!.Length / 2];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = unicodeText[i * 2];
        }
        
        return array;
    }

    // Convert byte arrays representing Big Endian Unicode text to ASCII byte arrays with a specified size.
    public static byte[] UnicodeToAscii(byte[]? unicodeText, int size)
    {
        var array = MemSet(size, AsciiBlank);
        for (var i = 0; i < array.Length && i < unicodeText!.Length / 2; i++)
        {
            array[i] = unicodeText[i * 2];
        }
        
        return array;
    }

    // Convert a string to a byte array.
    public static byte[] StringToByteArray(string text)
    {
        var array = new byte[text.Length];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = (byte)text[i];
        }
        
        return array;
    }

    // Convert a string to a byte array with a specified size.
    public static byte[] StringToByteArray(string text, int size)
    {
        var array = StringToByteArray(text);
        var array2 = MemSet(size, AsciiBlank);
        Array.Copy(array, array2, Math.Min(array2.Length, array.Length));
        
        return array2;
    }

    // Convert a byte array to a string.
    public static string ByteArrayToString(byte[]? array)
    {
        var charArray = new char[array!.Length];
        for (var i = 0; i < charArray.Length; i++)
        {
            charArray[i] = (char)array[i];
        }
        
        return new string(charArray);
    }

    // Reverse byte endianness of a 32-bit unsigned integer.
    public static uint ChangeEndian(uint value)
    {
        const uint mask0 = 4278190080u;
        const uint mask1 = 16711680u;
        const uint mask2 = 65280u;
        const uint mask3 = 255u;
        
        return ((value & mask0) >> 24) | ((value & mask1) >> 8) | ((value & mask2) << 8) | ((value & mask3) << 24);
    }

    // Reverse byte endianness of a 16-bit unsigned integer.
    public static ushort ChangeEndian(ushort value)
    {
        return (ushort)((value >> 8) | (ushort)((value & 0xFF) << 8));
    }
}