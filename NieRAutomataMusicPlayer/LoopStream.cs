using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NieRAutomataMusicTest
{
    public class LoopStream : WaveStream
    {
        private readonly WaveStream waveStream;
        private readonly int loopStart;
        private readonly int loopEnd;

        public LoopStream(WaveStream source, int loopStart, int loopEnd)
        {
            waveStream = source;

            // Convert samples to bytes
            this.loopStart = loopStart * (waveStream.WaveFormat.BitsPerSample / 4);
            this.loopEnd = loopEnd * (waveStream.WaveFormat.BitsPerSample / 4);

            WaveFormat = waveStream.WaveFormat;
            Length = source.Length;
        }

        public override WaveFormat WaveFormat { get; }

        public override long Length { get; }

        public override long Position { get => waveStream.Position; set => waveStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long remainder = loopEnd - Position; // Remaining bytes before looping

            if (remainder < 0) // For some reason, remainder is negative when about to reach the loop end
            {
                Position = loopStart; // Loop

                waveStream.Read(buffer, offset, (int)Math.Abs(remainder)); // Read the remaining bytes
            }

            int bytesRead = waveStream.Read(buffer, offset, count); // Normal read

            return bytesRead;
        }

        // Backup lol
        //
        //public override int Read(byte[] buffer, int offset, int count)
        //{
        //    int bytesRead = 0;

        //    int remainder = (int)Position - loopEnd;

        //    if (Position >= loopEnd)
        //    {
        //        Position = loopStart;

        //        waveStream.Read(buffer, offset, remainder);
        //    }

        //    bytesRead = waveStream.Read(buffer, offset, count);

        //    return bytesRead;
        //}
    }
}
