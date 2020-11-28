using UnityEditor.IMGUI.Controls;

namespace ES
{
    public class TableViewItem<T> : TreeViewItem
    {
        public T data;

        public TableViewItem(T data, int id): base(id,0)
        {
            this.data = data;
        }
    }
}