using Folder2ISO;

namespace ISO9660.PrimitiveTypes;

internal class VolumeDescriptor
{
    // Represents a volume descriptor in the ISO 9660 file system format.
    
    public const byte FileStructureVersion = 1; // File structure version
    public const uint VolumeSetSize = 16777217u; // Volume set size
    public const uint VolumeSequenceNumber = 16777217u; // Volume sequence number
    public const uint SectorkSize = 526336u; // Sector size
    public readonly byte[] ApplicationData = new byte[512]; // Application data
    public readonly byte[] Reserved3_1 = "%/E"u8.ToArray(); // Reserved 3_1
    public readonly byte[] Reserved3_2 = new byte[29]; // Reserved 3_2
    public readonly byte[] Reserved5 = new byte[653]; // Reserved 5
    public readonly byte[] StandardIdentifier = { 67, 68, 48, 48, 49, 1 }; // Standard identifier (CD001)

    public byte[]? AbstractFileId = IsoAlgorithm.MemSet(IsoAlgorithm.AbstractFileIdLength, IsoAlgorithm.AsciiBlank); // Abstract file identifier
    public byte[]? ApplicationId = IsoAlgorithm.MemSet(IsoAlgorithm.ApplicationIdLength, IsoAlgorithm.AsciiBlank); // Application identifier
    public byte[]? BibliographicFileId = IsoAlgorithm.MemSet(IsoAlgorithm.BibliographicFileIdLength, IsoAlgorithm.AsciiBlank); // Bibliographic file identifier

    // ID related attributes
    public byte[]? CopyrightFileId = IsoAlgorithm.MemSet(IsoAlgorithm.CopyrightFileIdLength, IsoAlgorithm.AsciiBlank); // Copyright file identifier
    public uint OptionalTypeLPathTable; // Optional Type L path table
    public uint OptionalTypeMPathTable; // Optional Type M path table
    public ulong PathTableSize; // Path table size
    public byte[]? PreparerId = IsoAlgorithm.MemSet(IsoAlgorithm.PreparerIdLength, IsoAlgorithm.AsciiBlank); // Preparer identifier
    public byte[]? PublisherId = IsoAlgorithm.MemSet(IsoAlgorithm.PublisherIdLength, IsoAlgorithm.AsciiBlank); // Publisher identifier

    // Reserved attributes
    public byte Reserved1;
    public ulong Reserved2;
    public byte Reserved4;
    
    // Volume ID related attributes
    public byte[]? SystemId = IsoAlgorithm.MemSet(IsoAlgorithm.SystemIdLength, IsoAlgorithm.AsciiBlank); // System identifier
    public uint TypeLPathTable; // Type L path table
    public uint TypeMPathTable; // Type M path table
    public byte VolumeDescType; // Volume descriptor type
    public byte[]? VolumeId = IsoAlgorithm.MemSet(IsoAlgorithm.VolumeIdLength, IsoAlgorithm.AsciiBlank); // Volume identifier
    public byte[]? VolumeSetId = IsoAlgorithm.MemSet(IsoAlgorithm.VolumeSetIdLength, IsoAlgorithm.AsciiBlank); // Volume set identifier
    public ulong VolumeSpaceSize; // Volume space size
}