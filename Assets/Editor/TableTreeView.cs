using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ES
{
    public class TableTreeView<T> : TreeView
    {
        private static readonly string[] DeleteCommands = new[] {"Delete", "SoftDelete"};

        public IList<T> list;
        public TreeViewEvents<T> events = new TreeViewEvents<T>();
        public Table<T>.Column[] columns;

        public T GetItemData(int treeViewItemId) => list[treeViewItemId];
        public T GetItemData(TreeViewItem item) => GetItemData(item.id);

        public TableTreeView(IList<T> list, TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state,
            multiColumnHeader)
        {
            this.list = list;
            columns = multiColumnHeader.state.columns.Cast<Table<T>.Column>().ToArray();
            multiColumnHeader.sortingChanged +=
                c => SortIfNeeded(rootItem, GetRows());
            Reload();
        }

        protected override TreeViewItem BuildRoot() => new TreeViewItem {id = -1, depth = -1, displayName = "root"};

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = list
                .Where(i => events.ShouldDrawRow(i, searchString))
                .Select((_, index) => new TreeViewItem(index, 0))
                .ToList();

            SetupParentsAndChildrenFromDepths(root, rows);
            SortIfNeeded(root, rows);
            Debug.Log($"Build Rows");
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item;
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var drawCellArgs = new DrawCellArgs<T>
                {
                    item = GetItemData(item.id),
                    rect = args.GetCellRect(i),
                    selected = args.selected,
                    focused = args.focused
                };
                columns[args.GetColumn(i)].drawCell(drawCellArgs);
            }
        }

        private void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1 || multiColumnHeader.sortedColumnIndex == -1 ||
                multiColumnHeader.state.sortedColumns.Length == 0)
                return;

            var sortedColumns = multiColumnHeader.state.sortedColumns;

            var orderedRows = DoSort(sortedColumns, rows).ToList();
            rows.Clear();
            foreach (var i in orderedRows)
            {
                rows.Add(i);
            }

            root.children = orderedRows;
            Repaint();
        }

        private IOrderedEnumerable<TreeViewItem> DoSort(int[] sortedColumns, IEnumerable<TreeViewItem> items)
        {
            var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[0]);
            var selector = columns[sortedColumns[0]].selector;
            var orderedQuery = items.Order(i => selector?.Invoke(GetItemData(i)), ascending);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                if (columns[sortedColumns[i]]?.selector == null)
                    continue;
                ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);
                selector = columns[sortedColumns[i]].selector;
                orderedQuery = orderedQuery.ThenBy(item => selector(GetItemData(item)), ascending);
            }

            return orderedQuery;
        }

        private IEnumerable<T> GetSelectedItems() => GetSelection().Select(GetItemData);

        private T GetSelectedItem() => GetSelectedItems().FirstOrDefault();

        protected override bool CanMultiSelect(TreeViewItem item) =>
            events.CanMultiSelect?.Invoke(GetItemData(item)) ?? false;

        protected override void CommandEventHandling()
        {
            var current = Event.current;
            if (current.type != EventType.ExecuteCommand && current.type != EventType.ValidateCommand)
            {
                return;
            }

            TryExecuteCommand(current, () => events.OnDelete?.Invoke(GetSelectedItems()), DeleteCommands);
            TryExecuteCommand(current, () => events.OnDuplicate?.Invoke(GetSelectedItems()), "Duplicate");
            TryExecuteCommand(current, () => events.OnCopy?.Invoke(GetSelectedItems()), "Copy");
            TryExecuteCommand(current, () => events.OnCut?.Invoke(GetSelectedItems()), "Cut");
            TryExecuteCommand(current, () => events.OnPaste?.Invoke(GetSelectedItems()), "Pase");
            base.CommandEventHandling();
        }

        private void TryExecuteCommand(Event current, Action action, params string[] commandNames)
        {
            if (HasFocus() && commandNames.Contains(current.commandName))
            {
                if (current.type == EventType.ExecuteCommand)
                {
                    action?.Invoke();
                }

                current.Use();
                GUIUtility.ExitGUI();
            }
        }

        protected override void ContextClicked() => events.ContextClicked?.Invoke();

        protected override void ContextClickedItem(int id) => events.ContextClickedItem?.Invoke(GetSelectedItem());

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            events.SelectionChanged?.Invoke(selectedIds.Select(GetItemData).ToList());
        }

        protected override void SingleClickedItem(int id) =>
            events.SingleClickedItem?.Invoke(GetSelectedItem());

        protected override void DoubleClickedItem(int id) =>
            events.DoubleClickedItem?.Invoke(GetSelectedItem());

        protected override bool CanStartDrag(CanStartDragArgs args) =>
            events.SetupDragAndDrop != null;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args) =>
            events.SetupDragAndDrop(GetSelectedItems());

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return events.HandleDragAndDrop?.Invoke(args.performDrop, args.insertAtIndex) ?? DragAndDropVisualMode.None;
        }

        // TODO: Check in cs source if this is used somewhere
        //protected override bool DoesItemMatchSearch(TreeViewItem item, string search)

        // TODO: Add Later
        //protected override float GetCustomRowHeight(int row, TreeViewItem item)

        // TODO: Add Later
        //protected override void KeyEvent() 

        // TODO: Add Later
        //protected override void RefreshCustomRowHeights() 

        // TODO: Add Later
        //protected override void SearchChanged(string newSearch)
    }
}