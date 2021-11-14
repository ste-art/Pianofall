using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class FadeFilter
{
    public class AudioInfo
    {
        public long StartSample;
        public long FadeSample;
        public long FadeLength;
        public float Volume;
        public int PlayIterator = -2;
        public int FadeIterator = -1;


        public AudioInfo(long startSample, long fadeSample, float volume)
        {
            StartSample = startSample;
            FadeSample = fadeSample;
            Volume = volume;
            FadeLength = 22050;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", StartSample, FadeSample);
        }
    }

    public float[] Samples;
    public LinkedList<AudioInfo> _audioData = new LinkedList<AudioInfo>();
    public long SampleRate = 0;


	void Start ()
	{
        SampleRate = AudioSettings.outputSampleRate;
	}

    public int Count;

    public void Fade(double time, double length)
    {
        if (_audioData.Last == null) return;
        var last = _audioData.Last.Value;
        if (last.FadeSample != long.MaxValue) return;
        last.FadeSample = (long) (time * SampleRate);
        last.FadeLength = (int)(length * SampleRate);
    }

    public void FadeExact(long time, double length)
    {
        if (_audioData.Last == null) return;
        var last = _audioData.Last.Value;
        if (last.FadeSample != long.MaxValue) return;
        last.FadeSample = time;
        last.FadeLength = (int)(length * SampleRate);
    }

    public void Play(double time, double volume)
    {
        lock (_locker)
        {
            long startSample = (long) (time*SampleRate);
            _audioData.AddLast(new AudioInfo(startSample, long.MaxValue, (float) volume));
            Count++;
        }
    }

    public void PlayExact(long time, double volume)
    {
        lock (_locker)
        {
            _audioData.AddLast(new AudioInfo(time, long.MaxValue, (float) volume));
            Count++;
        }
    }

    private object _locker = new object();
    public void OnAudioFilterRead(float[] data, int channels, long time)
    {
        lock (_locker)
        {
            var iterator = _audioData.First;
            while (iterator != null)
            {
                var next = iterator.Next;
                if (iterator.Value.FadeIterator >= iterator.Value.FadeLength ||
                    iterator.Value.PlayIterator >= Samples.Length)
                {
                    _audioData.Remove(iterator);
                    Count--;
                }
                iterator = next;
            }

            iterator = _audioData.First;
            if (iterator == null) return;
            do
            {
                var audioInfo = iterator.Value;
                var sample = time;
                for (int i = 0; i < data.Length; i += 2, sample++)
                {
                    if (sample < audioInfo.StartSample) continue;
                    if (audioInfo.FadeIterator >= audioInfo.FadeLength) break;
                    audioInfo.PlayIterator += 2;
                    if (audioInfo.PlayIterator + 2 >= Samples.Length) break;
                    if (sample < audioInfo.FadeSample)
                    {
                        var newValue = Samples[audioInfo.PlayIterator]*audioInfo.Volume;
                        data[i] = data[i] + newValue; // -data[i] * newValue;
                        newValue = Samples[audioInfo.PlayIterator + 1]*audioInfo.Volume;
                        data[i + 1] = data[i + 1] + newValue; // -data[i + 1] * newValue;
                    }
                    else
                    {
                        audioInfo.FadeIterator++;
                        var multiplier = (1 - (float) (audioInfo.FadeIterator)/audioInfo.FadeLength)*audioInfo.Volume;
                        var newValue = Samples[audioInfo.PlayIterator]*multiplier;
                        data[i] = data[i] + newValue; // -data[i] * newValue;
                        newValue = Samples[audioInfo.PlayIterator + 1]*multiplier;
                        data[i + 1] = data[i + 1] + newValue; // -data[i + 1] * newValue;
                    }

                }
            } while ((iterator = iterator.Next) != null);
        }
    }
}
