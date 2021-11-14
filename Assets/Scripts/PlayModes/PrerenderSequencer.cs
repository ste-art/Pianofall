using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Midi;
using Sanford.Multimedia.Midi;
using NMPB.Timers;
using Shaders;
using UnityEngine.Analytics;
using Debug = UnityEngine.Debug;

public class PrerenderSequencer : MonoBehaviour
{
    private Sequencer _sequencer;
    private Sustain _sustain = new Sustain();
    public AudioController Audio;
    public CubeController Controller;
    public CubeQueue _queue;
    public static string OutDir;
    public string Piano = "Piano1";
    private string _audioFile;
    private Stopwatch _sw = new Stopwatch();
    public string MidiPath;
    public int NoDoubles = 0;
    private long[] _doubles = new long[128];
    private long _playedNotes = 0;
    private long _totalNotes = 0;
    private int[] _overloads;
    public double Volume = 0.5;
    private int _currentFrame;

    private int _vSync;
    private int _captureFramerate;

    void Start()
    {
        _sustain.NoteReleased += OnRelease;
        _sw.Start();
        _currentFrame = 0;
        var directory = new DirectoryInfo(OutDir);
        directory.Create();
        foreach (FileInfo file in directory.GetFiles()) file.Delete();
        foreach (DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        _audioFile = Path.Combine(OutDir, "audio.raw");
        Audio = new AudioController(Piano);
        _captureFramerate = Time.captureFramerate;
        Time.captureFramerate = 60;
        _vSync = QualitySettings.vSyncCount;
        QualitySettings.vSyncCount = 0;
        _queue = new CubeQueue(Controller);
        TimerFactory.IsManual = true;
        _sequencer = new Sequencer();
        _sequencer.Sequence = new Sequence(MidiPath);
        _sequencer.ChannelMessagePlayed += OnChannelMessage;
        _sequencer.Stopped += OnStop;
        if (NoDoubles == 3) _overloads = BlackCounter.Count(_sequencer.Sequence, 660);
        if (NoDoubles == 4) _overloads = BlackCounter.Count(_sequencer.Sequence, 128, 256, 384, 512);
        //_sequencer.Position = (int) (_sequencer.Sequence.GetLength()*0.30);
        GameObject.Find("Main Camera").GetComponent<PreparePng>().enabled = true;
        StartCoroutine(DelayedStart());
        
    }

    private void OnRelease(object sender, NoteReleasedEventArgs e)
    {
        Audio.FadeExact(NoteConverter.Notes[e.Note], _audioTime + 735, e.TrackChannelKey);
    }

    private void OnStop(object sender, StoppedEventArgs e)
    {
        if(gameObject.activeSelf)
        {
            StartCoroutine(DelayedQuit());
        }
    }

    private void WriteInfoFile(bool finished)
    {
        try
        {
            using (var wr = File.CreateText(Path.Combine(OutDir, "!log.txt")))
            {
                wr.WriteLine("File: {0}", MidiPath);
                wr.WriteLine("Block limit: {0}", Controller.MaxObjects);
                wr.WriteLine("Video Length: {0}", TimeSpan.FromMilliseconds(_renderTime));
                wr.WriteLine("Render time: {0}", _sw.Elapsed);
                wr.WriteLine("Total notes: {0}", _totalNotes);
                if (NoDoubles != 0)
                {
                    wr.WriteLine("Total blocks: {0}", _playedNotes);
                }
                wr.WriteLine("Status: {0}", finished? "Finished" : "In Progress");
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private IEnumerator DelayedQuit()
    {
        yield return new WaitForSeconds(9);
        _sw.Stop();

        WriteInfoFile(true);

        Analytics.CustomEvent("PrerenderFinished", new Dictionary<string, object>
        {
            {"TrackName", Path.GetFileNameWithoutExtension(MidiPath).Clamp(150)},
            {"RenderTime", (int)_sw.Elapsed.TotalSeconds},
            {"VideoLength", (int)TimeSpan.FromMilliseconds(_renderTime).TotalSeconds},
            {"BlockLimit", Controller.MaxObjects},
            {"DoubleReductionMode", "DRM:" + NoDoubles},
            {"TotalNotes", _totalNotes},
            {"TotalBlocks", _playedNotes},
            {"Resolution", string.Format("{0}x{1}", Screen.width, Screen.height) }
        });

        Raw2Wav.Convert(_audioFile, Path.Combine(OutDir, "Audio.wav"), 22050);

        MainMenu.Exit();
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        _sequencer.Start();
        _started = true;
    }

    void OnDestroy()
    {

        if (_sequencer != null)
            _sequencer.Dispose();
        TimerFactory.IsManual = false;
        QualitySettings.vSyncCount = _vSync;
        Time.captureFramerate = _captureFramerate;

    }

    private void OnChannelMessage(object sender, ChannelMessageEventArgs e)
    {
        _sustain.Process(e.Message);
        if (e.Message.Data2 == 0 || (e.Message.Command != ChannelCommand.NoteOn) || e.Message.MidiChannel == 9)
            return;
        Audio.PlayExact(NoteConverter.Notes[e.Message.Data1], e.Message.Data2 / 255.0 * Volume, _audioTime + 735, e.Message.GetTrackChannelKey());
        _totalNotes++;

        if (NoDoubles > 2)
        {
            if (_doubles[e.Message.Data1] > _renderTime - _overloads[_frame])
            {
                return;
            }
        }
        else if (NoDoubles == 2 && _doubles[e.Message.Data1] == _renderTime)
        {
            return;
        }

        _doubles[e.Message.Data1] = _renderTime;
        _playedNotes++;
        _queue.InsertNote(e.Message.Data1, _renderTime + 50, NoteHelper.GetColor(e.Message));

    }

    private long _audioTime = 0;
    private long _renderTime = 0;
    private bool _started = false;
    private long _frame = 0;


    void Update()
    {
        var bufferLength = Time.frameCount%2 == 0 ? 367 : 368;
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

        if (_started)
        {
            _frame++;
        }

        var steps = Time.frameCount % 3 == 0 ? 16 : 17;
        for (var i = 0; i < steps; i++)
        {
            _audioTime += 22;
            _renderTime++;
            if (_renderTime % 20 == 0) _audioTime++;
            ManualTimer.RaiseTick();
        }
        _queue.Update(_renderTime, NoDoubles == 1);
        //Application.CaptureScreenshot(Path.Combine(OutDir,"img"+_currentFrame.ToString("D5") + ".png"));
        WriteInfoFile(false);

        if (Time.frameCount % 60 == 0)
        {
            GC.Collect();
        }
        _currentFrame++;
    }
}