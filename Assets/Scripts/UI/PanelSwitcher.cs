using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PanelSwitcher : MonoBehaviour
    {
        public GameObject Panels;
        public GameObject Buttons;

        private List<GameObject> _panels;
        void Start ()
        {
            _panels = Panels == null 
                ? GetChildren(transform, "Background/Panels").ToList() 
                : GetChildren(Panels.transform).ToList();

            var buttons = Buttons == null 
                ? GetButtons(transform, "Background/TabButtons") 
                : GetButtons(Buttons.transform);
            
            foreach (var button in buttons)
            {
                var tabName = button.name;
                button.onClick.AddListener(() => { SwitchTab(tabName); });
            }

            SwitchTab(_panels.First().name);
        }

        private void SwitchTab(string tab)
        {
            foreach (var panel in _panels)
            {
                panel.SetActive(false);
            }
            _panels.First(p => p.name == tab).SetActive(true);
        }

        public IEnumerable<GameObject> GetChildren(Transform parent, string subPath = null)
        {
            if (!string.IsNullOrEmpty(subPath))
            {
                parent = parent.Find(subPath);
            }
            return parent.Cast<Transform>().Select(child => child.gameObject);
        }

        private IEnumerable<Button> GetButtons(Transform parent, string subPath = null)
        {
            return GetChildren(parent, subPath)
                .Select(child => child.GetComponent<Button>())
                .Where(button => button != null);
        }

    }
}
