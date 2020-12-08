using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace QuickEye.UI.Editor
{
    public class TreeViewEvents<T>
    {
        public ShouldDrawRow<T> shouldDrawRow = (_, __) => true;
        public Predicate<T> canMultiSelect;
        public Action<IEnumerable<T>> onDelete; //selection
        public Action<IEnumerable<T>> onDuplicate; //selection
        public Action<IEnumerable<T>> onCopy; //selection
        public Action<IEnumerable<T>> onCut; //selection
        public Action<IEnumerable<T>> onPaste; //selection
        public Action contextClicked;
        public Action<T> contextClickedItem;
        public Action<T> doubleClickedItem;
        public Action<IEnumerable<T>> setupDragAndDrop;
        public HandleDragAndDrop handleDragAndDrop;
        public Action<IList<T>> selectionChanged;
        public Action<T> singleClickedItem;

        public Action<IList<T>> reloadList;

        //Sample SetupDragAndDrop implementation
        /*protected override void SetupDragAndDrop(IEnumerable<T> selection)
        {
            DragAndDrop.PrepareStartDrag();

            var objList = selection.ToList();
            DragAndDrop.objectReferences = objList;

            string title = objList.Count > 1 ? "<Multiple>" : objList[0].name;
            DragAndDrop.StartDrag(title);
        }*/
    }
    public delegate DragAndDropVisualMode HandleDragAndDrop(bool performDrop, int index);
    public delegate bool ShouldDrawRow<T>(T item, string searchString);
    public delegate void DrawCell<T>(DrawCellArgs<T> args);
}