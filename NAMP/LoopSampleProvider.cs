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

        private bool loopBegin = false;

        public LoopSampleProvider(WaveStream sourceStream, int start, int end)
        {
            SourceStream = sourceStream;
            providers = new Queue<ISampleProvider>();

            providers.Enqueue(new OffsetSampleProvider(sourceStream.ToSampleProvider())
            {
                TakeSamples = GetChannelMultiple(start) * 2
            });

            providers.Enqueue(new OffsetSampleProvider(sourceStream.ToSampleProvider())
            {
                TakeSamples = GetChannelMultiple(end - start) * 2
            });

            this.start = start;
            this.end = end;

            currentProvider = providers.Dequeue();
        }

        public WaveStream SourceStream { get; private set; }

        public WaveFormat WaveFormat => SourceStream.WaveFormat;

        public bool Loop { get; set; }

        public void Seek(int position)
        {
            providers.Clear();

            SourceStream.Position = position;

            if (position > start && position < end)
            {
                providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider()) { TakeSamples = GetChannelMultiple(end - (int)position) * 2 });

                loopBegin = true;
            }
            else
                providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider()));

            currentProvider = providers.Dequeue();
        }

        private int GetChannelMultiple(int value)
        {
            return value - (value % WaveFormat.Channels);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var read = 0;

            while (read < count
                && (Loop ? SourceStream.Position <= SourceStream.Length
                : SourceStream.Position < SourceStream.Length))
            {
                var needed = count - read;
                var readThisTime = currentProvider.Read(buffer, read, needed);

                read += readThisTime;

                if (readThisTime == 0)
                {
                    if (Loop)
                    {
                        Console.WriteLine("Loop.");

                        if (SourceStream.Position > start)
                            SourceStream.Position = start;

                        providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider())
                        {
                            TakeSamples = GetChannelMultiple(end - start) * 2
                        });

                        currentProvider = providers.Dequeue();
                    }
                    else
                    {
                        Console.WriteLine("End.");

                        if (SourceStream.Position > end)
                            SourceStream.Position = end;

                        providers.Enqueue(new OffsetSampleProvider(SourceStream.ToSampleProvider()));

                        currentProvider = providers.Dequeue();
                    }
                }
            }

            return read;
        }
    }
}
