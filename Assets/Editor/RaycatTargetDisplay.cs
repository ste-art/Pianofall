using UI;
using UnityEngine;
using UnityEditor;
using UnityEditor.UI;

[CanEditMultipleObjects, CustomEditor(typeof (RaycastTarget), false)]
public class RaycatTargetDisplay : GraphicEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_Script);
        serializedObject.ApplyModifiedProperties();
    }
}