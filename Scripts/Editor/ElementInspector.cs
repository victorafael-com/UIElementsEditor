using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace com.victorafael.EditorEditor { 
	public class ElementInspector
	{
		public event Action onChangeProperty;

		private VisualElement target;

		private VisualElement root;


		private VisualElement visualElementRoot;
		private VisualElement textElementRoot;
	

		public ElementInspector(VisualElement root) {
			this.root = root;
		}

		public void SetupInspector(VisualElement element) {
			root.Clear();

			target = element;

			CreateVisualElementInspector(root);

			if(target is TextElement)
				CreateTextElementInspector(root);
		}

		void CreateVisualElementInspector(VisualElement root) {
			/*visualElementRoot = new VisualElement();
			root.Add(visualElementRoot);*/

			visualElementRoot = CreateInspectorArea(root, "VisualElement");

			CreateInspectorLine<string>("Name", target.name, (string s) => target.name = s, visualElementRoot);
			CreateInspectorLine<bool>("Visible", target.style.display != DisplayStyle.None, (bool newValue) => SetVisible(target, newValue), visualElementRoot);
		}

		void CreateTextElementInspector(VisualElement root) {
			TextElement textElement = (TextElement)target;

			textElementRoot = CreateInspectorArea(root, "TextElemnt");

			float fontSize = textElement.style.fontSize.value.value;
			if (fontSize < 1)
				fontSize = 11;

			CreateInspectorLine<string>("Text", textElement.text, (string s) => textElement.text = s, textElementRoot);
			CreateInspectorLine<Enum>("Font Style", textElement.style.unityFontStyleAndWeight.value, (Enum f) => textElement.style.unityFontStyleAndWeight = (FontStyle) f, textElementRoot);
			CreateInspectorLine<float>("Font Size", fontSize, (float v) => textElement.style.fontSize = v, textElementRoot);
			CreateInspectorLine<Color>("Color", textElement.style.color.value, (Color c) => textElement.style.color = NoAlpha(c), textElementRoot);
			
		}

		VisualElement CreateInspectorArea(VisualElement root, string title) {
			var element = new VisualElement();
			root.Add(element);
			root.AddToClassList("eeditor-inspectorGroup");

			Label label = new Label(title);
			label.style.unityFontStyleAndWeight = FontStyle.Bold;

			element.Add(label);

			return element;
		}

		void CreateInspectorLine<T>(string label, T defaultValue, Action<T> onChange, VisualElement root){
			var line = new VisualElement();
			line.AddToClassList("eeditor-inspectorLine");

			var type = typeof(T);

			VisualElement element;
			var field = GetField<T>(label, defaultValue, out element);
			field.value = defaultValue;
			field.RegisterValueChangedCallback((ChangeEvent<T> change) => {
				onChange(change.newValue);
				onChangeProperty?.Invoke();
			});
			line.Add(element);
			
			root.Add(line);
		}

		#region Inspector Fields

		INotifyValueChanged<T> GetField<T>(string label, T value, out VisualElement field) {
			var type = typeof(T);

			field = null;

			if (type == typeof(string))
				field = new TextField(label);
			else if (type == typeof(int))
				field = new IntegerField(label);
			else if (type == typeof(bool))
				field = new Toggle(label);
			else if (type == typeof(Enum))
				field = new EnumField(label, Convert<Enum>(value));
			else if (type == typeof(Color)) {
				field = new ColorField(label);
				((ColorField)field).showAlpha = false;
			} else if (type == typeof(float))
				field = new FloatField(label);
			else
				throw new NotImplementedException();

			var result = (INotifyValueChanged<T>) field;
			result.value = value;

			return result;
		}

		void CreateEnumField<T>(string label, T defaultValue, Action<T> onChange, VisualElement root) {
			var enumEl = new EnumField(label, Convert<Enum>(defaultValue));
			enumEl.RegisterValueChangedCallback((ChangeEvent<Enum> change) => {
				onChange(Convert<T>(change.newValue));
				onChangeProperty?.Invoke();
			});

			root.Add(enumEl);
		}

		#endregion

		Color NoAlpha(Color c) {
			c.a = 1;
			return c;
		}

		T Convert<T>(object value) {
			return (T)System.Convert.ChangeType(value, typeof(T));
		}

		void SetVisible(VisualElement element, bool visible) {
			element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
		}
	}
}