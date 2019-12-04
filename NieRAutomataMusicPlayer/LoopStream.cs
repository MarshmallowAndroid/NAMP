using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NieRAutomataMusicTest
{
    //public class LoopStream : WaveStream
    //{
    //    private readonly CustomVorbisWaveReader sourceStream;
    //    private readonly int loopStart;
    //    private readonly int loopEnd;

    //    public LoopStream(CustomVorbisWaveReader source, int loopStart, int loopEnd)
    //    {
    //        sourceStream = source;

    //        WaveFormat = sourceStream.WaveFormat;
    //        Length = source.Length;

    //        // Convert samples to bytes
    //        //this.loopStart = (loopStart + (loopStart % WaveFormat.Channels)) * WaveFormat.BlockAlign;
    //        //this.loopEnd = (loopEnd + (loopEnd % WaveFormat.Channels)) * WaveFormat.BlockAlign;

    //        //int tempLoopStart = loopStart * ((WaveFormat.BitsPerSample / 8) * WaveFormat.Channels);
    //        //int tempLoopEnd = loopEnd * ((WaveFormat.BitsPerSample / 8) * WaveFormat.Channels);

    //        //this.loopStart = tempLoopStart;
    //        //this.loopEnd = tempLoopEnd;

    //        this.loopStart = loopStart;
    //        this.loopEnd = loopEnd;

    //        Console.WriteLine(Length);
    //    }

    //    public override WaveFormat WaveFormat { get; }

    //    public override long Length { get; }

    //    public override long Position
    //    {
    //        get
    //        {
    //            try
    //            {
    //                if (sourceStream != null)
    //                    return sourceStream.Position;
    //            }
    //            catch (NullReferenceException)
    //            {
    //                return 0;
    //            }

    //            return 0;
    //        }
    //        set
    //        {
    //            if (sourceStream != null)
    //                sourceStream.Position = value;
    //        }
    //    }

    //    bool loop = false;

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        int bytesRead = 0;

    //        if (loop)
    //        {
    //            Position = loopStart;
    //            loop = false;
    //        }

    //        if ((Position + count) >= loopEnd)
    //        {
    //            int remainder = loopEnd - (int)Position;

    //            bytesRead = sourceStream.Read(buffer, offset, count);

    //            loop = true;

    //            Console.WriteLine(remainder);
    //        }
    //        else
    //        {
    //            bytesRead = sourceStream.Read(buffer, offset, count);
    //        }


    //        //if (Position >= loopEnd)
    //        //{
    //        //    loop = true;
    //        //}

    //        return bytesRead;
    //    }
    //}
    
    public class LoopSampleProvider : ISampleProvider
    {
        private Queue<ISampleProvider> providers;

        private ISampleProvider currentProvider;

        private int start;
        private int end;

        public LoopSampleProvider(WaveStream baseSource, int start, int end)
        {
            providers = new Queue<ISampleProvider>();

            providers.Enqueue(new OffsetSampleProvider(baseSource.ToSampleProvider())
            {
                TakeSamples = ((start - 1) - ((start - 1) % 2)) * 2
            });

            providers.Enqueue(new OffsetSampleProvider(baseSource.ToSampleProvider())
            {
                TakeSamples = ((end - start) - ((end - start) % 2)) * 2
            });

            BaseSource = baseSource;
            this.start = start;
            this.end = end;

            currentProvider = providers.Dequeue();

            Console.WriteLine();
        }

        public WaveStream BaseSource { get; private set; }

        public WaveFormat WaveFormat => providers.Peek().WaveFormat;

        public bool Loop { get; set; }

        public int Read(float[] buffer, int offset, int count)
        {
            var read = 0;

            while (read < count)
            {
                var needed = count - read;
                var readThisTime = currentProvider.Read(buffer, read, needed);

                read += readThisTime;

                if (readThisTime == 0)
                {
                    if (Loop)
                    {
                        Console.WriteLine("Loop.");

                        // For some reason, readThisTime stays at 0 for some songs, so make it so that
                        // we only seek to the start when the stream has advanced.
                        if (BaseSource.Position > start) BaseSource.Position = start;

                        providers.Enqueue(new OffsetSampleProvider(BaseSource.ToSampleProvider()) { TakeSamples = ((end - start) - ((end - start) % 2)) * 2 });
                        currentProvider = providers.Dequeue();
                    }
                    else
                    {
                        providers.Enqueue(new OffsetSampleProvider(BaseSource.ToSampleProvider()));
                        currentProvider = providers.Dequeue();
                    }
                }
            }

            return read;
        }
    }
}
