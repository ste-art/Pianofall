using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NMPB.Timers;
using Sanford.Multimedia.Midi;

public static class BlackCounter
{
    private static long _frame = 0;
    private static long _renderTime;
    private static bool _finish = false;
    private static int _noteCount;

    public static int[] Count(Sequence sequece, params int[] levels)
    {
        var overloads = new List<int>();
        using (var sequencer = new Sequencer {Sequence = sequece})
        {
            var isManual = TimerFactory.IsManual;
            TimerFactory.IsManual = true;
            sequencer.ChannelMessagePlayed += OnMessage;
            sequencer.Stopped += OnStop;
            sequencer.Start();


            while (!_finish)
            {
                _noteCount = 0;
                _frame++;
                var steps = _frame%3 == 0 ? 16 : 17;
                for (var i = 0; i < steps; i++)
                {
                    ManualTimer.RaiseTick();
                }
                var overload = levels.Count(level => _noteCount > level);
                overloads.Add(overload);
            }
            TimerFactory.IsManual = isManual;
            return overloads.ToArray();
        }
    }

    private static void OnStop(object sender, StoppedEventArgs e)
    {
        _finish = true;
    }

    private static void OnMessage(object sender, ChannelMessageEventArgs e)
    {
        if (e.Message.Data2 == 0 || (e.Message.Command != ChannelCommand.NoteOn) || e.Message.MidiChannel == 9)
            return;
        _noteCount++;
    }
}
