using System.Collections;

namespace Export;

public class TreeNodeCollection : CollectionBase
{
    public void AddRange(TreeNodeCollection collection)
    {
        InnerList.AddRange(collection);
    }

    public TreeNode[] ToArray()
    {
        return (TreeNode[])InnerList.ToArray(typeof(TreeNode));
    }
}