using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class DragStarterManipulator : MouseManipulator {
        private bool m_active = false;
        IDraggableItem m_draggable;
        EditorEditorWindow m_editor;

        public DragStarterManipulator(IDraggableItem draggable, EditorEditorWindow editor) {
            activators.Add(new ManipulatorActivationFilter {button = MouseButton.LeftMouse });

            m_active = false;
            m_draggable = draggable;
            m_editor = editor;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
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
            
            if (!m_active  || !CanStopManipulation(e)) {
                m_active = false;
                return;
            }
            m_active = false;
            target.ReleaseMouse();
            m_editor.DragComplete();
        }
    }
}