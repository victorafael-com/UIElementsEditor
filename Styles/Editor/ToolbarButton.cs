using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class ToolbarButton<T> where T : VisualElement {
        public event Action<ToolbarButton<T>> onClick;
        private const string spriteRoot = "Scripts/Editor/Resources/{0}";

        private Action<T> setupElement;

        private Button m_button;

        public Button Button => m_button;

        public ToolbarButton(string sprite, string label, VisualElement root, Action<T> setup = null) {
            setupElement = setup;

            m_button = new Button(OnClick);
            m_button.AddToClassList("eeditor-toolbarButton");

            var icon = new VisualElement();
            icon.AddToClassList("eeditor-icon");

            icon.style.backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(
                string.Format(EditorEditorWindow.BasePath, string.Format(spriteRoot, sprite))
            );

            m_button.Add(icon);
            m_button.Add(new Label(label));

            m_button.RegisterCallback<DragUpdatedEvent>(DragUpdated);

            root.Add(m_button);
        }

        void DragUpdated(DragUpdatedEvent upd) {
            Debug.Log("Updated");
        }

        public void OnClick() {
            onClick?.Invoke(this);
        }

        public T GetNewElement() {
            var element = Activator.CreateInstance<T>();

            setupElement?.Invoke(element);

            return element;
        }
    }
}