using System.ComponentModel;

namespace Controls
{
    public enum ControllableAction
    {
        [Description("Floor height")]
        FloorHeight,

        [Description("Floor position")]
        FloorPosition,

        [Description("Floor rotation")]
        FloorRotation,

        [Description("Gravity force")]
        GravityForce,

        [Description("Gravity rotation")]
        GravityRotation,

        [AlsoKeyboard]
        [Description("Press all keys")]
        PressAllNotes,

        [AlsoKeyboard]
        [Description("Stop all notes")]
        StopAllSounds,

        [Description("Volume")]
        Volume,

        [AlsoKeyboard]
        [Description("Play / Pause")]
        PlayPause,

        [AlsoKeyboard]
        [Description("Blast")]
        Blast

    }
}
