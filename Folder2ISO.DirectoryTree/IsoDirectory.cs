using System.Collections;
using Export;
using Folder2ISO.IsoWrappers;
using ISO9660.Enums;
using TreeNode = Export.TreeNode;

namespace Folder2ISO.DirectoryTree;

internal class IsoDirectory : IsoFolderElement
{
    private uint m_size1;
    private uint m_size2;

    public IsoDirectory(DirectoryInfo directory, uint level, string childNumber, ProgressDelegate? Progress) : base(
        directory, true, childNumber)
    {
        Parent = this;
        Initialize(directory, level, Progress);
    }

    private IsoDirectory(IsoDirectory parent, DirectoryInfo directory, uint level, string childNumber) : base(directory,
        false, childNumber)
    {
        Parent = parent;
        Initialize(directory, level, null);
    }

    private IsoDirectory(IsoDirectory parent, TreeNode directory, uint level, string childNumber) : base(directory,
        false, childNumber)
    {
        Parent = parent;
        Initialize(directory, level, null);
    }

    private FolderElementList Children { get; } = new();
    private IsoDirectory Parent { get; }
    public ushort Number { get; set; }
    public override uint Extent1 { get; set; }
    public override uint Extent2 { get; set; }
    public override uint Size1 => m_size1 * IsoAlgorithm.SectorSize;
    public override uint Size2 => m_size2 * IsoAlgorithm.SectorSize;
    public override bool IsDirectory => true;

    public uint TotalSize
    {
        get
        {
            var totalSize = (Size1 + Size2) / IsoAlgorithm.SectorSize;
            foreach (IsoFolderElement child in Children)
                // Check if the child element is not a directory
                if (!child.IsDirectory)
                {
                    // If it's a file, add the file size divided by the sector size to the total size
                    totalSize += child.Size1 / IsoAlgorithm.SectorSize;

                    // If the file size is not an exact multiple of the sector size, add one more sector to the total size
                    if (child.Size1 % IsoAlgorithm.SectorSize != 0) totalSize++;
                }
                else
                {
                    // If it's a directory, recursively calculate the total size of the directory and add it to the total size
                    totalSize += ((IsoDirectory)child).TotalSize;
                }

            return totalSize;
        }
    }

    private void CalculateSize()
    {
        m_size1 = 1u;
        m_size2 = 1u;

        // Calculate initial space consumed by the two "." and ".." directory records
        var currentSize1 = (uint)(2 * IsoAlgorithm.DefaultDirectoryRecordLength);
        var currentSize2 = (uint)(2 * IsoAlgorithm.DefaultDirectoryRecordLength);

        foreach (IsoFolderElement child in Children)
        {
            // Calculate space consumed by the current child's short name record
            var childRecordSize1 = (uint)(!child.IsDirectory
                ? child.ShortName.Length + 2 + IsoAlgorithm.DefaultDirectoryRecordLength - 1
                : child.ShortName.Length + IsoAlgorithm.DefaultDirectoryRecordLength - 1);

            // Ensure the record size is an even number by rounding up
            if (childRecordSize1 % 2u == 1) childRecordSize1++;

            // Check if adding the current child's record size would exceed the sector size limit
            if (currentSize1 + childRecordSize1 > IsoAlgorithm.SectorSize)
            {
                // If so, increment size1 and reset currentSize1 to the current child's record size
                m_size1++;
                currentSize1 = childRecordSize1;
            }
            else
            {
                // Otherwise, add the current child's record size to currentSize1
                currentSize1 += childRecordSize1;
            }

            // Calculate space consumed by the current child's long name record
            var childRecordSize2 = (uint)(!child.IsDirectory
                ? 2 * (child.LongName.Length + 2) + IsoAlgorithm.DefaultDirectoryRecordLength - 1
                : 2 * child.LongName.Length + IsoAlgorithm.DefaultDirectoryRecordLength - 1);

            // Ensure the record size is an even number by rounding up
            if (childRecordSize2 % 2u == 1) childRecordSize2++;

            // Check if adding the current child's record size would exceed the sector size limit
            if (currentSize2 + childRecordSize2 > IsoAlgorithm.SectorSize)
            {
                // If so, increment size2 and reset currentSize2 to the current child's record size
                m_size2++;
                currentSize2 = childRecordSize2;
            }
            else
            {
                // Otherwise, add the current child's record size to currentSize2
                currentSize2 += childRecordSize2;
            }
        }
    }

