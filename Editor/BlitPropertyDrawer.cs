using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Blit.BlitSettings))]
public class BlitEditor : PropertyDrawer {

    private bool createdStyles = false;
    private GUIStyle boldLabel;

    private void CreateStyles() {
        createdStyles = true;
        boldLabel = GUI.skin.label;
        boldLabel.fontStyle = FontStyle.Bold;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        //base.OnGUI(position, property, label);
        if (!createdStyles) CreateStyles();
        
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.LabelField(position, "Blit Settings", boldLabel);
        SerializedProperty _event = property.FindPropertyRelative("Event");
        EditorGUILayout.PropertyField(_event);
        if (_event.intValue == (int)UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingPostProcessing) {
            EditorGUILayout.HelpBox("The \"After Rendering Post Processing\" event does not work with Camera Color targets. " +
                "Unsure how to actually obtain the target after post processing has been applied. " +
                "Frame debugger seems to suggest a <no name> target?\n\n" +
                "Use the \"After Rendering\" event instead!", MessageType.Warning, true);
        }

        EditorGUILayout.PropertyField(property.FindPropertyRelative("blitMaterial"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("blitMaterialPassIndex"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("setInverseViewMatrix"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("generateDepthNormals"));

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Source", boldLabel);
        SerializedProperty srcType = property.FindPropertyRelative("srcType");
        EditorGUILayout.PropertyField(srcType);
        int enumValue = srcType.intValue;
        if (enumValue == (int)Blit.Target.TextureID) {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("srcTextureId"));
        } else if (enumValue == (int)Blit.Target.RenderTextureObject) {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("srcTextureObject"));
        }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Destination", boldLabel);
        SerializedProperty dstType = property.FindPropertyRelative("dstType");
        EditorGUILayout.PropertyField(dstType);
        enumValue = dstType.intValue;
        if (enumValue == (int)Blit.Target.TextureID) {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("dstTextureId"));
        } else if (enumValue == (int)Blit.Target.RenderTextureObject) {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("dstTextureObject"));
        }

        EditorGUI.indentLevel = 1;

        EditorGUI.EndProperty();

        property.serializedObject.ApplyModifiedProperties();
    }

}
