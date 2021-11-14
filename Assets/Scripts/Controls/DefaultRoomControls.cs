using System;
using System.Collections.Generic;
using System.Globalization;
using Commons.Music.Midi;
using Controls;
using Sanford.Multimedia.Midi;
using UI;
using UnityEngine;

namespace Midi
{
    public class DefaultRoomControls : MonoBehaviour
    {
        public event EventHandler<ChannelMessageEventArgs> ChannelEvent;

        const int MaxMidiValue = 128;
        const int MaxKeyValue = 320;

        private readonly Action<byte>[] _controlActions = new Action<byte>[MaxMidiValue]; 
        private readonly Action<byte>[] _noteActions = new Action<byte>[MaxMidiValue]; 
        private readonly Action<byte>[] _keyActions = new Action<byte>[MaxKeyValue];
        private Action<byte> _pitchAction;


        private float _gravityForce;
        private float _gravityRotation;
        private bool _gravityChanged = false;
        private float _floorRotation;
        private bool _floorChanged = false;
        private Vector3 _floorNewPosition;
        private Vector3 _floorOldPosition;
        private bool _blast;
        private float _volume;
        private bool _volumeChanged;

        void Awake()
        {
            SetAction(ControllableAction.Volume, OnVolume);
            SetAction(ControllableAction.Blast, OnBlast);
            SetAction(ControllableAction.FloorHeight, OnFloorHeight);
            SetAction(ControllableAction.FloorPosition, OnFloorPosition);
            SetAction(ControllableAction.FloorRotation, OnFloorRotation);
            SetAction(ControllableAction.GravityForce, OnGravityForce);
            SetAction(ControllableAction.GravityRotation, OnGravityRotation);
            SetAction(ControllableAction.PlayPause, OnPlayPause);
            SetAction(ControllableAction.PressAllNotes, OnPressAllNotes);
            SetAction(ControllableAction.StopAllSounds, OnStopAllSounds);
        }

        private void OnStopAllSounds(byte data)
        {
            SendToAllChannels(ChannelCommand.Controller, 0x7b, 0);
        }

        private void SendToAllChannels(ChannelCommand command, int data1, int data2)
        {
            for (byte i = 0; i < 16; i++)
            {
                OnChannelEvent(new ChannelMessage(command, i, data1, data2));
            }
        }

        private void OnPressAllNotes(byte data)
        {
            for (int i = 0; i < 128; i++)
            {
                OnChannelEvent(new ChannelMessage(ChannelCommand.NoteOn, 0, i, 127));
            }
        }

        private void OnPlayPause(byte data)
        {
            Globals.MicroMenu.OnPlay();
        }

        private void OnGravityRotation(byte data)
        {
            _gravityRotation = data == 64 ? 0 : (data / 127f - 0.5f) * 180 * Mathf.Deg2Rad;
            _gravityChanged = true;
        }

        private void OnGravityForce(byte data)
        {
            _gravityForce = data == 64 ? 0 : (data / 127f - 0.5f) * 20;
            _gravityChanged = true;
        }

        private void OnFloorRotation(byte data)
        {
            if (data == 64)
            {
                _floorRotation = 0;
            }
            else
            {
                _floorRotation = data - 64;
                _floorRotation = -(_floorRotation + 0.5f) / 63.5f * 90;
            }
            _floorChanged = true;
        }

        private void OnFloorPosition(byte data)
        {
            _floorNewPosition = new Vector3((data - 64) / 3f, _floorNewPosition.y, 0);
            _floorChanged = true;
        }

        private void OnFloorHeight(byte data)
        {
            _floorNewPosition = new Vector3(_floorNewPosition.x, (data - 64) / 10f, 0);
            _floorChanged = true;
        }

        private void OnBlast(byte data)
        {
            _blast = true;
        }

        private void OnVolume(byte value)
        {
            _volume = value/127f;
            _volumeChanged = true;
            SendToAllChannels(ChannelCommand.Controller, 0x07, value);
        }

        private string CutIfStartsWith(string data, string prefix)
        {
            if (data.StartsWith(prefix))
            {
                return data.Substring(prefix.Length);
            }
            return null;
        }

