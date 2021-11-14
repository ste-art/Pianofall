using System;
using System.IO;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace UI.FileSelectors
{
    public abstract class FileSelectorBase : Initable
    {
        public abstract void OpenDialog();

        private string _value;
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                Text.text = Path.GetFileName(value);
                ClearButton.interactable = !string.IsNullOrEmpty(Value);
            }
        }

        public Text Text;
        public Button OpenButton;
        public Button ClearButton;

        void Awake()
        {
            if (Text == null)
            {
                Init();
            }
            OpenButton.onClick.AddListener(OpenDialog);
            ClearButton.onClick.AddListener(Clear);
        }

        private void Clear()
        {
            Value = "";
        }


        protected string GetLastLocation()
        {
            if (!string.IsNullOrEmpty(Value))
            {
                return Value;
            }
            var lastLocation = LoadLastLocation();
            return string.IsNullOrEmpty(lastLocation) ? Environment.CurrentDirectory : lastLocation;
        }

        // ReSharper disable once InconsistentNaming
        public bool interactable
        {
            get
            {
                return OpenButton.interactable;
            }
            set
            {
                OpenButton.interactable = value;
                ClearButton.interactable = value && !string.IsNullOrEmpty(Value);
            }
        }

        protected void SaveLastLocation(string value)
        {
            Options.LastFile = value;
        }

        private string LoadLastLocation()
        {
            return Options.LastFile ?? "";
        }

        public override void Init()
        {
            Text = transform.Find("Filename").GetComponent<Text>();
            OpenButton = transform.Find("Button").GetComponent<Button>();
            ClearButton = transform.Find("Clear").GetComponent<Button>();
        }
    }
}
