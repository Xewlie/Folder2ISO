using System.Collections;
using Export;
using Folder2ISO.DirectoryTree;
using Folder2ISO.IsoWrappers;
using ISO9660.Enums;

namespace Folder2ISO;

public class Folder2ISO
{
    public event ProgressDelegate? Progress;

    public event FinishDelegate? Finish;

    public event AbortDelegate? Abort;

    // Sets the directory numbers for the given list of IsoDirectories.
    private void SetDirectoryNumbers(IList<IsoDirectory>? directories)
    {
        if (directories != null)
        {
            for (var i = 0; i < directories.Count; i++)
            {
                directories[i].Number = (ushort)(i + 1);
            }
        }
    }

    // Writes the first 16 empty sectors in the BinaryWriter.
    private void WriteFirst16EmptySectors(BinaryWriter writer)
    {
        for (var i = 0; i < 16; i++)
        {
            writer.Write(new byte[IsoAlgorithm.SectorSize]);
        }
    }

    // Writes volume descriptors for the provided parameters.
    private void WriteVolumeDescriptors(BinaryWriter writer, string volumeName, IsoDirectory root,
        uint volumeSpaceSize, uint pathTableSize1, uint pathTableSize2, uint typeLPathTable1, uint typeMPathTable1,
        uint typeLPathTable2, uint typeMPathTable2)
    {
        var rootRecord = new DirectoryRecordWrapper(root.Extent1, root.Size1, root.Date, root.IsDirectory, ".");
        var volumeDescriptorWrapper = new VolumeDescriptorWrapper(volumeName, volumeSpaceSize, pathTableSize1,
            typeLPathTable1, typeMPathTable1, rootRecord, DateTime.Now, DateTime.Now, 8)
        {
            VolumeDescriptorType = VolumeType.Primary
        };
        volumeDescriptorWrapper.Write(writer);
        rootRecord = new DirectoryRecordWrapper(root.Extent2, root.Size2, root.Date, root.IsDirectory, ".");
        volumeDescriptorWrapper = new VolumeDescriptorWrapper(volumeName, volumeSpaceSize, pathTableSize2,
            typeLPathTable2, typeMPathTable2, rootRecord, DateTime.Now, DateTime.Now, 8)
        {
            VolumeDescriptorType = VolumeType.Suplementary
        };
        volumeDescriptorWrapper.Write(writer);
        volumeDescriptorWrapper.VolumeDescriptorType = VolumeType.SetTerminator;
        volumeDescriptorWrapper.Write(writer);
    }