    /// Initializes the IsoDirectory instance with the provided directory information and level.
    private void Initialize(DirectoryInfo directory, uint level, ProgressDelegate? progress)
    {
        var fileSystemInfos = directory.GetFileSystemInfos();

        // Invoke the progress delegate to report the initial progress.
        progress?.Invoke(this, new ProgressEventArgs(0, fileSystemInfos.Length));

        var indexStringPaddingLength = fileSystemInfos.Length.ToString().Length;

        for (var i = 0; i < fileSystemInfos.Length; i++)
        {
            // Format the child index string with leading zeros.
            var childIndexString = $"{i.ToString().PadLeft(indexStringPaddingLength, '0')}";

            // Determine if the file system item is a file or a directory, and create a new IsoFolderElement accordingly.
            IsoFolderElement folderElement = fileSystemInfos[i] is not DirectoryInfo
                ? new IsoFile((FileInfo)fileSystemInfos[i], childIndexString)
                : new IsoDirectory(this, (DirectoryInfo)fileSystemInfos[i], level + 1, childIndexString);

            Children.Add(folderElement);

            // Invoke the progress delegate to report the progress.
            progress?.Invoke(this, new ProgressEventArgs(i));
        }

        Children.Sort();
        CalculateSize();
    }

    private void Initialize(TreeNode directory, uint level, ProgressDelegate? progress)
    {
        var allChildren = directory.GetAllChildren();

        // Invoke the progress delegate to report the initial progress.
        progress?.Invoke(this, new ProgressEventArgs(0, allChildren.Length));

        var indexStringPaddingLength = allChildren.Length.ToString().Length;

        for (var i = 0; i < allChildren.Length; i++)
        {
            // Format the child index string with leading zeros.
            var childIndexString = $"{i.ToString().PadLeft(indexStringPaddingLength, '0')}";

            // Determine if the TreeNode item is a file or a directory, and create a new IsoFolderElement accordingly.
            IsoFolderElement folderElement = !allChildren[i].IsDirectory
                ? new IsoFile(allChildren[i], childIndexString)
                : new IsoDirectory(this, allChildren[i], level + 1, childIndexString);

            Children.Add(folderElement);

            // Invoke the progress delegate to report the progress.
            progress?.Invoke(this, new ProgressEventArgs(i));
        }

        Children.Sort();
        CalculateSize();
    }

    public void WriteFiles(BinaryWriter writer, ProgressDelegate? Progress)
    {
        // Writes the files and directories of the current IsoDirectory using the given BinaryWriter.
        WriteFileElements(writer, Progress);
        WriteDirectoryElements(writer, Progress);
    }

    private void WriteFileElements(BinaryWriter writer, ProgressDelegate? Progress)
    {
        // Writes the file elements of the current IsoDirectory using the given BinaryWriter.
        foreach (IsoFolderElement child in Children)
            if (!child.IsDirectory)
            {
                ((IsoFile)child).Write(writer, Progress);
                ReportProgress(writer, Progress);
            }
    }

    private void WriteDirectoryElements(BinaryWriter writer, ProgressDelegate? Progress)
    {
        // Writes the directory elements of the current IsoDirectory using the given BinaryWriter.
        foreach (IsoFolderElement child in Children)
            if (child.IsDirectory)
            {
                ((IsoDirectory)child).WriteFiles(writer, Progress);
                ReportProgress(writer, Progress);
            }
    }

    private void ReportProgress(BinaryWriter writer, ProgressDelegate? Progress)
    {
        // Reports the current progress on the base stream of the given BinaryWriter.
        Progress?.Invoke(
            this,
            new ProgressEventArgs((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize))
        );
    }

    public void Write(BinaryWriter writer, VolumeType type)
    {
        // Get the current element and write the directory record for "."
        IsoFolderElement currentElement = type == VolumeType.Primary ? this : Parent;
        WriteDirectoryRecord(writer, currentElement, ".");

        // Get the parent element and write the directory record for ".."
        IsoFolderElement parentElement = type == VolumeType.Primary ? Parent : Parent.Parent;
        WriteDirectoryRecord(writer, parentElement, "..");

        // Write the directory records for each child element
        var writtenBytes = 2 * IsoAlgorithm.DefaultDirectoryRecordLength;
        foreach (IsoFolderElement child in Children)
        {
            // Determine the name of the child element and write its directory record
            var name = type == VolumeType.Primary ? child.ShortName : child.LongName;
            var elementLength = WriteDirectoryRecord(writer, child, name, type);

            // Pad the remaining bytes in the sector if necessary
            if (elementLength + writtenBytes > IsoAlgorithm.SectorSize)
            {
                writer.Write(new byte[IsoAlgorithm.SectorSize - writtenBytes]);
                writtenBytes = 0;
            }

            writtenBytes += elementLength;
        }

        // Pad the remaining bytes in the sector
        writer.Write(new byte[IsoAlgorithm.SectorSize - writtenBytes]);
    }

