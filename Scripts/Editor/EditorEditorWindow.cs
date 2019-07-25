using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor { 
	public class EditorEditorWindow : EditorWindow
	{
		private TreeViewItem rootItem;

		private TreeViewItem selectedItem;
		private ElementInspector inspector;

		private Box treeViewContainer;
		private VisualElement previewRoot;

		private Dictionary<VisualElement, TreeViewItem> treeViewMap;

		[MenuItem("Window/UIElements/EditorEditorWindow")]
		public static void ShowExample() {
			EditorEditorWindow wnd = GetWindow<EditorEditorWindow>();
			wnd.titleContent = new GUIContent("Editor Editor Window");
		}

		public void OnEnable() {
			SetVisual();
		}

		void SetVisual() {
			VisualElement root = rootVisualElement;

			root.Clear();

			var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UIElementsEditor/Styles/Editor/EditorEditorWindow.uxml");
			VisualElement labelFromUXML = visualTree.CloneTree();
			root.Add(labelFromUXML);

			var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UIElementsEditor/Styles/Editor/EditorEditorWindow.uss");

			root.styleSheets.Add(styleSheet);

			//Tree View
			treeViewContainer = root.Q<Box>("treeViewContainer");

			treeViewMap = new Dictionary<VisualElement, TreeViewItem>();

			previewRoot = root.Q<VisualElement>("PreviewRoot");
			
			rootItem = CreateTreeViewItem(previewRoot);
			rootItem.IsRoot = false;

			rootItem.AppendTo(treeViewContainer);


			for (int i = 0; i < 3; i++) { 
				CreateTestLabel();
			}


			//Inspector
			inspector = new ElementInspector(root.Q<VisualElement>("inspectorRoot"));
			inspector.onChangeProperty += OnChangeInspector;

			///////
			var button = new Button(() => {
				SetVisual();
			});
			button.text = "Reload";
			root.Add(button);
		}

		void CreateTestLabel() {
			var label = new Label();

			int id = Random.Range(1, 5000);
			label.text = "Random: " + id;
			if(Random.value < 0.5f) {
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

		void SetParent(VisualElement element, VisualElement newParent, int index = -1) {
			if (treeViewMap.ContainsKey(newParent) && treeViewMap.ContainsKey(element)) {
				var parent = treeViewMap[newParent];
				treeViewMap[element].AppendTo(parent, index);
				parent.UpdateDisplay();
			}
		}

		
	}
}