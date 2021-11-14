using System;
using UnityEngine;
using System.Collections;
using Sanford.Multimedia.Midi;
/*
public class MidiFileInput : IMidiInput
{
    public event EventHandler<MidiMessageEventArgs> OnInput;

    private Sequencer _sequencer;
    private Sequence _sequence;
    public MidiFileInput(string file)
    {
        _sequencer = new Sequencer();
        _sequence = new Sequence(file);
        _sequencer.Sequence = _sequence.Trim(true);
    }

    public void Dispose()
    {
        if (_sequence != null)
        {
            _sequence.Dispose();
        }
        if (_sequencer != null)
        {
            _sequencer.Dispose();
        }
    }
}
*/