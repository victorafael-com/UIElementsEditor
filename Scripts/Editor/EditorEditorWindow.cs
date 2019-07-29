using System;
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

        private Label dragHandler;
        private IDraggableItem currentDraggable;
        public DropTargetInfo currentDropTarget;


        private Dictionary<VisualElement, TreeViewItem> treeViewMap;

        public static string BasePath => basePath;

        public bool IsDragging { get; private set; }

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
            VisualElement content = visualTree.CloneTree();
            root.Add(content);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(string.Format(basePath, "Styles/Editor/EditorEditorWindow.uss"));

            root.styleSheets.Add(styleSheet);

            //Tree View
            treeViewContainer = root.Q<Box>("treeViewContainer");

            treeViewMap = new Dictionary<VisualElement, TreeViewItem>();

            previewRoot = root.Q<VisualElement>("PreviewRoot");

            rootItem = CreateTreeViewItem(previewRoot);
            rootItem.IsRoot = true;

            rootItem.AppendTo(treeViewContainer);

            //Inspector
            inspector = new ElementInspector(root.Q<VisualElement>("inspectorRoot"));
            inspector.onChangeProperty += OnChangeInspector;
            
            //Toolbar
            toolbarRoot = root.Q<VisualElement>("toolbarRoot");
            CreateToolbar();

            //Drag Label
            dragHandler = new Label();
            dragHandler.AddToClassList("eeditor-dragLabel");
            dragHandler.focusable = false;
            dragHandler.style.display = DisplayStyle.None;

            root.Add(dragHandler);

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

            CreateButton<VisualElement>("empty-vertical.png", "Vertical", layoutArea);
            CreateButton<VisualElement>("empty-horizontal.png", "Horizontal", layoutArea, (VisualElement ve) => ve.style.flexDirection = FlexDirection.Row);
            CreateButton<Label>("label.png", "Label", layoutArea, (Label l) => l.text = "New Label");

            VisualElement inputArea = CreateToolbarArea("Input");
            CreateButton<Button>("button.png", "Button", inputArea, (Button b) => b.text = "New Button");
        }

        void CreateButton<T>(string image, string label, VisualElement parent, Action<T> action = null) where T : VisualElement {
            var button = new ToolbarButton<T>(this, image, label, parent, action);
            button.onClick += OnClickToolbarItem;
        }

        VisualElement CreateToolbarArea(string label) {
            var root = new VisualElement();

            var lbl = new Label(label);
            lbl.AddToClassList("eeditor-toolbarHeader");

            root.Add(lbl);

            toolbarRoot.Add(root);

            return root;
        }

        TreeViewItem CreateTreeViewItem(VisualElement element) {
            var item = new TreeViewItem(this, element);
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

            if(obj == rootItem) {
                selectedItem = null;
                ClearSelection();
                return;
            }

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
                        ClearSelection();
                    }
                    break;
                case KeyCode.Escape:
                    if (selectedItem != null) {
                        selectedItem.Selected = false;
                        ClearSelection();
                    }
                    break;
            }
        }

        void ClearSelection() {
            selectedItem = null;
            inspector.Clear();
        }

        public void StartDrag(IDraggableItem draggable) {
            currentDraggable = draggable;
            rootVisualElement.AddToClassList("eeditor-dragging");
            IsDragging = true;
        }

        public void ClearDrag() {
            rootVisualElement.RemoveFromClassList("eeditor-dragging");
            rootVisualElement.MarkDirtyRepaint();
            IsDragging = false;
        }

        public void DragComplete() {
            if (currentDropTarget != null) {
                var element = currentDraggable.GetElement();

                if(element != currentDropTarget.element) { 
                    if (!treeViewMap.ContainsKey(element)) { 
                        CreateTreeViewItem(element);
                    }
                    SetParent(element, currentDropTarget.element);
                }
            } 

            currentDraggable = null;
        }

        private void OnClickToolbarItem<T>(ToolbarButton<T> button) where T : VisualElement {
            var target = selectedItem != null ? selectedItem : rootItem;

            var element = button.GetNewElement();

            var treeItem = CreateTreeViewItem(element);

           // treeItem.AppendTo(target);

            SetParent(element, target.targetElement);
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