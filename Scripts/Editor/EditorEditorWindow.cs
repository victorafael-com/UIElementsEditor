using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class EditorEditorWindow : EditorWindow {
        private static string basePath = null;

        private TreeViewItem rootItem;

        private TreeViewItem selectedItem;
        private ElementInspector inspector;

        private VisualElement toolbarRoot;
        private Box treeViewContainer;
        private VisualElement previewRoot;

        private Dictionary<VisualElement, TreeViewItem> treeViewMap;

        public static string BasePath => basePath;

        [MenuItem("Window/UIElements/EditorEditorWindow")]
        public static void ShowExample() {
            EditorEditorWindow wnd = GetWindow<EditorEditorWindow>();
            wnd.titleContent = new GUIContent("Editor Editor Window");
        }

        public void OnEnable() {
            //Sets the base path (In case of renaming or moving folder)
            var asset = MonoScript.FromScriptableObject(this);
            string myPath = AssetDatabase.GetAssetPath(asset);
            basePath = myPath.Substring(0, myPath.IndexOf("Scripts/Editor")) + "{0}";

            SetVisual();
        }

        void SetVisual() {
            VisualElement root = rootVisualElement;

            root.Clear();

            //var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIElementsEditor/Styles/Editor/EditorEditorWindow.uxml");
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format(basePath, "Styles/Editor/EditorEditorWindow.uxml"));
            //var visualTree = LoadFromRelativePath<VisualTreeAsset>("EditorEditorWindow.uxml");
            VisualElement labelFromUXML = visualTree.CloneTree();
            root.Add(labelFromUXML);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(string.Format(basePath, "Styles/Editor/EditorEditorWindow.uss"));

            root.styleSheets.Add(styleSheet);

            //Tree View
            treeViewContainer = root.Q<Box>("treeViewContainer");

            treeViewMap = new Dictionary<VisualElement, TreeViewItem>();

            previewRoot = root.Q<VisualElement>("PreviewRoot");

            rootItem = CreateTreeViewItem(previewRoot);
            rootItem.IsRoot = true;

            rootItem.AppendTo(treeViewContainer);

            for (int i = 0; i < 3; i++) {
                CreateTestLabel();
            }

            //Inspector
            inspector = new ElementInspector(root.Q<VisualElement>("inspectorRoot"));
            inspector.onChangeProperty += OnChangeInspector;
            
            //Toolbar
            toolbarRoot = root.Q<VisualElement>("toolbarRoot");
            CreateToolbar();

            ///////
            var button = new Button(() => {
                SetVisual();
            });
            button.text = "Reload";
            root.Add(button);
        }

        private void OnGUI() {
            //I will evaluate keyboard events here while I'm trying to figure out how to handle them on UIElements
            var evt = Event.current;
            switch (evt.type) {
                case EventType.KeyUp:
                    OnKeyUp(evt);
                    break;
            }
        }

        void CreateToolbar() {
            VisualElement layoutArea = CreateToolbarArea("Layout");

            new ToolbarButton<VisualElement>("empty-horizontal.png", "Horizontal", layoutArea, (VisualElement ve) => ve.style.flexDirection = FlexDirection.Row);
            new ToolbarButton<VisualElement>("empty-vertical.png", "Vertical", layoutArea);
            new ToolbarButton<Label>("label.png", "Label", layoutArea, (Label l) => l.text = "New Label");

            VisualElement inputArea = CreateToolbarArea("Input");
            new ToolbarButton<Button>("button.png", "Button", inputArea, (Button b) => b.text = "New Button");
        }

        VisualElement CreateToolbarArea(string label) {
            var root = new VisualElement();

            var lbl = new Label(label);
            lbl.AddToClassList("eeditor-toolbarHeader");

            root.Add(lbl);

            toolbarRoot.Add(root);

            return root;
        }

        void CreateTestLabel() {
            var label = new Label();

            int id = Random.Range(1, 5000);
            label.text = "Random: " + id;
            if (Random.value < 0.5f) {
                label.name = "label_" + id;
            }

            previewRoot.Add(label);

            var treeViewItem = CreateTreeViewItem(label);

            SetParent(label, previewRoot);
        }

        TreeViewItem CreateTreeViewItem(VisualElement element) {
            var item = new TreeViewItem(element);
            treeViewMap.Add(element, item);
            item.onClick += OnClickTreeViewItem;
            return item;
        }

        private void OnChangeInspector() {
            selectedItem.UpdateDisplay();
        }

        private void OnClickTreeViewItem(TreeViewItem obj) {
            if (selectedItem != null)
                selectedItem.Selected = false;

            inspector.SetupInspector(obj.targetElement);
            selectedItem = obj;
            selectedItem.Selected = true;
        }

        private void OnKeyUp(Event evt) {
            switch (evt.keyCode) {
                case KeyCode.Delete:
                case KeyCode.Backspace:
                    if (selectedItem != null) {
                        selectedItem.Destroy();
                        selectedItem = null;
                        inspector.Clear();
                    }
                    break;
                case KeyCode.Escape:
                    if (selectedItem != null) {
                        selectedItem.Selected = false;
                        selectedItem = null;
                        inspector.Clear();
                    }
                    break;
            }
        }

        void SetParent(VisualElement element, VisualElement newParent, int index = -1) {
            if (treeViewMap.ContainsKey(newParent) && treeViewMap.ContainsKey(element)) {
                var parent = treeViewMap[newParent];
                treeViewMap[element].AppendTo(parent, index);
                parent.UpdateDisplay();
            }
        }
    }
}