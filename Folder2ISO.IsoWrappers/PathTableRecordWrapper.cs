using ISO9660.Enums;
using ISO9660.PrimitiveTypes;

namespace Folder2ISO.IsoWrappers;


internal class PathTableRecordWrapper
{
    //The PathTableRecordWrapper class wraps the PathTableRecord with additional functionality.
    //It provides endian conversion as well as volume descriptor type handling.
    
    private Endian m_endian;
    private VolumeType m_volumeDescriptorType = VolumeType.Primary;
    private PathTableRecord Record { get; } = new();
    
    public PathTableRecordWrapper(uint extentLocation, ushort parentNumber, string name)
    {
        // Initializes a new instance of the PathTableRecordWrapper class.
        SetPathTableRecord(extentLocation, parentNumber, name);
    }

    public Endian Endian
    {
        // Gets or sets the endian type.
        set
        {
            if (value != m_endian)
            {
                Record.ExtentLocation = IsoAlgorithm.ChangeEndian(Record.ExtentLocation);
                Record.ParentNumber = IsoAlgorithm.ChangeEndian(Record.ParentNumber);
            }

            m_endian = value;
        }
    }

    public VolumeType VolumeDescriptorType
    {
        // Gets or sets the volume descriptor type.
        set
        {
            if (Record.Identifier is [0])
            {
                m_volumeDescriptorType = value;
                return;
            }

            if (m_volumeDescriptorType != value && (m_volumeDescriptorType == VolumeType.Suplementary || value == VolumeType.Suplementary))
            {
                if (value == VolumeType.Suplementary)
                {
                    Record.Identifier = IsoAlgorithm.AsciiToUnicode(Record.Identifier);
                    Record.Length = (byte)Record.Identifier.Length;
                    
                    if (Record.Identifier.Length > 255)
                    {
                        throw new Exception("Excceeds Maximum 8bit limit");
                    }
                }
                else
                {
                    Record.Identifier = IsoAlgorithm.UnicodeToAscii(Record.Identifier);
                    Record.Length = (byte)Record.Identifier.Length;
                    
                    if (Record.Identifier.Length > 255)
                    {
                        throw new Exception("Excceeds Maximum 8bit limit");
                    }
                }
            }

            m_volumeDescriptorType = value;
        }
    }

    private void SetPathTableRecord(uint extentLocation, ushort parentNumber, byte[]? identifier)
    {
        // Sets the path table record properties.
        if (identifier != null)
        {
            Record.Length = (byte)identifier.Length;
            
            if (identifier.Length > 255)
            {
                throw new Exception("Excceeds Maximum 8bit limit");
            }
            
            Record.Identifier = identifier;
            Record.ExtentLocation = extentLocation;
            Record.ParentNumber = parentNumber;
            
            if (m_volumeDescriptorType == VolumeType.Suplementary && (identifier.Length > 1 || identifier[0] != 0))
            {
                m_volumeDescriptorType = VolumeType.Primary;
                VolumeDescriptorType = VolumeType.Suplementary;
            }
        }
    }
    
    private void SetPathTableRecord(uint extentLocation, ushort parentNumber, string name)
    {
        // Sets the path table record properties.
        if (m_endian == Endian.BigEndian)
        {
            extentLocation = IsoAlgorithm.ChangeEndian(extentLocation);
            parentNumber = IsoAlgorithm.ChangeEndian(parentNumber);
        }

        byte[]? identifier;
        
        if (name != ".")
        {
            identifier = IsoAlgorithm.StringToByteArray(name);
        }
        else
        {
            var array = new byte[1];
            identifier = array;
        }

        SetPathTableRecord(extentLocation, parentNumber, identifier);
    }
    
    public int Write(BinaryWriter writer)
    {
        // Writes the path table record data to a binary writer in the provided format.
        writer.Write(Record.Length);
        writer.Write(Record.ExtendedLength);
        writer.Write(Record.ExtentLocation);
        writer.Write(Record.ParentNumber);
        writer.Write(Record.Identifier ?? Array.Empty<byte>());
        if (Record.Length % 2 == 1) writer.Write((byte)0);
        return 8 + Record.Length + Record.Length % 2;
    }
}