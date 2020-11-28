using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ES
{
    public delegate bool ShouldDrawRow<T>(T item, string searchString);
    public delegate void DrawCell<T>(DrawCellArgs<T> args);

    public class TableTreeView<T> : TreeView
    {
        private static readonly string[] DeleteCommands = new[] { "Delete", "SoftDelete" };

        public IList<T> list;
        public TreeViewEvents<T> events = new TreeViewEvents<T>();
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
                .Where(i => events.ShouldDrawRow(i, searchString))
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

        private IEnumerable<T> GetSelectedItems() => GetSelection().Cast<TableViewItem<T>>().Select(i => i.data);

        private T GetSelectedItem() => GetSelectedItems().FirstOrDefault();

        protected override bool CanMultiSelect(TreeViewItem item) => events.CanMultiSelect?.Invoke((item as TableViewItem<T>).data) ?? false;

        protected override void CommandEventHandling()
        {
            Event current = Event.current;
            if (current.type != EventType.ExecuteCommand && current.type != EventType.ValidateCommand)
            {
                return;
            }

            Debug.Log($"Command: {current.commandName}");
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
            events.SelectionChanged?.Invoke(selectedIds.Cast<TableViewItem<T>>().Select(i => i.data).ToList());
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