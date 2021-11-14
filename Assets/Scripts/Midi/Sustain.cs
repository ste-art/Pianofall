using System;
using System.Collections.Generic;
using Midi;
using Sanford.Multimedia.Midi;

class Sustain
{
    public event EventHandler<NoteReleasedEventArgs> NoteReleased = delegate { };

    private const int NoteValueMax = 128;

    private readonly Dictionary<int, bool[]> _hold = new Dictionary<int, bool[]>();
    private readonly Dictionary<int, bool[]> _sound = new Dictionary<int, bool[]>();
    private readonly Dictionary<int, bool> _sustain = new Dictionary<int, bool>();

    void TryAddChannel(int id)
    {
        if (!_sustain.ContainsKey(id))
        {
            _hold.Add(id, new bool[NoteValueMax]);
            _sound.Add(id, new bool[NoteValueMax]);
            _sustain.Add(id, false);
        }
    }

    public void Process(ChannelMessage msg)
    {
        if (msg.MidiChannel == 9) return;
        var key = msg.GetTrackChannelKey();
        TryAddChannel(key);
        switch (msg.Command)
        {
            case ChannelCommand.NoteOn:
                if (msg.Data2 > 0)
                    NoteOn(key, msg.Data1);
                else
                    NoteOff(key, msg.Data1);
                return;
            case ChannelCommand.NoteOff:
                NoteOff(key, msg.Data1);
                return;
            case ChannelCommand.Controller:
                if (msg.Data1 != (int)ControllerType.HoldPedal1) return;
                if (msg.Data2 >= 64)
                    PedalPress(key);
                else
                    PedalRelease(key);
                return;

        }

    }

    private void PedalRelease(int channel)
    {
        _sustain[channel] = false;
        for (int i = 0; i < NoteValueMax; i++)
        {
            if (!_sound[channel][i]) continue; // not playing right now
            if (_hold[channel][i]) continue; // still pressed
            
            Release(channel, i);
        }
    }

    private void Release(int channel, int note)
    {
        _sound[channel][note] = false;
        NoteReleased(this, new NoteReleasedEventArgs(note, channel));
    }

    private void PedalPress(int channel)
    {
        _sustain[channel] = true;
    }

    private void NoteOff(int channel, int note)
    {
        _hold[channel][note] = false;
        if (_sound[channel][note])
        {
            if (!_sustain[channel])
            {
                Release(channel,note);
            }
        }
    }

    private void NoteOn(int channel, int note)
    {
        _hold[channel][note] = _sound[channel][note] = true;
    }

}

public class NoteReleasedEventArgs : EventArgs
{
    public int Note { get; private set; }
    public int TrackChannelKey { get; private set; }

    public NoteReleasedEventArgs(int note, int trackChannelKey)
    {
        Note = note;
        TrackChannelKey = trackChannelKey;
    }
}