        private void SetAction(ControllableAction control, Action<byte> action)
        {
            var setting = Options.GetControl(control);
            var subkey = CutIfStartsWith(setting, ControlBindManager.MidiKeyPrefix);
            if (subkey != null)
            {
                int index;
                if (int.TryParse(subkey, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out index))
                {
                    AddAction(_noteActions, index, action);
                }
                return;
            }
            subkey = CutIfStartsWith(setting, ControlBindManager.ControlPrefix);
            if (subkey != null)
            {
                int index;
                if (int.TryParse(subkey, NumberStyles.AllowHexSpecifier, CultureInfo.CurrentCulture, out index))
                {
                    AddAction(_controlActions, index, action);
                }
                return;
            }
            subkey = CutIfStartsWith(setting, ControlBindManager.KeyboardPrefix);
            if (subkey != null)
            {
                KeyCode key;
                if (TryParse(subkey, out key))
                {
                    AddAction(_keyActions, (int) key, action);
                }
                return;
            }
            if (setting == ControlBindManager.PitchPrefix)
            {
                if (_pitchAction == null)
                {
                    _pitchAction = action;
                }
                else
                {
                    _pitchAction += action;
                }
                return;
            }
        }

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            try
            {
                result = (TEnum)Enum.Parse(typeof(TEnum), value);
                return true;
            }
            catch
            {
                result = default(TEnum);
                return false;
            }
        }

        private void AddAction(Action<byte>[] actions, int index, Action<byte> action)
        {
            if (actions[index] == null)
            {
                actions[index] = action;
            }
            else
            {
                actions[index] += action;
            }
        }

        

        public void ProcessMidiInput(byte[] data)
        {
            if (data.Length < 3)
            {
                return;
            }

            if (MidiDeviceHepler.IsPitchEvent(data[0]))
            {
                ProcessPitch(data);
            }
            if (MidiDeviceHepler.IsControlEvent(data[0]))
            {
                ProcessControl(data);
            }
            if (MidiDeviceHepler.IsNoteEvent(data[0]))
            {
                ProcessNote(data[1]);
            }
        }

        private void ProcessNote(byte note)
        {
            if (_noteActions[note] != null)
            {
                _noteActions[note](0);
            }
        }

        private void ProcessControl(byte[] data)
        {
            if (_controlActions[data[1]] != null)
            {
                _controlActions[data[1]](data[2]);
            }
        }

        private void ProcessPitch(byte[] data)
        {
            if (_pitchAction != null)
            {
                _pitchAction(data[2]);
            }
        }

        private void ProcessKeyboardKey(KeyCode key)
        {
            var index = (int) key;
            if (_keyActions[index] != null)
            {
                _keyActions[index](0);
            }
        }

        void Update()
        {
            foreach (var key in Keyboard.GetKeyCodes())
            {
                ProcessKeyboardKey(key);
            }

            if (_gravityChanged)
            {
                _gravityChanged = false;
                Physics.gravity = new Vector3(_gravityForce * Mathf.Sin(_gravityRotation), _gravityForce * Mathf.Cos(_gravityRotation), 0);
                WakeUpAll();
            }

            if (_floorChanged)
            {
                _floorChanged = false;

                var currentPosition = _floorNewPosition;
                var offset = currentPosition - _floorOldPosition;
                _floorOldPosition = currentPosition;

                var floorPosition = Loader.Instance.Floor.transform.position;
                floorPosition = floorPosition + offset;
                Loader.Instance.Floor.transform.position = floorPosition;
                Loader.Instance.Floor.transform.eulerAngles = new Vector3(0, 0, _floorRotation);
                WakeUpAll();
            }

            if (_blast)
            {
                _blast = false;
                foreach (var cube in Loader.Instance.MidiSequencer.Controller.Pool)
                {
                    cube.GetComponent<Rigidbody>().AddExplosionForce(3000, new Vector3(0, -10, 0), 1000);
                }
            }
            if (_volumeChanged)
            {
                _volumeChanged = false;
                AudioListener.volume = _volume;
            }
        }

        private void WakeUpAll()
        {
            foreach (var cube in Loader.Instance.MidiSequencer.Controller.Pool)
            {
                cube.GetComponent<Rigidbody>().WakeUp();
            }
        }

        protected virtual void OnChannelEvent(ChannelMessage e)
        {
            var handler = ChannelEvent;
            if (handler != null) handler(this, new ChannelMessageEventArgs(e));
        }
    }
}
