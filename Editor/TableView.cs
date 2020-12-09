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
        public TableState state;
        public Dictionary<int, DrawCell<T>> columnDrawCell = new Dictionary<int, DrawCell<T>>();
        public Dictionary<int, Func<T, object>> columnGetSortingValue = new Dictionary<int, Func<T, object>>();
        public TreeViewEvents<T> Events { get; } = new TreeViewEvents<T>();

        public IList<T> list;

        public Column[] Columns
        {
            get => state.columnState.columns.Cast<Column>().ToArray();
            set => InitColumns(value);
        }

        public TableView(TableState tableState, IList<T> list) : base(tableState.treeViewState)
        {
            state = tableState;
            this.list = list;
        }

        public TableView(Column[] columns, TableState tableState, IList<T> list) : this(tableState, list)
        {
            InitColumns(columns);
        }

        private void InitColumns(Column[] columns)
        {
            for (var i = 0; i < columns.Length; i++)
            {
                var column = columns[i];
                columnDrawCell[i] = column.drawCell;
                columnGetSortingValue[i] = column.getSortingValue;
            }

            state.Init(columns, out var header);
            multiColumnHeader = header;
            multiColumnHeader.sortingChanged +=
                c => SortIfNeeded(rootItem, GetRows());
            Reload();
        }

        public void OnGUI()
        {
            var rect = GUILayoutUtility.GetRect(0, 1000, 0, 1000);
            OnGUI(rect);
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
            Events.reloadList?.Invoke(list);

            var rows = CreateRows().ToList();

            SetupParentsAndChildrenFromDepths(root, rows);
            SortIfNeeded(root, rows);
            return rows;

            IEnumerable<TreeViewItem> CreateRows()
            {
                for (var i = 0; i < list.Count; i++)
                {
                    var item = list[i];
                    if (Events.shouldDrawRow(item, searchString))
                        yield return new TreeViewItem(i, 0);
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = GetItemData(args.item.id);
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                if (!columnDrawCell.TryGetValue(args.GetColumn(i), out var draw))
                    continue;
                var drawCellArgs = new DrawCellArgs<T>
                {
                    item = item,
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
            return Events.canMultiSelect?.Invoke(GetItemData(item)) ?? false;
        }

        protected override void CommandEventHandling()
        {
            var current = Event.current;
            if (current.type != EventType.ExecuteCommand && current.type != EventType.ValidateCommand)
                return;
            
            TryExecuteCommand(current, () => Events.onDelete?.Invoke(GetSelectedItems()), new[]{"Delete", "SoftDelete"});
            TryExecuteCommand(current, () => Events.onDuplicate?.Invoke(GetSelectedItems()), "Duplicate");
            TryExecuteCommand(current, () => Events.onCopy?.Invoke(GetSelectedItems()), "Copy");
            TryExecuteCommand(current, () => Events.onCut?.Invoke(GetSelectedItems()), "Cut");
            TryExecuteCommand(current, () => Events.onPaste?.Invoke(GetSelectedItems()), "Paste");
            base.CommandEventHandling();
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
            Events.contextClicked?.Invoke();
        }

        protected override void ContextClickedItem(int id)
        {
            Events.contextClickedItem?.Invoke(GetSelectedItem());
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            Events.selectionChanged?.Invoke(selectedIds.Select(GetItemData).ToList());
        }

        protected override void SingleClickedItem(int id)
        {
            Events.singleClickedItem?.Invoke(GetSelectedItem());
        }

        protected override void DoubleClickedItem(int id)
        {
            Events.doubleClickedItem?.Invoke(GetSelectedItem());
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return Events.setupDragAndDrop != null;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            Events.setupDragAndDrop(args.draggedItemIDs.Select(GetItemData));
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            return Events.handleDragAndDrop?.Invoke(args.performDrop, args.insertAtIndex) ?? DragAndDropVisualMode.None;
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