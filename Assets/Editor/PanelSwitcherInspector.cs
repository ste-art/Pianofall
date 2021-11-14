using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PanelSwitcher))]
    class PanelSwitcherInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var instance = (PanelSwitcher) target;

            if (instance.Panels == null)
            {
                EditorGUILayout.HelpBox("Panels have not been set.", MessageType.Info);
                return;
            }

            var panels = instance.GetChildren(instance.Panels.transform).ToList();

            foreach (var panel in panels)
            {
                if (GUILayout.Button(panel.name))
                {
                    SwitchTab(panel.name, panels);
                }
            }
        }

        private void SwitchTab(string tab, List<GameObject> objects)
        {
            foreach (var panel in objects)
            {
                panel.SetActive(false);
            }
            objects.First(p => p.name == tab).SetActive(true);
        }
    }
}
