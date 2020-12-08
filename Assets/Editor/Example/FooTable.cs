using System.Collections.Generic;

namespace ES
{
    [System.Serializable]
    public class FooTable : Table<Foo>
    {
        public FooTable(Column[] columns,TableState state, IList<Foo> list) : base(columns, state,list)
        {
        }
    }

    public class Foo
    {
        public string name; public int id;
    }
}
