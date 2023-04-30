using Folder2ISO;

namespace ISO9660.PrimitiveTypes;

internal class DirectoryRecord
{
    public const uint VolumeSequnceNumber = 16777217u;
    
    public ulong DataLength;
    
    public BinaryDateRecord? Date = new();

    public ulong ExtentLocation;
    
    public byte[]? FileIdentifier;
    
    public byte ExtendedAttributeLength;
    public byte FileFlags;
    public byte FileUnitSize;
    public byte InterleaveGapSize;
    public byte Length = IsoAlgorithm.DefaultDirectoryRecordLength;
    public byte LengthOfFileIdentifier;
    
    public sbyte TimeZone;
}