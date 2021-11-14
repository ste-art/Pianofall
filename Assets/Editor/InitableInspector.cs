using UnityEditor;
using Interfaces;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(Initable), true)]
    class InitableInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var targetInstance = (Initable) target;

            if (GUILayout.Button("Get Objects"))
            {
                targetInstance.Init();
            }
        }
    }
}
