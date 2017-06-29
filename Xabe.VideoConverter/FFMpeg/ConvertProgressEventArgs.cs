using System;

namespace Xabe.VideoConverter.FFMpeg
{
    public class ConvertProgressEventArgs: EventArgs
    {
        public ConvertProgressEventArgs(TimeSpan duration, TimeSpan totalLength)
        {
            Duration = duration;
            TotalLength = totalLength;
        }

        public TimeSpan Duration { get; }
        public TimeSpan TotalLength { get; }

        public int Percent => (int) (Math.Round(Duration.TotalSeconds / TotalLength.TotalSeconds, 2) * 100);
    }
}
