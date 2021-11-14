using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Midi;
using Sanford.Multimedia.Midi;
using NMPB.Timers;
using Debug = UnityEngine.Debug;

public class JustAudio : MonoBehaviour
{
    private Sequencer _sequencer;
    public AudioController Audio;
    private Sustain _sustain = new Sustain();
    public string OutDir;
    public string Piano = "Piano1";
    private string _audioFile;
    public string MidiPath;
    private long _finishTime;
    private long _renderTime;
    private bool _started;
    public double Volume;

    private bool _done;
    private int _length = 1;
    private int _position = 0;
    private bool _forceStop;
    private Thread _workerThread;

    void Start()
    {
        _done = false;
        AudioProgress.Enable();
        Audio = new AudioController(Piano);
        _sustain.NoteReleased += OnRelease;
        _workerThread = new Thread(GenerateAudio);
        _workerThread.Start();
    }

    private void OnRelease(object sender, NoteReleasedEventArgs e)
    {
        Audio.FadeExact(NoteConverter.Notes[e.Note], _audioTime + 735, e.TrackChannelKey);
    }

    void Update()
    {
        if (_done)
        {
            Application.Quit();
        }
        var percent = Math.Round(_position*100/(double) _length);
        percent = Math.Min(100, percent);
        AudioProgress.SetText(string.Format("{0}%", percent));
    }

    private void GenerateAudio()
    {
        var directory = new DirectoryInfo(OutDir);
        directory.Create();
        _audioFile = Path.Combine(OutDir, "audio.raw");
        if (File.Exists(_audioFile))
        {
            File.Delete(_audioFile);
        }
        TimerFactory.IsManual = true;
        _sequencer = new Sequencer();
        _sequencer.Sequence = new Sequence(MidiPath);
        _sequencer.ChannelMessagePlayed += OnChannelMessage;
        _sequencer.Stopped += OnStop;
        _length = _sequencer.Sequence.GetLength();
        //_sequencer.Start();
        _started = true;
        var frame = 0;
        while (_started || _renderTime < _finishTime)
        {
            if (_forceStop)
            {
                return;
            }
            frame++;
            var bufferLength = frame%2 == 0 ? 367 : 368;
            var buffer = new float[bufferLength*2];
            Audio.OnAudioFilterRead(buffer, 2, _audioTime);
            using (var stream = File.Open(_audioFile, FileMode.Append))
            using (var bw = new BinaryWriter(stream))
            {
                foreach (var f in buffer)
                {
                    bw.Write(f);
                }
            }

            if (frame == 65) _sequencer.Start();

            var steps = frame%3 == 0 ? 16 : 17;
            for (var i = 0; i < steps; i++)
            {
                _audioTime += 22;
                _renderTime++;
                if (_renderTime%20 == 0) _audioTime++;
                ManualTimer.RaiseTick();
            }
            _position = _sequencer.Position;
        }
        Raw2Wav.Convert(_audioFile, Path.Combine(OutDir, "Audio.wav"), 22050);
        _done = true;
    }

    private void OnStop(object sender, StoppedEventArgs e)
    {
        _finishTime = _renderTime + 7000;
        _started = false;
    }



    void OnDestroy()
    {
        _forceStop = true;
        _workerThread.Join();
        if (_sequencer != null)
            _sequencer.Dispose();
        TimerFactory.IsManual = false;
    }

    private void OnChannelMessage(object sender, ChannelMessageEventArgs e)
    {
        _sustain.Process(e.Message);
        if (e.Message.Data2 == 0 || (e.Message.Command != ChannelCommand.NoteOn) || e.Message.MidiChannel == 9)
            return;

        Audio.PlayExact(NoteConverter.Notes[e.Message.Data1], e.Message.Data2 / 255.0 * Volume, _audioTime + 735, e.Message.GetTrackChannelKey());
    }

    private long _audioTime = 0;
}