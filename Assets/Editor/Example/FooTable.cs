﻿using System.Collections.Generic;

namespace QuickEye.UI.Editor
{
    [System.Serializable]
    public class FooTable : TableView<Foo>
    {
        public FooTable(Column[] columns, TableState state, IList<Foo> list) : base(columns, state, list) { }
    }

    public class Foo
    {
        public string name;
        public int id;
    }
}