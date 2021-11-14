using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class SwitchSelector : MonoBehaviour
    {
        public string Value
        {
            get { return _value; }
            set
            {
                if (_buttons == null)
                {
                    _value = value;
                }
                else
                {
                    Switch(value);
                }
            }
        }

        public UnityEvent onValueChanged = new UnityEvent();
        private string _value;

        public ColorBlock ColorBlock;
        private ColorBlock _pressedColor;

        private List<Button> _buttons;

        void Awake()
        {
            _pressedColor = ColorBlock;
            _pressedColor.normalColor = _pressedColor.pressedColor;
            _pressedColor.highlightedColor = _pressedColor.pressedColor;

            _buttons = new List<Button>();
            foreach (Transform child in transform)
            {
                var button = child.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => { Switch(button.name); });
                    _buttons.Add(button);
                }
            }
            Switch(string.IsNullOrEmpty(_value) ? _buttons.First().name : _value);
        }

        void Switch(string value)
        {
            _value = value;
            foreach (var button in _buttons)
            {
                button.colors = ColorBlock;
            }
            try
            {
                _buttons.First(b => b.name == value).colors = _pressedColor;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            onValueChanged.Invoke();
        }
    }
}
