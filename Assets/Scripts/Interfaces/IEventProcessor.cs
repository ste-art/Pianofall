using System;

public interface IEventProcessor
{
    event EventHandler<NoteEventArgs> OnNote;
}

public class NoteEventArgs : EventArgs
{
    public RenderNote Note;

    public NoteEventArgs(RenderNote note)
    {
        Note = note;
    }

}

public class RenderNote
{
    public int Data;
    public int Velocity;
    public long Time;
    public bool Stop;
    public Note Linked;

    public RenderNote(int data, int velocity, long time, bool stop = false, Note linked = null)
    {
        Data = data;
        Velocity = velocity;
        Time = time;
        Stop = stop;
        Linked = linked;
    }
}
