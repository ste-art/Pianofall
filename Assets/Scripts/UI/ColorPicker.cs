using Interfaces;
using UnityEngine;

namespace UI
{
    public class ColorPicker : Initable
    {
        public HSVPicker.ColorPicker Picker;
 
        private Color _color;
        public Color Color
        {
            get { return _color; }
            private set
            {
                _color = value;
                Picker.AssignColor(Color);
            }
        }

        public string Value
        {
            get
            {
                return "#" + ColorUtility.ToHtmlStringRGB(_color);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                Color clr;
                if (!ColorUtility.TryParseHtmlString(value, out clr)) return;
                Color = clr;
            }
        }

        public void Awake()
        {
            Picker.onValueChanged.AddListener(clr =>
            {
                _color = clr;
            });
        }

        // ReSharper disable once InconsistentNaming
        public bool interactable
        {
            get;
            set;
        }

        public override void Init()
        {
            
        }
    }
}
