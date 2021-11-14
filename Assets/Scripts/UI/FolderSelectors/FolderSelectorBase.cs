using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace UI.FolderSelectors
{
    public abstract class FolderSelectorBase : Initable
    {
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Text.text = value;
            }
        }

        public InputField Text;
        public Button Button;
        private string _value;
        

        void Awake()
        {
            if (Text == null)
            {
                Init();
            }
            Text.onValueChanged.AddListener(OnTextEdit);
            Button.onClick.AddListener(BrowseFolder);
        }

        private void OnTextEdit(string value)
        {
            Value = value;
        }

        // ReSharper disable once InconsistentNaming
        public bool interactable
        {
            get { return Button.interactable && Text.interactable; }
            set
            {
                Button.interactable = value;
                Text.interactable = value;
            }
        }

        public abstract void BrowseFolder();

        public override void Init()
        {
            Text = transform.Find("InputField").GetComponent<InputField>();
            Button = transform.Find("Button").GetComponent<Button>();
        }
    }
}
