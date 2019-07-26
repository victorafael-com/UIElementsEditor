using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class DragManipulator : MouseManipulator {
        private bool m_active = false;
        IDraggableItem m_draggable;
        EditorEditorWindow m_editor;

        public DragManipulator(IDraggableItem draggable, EditorEditorWindow editor) {
            activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse });

            m_active = false;
            m_draggable = draggable;
            m_editor = editor;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e) {
            if (m_active) {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e)) {
                m_active = true;
                m_editor.StartDrag(m_draggable);
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent e) {
            if (!m_active || !target.HasMouseCapture())
                return;

            target.CaptureMouse();
            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e) {
            m_editor.ClearDrag();

            if (!m_active || !target.HasMouseCapture() || !CanStopManipulation(e)) {
                m_active = false;
                return;
            }
            m_active = false;
            target.ReleaseMouse();
            e.StopPropagation();
            m_editor.DragComplete();
        }
    }
}