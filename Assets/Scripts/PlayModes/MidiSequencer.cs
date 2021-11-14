using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Commons.Music.Midi;
using Commons.Music.Midi.RtMidi;
using Midi;
using NMPB.Timers;
using Sanford.Multimedia.Midi;
using UnityEngine;
using UnityEngine.Analytics;
using Debug = UnityEngine.Debug;

namespace PlayModes
{
    public class MidiSequencer : MonoBehaviour
    {
        static MidiSequencer()
        {
            TimerFactory.UseManagedOnWin = true;
        }

        private IMidiAccess _access;
        IMidiOutput _output;
        private IMidiInput _input;
        public Sequencer Sequencer;
        public AudioProcessor Processor;
        public CubeController Controller;
        private CubeQueue _queue;
        private Stopwatch _sw = new MicroStopwatch();
        private int _frames = 0;
        public string MidiPath;
        public int NoDoubles;
        private Sustain _sustain = new Sustain();
        private DefaultRoomControls _controls;
        private long _notesCount;

        void Start ()
        {
            _controls = new GameObject().AddComponent<DefaultRoomControls>();
            _controls.ChannelEvent += OnChannelMessageAsap;

            SetupMidiDevice();
            _queue = new CubeQueue(Controller);
            _sw.Start();
            Sequencer = new Sequencer();
            if(!string.IsNullOrEmpty(MidiPath))
            {
                Sequencer.Sequence = (new Sequence(MidiPath)).Trim(true);
            }
            _sustain.NoteReleased += OnRelease;

            Sequencer.ChannelMessagePlayed += OnChannelMessage;
            new WaitForSeconds(1);
            //_sequencer.Start(); 
            //_sequencer.Position = (int) (_sequencer.Sequence.GetLength()*0.80);
            StartCoroutine(DelayedStart());
        }

        private void SetupMidiDevice()
        {
            if (Globals.Settings == null)
            {
                return;
            }
            _access = new RtMidiAccess();

            if (Globals.Settings.UseMidiInput)
            {
                var desiredInput = _access.Inputs.FirstOrDefault(d => d.Name.StartsWith(Options.InputDevice));
                if (desiredInput != null)
                {
                    _input = _access.OpenInputAsync(desiredInput.Id);
                    _input.MessageReceived += OnInputOnMessageReceived;
                }
            }

            if (Globals.Settings.UseMidiOutput)
            {
                var desiredOutput = _access.Outputs.FirstOrDefault(d => d.Name.StartsWith(Options.OutputDevice));
                if (desiredOutput != null)
                {
                    _output = _access.OpenOutputAsync(desiredOutput.Id);
                    var bytes = new byte[] {0xC0, 0, 0};
                    if (Globals.Settings.InstrumentOverride.HasValue)
                    {
                        bytes[1] = (byte) Globals.Settings.InstrumentOverride;
                        for (byte i = 0; i < 16; i++)
                        {
                            bytes[0] = (byte) (0xC0 + i);
                            _output.SendAsync(bytes, 0, 0, 0);
                        }
                    }
                    else
                    {
                        _output.SendAsync(bytes, 0, 0, 0);
                    }
                
                }
            }
        }

        private void OnInputOnMessageReceived(object s, MidiReceivedEventArgs a)
        {
            _controls.ProcessMidiInput(a.Data);
            if (!a.Data.Any())
            {
                return;
            }
            if (a.Data[0] < 0x80 || a.Data[0] > 0xef)
            {
                return;
            }
        
            var message = new ChannelMessage((ChannelCommand) (a.Data[0] & 0xf0), a.Data[0] & 0x0f, a.Data[1], a.Data[2]);
            OnChannelMessageAsap(this, new ChannelMessageEventArgs(message));
        }

        private void OnRelease(object sender, NoteReleasedEventArgs e)
        {
            Processor.Controller.Fade(NoteConverter.Notes[e.Note], Processor.Time / 10000000.0 + Processor.Delay, e.TrackChannelKey);
        }

