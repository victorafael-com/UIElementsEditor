using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using Cursor = UnityEngine.UIElements.Cursor;
using System.Reflection;

namespace com.victorafael.EditorEditor {

    public static class InspectorStyleLine {
        public static VisualElement GetLine(IStyle style, PropertyInfo property, string label) {
            VisualElement element = new VisualElement();

            element.style.flexDirection = FlexDirection.Row;

            Toggle toggle = new Toggle();

            bool val;
            VisualElement editElement = GetStyleEditor(style, property, label, out val);

            toggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) => {
                editElement.SetEnabled(evt.newValue);
            });

           // element.Add(toggle);
            element.Add(editElement);

            toggle.value = val;
            editElement.SetEnabled(val);

            return element;
        }

        private static VisualElement GetStyleEditor(IStyle style, PropertyInfo property, string label, out bool isActive) {
            var t = property.PropertyType;
            object o = property.GetValue(style);
            isActive = true;

            if (t == typeof(StyleLength)) {

                var styleLength = (StyleLength)o;
                var el = CreateField<float>(label, styleLength.value.value, (ChangeEvent<float> evt) => 
                        property.SetValue(style, new StyleLength(evt.newValue)));

                el.Add(GetLengthLabel(styleLength.value.unit));

                return el;

            } else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StyleEnum<>)) {
                var field = t.GetProperty("value");
                var val = (Enum)field.GetValue(o);

                return CreateField<Enum>(label, val,
                    (ChangeEvent<Enum> evt) => {
                        var newStyleEnum = Activator.CreateInstance(property.PropertyType); //New StyleEnum<T>
                        field.SetValue(newStyleEnum, evt.newValue);
                        property.SetValue(style, newStyleEnum);
                    });

            } else if (t == typeof(StyleFloat)) {
                var styleFloat = (StyleFloat)o;
                return CreateField<float>(label, styleFloat.value, (ChangeEvent<float> evt) =>
                        property.SetValue(style, new StyleFloat(evt.newValue)));

            } else if (t == typeof(StyleInt)) {
                var styleInt = (StyleInt)o;
                return CreateField<int>(label, styleInt.value, (ChangeEvent<int> evt) =>
                        property.SetValue(style, new StyleInt(evt.newValue)));

            } else if (t == typeof(StyleColor)) {
                var styleColor = (StyleColor)o;
                return CreateField<Color>(label, styleColor.value, (ChangeEvent<Color> evt) =>
                        property.SetValue(style, new StyleColor(evt.newValue)));

            } else if (t == typeof(StyleCursor)) {
                return new Label(label + " (Unsupported: Cursor)");
            } else if (t == typeof(StyleFont)) {
                return new Label(label + " (Unsupported: Font)");
            } else if (t == typeof(StyleBackground)) {
                return new Label(label + " (Unsupported: Background)");
            } else {
                return new Label(label + "? > " + t.ToString());
            }
        }

        static Label GetLengthLabel(LengthUnit unit) {
            Label lbl = new Label();
            switch (unit) {
                case LengthUnit.Pixel:
                    lbl.text = "px";
                    break;
                default:
                    lbl.text = unit.ToString();
                    break;
            }
            return lbl;
        }

        static VisualElement CreateField<T>(string label, T value, EventCallback<ChangeEvent<T>> evt) {
            VisualElement fieldElement;


            var lblText = label.Length > 16 ? label.Substring(0, 13) + "..." : label;


            var field = ElementInspector.GetField<T>(lblText, value, out fieldElement);
            field.RegisterValueChangedCallback(evt);

            fieldElement.AddToClassList("eeditor-inspectorField");
            fieldElement.tooltip = label;

            VisualElement root = new VisualElement();

            root.Add(fieldElement);

            root.AddToClassList("eeditor-inspectorProperty");

            return root;
        }
    }
}