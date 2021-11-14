using System;
using System.Linq;
using Commons.Music.Midi;
using Commons.Music.Midi.RtMidi;
using Sanford.Multimedia.Midi;
using UnityEngine;

namespace Midi
{
    public static class MidiDeviceHepler {

        public static IMidiInput GetInputDevice()
        {
            var name = Options.InputDevice;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var access = new RtMidiAccess();
            var input = access.Inputs.FirstOrDefault(i => i.Name.StartsWith(name));
            if (input == null)
            {
                Debug.LogFormat("Input device with name '{0}' is not found.", name);
                return null;
            }
            try
            {
                return access.OpenInputAsync(input.Id);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        public static IMidiInput GetInputDevice(EventHandler<MidiReceivedEventArgs> callback)
        {
            var device = GetInputDevice();
            if (device == null)
            {
                return null;
            }
            device.MessageReceived += callback;
            return device;
        }

        public static IMidiOutput GetOutputDevice()
        {
            var name = Options.OutputDevice;
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var access = new RtMidiAccess();
            var input = access.Inputs.FirstOrDefault(i => i.Name.StartsWith(name));
            if (input == null)
            {
                return null;
            }
            try
            {
                return access.OpenOutputAsync(input.Id);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            return null;
        }

        public static bool IsChannelMessage(byte[] data)
        {
            return data.Length == 3
                   && data[0] >= (byte) ChannelCommand.NoteOff
                   && data[0] <= (byte) ChannelCommand.PitchWheel + ChannelMessage.MidiChannelMaxValue;
        }

        public static bool IsControlEvent(byte data)
        {
            return (data &0xf0) == (int) ChannelCommand.Controller;
        }

        public static bool IsPitchEvent(byte data)
        {
            return (data & 0xf0) == (int)ChannelCommand.PitchWheel;
        }

        public static bool IsNoteEvent(byte data)
        {
            return (data & 0xf0) == (int)ChannelCommand.NoteOn;
        }

        public static ChannelMessage ToChannelMessage(byte[] data)
        {
            return new ChannelMessage((ChannelCommand) (data[0] & 0xf0), data[0] & 0x0f, data[1], data[2]);
        }

        public static bool IsSysCommonMessage(byte[] data)
        {
            return data.Length == 3
                   && (data[0] == (byte) SysCommonType.MidiTimeCode
                       || data[0] == (byte) SysCommonType.SongPositionPointer
                       || data[0] == (byte) SysCommonType.SongSelect
                       || data[0] == (byte) SysCommonType.TuneRequest);
        }

        public static SysCommonMessage ToSysCommonMessage(byte[] data)
        {
            return new SysCommonMessage((SysCommonType)data[0], data[1], data[2]);
        }
    }
}