    // Writes the provided directories in the BinaryWriter for the given volume type.
    private void WriteDirectories(BinaryWriter writer, IsoDirectory[]? directories, VolumeType type)
    {
        if (directories != null)
        {
            foreach (var directory in directories)
            {
                directory.Write(writer, type);
                OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));
            }
        }
    }

    // Writes the path table with the provided directories, endianness, and volume type. Returns the table size.
    private int WritePathTable(BinaryWriter writer, IsoDirectory[]? directories, Endian endian, VolumeType type)
    {
        if (directories == null)
        {
            return 0;
        }
        
        var tableSize = directories.Select((dir, i) => dir.WritePathTable(writer, i == 0, endian, type)).Sum();
        writer.Write(new byte[IsoAlgorithm.SectorSize - tableSize % IsoAlgorithm.SectorSize]);
        return tableSize;
    }

    // Main method to convert a folder into an ISO image using a BinaryWriter.
    private void Folder2Iso(DirectoryInfo rootDirectoryInfo, BinaryWriter writer, string volumeName,
        CancellationToken cancellationToken)
    {
        OnProgress("Initializing ISO root directory...", 0, 1);
        var isoDirectory = new IsoDirectory(rootDirectoryInfo, 1u, "0", Progress);

        cancellationToken.ThrowIfCancellationRequested();

        OnProgress("Preparing first set of directory extents...", 0, 1);
        var directoryList = new ArrayList { isoDirectory };
        IsoDirectory.SetExtent1(directoryList, 0, 19u);

        cancellationToken.ThrowIfCancellationRequested();

        OnProgress(1);
        OnProgress("Calculating directory numbers...", 0, 1);
        var directoryArray = new IsoDirectory[directoryList.Count];
        directoryList.ToArray().CopyTo(directoryArray, 0);

        cancellationToken.ThrowIfCancellationRequested();

        SetDirectoryNumbers(directoryArray);
        OnProgress(1);
        OnProgress("Preparing first set of path tables...", 0, 2);

        cancellationToken.ThrowIfCancellationRequested();

        var memoryStream = new MemoryStream();
        var binaryWriter = new BinaryWriter(memoryStream);
        var lastIsoDirectory = directoryArray[^1];
        var primaryExtentOffset = lastIsoDirectory.Extent1 + lastIsoDirectory.Size1 / IsoAlgorithm.SectorSize;
        WritePathTable(binaryWriter, directoryArray, Endian.LittleEndian, VolumeType.Primary);
        OnProgress(1);

        cancellationToken.ThrowIfCancellationRequested();

        var primaryTypeMPathTableOffset = primaryExtentOffset + (uint)memoryStream.Length / IsoAlgorithm.SectorSize;
        var primaryPathTableSize =
            (uint)WritePathTable(binaryWriter, directoryArray, Endian.BigEndian, VolumeType.Primary);
        OnProgress(2);
        OnProgress("Preparing second set of directory extents...", 0, 1);

        cancellationToken.ThrowIfCancellationRequested();

        directoryList = new ArrayList { isoDirectory };
        var currentExtent = primaryExtentOffset + (uint)memoryStream.Length / IsoAlgorithm.SectorSize;
        IsoDirectory.SetExtent2(directoryList, 0, currentExtent);
        directoryArray = new IsoDirectory[directoryList.Count];
        directoryList.ToArray().CopyTo(directoryArray, 0);

        OnProgress(1);
        OnProgress("Preparing second set of path tables...", 0, 2);

        cancellationToken.ThrowIfCancellationRequested();

        var memoryStream2 = new MemoryStream();
        var binaryWriter2 = new BinaryWriter(memoryStream2);
        lastIsoDirectory = directoryArray[^1];
        var secondaryExtentOffset = lastIsoDirectory.Extent2 + lastIsoDirectory.Size2 / IsoAlgorithm.SectorSize;
        WritePathTable(binaryWriter2, directoryArray, Endian.LittleEndian, VolumeType.Suplementary);
        OnProgress(1);

        cancellationToken.ThrowIfCancellationRequested();

        var secondaryTypeMPathTableOffset =
            secondaryExtentOffset + (uint)memoryStream2.Length / IsoAlgorithm.SectorSize;
        var secondaryPathTableSize =
            (uint)WritePathTable(binaryWriter2, directoryArray, Endian.BigEndian, VolumeType.Suplementary);
        OnProgress(2);
        OnProgress("Initializing...", 0, 1);

        cancellationToken.ThrowIfCancellationRequested();

        currentExtent = secondaryExtentOffset + (uint)memoryStream2.Length / IsoAlgorithm.SectorSize;
        isoDirectory.SetFilesExtent(ref currentExtent);
        var totalSectors = 19u;
        totalSectors += isoDirectory.TotalSize;
        totalSectors += (uint)memoryStream.Length / IsoAlgorithm.SectorSize;
        totalSectors += (uint)memoryStream2.Length / IsoAlgorithm.SectorSize;
        var primaryPathTableBuffer = memoryStream.GetBuffer();

        cancellationToken.ThrowIfCancellationRequested();

        Array.Resize(ref primaryPathTableBuffer, (int)memoryStream.Length);

        cancellationToken.ThrowIfCancellationRequested();

        var secondaryPathTableBuffer = memoryStream2.GetBuffer();

        cancellationToken.ThrowIfCancellationRequested();

        Array.Resize(ref secondaryPathTableBuffer, (int)memoryStream2.Length);

        cancellationToken.ThrowIfCancellationRequested();

        memoryStream.Close();
        memoryStream2.Close();
        binaryWriter.Close();
        binaryWriter2.Close();
        OnProgress(1);
        OnProgress("Writing data to file...", 0, (int)totalSectors);

        cancellationToken.ThrowIfCancellationRequested();

        WriteFirst16EmptySectors(writer);
        OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

        cancellationToken.ThrowIfCancellationRequested();

        WriteVolumeDescriptors(writer, volumeName, isoDirectory, totalSectors, primaryPathTableSize,
            secondaryPathTableSize,
            primaryExtentOffset, primaryTypeMPathTableOffset, secondaryExtentOffset, secondaryTypeMPathTableOffset);

        OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

        cancellationToken.ThrowIfCancellationRequested();

        WriteDirectories(writer, directoryArray, VolumeType.Primary);
        writer.Write(primaryPathTableBuffer);
        OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

        cancellationToken.ThrowIfCancellationRequested();

        WriteDirectories(writer, directoryArray, VolumeType.Suplementary);
        writer.Write(secondaryPathTableBuffer);
        OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

        cancellationToken.ThrowIfCancellationRequested();

        isoDirectory.WriteFiles(writer, Progress);

        cancellationToken.ThrowIfCancellationRequested();

        OnProgress("Finished.", 1, 1);
    }

    // Method to convert a folder into an ISO image with string paths.
    private void Folder2Iso(string folderPath, string isoPath, string volumeName, CancellationToken cancellationToken)
    {
        try
        {
            using var fileStream = new FileStream(isoPath, FileMode.Create);
            using var binaryWriter = new BinaryWriter(fileStream);

            var rootDirectoryInfo = new DirectoryInfo(folderPath);

            // Pass the cancellation token.
            Folder2Iso(rootDirectoryInfo, binaryWriter, volumeName, cancellationToken);

            OnFinished("ISO writing process finished successfully");
        }
        catch (OperationCanceledException)
        {
            OnAbort();
        }
        catch (Exception)
        {
            OnAbort();
        }
    }

    // Method to convert a folder into an ISO image using an object parameter containing the paths and volume name.
    public void Folder2Iso(object? data, CancellationToken cancellationToken)
    {
        if (data is Folder2ISOArgs folder2ISOArgs)
        {
            Folder2Iso(folder2ISOArgs.FolderPath, folder2ISOArgs.IsoPath, folder2ISOArgs.VolumeName, cancellationToken);
        }
    }

    // Invoke the Finish event.
    private void OnFinished(string message)
    {
        Finish?.Invoke(this, new FinishEventArgs(message));
    }

    // Invoke the Progress event.
    private void OnProgress(int current)
    {
        Progress?.Invoke(this, new ProgressEventArgs(current));
    }

    // Invoke the Progress event with an action, current status, and maximum status.
    private void OnProgress(string? action, int current, int maximum)
    {
        Progress?.Invoke(this, new ProgressEventArgs(action, current, maximum));
    }

    // Invoke the Abort event.
    private void OnAbort()
    {
        Abort?.Invoke(this, new AbortEventArgs());
    }

    public class Folder2ISOArgs
    {
        public Folder2ISOArgs(string folderPath, string isoPath, string volumeName)
        {
            FolderPath = folderPath;
            IsoPath = isoPath;
            VolumeName = volumeName;
        }

        public string FolderPath { get; }

        public string IsoPath { get; }

        public string VolumeName { get; }
    }
}