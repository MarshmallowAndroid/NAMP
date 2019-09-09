using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Vorbis;

namespace NieRAutomataMusicTest
{
    public partial class MainWindow : Form
    {
        WaveOut OutputDevice = new WaveOut();
        AudioFileReader AudioFile;
        VorbisWaveReader VorbisFile1;
        VorbisWaveReader VorbisFile2;
        VorbisWaveReader VorbisFile3;
        BetterCustomLooper TestStream1;
        BetterCustomLooper TestStream2;
        BetterCustomLooper TestStream3;
        VolumeSampleProvider VolumeSampleProvider1;
        VolumeSampleProvider VolumeSampleProvider2;
        VolumeSampleProvider VolumeSampleProvider3;
        MixingSampleProvider MixingSampleProvider;

        public MainWindow()
        {
            InitializeComponent();

            string songName = "the sound of the end";
            Environment.CurrentDirectory = @"F:\NieRAutomata Modding\NME2-0.5-alpha-x64\x64\separated\" + songName;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OutputDevice?.Stop();
            VorbisFile1 = new VorbisWaveReader(@"BGM_2_000_[13].ogg");
            VorbisFile2 = new VorbisWaveReader(@"BGM_0_004_[1].ogg");
            VorbisFile3 = new VorbisWaveReader(@"BGM_2_000_[16].ogg");

            int loopStart = 1276101;
            int loopEnd = 7479221;

            TestStream1 = new BetterCustomLooper(VorbisFile1, loopStart, loopEnd);
            TestStream2 = new BetterCustomLooper(VorbisFile2, loopStart, loopEnd);
            TestStream3 = new BetterCustomLooper(VorbisFile3, loopStart, loopEnd);

            VolumeSampleProvider1 = new VolumeSampleProvider(TestStream1.ToSampleProvider());
            VolumeSampleProvider2 = new VolumeSampleProvider(TestStream2.ToSampleProvider());
            VolumeSampleProvider3 = new VolumeSampleProvider(TestStream3.ToSampleProvider());

            MixingSampleProvider = new MixingSampleProvider(TestStream1.WaveFormat);

            if (MixingSampleProvider.MixerInputs.Count() > 0)
            {
                MixingSampleProvider.RemoveAllMixerInputs();
            }

            MixingSampleProvider.AddMixerInput(VolumeSampleProvider1);
            MixingSampleProvider.AddMixerInput(VolumeSampleProvider2);
            MixingSampleProvider.AddMixerInput(VolumeSampleProvider3);

            VolumeSampleProvider1.Volume = 1.0f;
            VolumeSampleProvider2.Volume = 0.0f;
            VolumeSampleProvider3.Volume = 0.0f;

            OutputDevice.Init(MixingSampleProvider);

            timer1.Enabled = true;

            OutputDevice.Play();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                trackBar1.Value = (int)((double)trackBar1.Maximum * (VorbisFile2.CurrentTime.TotalSeconds / VorbisFile2.TotalTime.TotalSeconds));
            else
                trackBar1.Value = (int)((double)trackBar1.Maximum * (VorbisFile1.CurrentTime.TotalSeconds / VorbisFile1.TotalTime.TotalSeconds));
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            if (VorbisFile1 != null)
            {
                double newTime = VorbisFile1.TotalTime.TotalSeconds * (trackBar1.Value / (double)trackBar1.Maximum);

                try
                {
                    VorbisFile1.CurrentTime = TimeSpan.FromSeconds(newTime);
                    VorbisFile2.CurrentTime = TimeSpan.FromSeconds(newTime);
                }
                catch (Exception)
                {
                }
            }
        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            float speed = 0.025f;

            Timer timer = new Timer()
            {
                Interval = 33
            };

            VorbisFile2.Position = VorbisFile1.Position;

            if (checkBox1.Checked)
            {
                VolumeSampleProvider1.Volume = 1.0f;
                VolumeSampleProvider2.Volume = 0.0f;

                timer.Tick += (sndr, evt) =>
                {
                    VolumeSampleProvider1.Volume -= speed;
                    VolumeSampleProvider2.Volume += speed;

                    if (VolumeSampleProvider1.Volume <= 0.0f)
                    {
                        VolumeSampleProvider1.Volume = 0.0f;
                        VolumeSampleProvider2.Volume = 1.0f;


                        timer.Enabled = false;
                    }
                };

                timer.Enabled = true;
            }
            else
            {
                VolumeSampleProvider1.Volume = 0.0f;
                VolumeSampleProvider2.Volume = 1.0f;

                timer.Tick += (sndr, evt) =>
                {
                    VolumeSampleProvider2.Volume -= speed;
                    VolumeSampleProvider1.Volume += speed;

                    if (VolumeSampleProvider2.Volume <= 0.0f)
                    {
                        VolumeSampleProvider1.Volume = 1.0f;
                        VolumeSampleProvider2.Volume = 0.0f;

                        timer.Enabled = false;
                    }
                };

                timer.Enabled = true;
            }
        }

        private void CheckBox2_CheckedChanged(object sender, EventArgs e)
        {
            float speed = 0.025f;

            Timer timer = new Timer()
            {
                Interval = 33
            };

            VorbisFile3.Position = VorbisFile1.Position;

            if (checkBox2.Checked)
            {
                VolumeSampleProvider3.Volume = 0.0f;

                timer.Tick += (sndr, evt) =>
                {
                    VolumeSampleProvider3.Volume += speed;
                    Console.WriteLine(VolumeSampleProvider3.Volume);

                    if (VolumeSampleProvider3.Volume >= 1.0f)
                    {
                        VolumeSampleProvider3.Volume = 1.0f;

                        timer.Enabled = false;
                    }
                };

                timer.Enabled = true;
            }
            else
            {
                VolumeSampleProvider3.Volume = 1.0f;

                timer.Tick += (sndr, evt) =>
                {
                    VolumeSampleProvider3.Volume -= speed;

                    if (VolumeSampleProvider3.Volume <= 0.0f)
                    {
                        VolumeSampleProvider3.Volume = 0.0f;

                        timer.Enabled = false;
                    }
                };

                timer.Enabled = true;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (OutputDevice.PlaybackState == PlaybackState.Paused)
                OutputDevice.Resume();
            else
                OutputDevice.Pause();
        }
    }

    public class CachedMusic
    {
        public float[] SoundData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }

        public CachedMusic(string filename)
        {
            using (var vorbisWaveReader = new VorbisWaveReader(filename))
            {
                WaveFormat = vorbisWaveReader.WaveFormat;

                var whole = new List<float>((int)(vorbisWaveReader.Length / 4));
                var buffer = new float[vorbisWaveReader.WaveFormat.SampleRate * vorbisWaveReader.WaveFormat.Channels];

                int samplesRead;

                while ((samplesRead = vorbisWaveReader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    whole.AddRange(buffer.Take(samplesRead));
                }

                SoundData = whole.ToArray();
            }
        }
    }

    public class CachedMusicSampleProvider : ISampleProvider
    {
        private readonly CachedMusic cachedMusic;
        private long position;

        public WaveFormat WaveFormat => throw new NotImplementedException();

        public int Read(float[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }

    // Loops through specified samples
    public class BetterCustomLooper : WaveStream
    {
        private WaveStream waveStream;
        private readonly int loopStart;
        private readonly int loopEnd;

        public BetterCustomLooper(WaveStream source, int loopStart, int loopEnd)
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
