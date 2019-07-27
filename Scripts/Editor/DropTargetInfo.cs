using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor {
    public class DropTargetInfo {
        public VisualElement element;
        public bool isPrevious;

        public DropTargetInfo(VisualElement element) {
            this.element = element;
            this.isPrevious = false;
        }

        public DropTargetInfo(VisualElement element, bool isPrevious) {
            this.element = element;
            this.isPrevious = isPrevious;
        }
    }
}