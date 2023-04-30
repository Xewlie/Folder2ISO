using ISO9660.Enums;
using ISO9660.PrimitiveTypes;

namespace Folder2ISO.IsoWrappers;

internal class VolumeDescriptorWrapper
{
    private readonly VolumeDescriptor m_volumeDescriptor = new();
    private DateWrapper? m_creationDate;
    private DateWrapper? m_effectiveDate;
    private DateWrapper? m_expirationDate;
    private DateWrapper? m_modificationDate;
    private DirectoryRecordWrapper? m_rootDirRecord;
    private VolumeType m_volumeDescriptorType = VolumeType.Primary;

    // Constructor for Volume Descriptor Wrapper
    public VolumeDescriptorWrapper(string volumeName, uint volumeSpaceSize, uint pathTableSize, uint typeLPathTable,
        uint typeMPathTable, DirectoryRecordWrapper root, DateTime creationDate, DateTime modificationDate,
        sbyte timeZone)
    {
        SetVolumeDescriptor(volumeName, volumeSpaceSize, pathTableSize, typeLPathTable, typeMPathTable, root,
            creationDate, modificationDate, timeZone);
    }

    // Property for getting & setting the Volume Descriptor Type
    public VolumeType VolumeDescriptorType
    {
        get => m_volumeDescriptorType;
        set
        {
            if (m_volumeDescriptorType != value &&
                (m_volumeDescriptorType == VolumeType.Suplementary || value == VolumeType.Suplementary))
            {
                if (value == VolumeType.Suplementary)
                {
                    m_volumeDescriptor.SystemId =
                        IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.SystemId, IsoAlgorithm.SystemIdLength);
                    m_volumeDescriptor.VolumeId =
                        IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.VolumeId, IsoAlgorithm.VolumeIdLength);
                    m_volumeDescriptor.VolumeSetId = IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.VolumeSetId,
                        IsoAlgorithm.VolumeSetIdLength);
                    m_volumeDescriptor.PublisherId = IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.PublisherId,
                        IsoAlgorithm.PublisherIdLength);
                    m_volumeDescriptor.PreparerId = IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.PreparerId,
                        IsoAlgorithm.PreparerIdLength);
                    m_volumeDescriptor.ApplicationId = IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.ApplicationId,
                        IsoAlgorithm.ApplicationIdLength);
                    m_volumeDescriptor.CopyrightFileId = IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.CopyrightFileId,
                        IsoAlgorithm.CopyrightFileIdLength);
                    m_volumeDescriptor.AbstractFileId = IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.AbstractFileId,
                        IsoAlgorithm.AbstractFileIdLength);
                    m_volumeDescriptor.BibliographicFileId =
                        IsoAlgorithm.AsciiToUnicode(m_volumeDescriptor.BibliographicFileId,
                            IsoAlgorithm.BibliographicFileIdLength);
                }
                else
                {
                    m_volumeDescriptor.SystemId =
                        IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.SystemId, IsoAlgorithm.SystemIdLength);
                    m_volumeDescriptor.VolumeId =
                        IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.VolumeId, IsoAlgorithm.VolumeIdLength);
                    m_volumeDescriptor.VolumeSetId = IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.VolumeSetId,
                        IsoAlgorithm.VolumeSetIdLength);
                    m_volumeDescriptor.PublisherId = IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.PublisherId,
                        IsoAlgorithm.PublisherIdLength);
                    m_volumeDescriptor.PreparerId = IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.PreparerId,
                        IsoAlgorithm.PreparerIdLength);
                    m_volumeDescriptor.ApplicationId = IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.ApplicationId,
                        IsoAlgorithm.ApplicationIdLength);
                    m_volumeDescriptor.CopyrightFileId = IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.CopyrightFileId,
                        IsoAlgorithm.CopyrightFileIdLength);
                    m_volumeDescriptor.AbstractFileId = IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.AbstractFileId,
                        IsoAlgorithm.AbstractFileIdLength);
                    m_volumeDescriptor.BibliographicFileId =
                        IsoAlgorithm.UnicodeToAscii(m_volumeDescriptor.BibliographicFileId,
                            IsoAlgorithm.BibliographicFileIdLength);
                }
            }

            m_volumeDescriptorType = value;
            m_volumeDescriptor.VolumeDescType = (byte)value;
        }
    }

    // Method for setting Volume Descriptor data with primitive data types
    private void SetVolumeDescriptor(byte[]? systemId, byte[]? volumeId, ulong volumeSpaceSize, ulong pathTableSize,
        uint typeLPathTable, uint typeMPathTable, DirectoryRecord rootDirRecord, AsciiDateRecord? creationDate,
        AsciiDateRecord? modificationDate, AsciiDateRecord? expirationDate, AsciiDateRecord? effectiveDate)
    {
        m_volumeDescriptor.VolumeDescType = (byte)m_volumeDescriptorType;
        systemId?.CopyTo(m_volumeDescriptor.SystemId ?? Array.Empty<byte>(), 0);
        volumeId?.CopyTo(m_volumeDescriptor.VolumeId ?? Array.Empty<byte>(), 0);
        m_volumeDescriptor.VolumeSpaceSize = volumeSpaceSize;
        m_volumeDescriptor.PathTableSize = pathTableSize;
        m_volumeDescriptor.TypeLPathTable = typeLPathTable;
        m_volumeDescriptor.TypeMPathTable = typeMPathTable;
        m_rootDirRecord = new DirectoryRecordWrapper(rootDirRecord);
        m_creationDate = new DateWrapper(creationDate);
        m_modificationDate = new DateWrapper(modificationDate);
        m_expirationDate = new DateWrapper(expirationDate);
        m_effectiveDate = new DateWrapper(effectiveDate);
    }

    // Method for setting Volume Descriptor data with string identifiers and DateTime objects
    private void SetVolumeDescriptor(string systemId, string volumeId, uint volumeSpaceSize, uint pathTableSize,
        uint typeLPathTable, uint typeMPathTable, DirectoryRecordWrapper rootDir, DateTime creationDate,
        DateTime modificationDate, sbyte timeZone)
    {
        byte[]? systemIdBytes;
        byte[]? volumeIdBytes;

        // Set systemIdBytes and volumeIdBytes depending on the Volume Descriptor Type
        if (VolumeDescriptorType == VolumeType.Primary)
        {
            systemIdBytes = IsoAlgorithm.StringToByteArray(systemId, IsoAlgorithm.SystemIdLength);
            volumeIdBytes = IsoAlgorithm.StringToByteArray(volumeId, IsoAlgorithm.VolumeIdLength);
        }
        else
        {
            if (VolumeDescriptorType != VolumeType.Suplementary)
            {
                m_volumeDescriptor.VolumeDescType = (byte)m_volumeDescriptorType;
                return;
            }

            systemIdBytes = IsoAlgorithm.AsciiToUnicode(systemId, IsoAlgorithm.SystemIdLength);
            volumeIdBytes = IsoAlgorithm.AsciiToUnicode(volumeId, IsoAlgorithm.VolumeIdLength);
        }

        // Processes the values for use in setting the Volume Descriptor
        var volumeSpaceSizeAdjusted = IsoAlgorithm.BothEndian(volumeSpaceSize);
        var pathTableSizeAdjusted = IsoAlgorithm.BothEndian(pathTableSize);
        var typeMPathTableAdjusted = IsoAlgorithm.ChangeEndian(typeMPathTable);
        var creationDateWrapper = new DateWrapper(creationDate, timeZone);
        var modificationDateWrapper = new DateWrapper(modificationDate, timeZone);
        var noDateWrapper = new DateWrapper(IsoAlgorithm.NoDate);

        // sets the Volume Descriptor with processed data
        SetVolumeDescriptor(systemIdBytes, volumeIdBytes, volumeSpaceSizeAdjusted, pathTableSizeAdjusted,
            typeLPathTable, typeMPathTableAdjusted,
            rootDir.Record, creationDateWrapper.AsciiDateRecord, modificationDateWrapper.AsciiDateRecord,
            noDateWrapper.AsciiDateRecord,
            noDateWrapper.AsciiDateRecord);
    }

    // Method to create a Volume Descriptor initialized to default values
    private void SetVolumeDescriptor(string volumeName, uint volumeSpaceSize, uint pathTableSize,
        uint typeLPathTable, uint typeMPathTable, DirectoryRecordWrapper root, DateTime creationDate,
        DateTime modificationDate, sbyte timeZone)
    {
        SetVolumeDescriptor(" ", volumeName, volumeSpaceSize, pathTableSize, typeLPathTable, typeMPathTable, root,
            creationDate, modificationDate, timeZone);
    }

    // Writes the Volume Descriptor data into a binary format
    public void Write(BinaryWriter writer)
    {
        writer.Write(m_volumeDescriptor.VolumeDescType);
        writer.Write(m_volumeDescriptor.StandardIdentifier);

        // Write Set Terminator data
        if (VolumeDescriptorType == VolumeType.SetTerminator)
        {
            writer.Write(new byte[IsoAlgorithm.SectorSize - 7]);
            return;
        }

        writer.Write(m_volumeDescriptor.Reserved1);
        writer.Write(m_volumeDescriptor.SystemId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.VolumeId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.Reserved2);
        writer.Write(m_volumeDescriptor.VolumeSpaceSize);
        writer.Write(m_volumeDescriptorType == VolumeType.Suplementary ? m_volumeDescriptor.Reserved3_1 : new byte[3]);
        writer.Write(m_volumeDescriptor.Reserved3_2);
        writer.Write(VolumeDescriptor.VolumeSetSize);
        writer.Write(VolumeDescriptor.VolumeSequenceNumber);
        writer.Write(VolumeDescriptor.SectorkSize);
        writer.Write(m_volumeDescriptor.PathTableSize);
        writer.Write(m_volumeDescriptor.TypeLPathTable);
        writer.Write(m_volumeDescriptor.OptionalTypeLPathTable);
        writer.Write(m_volumeDescriptor.TypeMPathTable);
        writer.Write(m_volumeDescriptor.OptionalTypeMPathTable);

        // Write the Directory Record and its sub-records
        if (m_rootDirRecord != null)
        {
            m_rootDirRecord.VolumeDescriptorType = VolumeDescriptorType;
            m_rootDirRecord.Write(writer);
        }

        writer.Write(m_volumeDescriptor.VolumeSetId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.PublisherId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.PreparerId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.ApplicationId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.CopyrightFileId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.AbstractFileId ?? Array.Empty<byte>());
        writer.Write(m_volumeDescriptor.BibliographicFileId ?? Array.Empty<byte>());

        // Write date records
        m_creationDate?.WriteAsciiDateRecord(writer);
        m_modificationDate?.WriteAsciiDateRecord(writer);
        m_expirationDate?.WriteAsciiDateRecord(writer);
        m_effectiveDate?.WriteAsciiDateRecord(writer);

        writer.Write(VolumeDescriptor.FileStructureVersion);
        writer.Write(m_volumeDescriptor.Reserved4);
        writer.Write(m_volumeDescriptor.ApplicationData);
        writer.Write(m_volumeDescriptor.Reserved5);
    }
}