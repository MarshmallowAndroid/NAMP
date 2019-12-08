using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NAMP
{
    class SfxPlayer : IDisposable
    {
        private readonly WaveOut outputDevice;
        private readonly MixingSampleProvider mixer;

        public SfxPlayer(int sampleRate = 48000, int channels = 2)
        {
            outputDevice = new WaveOut() { DesiredLatency = 100 };
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels))
            {
                ReadFully = true
            };

            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public float Volume { get; set; }

        public void PlaySfx(Stream sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(new CachedSound(sound)));
        }

        private void AddMixerInput(ISampleProvider mixerInput)
        {
            mixer.AddMixerInput(new VolumeSampleProvider(StereoToMono(mixerInput)) { Volume = Volume });
        }

        private ISampleProvider StereoToMono(ISampleProvider input)
        {
            return input.ToStereo();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    class CachedSound
    {
        public CachedSound(Stream audioResource)
        {
            using (CustomVorbisWaveReader cvwr = new CustomVorbisWaveReader(audioResource))
            {
                WaveFormat = cvwr.WaveFormat;
                List<float> wholeFile = new List<float>((int)(cvwr.Length / 4));
                float[] buffer = new float[cvwr.WaveFormat.SampleRate * cvwr.WaveFormat.Channels];

                int samplesRead;
                while ((samplesRead = cvwr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    wholeFile.AddRange(buffer.Take(samplesRead));
                }

                AudioData = wholeFile.ToArray();
            }
        }

        public float[] AudioData { get; private set; }

        public WaveFormat WaveFormat { get; private set; }
    }

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public WaveFormat WaveFormat => cachedSound.WaveFormat;

        public int Read(float[] buffer, int offset, int count)
        {
            long availableSamples = cachedSound.AudioData.Length - position;
            long samplesNeeded = Math.Min(availableSamples, count);

            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesNeeded);

            position += samplesNeeded;

            return (int)samplesNeeded;
        }
    }
}