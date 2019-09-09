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

            Environment.CurrentDirectory = @"F:\NieRAutomata Modding\NME2-0.5-alpha-x64\x64\separated\pascal's village";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            OutputDevice?.Stop();
            VorbisFile1 = new VorbisWaveReader(@"BGM_0_004_[9].ogg");
            VorbisFile2 = new VorbisWaveReader(@"BGM_0_004_[10].ogg");
            //VorbisFile3 = new VorbisWaveReader(@"BGM_0_003_[8].ogg");

            int loopStart = 1460865;
            int loopEnd = 6970413;

            TestStream1 = new BetterCustomLooper(VorbisFile1, loopStart, loopEnd);
            TestStream2 = new BetterCustomLooper(VorbisFile2, loopStart, loopEnd);
            //testStream3 = new BetterCustomLooper(VorbisFile3, loopStart, loopEnd);

            VolumeSampleProvider1 = new VolumeSampleProvider(TestStream1.ToSampleProvider());
            VolumeSampleProvider2 = new VolumeSampleProvider(TestStream2.ToSampleProvider());
            //VolumeSampleProvider3 = new VolumeSampleProvider(TestStream3.ToSampleProvider());

            MixingSampleProvider = new MixingSampleProvider(TestStream1.WaveFormat);

            if (MixingSampleProvider.MixerInputs.Count() > 0)
            {
                MixingSampleProvider.RemoveAllMixerInputs();
            }
            MixingSampleProvider.AddMixerInput(VolumeSampleProvider1);
            MixingSampleProvider.AddMixerInput(VolumeSampleProvider2);
            //MixingSampleProvider.AddMixerInput(TestStream3);

            VolumeSampleProvider1.Volume = 1.0f;
            VolumeSampleProvider2.Volume = 0.0f;
            //VolumeSampleProvider3.Volume = 0.0f;

            OutputDevice.Init(MixingSampleProvider);

            timer1.Enabled = true;

            //VorbisFile.Position = 1481735 * VorbisFile.BlockAlign;

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
            //if (checkBox1.Checked)
            //{
            //    MixingSampleProvider.RemoveMixerInput(MixingSampleProvider.MixerInputs.ElementAt(lastMixer));
            //    MixingSampleProvider.AddMixerInput(TestStream2);
            //    TestStream2.Position = TestStream1.Position;
            //    //lastMixer++;
            //}
            //else
            //{
            //    MixingSampleProvider.RemoveMixerInput(MixingSampleProvider.MixerInputs.ElementAt(lastMixer));
            //    MixingSampleProvider.AddMixerInput(TestStream1);
            //    TestStream1.Position = TestStream2.Position;
            //    //lastMixer--;
            //}
            if (checkBox1.Checked)
            {
                VolumeSampleProvider1.Volume = 1.0f;
                VolumeSampleProvider2.Volume = 0.0f;

                Timer timer = new Timer()
                {
                    Interval = 33
                };

                timer.Tick += (sndr, evt) =>
                {
                    VolumeSampleProvider1.Volume -= 0.025f;
                    VolumeSampleProvider2.Volume += 0.025f;

                    if (VolumeSampleProvider1.Volume <= 0.0f)
                    {
                        VolumeSampleProvider2.Volume = 1.0f;
                        VolumeSampleProvider1.Volume = 0.0f;

                        timer.Enabled = false;
                    }
                };

                timer.Enabled = true;
            }
            else
            {
                VolumeSampleProvider2.Volume = 1.0f;
                VolumeSampleProvider1.Volume = 0.0f;

                Timer timer = new Timer()
                {
                    Interval = 33
                };

                timer.Tick += (sndr, evt) =>
                {
                    VolumeSampleProvider2.Volume -= 0.025f;
                    VolumeSampleProvider1.Volume += 0.025f;

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
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            if (OutputDevice.PlaybackState == PlaybackState.Paused)
                OutputDevice.Resume();
            else
                OutputDevice.Pause();
        }
    }

    public class BetterCustomLooper : WaveStream
    {
        private WaveStream waveStream;
        private int loopStart;
        private int loopEnd;

        public BetterCustomLooper(WaveStream source, int loopStart, int loopEnd)
        {
            waveStream = source;
            this.loopStart = loopStart * waveStream.BlockAlign;
            this.loopEnd = loopEnd * waveStream.BlockAlign;

            WaveFormat = waveStream.WaveFormat;
            Length = source.Length;
        }

        public override WaveFormat WaveFormat { get; }

        public override long Length { get; }

        public override long Position { get => waveStream.Position; set => waveStream.Position = value; }

        int previous;

        public override int Read(byte[] buffer, int offset, int count)
        {

            int remainder = (int)Position - loopEnd;

            if (Position >= loopEnd)
            {
                Position = loopStart;

                waveStream.Read(buffer, offset, remainder);
            }

            int bytesRead = waveStream.Read(buffer, offset, count);
            previous = bytesRead;

            return bytesRead;
        }

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
