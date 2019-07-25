using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class TreeViewItem {
        public event Action<TreeViewItem> onClick;

        public VisualElement targetElement;

        private VisualElement root;

        private Foldout expandToggle;
        private Label nameLabel;

        private Button itemHandler;
        private VisualElement childContainer;
        private VisualElement dropDisplayLine;

        const string nameFormat = "#{0}";
        const string classFormat = ".{0} ";

        public bool IsRoot { get; set; }

        public bool Selected {
            set {
                if (value) {
                    root.AddToClassList("eeditor-selected");
                } else {
                    root.RemoveFromClassList("eeditor-selected");
                }
            }
        }

        public TreeViewItem(VisualElement element) {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(string.Format(EditorEditorWindow.BasePath, "Styles/Editor/TreeViewItem.uxml"));
            root = visualTree.CloneTree();

            targetElement = element;

            itemHandler = root.Q<Button>("dragHandler");
            expandToggle = root.Q<Foldout>("expandChildren");
            nameLabel = root.Q<Label>("elementName");
            childContainer = root.Q<VisualElement>("container");
            dropDisplayLine = childContainer.Q<VisualElement>("dropLine");

            var type = targetElement.GetType();
            root.Q<Label>("elementType").text = type.Name;

            UpdateDisplay();

            expandToggle.value = false;
            SetVisible(childContainer, false);

            RegisterEvents();
        }

        void RegisterEvents() {
            itemHandler.clickable.clicked += OnClickHandler;
            expandToggle.RegisterValueChangedCallback(ToggleDisplayChildren);
        }

        void OnClickHandler() {
            if (onClick != null && !IsRoot) {
                onClick?.Invoke(this);
            }
        }

        void ToggleDisplayChildren(ChangeEvent<bool> changeEvent) {
            SetVisible(childContainer, changeEvent.newValue);
        }

        public void UpdateDisplay() {

            if (string.IsNullOrEmpty(targetElement.name)) {
                SetVisible(nameLabel, false);
            } else {
                SetVisible(nameLabel, true);
                nameLabel.text = string.Format(nameFormat, targetElement.name);
            }

            if (IsVisible(targetElement)) {
                root.RemoveFromClassList("eeditor-invisible");
            } else {
                root.AddToClassList("eeditor-invisible");
            }

            SetVisible(expandToggle, childContainer.childCount > 1);

            targetElement.MarkDirtyRepaint();
        }

        public void Expand() {
            expandToggle.value = true;
            SetVisible(childContainer, true);
        }

        public void Destroy() {
            targetElement.RemoveFromHierarchy();
            root.RemoveFromHierarchy();
        }


        public void AppendTo(VisualElement parent, int index = -1) {
            if (index < 0) {
                parent.Add(root);
            } else {
                parent.Insert(index, root);
            }
        }

        /// <summary>
        /// Attaches the tree element on item and Moves targetElement to item.targetElement
        /// </summary>
        /// <param name="item">The item to attach this treeVIewItem</param>
        /// <param name="index">The index to insert (-1 = last item)</param>
        public void AppendTo(TreeViewItem item, int index = -1) {
            AppendTo(item.childContainer, index);

            if(index < 0) {
                item.targetElement.Add(targetElement);
            } else {
                item.targetElement.Insert(index, targetElement);
            }
        }

        bool IsVisible(VisualElement element) {
            return element.style.display != DisplayStyle.None;
        }
        void SetVisible(VisualElement element, bool visible) {
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}