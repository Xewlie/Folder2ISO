using TreeNode = Export.TreeNode;

namespace Folder2ISO.DirectoryTree;

internal abstract class IsoFolderElement
{
    // The abstract class IsoFolderElement serves as a base class for representing files and folders within an ISO
    
    public abstract uint Extent1 { get; set; }
    public abstract uint Extent2 { get; set; }
    public abstract uint Size1 { get; }
    public abstract uint Size2 { get; }
    public abstract bool IsDirectory { get; }
    public DateTime Date { get; private set; }
    public string ShortName { get; private set; } = null!;
    public string LongName { get; private set; } = null!;

    protected IsoFolderElement(FileSystemInfo folderElement, bool isRoot, string childNumber)
    {
        InitializeValues(folderElement.CreationTime, folderElement.Name, isRoot, childNumber);
    }

    protected IsoFolderElement(TreeNode folderElement, bool isRoot, string childNumber)
    {
        InitializeValues(folderElement.CreationTime, TreeNode.Name, isRoot, childNumber);
    }

    private void InitializeValues(DateTime creationTime, string name, bool isRoot, string childNumber)
    {
        Date = creationTime;
        LongName = name;

        if (isRoot)
        {
            ShortName = ".";
            LongName = ".";
        }
        else if (LongName.Length > 8)
        {
            ShortName = LongName[..(8 - childNumber.Length)].ToUpper().Replace(' ', '_').Replace('.', '_');
            ShortName += childNumber;
        }
        else
        {
            ShortName = LongName.ToUpper().Replace(' ', '_').Replace('.', '_');
        }

        if (LongName.Length > IsoAlgorithm.FileNameMaxLength)
        {
            LongName = LongName[..(IsoAlgorithm.FileNameMaxLength - childNumber.Length)] + childNumber;
        }
        
    }
}