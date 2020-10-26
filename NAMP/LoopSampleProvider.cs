using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NAMP
{
    public class LoopSampleProvider : ISampleProvider
    {
        private Queue<ISampleProvider> providers;

        private ISampleProvider currentProvider;

        private int start;
        private int end;
        private int startSample;
        private int loopLength;

        public LoopSampleProvider(WaveStream sourceStream, int start, int end, int startSample = 0)
        {
            SourceStream = sourceStream;

            providers = new Queue<ISampleProvider>();

            this.start = start;
            this.end = end;
            this.startSample = startSample;
            this.loopLength = (end - start) * 2;

            providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider())
            {
                TakeSamples = start * 2
            });

            currentProvider = providers.Dequeue();
        }

        public WaveStream SourceStream { get; private set; }

        public long Length => SourceStream.Length;

        public long Position => SourceStream.Position;

        public WaveFormat WaveFormat => SourceStream?.WaveFormat;

        public bool Loop { get; set; }

        public void Seek(long position)
        {
            providers.Clear();

            SourceStream.Position = position;

            if (position > start && position < end)
                providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider())
                {
                    TakeSamples = (end - (int)position) * 2
                });
            else if (position < start)
                providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider())
                {
                    TakeSamples = (start - (int)position) * 2
                });
            else
                providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider()));

            currentProvider = providers.Dequeue();
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int read = 0;

            while (read < count)
            {
                int readThisTime = currentProvider.Read(buffer, offset + read, count - read);

                if (readThisTime == 0)
                {
                    Console.WriteLine("Ended at     :" + SourceStream.Position * 2);
                    Console.WriteLine("Loop start   :" + start * 2);
                    Console.WriteLine("Loop end     :" + end * 2);
                    Console.WriteLine();

                    if (Loop)
                    {
                        if (SourceStream.Position > start)
                            SourceStream.Position = start;

                        providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider())
                        {
                            TakeSamples = loopLength
                        });

                        currentProvider = providers.Dequeue();
                    }
                    else
                    {
                        providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider()));

                        currentProvider = providers.Dequeue();
                    }
                }

                read += readThisTime;
            }

            return read;
        }
    }
}
