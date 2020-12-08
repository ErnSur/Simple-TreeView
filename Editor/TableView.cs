using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace QuickEye.UI.Editor
{
    public class TableView<T> : TreeView
    {
        private static readonly string[] DeleteCommands = {"Delete", "SoftDelete"};
        public Dictionary<int, DrawCell<T>> columnDrawCell = new Dictionary<int, DrawCell<T>>();
        public Dictionary<int, Func<T, object>> columnGetSortingValue = new Dictionary<int, Func<T, object>>();
        public TreeViewEvents<T> events = new TreeViewEvents<T>();

        public IList<T> list;

        public TableView(IList<T> list, TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state,
            multiColumnHeader)
        {
            this.list = list;
            multiColumnHeader.sortingChanged +=
                c => SortIfNeeded(rootItem, GetRows());
            Reload();
        }

        public T GetItemData(int treeViewItemId)
        {
            return list[treeViewItemId];
        }

        public T GetItemData(TreeViewItem item)
        {
            return GetItemData(item.id);
        }

        protected override TreeViewItem BuildRoot()
        {
            return new TreeViewItem {id = -1, depth = -1, displayName = "root"};
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            events.reloadList?.Invoke(list);
            var rows = list
                .Where(obj => events.shouldDrawRow(obj, searchString))
                .Select((_, index) => new TreeViewItem(index, 0))
                .ToList();

            SetupParentsAndChildrenFromDepths(root, rows);
            SortIfNeeded(root, rows);
            Debug.Log("Build Rows");
            return rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                if (!columnDrawCell.TryGetValue(args.GetColumn(i), out var draw))
                    continue;
                var drawCellArgs = new DrawCellArgs<T>
                {
                    item = GetItemData(args.item.id),
                    rect = args.GetCellRect(i),
                    selected = args.selected,
                    focused = args.focused
                };
                draw(drawCellArgs);
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
            foreach (var i in orderedRows) rows.Add(i);

            root.children = orderedRows;
            Repaint();
        }

        private IOrderedEnumerable<TreeViewItem> DoSort(int[] sortedColumns, IEnumerable<TreeViewItem> items)
        {
            var ascending = multiColumnHeader.IsSortedAscending(sortedColumns[0]);
            columnGetSortingValue.TryGetValue(sortedColumns[0], out var selector);
            var orderedQuery = items.Order(i => selector?.Invoke(GetItemData(i)), ascending);
            for (var i = 1; i < sortedColumns.Length; i++)
            {
                if (!columnGetSortingValue.TryGetValue(sortedColumns[i], out selector))
                    continue;
                ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);
                orderedQuery = orderedQuery.ThenBy(item => selector(GetItemData(item)), ascending);
            }

            return orderedQuery;
        }

        public IEnumerable<T> GetSelectedItems()
        {
            return GetSelection().Select(GetItemData);
        }

        private T GetSelectedItem()
        {
            return GetSelectedItems().FirstOrDefault();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return events.canMultiSelect?.Invoke(GetItemData(item)) ?? false;
        }

        protected override void CommandEventHandling()
        {
            base.CommandEventHandling();
            var current = Event.current;
            if (current.type != EventType.ExecuteCommand)
                return;

            TryExecuteCommand(current, () => events.onDelete?.Invoke(GetSelectedItems()), DeleteCommands);
            TryExecuteCommand(current, () => events.onDuplicate?.Invoke(GetSelectedItems()), "Duplicate");
            TryExecuteCommand(current, () => events.onCopy?.Invoke(GetSelectedItems()), "Copy");
            TryExecuteCommand(current, () => events.onCut?.Invoke(GetSelectedItems()), "Cut");
            TryExecuteCommand(current, () => events.onPaste?.Invoke(GetSelectedItems()), "Paste");
        }

        private void TryExecuteCommand(Event current, Action action, params string[] commandNames)
        {
            if (HasFocus() && commandNames.Contains(current.commandName))
            {
                if (current.type == EventType.ExecuteCommand)
                    action?.Invoke();

                current.Use();
                GUIUtility.ExitGUI();
            }
        }

        protected override void ContextClicked()
        {
            events.contextClicked?.Invoke();
        }

        protected override void ContextClickedItem(int id)
        {
            events.contextClickedItem?.Invoke(GetSelectedItem());
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            events.selectionChanged?.Invoke(selectedIds.Select(GetItemData).ToList());
        }

        protected override void SingleClickedItem(int id)
        {
            events.singleClickedItem?.Invoke(GetSelectedItem());
        }

        protected override void DoubleClickedItem(int id)
        {
            events.doubleClickedItem?.Invoke(GetSelectedItem());
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return events.setupDragAndDrop != null;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            events.setupDragAndDrop(args.draggedItemIDs.Select(GetItemData));
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return events.handleDragAndDrop?.Invoke(args.performDrop, args.insertAtIndex) ?? DragAndDropVisualMode.None;
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