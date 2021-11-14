using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using UI.FileSelectors;
using UI.FolderSelectors;

namespace UI
{
    public class MenuController : MonoBehaviour
    {
        private Dropdown _preset;
        private Button _presetDelete;
        private SimpleFileSelector _fileSelector;
        private SwitchSelector _mode;
        private Dropdown _doubles;
        private ValueSlider _blockLimit;
        private Dropdown _soundfont;
        private Slider _volume;
        private ColorConfigSelector _colorConfig;
        private ColorPicker _bgColor;
        private ColorPicker _floorColor;
        private SimpleFolderSelector _output;
        private Button _returnButton;
        private Button _saveButton;
        private Button _startButton;

        private Toggle _buildInAudio;
        private Toggle _midiInputToggle;
        private Toggle _midiOutputToggle;
        private Dropdown _midiInstrumentOverride;

        private Transform _savePresetPanel;
        private Button _savePresetApprove;
        private Button _savePresetCancel;
        private InputField _sevePresetInput;

        private Button _colorsPanelButton;
        private Button _otherPanelButton;


        void Awake()
        {
            Globals.StartMenu = this;
/*#if UNITY_EDITOR
            ActivatePanels();
#endif*/
            gameObject.SetActive(false);
            CacheComponents();
            Settings = Settings.Generate();
            EnumeratePresets();
        }

        void Start()
        {
            EnumerateInstruments();
            LoadLastPreset();
            Settings.ParseArguments();
            ApplyModeFilter();
            InitializeValues();
            ApplyListeners();
        }

