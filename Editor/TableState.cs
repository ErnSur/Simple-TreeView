using System;
using UnityEditor.IMGUI.Controls;

namespace QuickEye.UI.Editor
{
    [Serializable]
    public class TableState
    {
        public TreeViewState treeViewState = new TreeViewState();
        public MultiColumnHeaderState columnState;

        private bool hadSerializedData;
        public void Init(MultiColumnHeaderState.Column[] columns, out MultiColumnHeader header)
        {
            InitColumnState(columns);
            header = CreateColumnHeader();
        }

        private MultiColumnHeader CreateColumnHeader()
        {
            var header = new MultiColumnHeader(columnState);
            if (!hadSerializedData)
                header.ResizeToFit();
            return header;
        }

        private void InitColumnState(MultiColumnHeaderState.Column[] columns)
        {
            hadSerializedData = columnState != null;
            var newColumnState = new MultiColumnHeaderState(columns);
            OverwriteSerializedFields(columnState, newColumnState);

            columnState = newColumnState;
        }


        private void OverwriteSerializedFields(MultiColumnHeaderState source, MultiColumnHeaderState destination)
        {
            if (!CanOverwriteSerializedFields()) return;

            destination.visibleColumns = (int[]) source.visibleColumns.Clone();
            destination.sortedColumns = (int[]) source.sortedColumns.Clone();
            for (var i = 0; i < destination.columns.Length; i++)
            {
                destination.columns[i].width = source.columns[i].width;
                destination.columns[i].sortedAscending = source.columns[i].sortedAscending;
            }

            bool CanOverwriteSerializedFields()
            {
                return source?.columns != null && destination?.columns != null &&
                       source.columns.Length == destination.columns.Length;
            }
        }
    }
}