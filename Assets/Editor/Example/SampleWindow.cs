using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace QuickEye.UI.Editor
{
    public class SampleWindow : EditorWindow
    {
        [MenuItem("Window/FOOOO")]
        public static void O() => GetWindow<SampleWindow>(typeof(SceneView));
        
        [SerializeField]
        private TableState state = new TableState();
        private FooTable table;
        public Foo[] list = new[]
        {
            new Foo{name = "Eric", id=0},
            new Foo{name = "Andrew", id=1},
        };
        
        private SearchField searchField;
        
        private void OnEnable()
        {
            searchField = new SearchField();
            table = new FooTable(CreateTableColumns(), state, list);
            table.Events.shouldDrawRow = (i,search) =>
            {
                return i.name.Contains(search) || i.id.ToString().Contains(search);
            };
        }

        private void OnGUI()
        {
            table.searchString = searchField.OnGUI(table.searchString);
            table.OnGUI();
        }

        private FooTable.Column[] CreateTableColumns()
        {
            return new[]
            {
                new FooTable.Column
                {
                    headerContent = new GUIContent("Name"),
                    drawCell = args =>
                    {
                        GUI.Label(args.rect,args.item.name.ToString());
                    },
                    getSortingValue = i => i.name
                },
                new FooTable.Column
                {
                    headerContent = new GUIContent("Id"),
                    drawCell = args =>
                    {
                        GUI.Label(args.rect,args.item.id.ToString());
                    },
                    getSortingValue = i => i.id
                }
            };
        }
    }
}