        private void EnumerateInstruments()
        {
            var text = (TextAsset)Resources.Load("Instruments");
            var instruments =
                text.text.Split(new[] {'\n'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select((t, i) => string.Format("{0}: {1}", i, t))
                    .ToList();
            _midiInstrumentOverride.ClearOptions();
            _midiInstrumentOverride.AddOptions(new List<string> {"No"});
            _midiInstrumentOverride.AddOptions(instruments);
        }

        private void ActivatePanels()
        {
            foreach (Transform panel in transform.Find("Background/Panels/"))
            {
                panel.gameObject.SetActive(true);
            }
        }

        private void ApplyListeners()
        {
            _preset.onValueChanged.AddListener(OnPresetChanged);
            _presetDelete.onClick.AddListener(OnPresetDelete);
            _mode.onValueChanged.AddListener(OnModeChanged);
            _returnButton.onClick.AddListener(OnReturn);
            _saveButton.onClick.AddListener(OnSave);
            _startButton.onClick.AddListener(OnStart);
            _savePresetApprove.onClick.AddListener(SavePreset);
            _savePresetCancel.onClick.AddListener(ClosePresetPanel);
        }

        private void OnModeChanged()
        {
            ApplyModeFilter();
        }

        private void OnPresetDelete()
        {
            var preset = _preset.options[_preset.value].text;
            ModalWindow.Instance.ShowDialog($"Preset '{preset}' will be deleted.", () => { DeletePreset(preset); });
        }

        private void DeletePreset(string preset)
        {
            Settings.Delete(preset);
            EnumeratePresets();
            SelectPreset(Settings.DefaultName);
        }

        private void ClosePresetPanel()
        {
            _savePresetPanel.gameObject.SetActive(false);
        }

        private void SavePreset()
        {
            var presetName = _sevePresetInput.text;
            if (string.IsNullOrEmpty(presetName) || presetName.EqualsI(Settings.DefaultName))
            {
                return;
            }
            GetSettings();
            Settings.Save(presetName);
            EnumeratePresets();
            SelectPreset(presetName);
            ClosePresetPanel();
        }

        private void SelectPreset(string presetName)
        {
            var index = _preset.options.FindIndex(p => p.text.EqualsI(presetName));
            _preset.value = Math.Max(0, index);
            _preset.RefreshShownValue();
        }

        private void OnSave()
        {
            _savePresetPanel.gameObject.SetActive(true);
            var currentName = _preset.options[_preset.value].text;
            if (!currentName.Equals(Settings.DefaultName))
            {
                _sevePresetInput.text = currentName;
            }
            _sevePresetInput.OnPointerClick(new PointerEventData(EventSystem.current));
        }

        private void OnReturn()
        {
            gameObject.SetActive(false);
        }

        private void OnPresetChanged(int index)
        {
            Settings = Settings.Deseriaize(_preset.options[index].text);
            _presetDelete.interactable = index != 0;
            ApplyModeFilter();
            InitializeValues();
        }

        private void EnumeratePresets()
        {
            _preset.ClearOptions();
            var presets = FileSystemHelpers.Presets.GetFiles("*.conf")
                .Select(p => Path.GetFileNameWithoutExtension(p.Name))
                .Where(p => !p.Equals(Settings.DefaultName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            _preset.AddOptions(new List<string> { Settings.DefaultName });
            _preset.AddOptions(presets);
            _preset.value = 0;
            _preset.RefreshShownValue();
        }

        private void CacheComponents()
        {
            _preset = transform.Find("Background/Preset/Dropdown").GetComponent<Dropdown>();
            _presetDelete = transform.Find("Background/Preset/Delete").GetComponent<Button>();
            _fileSelector = transform.Find("Background/Panels/Main/FileSelector").GetComponent<SimpleFileSelector>();
            _mode = transform.Find("Background/Panels/Main/Mode/Modes").GetComponent<SwitchSelector>();
            _doubles = transform.Find("Background/Panels/Other/DoublesReduction/Dropdown").GetComponent<Dropdown>();
            _soundfont = transform.Find("Background/Panels/Audio/BuildInAudio/Soundfont/Dropdown").GetComponent<Dropdown>();
            _volume = transform.Find("Background/Panels/Audio/BuildInAudio/Volume/Slider").GetComponent<Slider>();
            _colorConfig = transform.Find("Background/Panels/Colors/ColorConfig").GetComponent<ColorConfigSelector>();
            _bgColor = transform.Find("Background/Panels/Colors/BgColor").GetComponent<ColorPicker>();
            _floorColor = transform.Find("Background/Panels/Colors/FloorColor").GetComponent<ColorPicker>();
            _output = transform.Find("Background/Panels/Main/OutputFolder").GetComponent<SimpleFolderSelector>();
            _blockLimit = transform.Find("Background/Panels/Other/BlockLimit").GetComponent<ValueSlider>();
            _returnButton = transform.Find("Background/ControlButtons/Return").GetComponent<Button>();
            _saveButton = transform.Find("Background/ControlButtons/Save").GetComponent<Button>();
            _startButton = transform.Find("Background/ControlButtons/Start").GetComponent<Button>();

            _buildInAudio = transform.Find("Background/Panels/Audio/BuildInAudio/BuildInEnabled/Toggle").GetComponent<Toggle>();
            _midiInputToggle = transform.Find("Background/Panels/Audio/MidiIn/Toggle").GetComponent<Toggle>();
            _midiOutputToggle = transform.Find("Background/Panels/Audio/MidiOut/Toggle").GetComponent<Toggle>();
            _midiInstrumentOverride = transform.Find("Background/Panels/Audio/MidiOut/Soundfont/Dropdown").GetComponent<Dropdown>();

            _savePresetPanel = transform.Find("SavePreset");
            _savePresetApprove = _savePresetPanel.Find("SavePresetPanel/Save").GetComponent<Button>();
            _savePresetCancel = _savePresetPanel.Find("SavePresetPanel/Cancel").GetComponent<Button>();
            _sevePresetInput = _savePresetPanel.Find("SavePresetPanel/InputField").GetComponent<InputField>();

            _colorsPanelButton = transform.Find("Background/TabButtons/Colors").GetComponent<Button>();
            _otherPanelButton = transform.Find("Background/TabButtons/Other").GetComponent<Button>();
        }

        private void InitializeValues()
        {
            _fileSelector.Value = Settings.MidiPath;
            _mode.Value = Settings.Mode.ToString();
            _doubles.value = Settings.NoDoubles;
            _doubles.RefreshShownValue();
            _blockLimit.Value = Settings.Blocks > 10 ? Settings.Blocks : 1000;
            _soundfont.value = Settings.Soundfont.Equals("piano1", StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            _volume.value = Settings.Volume;
            _colorConfig.Value = Settings.ColorConfig;
            _bgColor.Value = Settings.BackgroundColor;
            _floorColor.Value = Settings.FloorColor;
            _output.Value = Settings.OutPath;
            _buildInAudio.isOn = Settings.UseBuildInAduio;
            _midiInputToggle.isOn = Settings.UseMidiInput;
            _midiOutputToggle.isOn = Settings.UseMidiOutput;
            _midiInstrumentOverride.value = Settings.InstrumentOverride + 1 ?? 0;
        }

        public Settings Settings;

        public void OnStart()
        {
            GetSettings();
            if (Settings.Mode != Modes.Realtime && !File.Exists(Settings.MidiPath))
            {
                _fileSelector.OpenDialog();
                return;
            }
            if (IsWarningNeeded())
            {
                /*
                var result = MessageBox.Show(string.Format("Content of the folder '{0}' will be deleted.", _output.Value), "Warning",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (result != DialogResult.OK)
                {
                    return;
                }*/
                ModalWindow.Instance.ShowDialog(string.Format("Content of the folder '{0}' will be deleted.", _output.Value), StartScene);
            }
            else
            {
                StartScene();
            }
        }

        private void StartScene()
        {
            SaveLastPreset();
            SceneManager.LoadScene("1", LoadSceneMode.Single);
        }

        private bool IsWarningNeeded()
        {
            return GetValueFromSelector<Modes>(_mode) == Modes.Prerender
                && Directory.Exists(_output.Value)
                && Directory.GetFiles(_output.Value).Any();
        }

        private void SaveLastPreset()
        {
            var preset = _preset.options[_preset.value].text;
            Options.LastPreset = preset;
            Options.Save();
        }

        private void LoadLastPreset()
        {
            var preset = Options.LastPreset;
            if (!string.IsNullOrEmpty(preset))
            {
                SelectPreset(preset);
            }
            OnPresetChanged(_preset.value);
        }

        private void GetSettings()
        {
            Settings.MidiPath = _fileSelector.Value;
            Settings.Mode = GetValueFromSelector<Modes>(_mode);
            Settings.NoDoubles = _doubles.value;
            Settings.Blocks = _blockLimit.Value;
            Settings.Soundfont = "piano" + (_soundfont.value);
            Settings.Volume = (int) Mathf.Round(_volume.value);
            Settings.ColorConfig = _colorConfig.Value;
            Settings.BackgroundColor = _bgColor.Value;
            Settings.FloorColor = _floorColor.Value;
            Settings.OutPath = _output.Value;
            Settings.UseBuildInAduio = _buildInAudio.isOn;
            Settings.UseMidiInput = _midiInputToggle.isOn;
            Settings.UseMidiOutput = _midiOutputToggle.isOn;
            Settings.InstrumentOverride = _midiInstrumentOverride.value == 0 ? (int?) null : _midiInstrumentOverride.value - 1;
            Globals.Settings = Settings;
        }

        private string GetValueFromDropdown(Dropdown dropdown)
        {
            return dropdown.options[dropdown.value].text;
        }

        private T GetValueFromDropdown<T>(Dropdown dropdown)
        {
            var text = GetValueFromDropdown(dropdown);
            return (T) Enum.Parse(typeof (T), text);
        }

        private T GetValueFromSelector<T>(SwitchSelector selector)
        {
            var text = selector.Value;
            if (string.IsNullOrEmpty(text))
            {
                return default(T);
            }
            return (T)Enum.Parse(typeof(T), text);
        }

        public void ApplyModeFilter()
        {
            var state = GetValueFromSelector<Modes>(_mode);
            _doubles.interactable = true;
            _blockLimit.interactable = true;
            _volume.interactable = true;
            _colorConfig.interactable = true;
            _bgColor.interactable = true;
            _floorColor.interactable = true;
            _output.gameObject.SetActive(true);
            _midiInputToggle.interactable = state == Modes.Realtime;
            _midiOutputToggle.interactable = state == Modes.Realtime;
            _midiInstrumentOverride.interactable = state == Modes.Realtime;
            _colorsPanelButton.interactable = state != Modes.Audio;
            _otherPanelButton.interactable = state != Modes.Audio;

            switch (state)
            {
                case Modes.Realtime:
                    _output.gameObject.SetActive(false);
                    _blockLimit.Max = 5000;
                    _blockLimit.Rounding = i => MathHelper.Round(i, 10);
                    break;
                case Modes.Audio:
                    _doubles.interactable = false;
                    _blockLimit.interactable = false;
                    _colorConfig.interactable = false;
                    _bgColor.interactable = false;
                    _floorColor.interactable = false;
                    break;
                case Modes.Prerender:
                    _blockLimit.Max = 50000;
                    _blockLimit.Rounding = i => MathHelper.Round(i, 100);
                    break;
            }

            EnumerateDoubleReduction(state);
        }

        private void EnumerateDoubleReduction(Modes mode)
        {
            if (mode == Modes.Audio)
            {
                return;
            }
            var currentId = _doubles.value;
            _doubles.ClearOptions();
            _doubles.AddOptions(new List<string>()
            {
                "Disabled",
                "Once per frame",
                "Once per millisecond"
            });
            if (mode == Modes.Prerender)
            {
                _doubles.AddOptions(new List<string>()
                {
                    "Smart: 2 levels",
                    "Smart: 5 levels"
                });
            }
            if (currentId >= _doubles.options.Count)
            {
                _doubles.value = 0;
            }
            _doubles.RefreshShownValue();
        }

        private void ChangeInteraction(string path, bool value)
        {
            if (path.EndsWith("Dropdown"))
            {
                transform.Find(path).GetComponent<Dropdown>().interactable = value;
                return;
            }
            if (path.EndsWith("Button"))
            {
                transform.Find(path).GetComponent<Button>().interactable = value;
                return;
            }
            if (path.EndsWith("InputField"))
            {
                transform.Find(path).GetComponent<InputField>().interactable = value;
                return;
            }
            if (path.EndsWith("Slider"))
            {
                transform.Find(path).GetComponent<Slider>().interactable = value;
                return;
            }
        }
    }
}

