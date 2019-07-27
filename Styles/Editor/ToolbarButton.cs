using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class ToolbarButton<T> : IDraggableItem where T : VisualElement {
        public event Action<ToolbarButton<T>> onClick;
        private const string spriteRoot = "Scripts/Editor/Resources/{0}";

        private Action<T> setupElement;

        private Button m_button;
        private string label;
        private EditorEditorWindow editor;

        public Button Button => m_button;

        public ToolbarButton(EditorEditorWindow editor, string sprite, string label, VisualElement root, Action<T> setup = null) {
            setupElement = setup;
            this.label = label;
            this.editor = editor;

            m_button = new Button(OnClick);
            m_button.AddToClassList("eeditor-toolbarButton");

            var elements = new VisualElement(); //This will solve the button focusing issues when clicking to the right or left from 

            var icon = new VisualElement();
            icon.AddToClassList("eeditor-icon");

            icon.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                string.Format(EditorEditorWindow.BasePath, string.Format(spriteRoot, sprite))
            );
            elements.Add(icon);

            var lbl = new Label(label);
            elements.Add(lbl);

            m_button.Add(elements);

            m_button.AddManipulator(new DragStarterManipulator(this, editor));

            root.Add(m_button);
        }

        public void OnClick() {
            onClick?.Invoke(this);
        }

        public T GetNewElement() {
            var element = Activator.CreateInstance<T>();

            setupElement?.Invoke(element);

            return element;
        }

        public VisualElement GetElement() {
            return GetNewElement();
        }

        public string GetDragLabel() {
            return label;
        }
    }
}