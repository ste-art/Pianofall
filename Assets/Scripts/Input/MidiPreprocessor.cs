using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sanford.Multimedia.Midi;

public class MidiPreprocessor
{
    public List<TrackBlock> Tracks;
    public int NoteCount;

    private List<TempoPair> _tickLengths;
    public MidiPreprocessor(Sequence sequence)
    {
        //_tickLengths = new List<TempoPair>() {new TempoPair(0, 500d / sequence.Division)};
        //_tickLengths.AddRange(sequence
        //    .SelectMany(track => track.Iterator())
        //    .Where(midi => midi.MidiMessage is MetaMessage && ((MetaMessage) midi.MidiMessage).MetaType == MetaType.Tempo)
        //    .Select(tempo =>
        //    {
        //        var builder = new TempoChangeBuilder((MetaMessage) tempo.MidiMessage);
        //        var tickLength1 = (double) builder.Tempo/sequence.Division/1000;
        //        return new TempoPair(tempo.AbsoluteTicks, tickLength1);
        //    })
        //    .OrderBy(tempo => tempo.AbsoluteTicks));

        //if (_tickLengths.Count >= 2 && _tickLengths[0].AbsoluteTicks == 0 && _tickLengths[1].AbsoluteTicks == 0)
        //{
        //    _tickLengths.RemoveAt(0);
        //}

        Tracks = new List<TrackBlock>(sequence.Count);
        for (int i = 0; i < sequence.Count; i++)
        {
            Tracks.Add(new TrackBlock());
        }

        double time = 0;
        double tickLength = 500d / sequence.Division;
        int lastTicks = 0;
        var sequenceLength = 0;
        foreach (var midi in sequence.SelectMany(track => track.Iterator()).OrderBy(m=>m.AbsoluteTicks))
        {
            sequenceLength = midi.AbsoluteTicks;

            var meta = midi.MidiMessage as MetaMessage;
            if (meta!= null && meta.MetaType == MetaType.Tempo)
            {
                time += (midi.AbsoluteTicks - lastTicks) * tickLength;
                lastTicks = midi.AbsoluteTicks;
                var builder = new TempoChangeBuilder(meta);
                tickLength = (double)builder.Tempo / sequence.Division / 1000;
            }

            var channel = midi.MidiMessage as ChannelMessage;
            if (channel != null)
            {
                var realTime = time + (midi.AbsoluteTicks - lastTicks) * tickLength;

                if (channel.Command == ChannelCommand.NoteOn && channel.Data2 > 0)
                {
                    Tracks[channel.TrackIndex].Channels[channel.MidiChannel]
                        .Start(channel, midi.AbsoluteTicks, realTime);
                    NoteCount++;
                }
                else if (channel.Command == ChannelCommand.NoteOn || channel.Command == ChannelCommand.NoteOff)
                {
                    Tracks[channel.TrackIndex].Channels[channel.MidiChannel]
                        .Stop(channel.Data1, midi.AbsoluteTicks, realTime);
                }
            }
        }

        foreach (var channel in Tracks.SelectMany(track => track.Channels))
        {
            channel.Finalize(sequenceLength, time + (sequenceLength - lastTicks) * tickLength);
        }

        Tracks.RemoveAll(t => t.Channels.All(c => !c.Notes.Any()));

    }

    public int GetIndexHash(int track, int channel)
    {
        return (track & 0x00FFFFFF) | (channel << 24);
    }

    private class TempoPair
    {
        public long AbsoluteTicks;
        public double TickLength;

        public TempoPair(long absoluteTicks, double tickLength)
        {
            AbsoluteTicks = absoluteTicks;
            TickLength = tickLength;
        }
    }


    public class NoteBlock
    {
        public ChannelMessage Message;
        public long StartTicks;
        public long EndTicks;
        public double StartTime;
        public double EndTime;
    }

    public class ChannelBlock
    {
        public List<NoteBlock> Notes = new List<NoteBlock>();
        private NoteBlock[] _keys = new NoteBlock[128];

        public void Start(ChannelMessage message, long ticks, double time)
        {
            var data = message.Data1;
            Stop(data, ticks, time);
            var block = new NoteBlock() {Message = message, StartTicks = ticks, StartTime = time};
            Notes.Add(block);
            _keys[data] = block;
        }

        public void Stop(int data, long ticks, double time)
        {
            if (_keys[data] != null)
            {
                _keys[data].EndTicks = ticks;
                _keys[data].EndTime = time;
                _keys[data] = null;
            }
        }

        public void Finalize(long ticks, double time)
        {
            for (int index = 0; index < _keys.Length; index++)
            {
                Stop(index, ticks, time);
            }
            _keys = null;
        }
    }

    public class TrackBlock
    {
        public ChannelBlock[] Channels;

        public TrackBlock()
        {
            Channels = new ChannelBlock[16];
            for (int i = 0; i < 16; i++)
            {
                Channels[i] = new ChannelBlock();
            }
        }
    }
}

