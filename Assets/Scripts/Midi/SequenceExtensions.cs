using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;

static class SequenceExtensions
{
    public static TimeSpan GetActualLength(this Sequence sequence, int position = -1)
    {
        var tempos =
            sequence.SelectMany(track => track.Iterator()).Where(
                midi =>
                    midi.MidiMessage is MetaMessage && ((MetaMessage) midi.MidiMessage).MetaType == MetaType.Tempo)
                .OrderBy(midi => midi.AbsoluteTicks);

        if (position < 0) position = sequence.GetLength();

        double time = 0;
        double tickLength = 500d/sequence.Division;
        int lastTicks = 0;

        foreach (var tempo in tempos)
        {
            if (tempo.AbsoluteTicks > position) break;
            time += (tempo.AbsoluteTicks - lastTicks)*tickLength;
            lastTicks = tempo.AbsoluteTicks;
            var builder = new TempoChangeBuilder((MetaMessage) tempo.MidiMessage);
            tickLength = (double) builder.Tempo/sequence.Division/1000;
        }
        if (position == sequence.GetLength())
        {
            time += (sequence.Max(track => track.GetMidiEvent(track.Count - 1).AbsoluteTicks) - lastTicks)*tickLength;
        }
        else
        {
            time +=
                (sequence.Max(
                    track =>
                        (track.Iterator().LastOrDefault(midi => midi.AbsoluteTicks <= position) ??
                         track.Iterator().First()).AbsoluteTicks) - lastTicks)*tickLength;
        }

        return TimeSpan.FromMilliseconds(time);
    }

    public static Sequence Trim(this Sequence sequence, bool disposeOld = false)
    {
        var lastActiveDelta = 0;
        var firstActiveDelta = Int32.MaxValue;
        foreach (var track in sequence)
        {
            foreach (var midiEvent in track.Iterator())
            {
                var msg = midiEvent.MidiMessage as ChannelMessage;
                if (msg == null || !(msg.Command == ChannelCommand.NoteOn || msg.Command == ChannelCommand.NoteOff))
                    continue;
                if (midiEvent.AbsoluteTicks > lastActiveDelta)
                    lastActiveDelta = midiEvent.AbsoluteTicks;
                if (midiEvent.AbsoluteTicks < firstActiveDelta)
                    firstActiveDelta = midiEvent.AbsoluteTicks;
            }
        }
        var seq = new Sequence(sequence.Division);
        foreach (var track in sequence)
        {
            var trk = new Track();
            foreach (var midiEvent in track.Iterator())
            {
                var ticks = Math.Min(midiEvent.AbsoluteTicks, lastActiveDelta);
                ticks = Math.Max(0, ticks - firstActiveDelta);
                //if (midiEvent.AbsoluteTicks > lastActiveDelta) continue;
                var msg = midiEvent.MidiMessage as MetaMessage;
                if (msg != null && msg.MetaType == MetaType.EndOfTrack) continue;
                trk.Insert(ticks, midiEvent.MidiMessage);
            }
            seq.Add(trk);
        }
        if (disposeOld)
            sequence.Dispose();
        return seq;
    }

    public static Sequence Reverse(this Sequence sequence, bool disposeOld = false)
    {
        var heads = sequence.Select(t => new LinkedList<MidiEvent>(t.Iterator())).ToList();
        var track = new Track();
        double time = 0;
        double tickLength = 500d/sequence.Division;
        int lastTicks = 0;

        while (heads.Any())
        {
            var min = heads[0].First.Value.AbsoluteTicks;
            var index = 0;
            for (int i = 0; i < heads.Count; i++)
            {
                var midiEvent = heads[i];
                if (midiEvent.First.Value.AbsoluteTicks < min)
                {
                    index = i;
                    min = heads[i].First.Value.AbsoluteTicks;
                }
            }
            var midi = heads[index].First.Value;
            if (heads[index].First.Next == null)
            {
                heads.RemoveAt(index);
            }
            else
            {
                heads[index].RemoveFirst();
            }
            var tempo = midi.MidiMessage as MetaMessage;
            if (tempo != null)
            {
                if (tempo.MetaType == MetaType.Tempo)
                {
                    time += (midi.AbsoluteTicks - lastTicks)*tickLength;
                    lastTicks = midi.AbsoluteTicks;
                    var builder = new TempoChangeBuilder(tempo);
                    tickLength = (double) builder.Tempo/sequence.Division/1000;
                }
                continue;
            }

            track.Insert((int) (time + (midi.AbsoluteTicks - lastTicks)*tickLength), midi.MidiMessage);

        }
        if (disposeOld)
        {
            sequence.Dispose();
        }
        var length = track.Length;
        var reversed = new Track();
        var tmp = new TempoChangeBuilder {Tempo = 192000};
        tmp.Build();
        reversed.Insert(0, tmp.Result);
        foreach (var midi in track.Iterator().Reverse())
        {
            reversed.Insert(length - midi.AbsoluteTicks, midi.MidiMessage);
        }
        return new Sequence(192) {reversed};
    }
}
