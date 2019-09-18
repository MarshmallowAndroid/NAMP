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
        private readonly WaveStream sourceStream;
        private readonly int loopStart;
        private readonly int loopEnd;
        private readonly object readLock = new object();

        public LoopStream(WaveStream source, int loopStart, int loopEnd)
        {
            sourceStream = source;

            WaveFormat = sourceStream.WaveFormat;
            Length = source.Length;

            // Convert samples to bytes
            this.loopStart = (loopStart - (loopStart % WaveFormat.Channels)) * WaveFormat.BlockAlign;
            this.loopEnd = (loopEnd - (loopEnd % WaveFormat.Channels)) * WaveFormat.BlockAlign;
        }

        public override WaveFormat WaveFormat { get; }

        public override long Length { get; }

        public override long Position
        {
            get
            {
                try
                {
                    if (sourceStream != null)
                        return sourceStream.Position;
                }
                catch (NullReferenceException)
                {
                    return 0;
                }

                return 0;
            }
            set
            {
                if (sourceStream != null)
                    sourceStream.Position = value;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int intPosition = Convert.ToInt32(Position);
            int ahead = intPosition + count;

            // plan ahead
            if (ahead >= loopEnd)
            {
                lock (readLock) // no idea if this actually makes a difference, but i'll check one day
                {
                    int remainder = loopEnd - intPosition;
                    int bytesRead = 0;

                    bytesRead += sourceStream.Read(buffer, offset, remainder); // first pass, get the end of the loop

                    Position = loopStart; // seek to start of loop

                    // make sure the count of the next bytes we'll be playing from stream
                    // are the same as always
                    while (bytesRead < count)
                    {
                        int leftover = 0;

                        // select what would give the same value as count
                        if (bytesRead + (count - bytesRead) == count) leftover = count - bytesRead;
                        else if (bytesRead + (count - remainder) == count) leftover = count - remainder;

                        // bytesRead seems to work for now, i'll fix it if something goes wrong :')
                        bytesRead += sourceStream.Read(buffer, offset + bytesRead, leftover);
                    }

                    return bytesRead; // pray to cthulhu the player doesn't shut down
                }
            }
            else // read normally
            {
                int bytesRead = sourceStream.Read(buffer, offset, count);

                return bytesRead;
            }
        }
    }
}
