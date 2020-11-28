using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace ES
{
    public delegate bool ShouldDrawRow<T>(T item, string searchString);
    public delegate void DrawCell<T>(DrawCellArgs<T> args);
    public class TableTreeView<T> : TreeView
    {
        public IList<T> list;
        public ShouldDrawRow<T> shouldDrawRow = (_, __) => true;
        public Table<T>.Column[] columns;
        public TableTreeView(IList<T> list, TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state, multiColumnHeader)
        {
            this.list = list;
            columns = multiColumnHeader.state.columns.Cast<Table<T>.Column>().ToArray();
            //multiColumnHeader.sortingChanged += this.sort
            Reload();
        }

        protected override TreeViewItem BuildRoot() => new TreeViewItem { id = 0, depth = -1 };

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = list
                .Where(i => shouldDrawRow(i, searchString))
                .Select((i, index) => new TableViewItem<T>(i, index) as TreeViewItem)
                .ToList();
            SetupParentsAndChildrenFromDepths(root, rows);
            return rows;
        }
        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as TableViewItem<T>;
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var drawCellArgs = new DrawCellArgs<T>
                {
                    item = item.data,
                    rect = args.GetCellRect(i),
                    selected = args.selected,
                    focused = args.focused
                };
                columns[args.GetColumn(i)].drawCell(drawCellArgs);
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return base.CanMultiSelect(item);
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return base.CanStartDrag(args);
        }

        protected override void CommandEventHandling()
        {
            base.CommandEventHandling();
        }

        protected override void ContextClicked()
        {
            base.ContextClicked();
        }

        protected override void ContextClickedItem(int id)
        {
            base.ContextClickedItem(id);
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return base.DoesItemMatchSearch(item, search);
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            return base.GetCustomRowHeight(row, item);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return base.HandleDragAndDrop(args);
        }

        protected override void KeyEvent()
        {
            base.KeyEvent();
        }

        protected override void RefreshCustomRowHeights()
        {
            base.RefreshCustomRowHeights();
        }


        protected override void SearchChanged(string newSearch)
        {
            base.SearchChanged(newSearch);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            base.SetupDragAndDrop(args);
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
        }
    }
}