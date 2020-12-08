using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ES
{
    [System.Serializable]
    public class TableState
    {
        public TreeViewState treeViewState = new TreeViewState();
        public MultiColumnHeaderState columnState;

        public void InitColumnState(MultiColumnHeaderState.Column[] columns, out bool hadSerializedData)
        {
            hadSerializedData = columnState != null;
            var newColumnState = new MultiColumnHeaderState(columns);
            OverwriteSerializedFields(columnState, newColumnState);
            
            columnState = newColumnState;
        }

        private void OverwriteSerializedFields(MultiColumnHeaderState source, MultiColumnHeaderState destination)
        {
            if (!CanOverwriteSerializedFields())
            {
                return;
            }

            destination.visibleColumns = (int[])source.visibleColumns.Clone();
            destination.sortedColumns = (int[])source.sortedColumns.Clone();
            for (int i = 0; i < destination.columns.Length; i++)
            {
                destination.columns[i].width = source.columns[i].width;
                destination.columns[i].sortedAscending = source.columns[i].sortedAscending;
            }

            bool CanOverwriteSerializedFields()
            {
                return source?.columns != null && destination?.columns != null && source.columns.Length == destination.columns.Length;
            }
        }
    }
}