    private int WriteDirectoryRecord(BinaryWriter writer, IsoFolderElement element, string name,
        VolumeType? type = null)
    {
        // Determine the extent location and data length of the element
        var extentLocation = type == VolumeType.Primary ? element.Extent1 : element.Extent2;
        var dataLength = type == VolumeType.Primary ? element.Size1 : element.Size2;

        // Create a new directory record wrapper
        var directoryRecordWrapper = new DirectoryRecordWrapper(
            extentLocation,
            dataLength,
            element.Date,
            element.IsDirectory,
            name);

        // Set the volume descriptor type if provided
        if (type.HasValue) directoryRecordWrapper.VolumeDescriptorType = type.Value;

        // Write the directory record and return its length
        return directoryRecordWrapper.Write(writer);
    }


    public int WritePathTable(BinaryWriter writer, bool isRoot, Endian endian, VolumeType type)
    {
        // Determine the extent location and name for this path table record
        var extentLocation = type == VolumeType.Primary ? Extent1 : Extent2;
        var name = type == VolumeType.Primary ? ShortName : LongName;

        // Create a new path table record wrapper
        var pathTableRecordWrapper = !isRoot
            ? new PathTableRecordWrapper(extentLocation, Parent.Number, name)
            : new PathTableRecordWrapper(extentLocation, Parent.Number, ".");

        // Set the volume descriptor type and endian
        pathTableRecordWrapper.VolumeDescriptorType = type;
        pathTableRecordWrapper.Endian = endian;

        // Write the path table record and return its length
        return pathTableRecordWrapper.Write(writer);
    }

    public void SetFilesExtent(ref uint currentExtent)
    {
        foreach (IsoFolderElement child in Children)
        {
            // Skip over directories and files with zero size
            if (child.IsDirectory) continue;
            if (child.Size1 == 0)
            {
                child.Extent1 = 0u;
                continue;
            }

            // Set the extent location of the file
            child.Extent1 = currentExtent;

            // Calculate the number of sectors needed for the file
            currentExtent += child.Size1 / IsoAlgorithm.SectorSize;
            if (child.Size1 % IsoAlgorithm.SectorSize != 0) currentExtent++;
        }

        // Recursively set the extent locations for files in all child directories
        foreach (IsoFolderElement child2 in Children)
            if (child2.IsDirectory)
                ((IsoDirectory)child2).SetFilesExtent(ref currentExtent);
    }

    public static void SetExtent1(ArrayList stack, int index, uint currentExtent)
    {
        while (true)
        {
            // Exit the loop if we've processed all elements in the stack
            if (index >= stack.Count) return;

            // Set the extent location for the current directory and update the current extent
            if (stack[index] is IsoDirectory isoDirectory)
            {
                isoDirectory.Extent1 = currentExtent;
                var currentExtent2 = currentExtent + isoDirectory.Size1 / IsoAlgorithm.SectorSize;

                // Add all child directories to the stack
                foreach (IsoFolderElement child in isoDirectory.Children)
                    if (child.IsDirectory)
                        stack.Add(child);

                index += 1;
                currentExtent = currentExtent2;
            }
        }
    }

    public static void SetExtent2(ArrayList stack, int index, uint currentExtent)
    {
        while (true)
        {
            // Exit the loop if we've processed all elements in the stack
            if (index >= stack.Count) return;

            // Set the extent location for the current directory and update the current extent
            if (stack[index] is IsoDirectory isoDirectory)
            {
                isoDirectory.Extent2 = currentExtent;
                var currentExtent2 = currentExtent + isoDirectory.Size2 / IsoAlgorithm.SectorSize;

                // Add all child directories to the stack
                foreach (IsoFolderElement child in isoDirectory.Children)
                    if (child.IsDirectory)
                        stack.Add(child);

                index += 1;
                currentExtent = currentExtent2;
            }
        }
    }
}