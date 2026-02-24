using UnityEditor;
using UnityEngine;

namespace Cyan {
    [CustomPropertyDrawer(typeof(IndentAttribute))]
    public class IndentPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.indentLevel--;
        }
    }
}