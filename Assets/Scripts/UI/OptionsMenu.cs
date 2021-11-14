using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Commons.Music.Midi;
using Commons.Music.Midi.RtMidi;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

namespace UI
{
    public class OptionsMenu : MonoBehaviour
    {
        private Button _cancel;
        private Button _ok;
        private Dropdown _inputList;
        private Dropdown _outputList;
        private ControlBindManager _controlManager;
        

        void Awake()
        {
            Globals.OptionsMenu = this;
            _cancel = transform.Find("Background/ControlButtons/Cancel").GetComponent<Button>();
            _ok = transform.Find("Background/ControlButtons/OK").GetComponent<Button>();
            _inputList = transform.Find("Background/Panels/Main/MidiInput/Dropdown").GetComponent<Dropdown>();
            _outputList = transform.Find("Background/Panels/Main/MidiOutput/Dropdown").GetComponent<Dropdown>();
            transform.Find("Background/Panels/Main/MidiInput/Button").GetComponent<Button>().onClick.AddListener(EnumerateMidiInputs);
            transform.Find("Background/Panels/Main/MidiOutput/Button").GetComponent<Button>().onClick.AddListener(EnumearteMidiOutputs);
            _controlManager = transform.Find("Background/Panels/Controls/Scroll/Content").GetComponent<ControlBindManager>();


            _cancel.onClick.AddListener(OnCancel);
            _ok.onClick.AddListener(OnOk);

            Deactivate();
        }

        void OnEnable()
        {
            EnumerateMidiInputs();
            EnumearteMidiOutputs();
        }

        private void EnumerateMidiInputs()
        {
            EnumerateMidiDevices(_inputList, new RtMidiAccess().Inputs, Options.InputDevice);
        }

        private void EnumearteMidiOutputs()
        {
            EnumerateMidiDevices(_outputList, new RtMidiAccess().Outputs, Options.OutputDevice);
        }

        private void EnumerateMidiDevices(Dropdown dropdown, IEnumerable<IMidiPortDetails> devices, string selected)
        {
            var result = new List<string> {"None"};
            result.AddRange(devices.Select(GetDeviceName));
            var index = result.IndexOf(selected);
            if (index < 0)
            {
                index = 0;
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(result);
            dropdown.value = index;
            dropdown.RefreshShownValue();
        }

        private string GetDeviceName(IMidiPortDetails device)
        {
            var match = Regex.Match(device.Name, @"^(.*) (\d+)$");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return device.Name;
        }


        private void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void OnOk()
        {
            ApplySettings();
            Deactivate();
        }

        private void ApplySettings()
        {
            var input = _inputList.options[_inputList.value].text;
            var output = _outputList.options[_outputList.value].text;
            Options.InputDevice = input;
            Options.OutputDevice = output;
            _controlManager.SaveSettings();
            Options.Save();

            Analytics.CustomEvent("OptionsSaved", new Dictionary<string, object>()
            {
                {"InputDevice", input.Clamp(200)},
                {"OutputDevice", output.Clamp(200)},
                {"Resolution", string.Format("{0}x{1}", Screen.width, Screen.height)}
            });
        }

        private void OnCancel()
        {
            EnumerateMidiInputs();
            EnumearteMidiOutputs();
            Deactivate();
        }
    }
}
