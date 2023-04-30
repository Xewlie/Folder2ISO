using ISO9660.Enums;
using ISO9660.PrimitiveTypes;

namespace Folder2ISO.IsoWrappers;

internal class DirectoryRecordWrapper
{
    //  A wrapper for the DirectoryRecord structure in the ISO 9660 file system.
    //  This class provides a more convenient way to work with directory records and to modify their properties.
    
    private DateWrapper? m_dateWrapper;
    private VolumeType m_volumeDescriptorType = VolumeType.Primary;
    public DirectoryRecord Record { get; } = new();

    public DirectoryRecordWrapper(uint extentLocation, uint dataLength, DateTime date, bool isDirectory, string name)
    {
        SetDirectoryRecord(extentLocation, dataLength, date, isDirectory, name);
    }

    public DirectoryRecordWrapper(DirectoryRecord directoryRecord)
    {
        Record = directoryRecord;
        m_dateWrapper = new DateWrapper(directoryRecord.Date);
    }
    
    public VolumeType VolumeDescriptorType
    {
        // The type of the volume descriptor associated with this directory record
        set
        {
            // If the file identifier is <= 1, just set the volume descriptor type and return
            if (Record.FileIdentifier is [<= 1])
            {
                m_volumeDescriptorType = value;
                return;
            }

            // If the volume descriptor type is changing to or from a supplementary type
            if (m_volumeDescriptorType != value && (m_volumeDescriptorType == VolumeType.Suplementary || value == VolumeType.Suplementary))
            {
                // If changing to supplementary
                if (value == VolumeType.Suplementary)
                {
                    // Convert file identifier to unicode and update length properties
                    Record.FileIdentifier = IsoAlgorithm.AsciiToUnicode(Record.FileIdentifier);
                    Record.LengthOfFileIdentifier = (byte)Record.FileIdentifier.Length;
                    Record.Length = (byte)(33 + Record.LengthOfFileIdentifier + (1 - Record.LengthOfFileIdentifier % 2));

                    // Check if length exceeds the limit
                    if (33 + Record.LengthOfFileIdentifier + (1 - Record.LengthOfFileIdentifier % 2) > 255)
                    {
                        throw new Exception("Exceeded maximum length");
                    }
                }
                else
                {
                    // Convert file identifier to ASCII and update length properties
                    Record.FileIdentifier = IsoAlgorithm.UnicodeToAscii(Record.FileIdentifier);
                    Record.LengthOfFileIdentifier = (byte)Record.FileIdentifier.Length;
                    Record.Length = (byte)(33 + Record.LengthOfFileIdentifier + (1 - Record.LengthOfFileIdentifier % 2));

                    // Check if length exceeds the limit
                    if (Record.FileIdentifier.Length > 255 || 33 + Record.LengthOfFileIdentifier + (1 - Record.LengthOfFileIdentifier % 2) > 255)
                    {
                        throw new Exception("Exceeded maximum length");
                    }
                }
            }

            m_volumeDescriptorType = value;
        }
    }

    // Set the directory record properties using the provided parameters
    private void SetDirectoryRecord(ulong extentLocation, ulong dataLength, BinaryDateRecord? date, sbyte timeZone,
        byte fileFlags, byte[]? fileIdentifier)
    {
        Record.ExtentLocation = extentLocation;
        Record.DataLength = dataLength;
        Record.Date = date;
        Record.TimeZone = timeZone;
        Record.FileFlags = fileFlags;

        if (fileIdentifier != null)
        {
            Record.LengthOfFileIdentifier = (byte)fileIdentifier.Length;
            Record.FileIdentifier = fileIdentifier;
            Record.Length = (byte)(Record.LengthOfFileIdentifier + 33);

            if (Record.Length % 2 == 1) Record.Length++;

            if (fileIdentifier.Length > 255 || Record.LengthOfFileIdentifier + 33 > 255)
                throw new Exception("Exceeded maximum length");

            // Ensure the primary and supplementary flags are set correctly for this directory record
            if (m_volumeDescriptorType == VolumeType.Suplementary &&
                ((fileFlags & 2) == 0 || fileIdentifier.Length != 1 || fileIdentifier[0] > 1))
            {
                m_volumeDescriptorType = VolumeType.Primary;
                VolumeDescriptorType = VolumeType.Suplementary;
            }
        }
    }

    // Sets the directory record properties using DateTime, timezone, directory flag, and name
    private void SetDirectoryRecord(uint extentLocation, uint dataLength, DateTime date, sbyte timeZone, bool isDirectory, string name)
    {
        m_dateWrapper = new DateWrapper(date);

        // Set the file flags based on whether the record is a directory
        var fileFlags = (byte)(isDirectory ? 2 : 0);

        // Generate the file identifier based on the provided name
        byte[]? fileIdentifier;
        if (name != ".")
            fileIdentifier = name == ".." ? new byte[] { 1 } :
                !isDirectory ? IsoAlgorithm.StringToByteArray(name + ";1") : IsoAlgorithm.StringToByteArray(name);
        else
            fileIdentifier = new byte[1];

        SetDirectoryRecord(IsoAlgorithm.BothEndian(extentLocation), IsoAlgorithm.BothEndian(dataLength),
            m_dateWrapper.BinaryDateRecord, timeZone, fileFlags, fileIdentifier);
    }

    // Sets the directory record properties using DateTime, directory flag, and name, default timezone is 8
    private void SetDirectoryRecord(uint extentLocation, uint dataLength, DateTime date, bool isDirectory,
        string name)
    {
        SetDirectoryRecord(extentLocation, dataLength, date, 8, isDirectory, name);
    }
    
    public int Write(BinaryWriter writer)
    {
        // Writes the DirectoryRecordWrapper to a given BinaryWriter.
        writer.Write(Record.Length);
        writer.Write(Record.ExtendedAttributeLength);
        writer.Write(Record.ExtentLocation);
        writer.Write(Record.DataLength);
        m_dateWrapper?.WriteBinaryDateRecord(writer);
        writer.Write(Record.TimeZone);
        writer.Write(Record.FileFlags);
        writer.Write(Record.FileUnitSize);
        writer.Write(Record.InterleaveGapSize);
        writer.Write(DirectoryRecord.VolumeSequnceNumber);
        writer.Write(Record.LengthOfFileIdentifier);
        writer.Write(Record.FileIdentifier ?? Array.Empty<byte>());

        // Add padding if necessary
        if (Record.LengthOfFileIdentifier % 2 == 0) writer.Write((byte)0);

        return Record.Length;
    }
}