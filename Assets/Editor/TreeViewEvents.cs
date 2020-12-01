using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace ES
{
    public class TreeViewEvents<T>
    {
        public ShouldDrawRow<T> ShouldDrawRow = (_, __) => true;
        public Predicate<T> CanMultiSelect;
        public Predicate<T> CanStartDrag;
        public Action<IEnumerable<T>> OnDelete; //selection
        public Action<IEnumerable<T>> OnDuplicate; //selection
        public Action<IEnumerable<T>> OnCopy; //selection
        public Action<IEnumerable<T>> OnCut; //selection
        public Action<IEnumerable<T>> OnPaste; //selection
        public Action ContextClicked;
        public Action<T> ContextClickedItem;
        public Action<T> DoubleClickedItem;
        public Action<IEnumerable<T>> SetupDragAndDrop;
        public HandleDragAndDrop HandleDragAndDrop;
        public Action<IList<T>> SelectionChanged;
        public Action<T> SingleClickedItem;


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