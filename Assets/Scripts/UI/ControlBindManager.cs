using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Commons.Music.Midi;
using Controls;
using Midi;
using Sanford.Multimedia.Midi;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ControlBindManager : MonoBehaviour
    {
        public ControlBinder BinderTemplate;
        private List<ControlBinder> _binders = new List<ControlBinder>();
        private ControlBinder _activeBinder;
        private IMidiInput _input;
        private readonly object _locker = new object();
        private string _valueToApply = null;
        public const string ControlPrefix = "Midi Control: ";
        public const string MidiKeyPrefix = "Midi Key: ";
        public const string PitchPrefix = "Midi Pitch";
        public const string KeyboardPrefix = "KB: ";

        void Awake ()
        {
            var type = typeof (ControllableAction);
            var actions = Enum.GetNames(type);
            foreach (var action in actions)
            {
                var member = type.GetMember(action).First();
                var descriptionAttribute = member.GetCustomAttributes(typeof (DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
                var description = descriptionAttribute == null ? action : descriptionAttribute.Description;
                var allowKeyboard = member.GetCustomAttributes(typeof (AlsoKeyboardAttribute), false).Any();
                var actionType = (ControllableAction) Enum.Parse(typeof (ControllableAction), action);
                var binder = Instantiate(BinderTemplate, transform);
                binder.Initialize(this, actionType, description, allowKeyboard/*, Options.GetControl(actionType)*/);
                _binders.Add(binder);
            }
            BinderTemplate.gameObject.SetActive(false);
        }

        void OnEnable()
        {
            foreach (var binder in _binders)
            {
                binder.Apply(Options.GetControl(binder.Action));
            }
            _input = MidiDeviceHepler.GetInputDevice(OnMidiInput);
        }

        private void OnMidiInput(object sender, MidiReceivedEventArgs e)
        {
            if (!MidiDeviceHepler.IsChannelMessage(e.Data))
            {
                return;
            }
            
            var message = MidiDeviceHepler.ToChannelMessage(e.Data);
            lock (_locker)
            {
                if (_activeBinder == null)
                {
                    return;
                }
                var prefix = "";
                switch (message.Command)
                {
                    case ChannelCommand.NoteOn:
                    {
                        if (!_activeBinder.AllowKeyboard)
                        {
                            return;
                        }
                        prefix = MidiKeyPrefix;
                        break;
                    }
                    case ChannelCommand.Controller:
                        prefix = ControlPrefix;
                        break;
                    case ChannelCommand.PitchWheel:
                        _valueToApply = PitchPrefix;
                        return;
                    default:
                        return;
                }

                _valueToApply = string.Format("{0}{1:X2}",prefix, e.Data[1]);
            }
        }


        void OnDisable()
        {
            _input.SafeDispose();
        }

        void Update()
        {

            lock (_locker)
            {
                if (_activeBinder!= null)
                {
                    var key = Keyboard.GetKeyDown();
                    switch (key)
                    {
                        case KeyCode.None:
                            break;
                        case KeyCode.Escape:
                            _activeBinder.Toggle();
                            return;
                        default:
                            if(_activeBinder.AllowKeyboard)
                            {
                                _valueToApply = KeyboardPrefix + key;
                            }
                            break;
                    }

                    if (!string.IsNullOrEmpty(_valueToApply))
                    {
                        _activeBinder.Apply(_valueToApply);
                        _valueToApply = null;
                        _activeBinder = null;
                    }
                }
            }
        }

        public void SetBinder(ControlBinder controlBinder)
        {
            lock (_locker)
            {
                foreach (var binder in _binders.Where(b=>b!=controlBinder))
                {
                    binder.Unfocus();
                }
                _activeBinder = controlBinder;
            }
        }

        public void ResetBinder(ControlBinder controlBinder)
        {
            lock (_locker)
            {
                if(_activeBinder != controlBinder) return;
                _activeBinder = null;
                _valueToApply = null;
            }
        }

        public void SaveSettings()
        {
            foreach (var controlBinder in _binders)
            {
                Options.SetControl(controlBinder.Action, controlBinder.Value);
            }
        }
    }
}
