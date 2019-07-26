using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.victorafael.EditorEditor { 
    public interface IDraggableItem
    {
        VisualElement GetElement();
        string GetDragLabel();
    }
}