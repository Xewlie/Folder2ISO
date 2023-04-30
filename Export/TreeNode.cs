namespace Export;

public class TreeNode
{
    private TreeNodeCollection Files { get; } = new();
    private TreeNodeCollection Directories { get; } = new();
    public static string Name => "";
    public uint Length { get; }
    public DateTime CreationTime { get; }
    public bool IsDirectory { get; }
    public static string FullName => "";

    public TreeNode[] GetAllChildren()
    {
        // Method to retrieve all child nodes (files and directories) from the tree node
        var treeNodeCollection = new TreeNodeCollection();
        treeNodeCollection.AddRange(Directories);
        treeNodeCollection.AddRange(Files);
        return treeNodeCollection.ToArray();
    }
    
    private IEnumerable<TreeNode> GetFiles()
    {
        return Files.ToArray();
    }
    
    private IEnumerable<TreeNode> GetDirectories()
    {
        return Directories.ToArray();
    }

    public TreeNode(uint length)
    {
        Length = length;
    }

    public override string ToString()
    {
        // Overridden ToString() method for displaying the node information as an XML-like string representation
        var xmlText = "<node name=\"" + Name + "\" dir=\"true\">";
        var directoryNodes = GetDirectories();
        xmlText = directoryNodes.Aggregate(xmlText, (current, directoryNode) => current + directoryNode);
        var fileNodes = GetFiles();
        
        xmlText = fileNodes.Aggregate(xmlText,
            (current, fileNode) => current + "<node name=\"" + Name + "\" dir=\"false\"/>");
        return xmlText + "</node>";
    }
}