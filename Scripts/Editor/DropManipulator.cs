using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class DropManipulator : MouseManipulator {
        private bool m_isHover = false;
        private EditorEditorWindow m_editor;
        private DropTargetInfo m_dropInfo;

        public DropManipulator(EditorEditorWindow editor, DropTargetInfo dropInfo) {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });

            m_isHover = false;
            m_editor = editor;
            m_dropInfo = dropInfo;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            target.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
            target.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        void OnMouseEnter(MouseEnterEvent evt) {
            m_isHover = m_editor.IsDragging;
            if (m_isHover) {
                m_editor.currentDropTarget = m_dropInfo;
            }
        }
       
        void OnMouseLeave(MouseLeaveEvent evt) {
            if (m_isHover) {
                m_editor.currentDropTarget = null;
            }
            m_isHover = false;
        }
    }
}