using Sanford.Multimedia.Midi;

namespace Midi
{
    public static class MidiExtensions
    {
        public static int GetTrackChannelKey(this ChannelMessage message)
        {
            return (message.TrackIndex << 4) + message.MidiChannel;
        }
    }
}
