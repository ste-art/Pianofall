using System.Linq;
using Controls;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ControlBinder : MonoBehaviour
    {
        public Color SelectedColor;
        public bool AllowKeyboard { get; private set; }

        public ColorBlock NormalColor;
        public ColorBlock MidiOnlyColor;

        private const string NoKey = "";
        private const string SelectKey = "??";

        private ControlBindManager _manager;
        private Button _button;
        private Image _buttonImage;
        private ColorBlock _defaultColors;
        private Button _reset;
        private Text _keyText;
        private bool _focused = false;
        private string _currentText = NoKey;
        public ControllableAction Action { get; private set; }
        public string Value { get { return _currentText; } }

        public void Apply(string value)
        {
            _currentText = value;
            Unfocus();
        }

        public void Toggle()
        {
            if (_focused)
            {
                Unfocus();
            }
            else
            {
                _focused = true;
                _buttonImage.color = SelectedColor;
                var colors = _button.colors;
                colors.highlightedColor = Color.white;
                _button.colors = colors;
                _reset.interactable = true;
                _currentText = _keyText.text;
                _keyText.text = SelectKey;
                _manager.SetBinder(this);
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void Unfocus()
        {
            _manager.ResetBinder(this);
            _focused = false;
            _keyText.text = _currentText;
            _buttonImage.color = Color.white;
            _button.colors = _defaultColors;
            _reset.interactable = _currentText != NoKey;
        }

        void Update ()
        {
        
        }

        public void Initialize(ControlBindManager manager, ControllableAction actionType, string description, bool allowKeyboard)
        {
            AllowKeyboard = allowKeyboard;
            Action = actionType;
            _manager = manager;

            _currentText = NoKey;

            transform.Find("Text").GetComponent<Text>().text = description;

            var button = transform.Find("Button").gameObject;
            _button = button.GetComponent<Button>();
            _buttonImage = button.GetComponent<Image>();
            _button.onClick.AddListener(Toggle);
            _defaultColors = allowKeyboard ? NormalColor : MidiOnlyColor;
            
            
            _keyText = button.transform.Find("Text").GetComponent<Text>();
            _keyText.text = _currentText;



            _reset = transform.Find("Reset").GetComponent<Button>();
            _reset.onClick.AddListener(OnReset);
            _reset.interactable = _currentText != NoKey;
        }

        public void OnReset()
        {
            _currentText = NoKey;
            Unfocus();
        }
    }
}
