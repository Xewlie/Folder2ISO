namespace ISO9660.PrimitiveTypes;

internal class PathTableRecord
{
    public byte ExtendedLength;
    public uint ExtentLocation;
    public byte[]? Identifier;
    public byte Length;
    public ushort ParentNumber;
}