using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(AudioSource))]
public class AudioProcessor : MonoBehaviour
{
    private Stopwatch _sw = new Stopwatch();
    public AudioController Controller;
    public long Time { get { return (long) (_sw.ElapsedTicks); } }

    public long SampleRate { get; private set; }
    public string Piano = "Piano1";
    public float Volume = 0.5f;

    public double Delay { get; private set; }

    void Start ()
    {
        
        var config = AudioSettings.GetConfiguration();
        config.sampleRate = 22050;
        config.speakerMode = AudioSpeakerMode.Stereo;
        SampleRate = config.sampleRate;
        Delay = Math.Ceiling(config.dspBufferSize/(double) SampleRate * 1000) / 1000;
        AudioSettings.Reset(config);
        AudioListener.volume = Volume;
        Controller = new AudioController(Piano);
        GetComponent<AudioSource>().Play();
        _sw.Start();
    }



    public void Play(string note, double volume, double time, int channel = 0)
    {
        Controller.Play(note, volume, time, channel);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        //if (Controller == null) return;
        //var time = (long) (Time * 0.002205);
        var time = (long) (Time * 0.004410);
        Controller.OnAudioFilterRead(data, channels, time);
    }

    public Text Count;
    void Update()
    {
        if(Count!=null)Count.text = Controller.Count.ToString();
    }
}
