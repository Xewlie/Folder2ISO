using Export;
using TreeNode = Export.TreeNode;

namespace Folder2ISO.DirectoryTree;

internal class IsoFile : IsoFolderElement
{
    //  Represents an ISO File in the ISO Directory Tree.
    
    private readonly string m_fullPath;
    private readonly uint m_size;
    private uint m_extent;
    public override uint Size1 => m_size;
    public override uint Size2 => m_size;
    public override bool IsDirectory => false;
    
    public override uint Extent1
    {
        get => m_extent;
        set => m_extent = value;
    }
    public override uint Extent2
    {
        get => m_extent;
        set => m_extent = value;
    }

    public IsoFile(FileInfo file, string childNumber) : base(file, false, childNumber)
    {
        m_fullPath = file.FullName;
        m_size = (uint)file.Length;
    }

    public IsoFile(TreeNode file, string childNumber) : base(file, false, childNumber)
    {
        m_fullPath = TreeNode.FullName;
        m_size = file.Length;
    }

    public void Write(BinaryWriter writer, ProgressDelegate? Progress)
    {
        // Return early if the extent or size is zero
        if (m_extent == 0 || m_size == 0) return;

        // Open the file for reading
        using var fileStream = new FileStream(m_fullPath, FileMode.Open, FileAccess.Read);
        using var binaryReader = new BinaryReader(fileStream);

        // Buffer for reading the file's content
        var buffer = new byte[IsoAlgorithm.SectorSize * 512];

        int bytesRead;
        while ((bytesRead = binaryReader.Read(buffer, 0, buffer.Length)) > 0)
        {
            writer.Write(buffer, 0, bytesRead);
            Progress?.Invoke(this, new ProgressEventArgs((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize)));

            // In case bytesRead is not an even multiple of SectorSize, pad with zeros
            if (bytesRead % IsoAlgorithm.SectorSize != 0)
            {
                writer.Write(new byte[IsoAlgorithm.SectorSize - bytesRead % IsoAlgorithm.SectorSize]);
            }
        }
    }
}