using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;

public class AudioController
{
    private Dictionary<int, Dictionary<string, FadeFilter>> _channels = new Dictionary<int, Dictionary<string, FadeFilter>>();
    private Dictionary<string, FadeFilter> _notes = new Dictionary<string, FadeFilter>();

    public long SampleRate { get; private set; }

    public AudioController(string pianoName)
    {
        foreach (var note in NoteConverter.Notes)
        {
            var clip = Resources.Load<AudioClip>(pianoName +"/"+ note);
            if (clip == null) continue;
            clip.LoadAudioData();
            var buffer = new float[clip.samples * clip.channels];
            
            clip.GetData(buffer, 0);

            var sound = new FadeFilter();
            sound.Samples = buffer;
            sound.SampleRate = AudioSettings.outputSampleRate;
            _notes.Add(note, sound);
        }
    }

    private void GenerateChannel(int id)
    {
        var channel = new Dictionary<string, FadeFilter>();
        foreach (var note in _notes)
        {
            var sound = new FadeFilter();
            sound.Samples = note.Value.Samples;
            sound.SampleRate = note.Value.SampleRate;
            channel.Add(note.Key, sound);
        }
        lock (_channels)
        {
            _channels.Add(id, channel);
        }
    }

    public void Play(string note, double volume, double time, int channel = 0)
    {
        if (!_notes.ContainsKey(note)) return;
        if (!_channels.ContainsKey(channel))
        {
            GenerateChannel(channel);
        }
        _channels[channel][note].Fade(time, 0.2);
        _channels[channel][note].Play(time, volume);
    }

    public void Fade(string note, double time, int channel = 0)
    {
        if (!_notes.ContainsKey(note)) return;
        if (!_channels.ContainsKey(channel))
        {
            GenerateChannel(channel);
        }
        _channels[channel][note].Fade(time, 0.3);
    }

    public void PlayExact(string note, double volume, long time, int channel = 0)
    {
        if (!_notes.ContainsKey(note)) return;
        if (!_channels.ContainsKey(channel))
        {
            GenerateChannel(channel);
        }
        _channels[channel][note].FadeExact(time, 0.2);
        _channels[channel][note].PlayExact(time, volume);
    }

    public void FadeExact(string note, long time, int channel = 0)
    {
        if (!_notes.ContainsKey(note)) return;
        if (!_channels.ContainsKey(channel))
        {
            GenerateChannel(channel);
        }
        _channels[channel][note].FadeExact(time, 0.2);
    }

    public string Count
    {
        get
        {
            lock (_channels)
            {
                return string.Join(", ",
                    _channels.SelectMany(p => p.Value.ToList())
                        .GroupBy(p => p.Key)
                        .Select(p => p.Sum(f => f.Value.Count).ToString("D3"))
                        .ToArray())
                    //_notes.Select(pair => pair.Value.Count.ToString("D3")).ToArray()) 
                       + "\r\n" +
                       //_notes.Sum(p=>p.Value.Count);
                       _channels.Sum(c => c.Value.Sum(f => f.Value.Count));
            }
        }
    }

    public void OnAudioFilterRead(float[] data, int channels, long time)
    {
        lock (_channels)
        {
            foreach (var channel in _channels)
            {
                foreach (var fadeFilter in channel.Value)
                {
                    fadeFilter.Value.OnAudioFilterRead(data, channels, time);
                }
            }

        }
    }

}
