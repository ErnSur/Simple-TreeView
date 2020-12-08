using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace QuickEye.UI.Editor
{
    public class Table<T>
    {
        public TableState tableState;

        public MultiColumnHeaderState.Column[] columns;
        public string SearchString
        {
            get => view.searchString;
            set => view.searchString = value;
        }

        public TreeViewEvents<T> Events => view.events;

        private TableView<T> view;

        public Table(Column[] columns, TableState tableState, IList<T> list)
        {
            this.tableState = tableState;
            var header = CreateColumnHeader(columns, tableState);
            view = new TableView<T>(list, tableState.treeViewState, header);
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                view.columnDrawCell[i] = column.drawCell;
                view.columnGetSortingValue[i] = column.getSortingValue;
            }
        }

        private static MultiColumnHeader CreateColumnHeader(Column[] columns, TableState tableState)
        {
            tableState.InitColumnState(columns, out var hadSerializedData);
            var header = new MultiColumnHeader(tableState.columnState);
            if (!hadSerializedData)
                header.ResizeToFit();
            return header;
        }

        public void OnGUI(Rect rect) => view.OnGUI(rect);

        public void OnGUI()
        {
            var rect = GUILayoutUtility.GetRect(0, 1000, 0, 1000);
            view.OnGUI(rect);
        }

        [System.Serializable]
        public class Column : MultiColumnHeaderState.Column
        {
            public Column()
            {
                headerTextAlignment = TextAlignment.Center;
                sortingArrowAlignment = TextAlignment.Right;
            }
            public DrawCell<T> drawCell;
            public Func<T, object> getSortingValue;
        }
    }
}