using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ES
{
    public class Table<T>
    {
        public TableState tableState;

        public MultiColumnHeaderState.Column[] columns;

        #region TheAPI
        public string SearchString
        {
            get => view.searchString;
            set => view.searchString = value;
        }
        public TreeViewEvents<T> Events => view.events;

        #endregion
        private TableTreeView<T> view;



        public Table(Column[] columns, TableState tableState, IList<T> list)
        {
            this.tableState = tableState;
            var header = CreateColumnHeader(columns, tableState);
            view = new TableTreeView<T>(list, tableState.treeViewState, header);
        }

        private MultiColumnHeader CreateColumnHeader(Column[] columns, TableState tableState)
        {
            tableState.InitColumnState(columns,out var hadSerializedData);
            var header = new MultiColumnHeader(tableState.columnState);
            if (!hadSerializedData)
                header.ResizeToFit();
            return header;
        }

        public void OnGUI(Rect rect) => view.OnGUI(rect);
        public void OnGUI()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 1000, 0, 1000);
            view.OnGUI(rect);
        }

        [System.Serializable]
        public class Column : MultiColumnHeaderState.Column
        {
            public DrawCell<T> drawCell;
        }
    }

}