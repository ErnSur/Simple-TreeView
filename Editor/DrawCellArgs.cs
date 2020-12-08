using UnityEngine;

namespace ES
{
    public struct DrawCellArgs<T>
    {
        public T item;
        public Rect rect;
        public bool selected;
        public bool focused;
    }
}