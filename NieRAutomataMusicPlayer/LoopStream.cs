using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NieRAutomataMusicTest
{
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
        }

        public WaveStream BaseSource { get; private set; }

        public WaveFormat WaveFormat => providers.Peek().WaveFormat;

        public bool Loop { get; set; }

        public void Seek(int position)
        {
            providers.Clear();

            BaseSource.Position = position;

            providers.Enqueue(new OffsetSampleProvider(BaseSource.ToSampleProvider()) { TakeSamples = ((end - position) - ((end - position) % 2)) * 2 });
            currentProvider = providers.Dequeue();
        }

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

                        // Only seek when the stream has advanced.
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
