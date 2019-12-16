using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAMP
{
    class CustomWaveOffsetStream : WaveStream
    {
        private WaveStream sourceStream;
        private long audioStartPosition;
        private long sourceOffsetBytes;
        private long sourceLengthBytes;
        private long length;
        private readonly int bytesPerSample; // includes all channels
        private long position;
        private int startSample;
        private int sourceOffset;
        private long sourceLength;
        private readonly object lockObject = new object();

        /// <summary>
        /// Creates a new WaveOffsetStream
        /// </summary>
        /// <param name="sourceStream">the source stream</param>
        /// <param name="startSample">the time at which we should start reading from the source stream</param>
        /// <param name="sourceOffset">amount to trim off the front of the source stream</param>
        /// <param name="sourceLength">length of time to play from source stream</param>
        public CustomWaveOffsetStream(WaveStream sourceStream, int startSample, int sourceOffset, long sourceLength)
        {
            //if (sourceStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            //    throw new ArgumentException("Only PCM supported");
            // TODO: add support for IEEE float + perhaps some others -
            // anything with a fixed bytes per sample

            this.sourceStream = sourceStream;
            position = 0;
            bytesPerSample = (sourceStream.WaveFormat.BitsPerSample / 8) * sourceStream.WaveFormat.Channels;
            StartSample = startSample;
            SourceOffset = sourceOffset;
            SourceLength = sourceLength;

            Console.WriteLine("SourceLength         : " + SourceLength);
            Console.WriteLine("SourceLengthBytes    : " + sourceLengthBytes);
            Console.WriteLine("BytesPerSample       : " + bytesPerSample);
            Console.WriteLine();
        }

        /// <summary>
        /// Creates a WaveOffsetStream with default settings (no offset or pre-delay,
        /// and whole length of source stream)
        /// </summary>
        /// <param name="sourceStream">The source stream</param>
        public CustomWaveOffsetStream(WaveStream sourceStream)
            : this(sourceStream, 0, 0, sourceStream.Length)
        {
        }

        /// <summary>
        /// The length of time before which no audio will be played
        /// </summary>
        public int StartSample
        {
            get
            {
                return startSample;
            }
            set
            {
                lock (lockObject)
                {
                    startSample = value;
                    audioStartPosition = (long)startSample * bytesPerSample;
                    // fix up our length and position
                    length = audioStartPosition + sourceLengthBytes;
                    Position = Position;
                }
            }
        }

        /// <summary>
        /// An offset into the source stream from which to start playing
        /// </summary>
        public int SourceOffset
        {
            get
            {
                return sourceOffset;
            }
            set
            {
                lock (lockObject)
                {
                    sourceOffset = value;
                    sourceOffsetBytes = (long)sourceOffset * bytesPerSample;
                    // fix up our position
                    Position = Position;
                }
            }
        }

        /// <summary>
        /// Length in samples to read from the source stream
        /// </summary>
        public long SourceLength
        {
            get
            {
                return sourceLength;
            }
            set
            {
                lock (lockObject)
                {
                    sourceLength = value;
                    sourceLengthBytes = sourceLength * bytesPerSample;
                    // fix up our length and position
                    length = audioStartPosition + sourceLengthBytes;
                    Position = Position;
                }
            }

        }

        /// <summary>
        /// Gets the block alignment for this WaveStream
        /// </summary>
        public override int BlockAlign => sourceStream.BlockAlign;

        /// <summary>
        /// Returns the stream length
        /// </summary>
        public override long Length
        {
            get
            {
                return length / bytesPerSample;
            }
        }

        /// <summary>
        /// Gets or sets the current position in the stream
        /// </summary>
        public override long Position
        {
            get
            {
                return position / bytesPerSample;
            }
            set
            {
                lock (lockObject)
                {
                    // make sure we don't get out of sync
                    //value -= (value % BlockAlign);
                    //if (value < audioStartPosition)
                    //    sourceStream.Position = sourceOffsetBytes;
                    //else
                    //    sourceStream.Position = sourceOffsetBytes + (value - audioStartPosition);
                    position = value * bytesPerSample;
                }
            }
        }

        /// <summary>
        /// Reads bytes from this wave stream
        /// </summary>
        /// <param name="destBuffer">The destination buffer</param>
        /// <param name="offset">Offset into the destination buffer</param>
        /// <param name="numBytes">Number of bytes read</param>
        /// <returns>Number of bytes read.</returns>
        public override int Read(byte[] destBuffer, int offset, int numBytes)
        {
            lock (lockObject)
            {
                int bytesWritten = 0;
                // 1. fill with silence
                if (position < audioStartPosition)
                {
                    bytesWritten = (int)Math.Min(numBytes, audioStartPosition - position);
                    for (int n = 0; n < bytesWritten; n++)
                        destBuffer[n + offset] = 0;
                }
                if (bytesWritten < numBytes)
                {
                    // don't read too far into source stream
                    int sourceBytesRequired = (int)Math.Min(
                        numBytes - bytesWritten,
                        sourceLengthBytes + sourceOffsetBytes - sourceStream.Position);
                    int read = sourceStream.Read(destBuffer, bytesWritten + offset, sourceBytesRequired);
                    bytesWritten += read;
                }
                // 3. Fill out with zeroes
                for (int n = bytesWritten; n < numBytes; n++)
                    destBuffer[offset + n] = 0;
                position += numBytes;
                return numBytes;
            }
        }

        /// <summary>
        /// <see cref="WaveStream.WaveFormat"/>
        /// </summary>
        public override WaveFormat WaveFormat => sourceStream.WaveFormat;

        /// <summary>
        /// Determines whether this channel has any data to play
        /// to allow optimisation to not read, but bump position forward
        /// </summary>
        public override bool HasData(int count)
        {
            if (position + count < audioStartPosition)
                return false;
            if (position >= length)
                return false;
            // Check whether the source stream has data.
            // source stream should be in the right poisition
            return sourceStream.HasData(count);
        }

        /// <summary>
        /// Disposes this WaveStream
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (sourceStream != null)
                {
                    sourceStream.Dispose();
                    sourceStream = null;
                }
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "WaveOffsetStream was not Disposed");
            }
            base.Dispose(disposing);
        }
    }
}
