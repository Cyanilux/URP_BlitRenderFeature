using UnityEditor;
using UnityEngine;

namespace Cyan {
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer {

        bool ShowProperty(SerializedProperty property) {
            ShowIfAttribute attr = attribute as ShowIfAttribute;
            var parentPropertyPath = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf('.'));
            SerializedProperty ifProperty = property.serializedObject.FindProperty(parentPropertyPath + "." + attr.property);

            if (ifProperty == null) {
                Debug.LogWarning("[ShowIf()] attribute : Could not find property '" + attr.property + "' on attribute for property '" + property.name +"' (" + property.serializedObject.targetObject.GetType().FullName + ")");
                return true;
            }

            bool show = false;
            if (ifProperty.propertyType == SerializedPropertyType.Enum && ifProperty.enumValueIndex == (int)attr.value) {
                show = true;
            }else if (ifProperty.propertyType == SerializedPropertyType.ObjectReference) {
                show = ifProperty.objectReferenceValue != null;
            }
            return show;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (ShowProperty(property)) {
                EditorGUI.PropertyField(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (ShowProperty(property)) {
                return base.GetPropertyHeight(property, label);
            } else return -EditorGUIUtility.standardVerticalSpacing;
        }

    }
}