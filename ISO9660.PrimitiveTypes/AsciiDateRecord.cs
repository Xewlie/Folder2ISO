namespace ISO9660.PrimitiveTypes;

internal class AsciiDateRecord
{
    public byte[]? DayOfMonth = "00"u8.ToArray();
    public byte[]? Hour = "00"u8.ToArray();
    public byte[]? HundredthsOfSecond = "00"u8.ToArray();
    public byte[]? Minute = "00"u8.ToArray();
    public byte[]? Month = "00"u8.ToArray();
    public byte[]? Second = "00"u8.ToArray();
    public byte[]? Year = "0000"u8.ToArray();
    
    public sbyte TimeZone;
}