using System.Collections;

namespace Folder2ISO.DirectoryTree;

internal class FolderElementList : CollectionBase
{
    public void Add(IsoFolderElement value)
    {
        InnerList.Add(value);
    }

    public void Sort()
    {
        var comparer = new DirEntryComparer();
        InnerList.Sort(comparer);
    }

    private class DirEntryComparer : IComparer
    {
        public int Compare(object? x, object? y)
        {
            if (x is not IsoFolderElement elementX || y is not IsoFolderElement elementY)
                //handle the case where either x or y is null, here we just return 0 to indicate they are equal
                return 0;

            var longName = elementX.LongName;
            var longName2 = elementY.LongName;

            return string.CompareOrdinal(longName, longName2);
        }
    }
}