        IEnumerator DelayedStart()
        {
            yield return new WaitForSeconds(1);
            Sequencer.Continue();
        }

        void OnDestroy()
        {
            var seconds = (int) Math.Max(1, _sw.Elapsed.TotalSeconds);
            Analytics.CustomEvent("PlaySessionEnd", new Dictionary<string, object>
            {
                {"NotesPlayed", _notesCount},
                {"PlayTime", seconds},
                {"FPS", _frames/seconds}
            });
            if(Sequencer!=null)
            {
                Sequencer.Dispose();
            }
            if (_output != null)
            {
                _output.Dispose();
            }
            if (_input != null)
            {
                _input.Dispose();
            }
        }

        private void OnChannelMessageAsap(object sender, ChannelMessageEventArgs e)
        {
            PlaySoundOnDevice(e);
            _sustain.Process(e.Message);
            if (e.Message.Data2 == 0 || (e.Message.Command != ChannelCommand.NoteOn) || e.Message.MidiChannel == 9)
                return;
            PlaySound(e.Message);
            _queue.InsertNote(e.Message.Data1, _sw.ElapsedMilliseconds, NoteHelper.GetColor(e.Message));
        }

        private void OnChannelMessage(object sender, ChannelMessageEventArgs e)
        {
            PlaySoundOnDevice(e);
            _sustain.Process(e.Message);
            if (e.Message.Data2 == 0 || (e.Message.Command != ChannelCommand.NoteOn) || e.Message.MidiChannel == 9)
                return;
            PlaySound(e.Message);
            _queue.InsertNote(e.Message.Data1,_sw.ElapsedMilliseconds+100 + (int)(Processor.Delay * 1000), NoteHelper.GetColor(e.Message));
        }

        private void PlaySoundOnDevice(ChannelMessageEventArgs e)
        {
            if (_output == null)
            {
                return;
            }

            var bytes = new[]
            {
                (byte) ((int) e.Message.Command | e.Message.MidiChannel),
                (byte) e.Message.Data1,
                (byte) e.Message.Data2
            };

            if (Globals.Settings.InstrumentOverride.HasValue && e.Message.Command == ChannelCommand.ProgramChange)
            {
                bytes[1] = (byte) Globals.Settings.InstrumentOverride;
            }

            _output.SendAsync(bytes, 0, 0, 0);
        }

        private void PlaySound(ChannelMessage message)
        {
            _notesCount++;
            if (Globals.Settings == null || Globals.Settings.UseBuildInAduio)
            {
                Processor.Play(
                    NoteConverter.Notes[message.Data1], 
                    message.Data2 / 127.0, 
                    Processor.Time / 10000000.0 + Processor.Delay, 
                    message.GetTrackChannelKey());
            }
        }

        void Update ()
        {
            _frames++;
            var transpose = 0;
            if (Input.GetKey(KeyCode.LeftShift)) transpose += 1;
            if (Input.GetKey(KeyCode.RightShift)) transpose += 1;
            if (Input.GetKey(KeyCode.LeftControl)) transpose -= 1;
            if (Input.GetKey(KeyCode.RightControl)) transpose -= 1;
            foreach (var key in KeyNoteMap.GetKeysDown())
            {
                PlayKeyboardKey(key, transpose, ChannelCommand.NoteOn);
            }
            foreach (var key in KeyNoteMap.GetKeysUp())
            {
                PlayKeyboardKey(key, transpose, ChannelCommand.NoteOff);
            }

            _queue.Update(_sw.ElapsedMilliseconds, NoDoubles == 1/*, Physics.gravity*/);
        }


        private void PlayKeyboardKey(KeyCode key, int offset, ChannelCommand command)
        {
            if (!KeyNoteMap.Map.ContainsKey(key))
            {
                return;
            }
            var data = KeyNoteMap.Map[key] + offset*12;
            if (data < 0 || data >= 128)
            {
                return;
            }
            OnChannelMessageAsap(this, new ChannelMessageEventArgs(new ChannelMessage(command, 0, data, 127)));
        }
    }
}