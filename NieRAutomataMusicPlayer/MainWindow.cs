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
using System.IO;

namespace NieRAutomataMusicTest
{
    public partial class MainWindow : Form
    {
        WaveOut OutputDevice = new WaveOut();

        List<VorbisWaveReader> VorbisWaveReaders;
        List<LoopStream> LoopStreams;
        List<KeyValuePair<string, VolumeSampleProvider>> VolumeSampleProviders;

        MixingSampleProvider MixingSampleProvider;

        FileMapReader Reader = new FileMapReader(@"C:\Users\Jacob\source\repos\NAMP\mapping.txt");

        public MainWindow()
        {
            InitializeComponent();

            Environment.CurrentDirectory = musicPath.Text;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            OutputDevice?.Stop();
            OutputDevice?.Dispose();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            string[] availableSongs = Reader.GetAvailableSongs();
            songList.Items.AddRange(availableSongs);
        }

        private void PositionUpdate_Tick(object sender, EventArgs e)
        {
            VorbisWaveReader vorbisWaveReader = VorbisWaveReaders.First();
            playPosition.Value = (int)((double)playPosition.Maximum * (vorbisWaveReader.CurrentTime.TotalSeconds / vorbisWaveReader.TotalTime.TotalSeconds));
        }

        private void PlayPosition_Scroll(object sender, EventArgs e)
        {
            if (VorbisWaveReaders.Count > 0)
            {
                double newTime = VorbisWaveReaders.First().TotalTime.TotalSeconds * (playPosition.Value / (double)playPosition.Maximum);

                try
                {
                    foreach (var vwr in VorbisWaveReaders)
                    {
                        vwr.CurrentTime = TimeSpan.FromSeconds(newTime);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (OutputDevice.PlaybackState == PlaybackState.Paused)
                OutputDevice.Resume();
            else
                OutputDevice.Pause();
        }

        private void SongList_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedSong = songList.SelectedItem?.ToString();

            if (VolumeSampleProviders?.Count > 0)
            {
                foreach (var item in LoopStreams)
                {
                    item.Dispose();
                }

                foreach (var item in VorbisWaveReaders)
                {
                    item.Dispose();
                }
            }

            if (selectedSong != null)
            {
                string[] trackNames = Reader.GetAvailableTracks(selectedSong);

                trackList.Items.Clear();
                mainTracks.Controls.Clear();

                foreach (var trackName in trackNames)
                {
                    string trackFile = Reader.GetValue(selectedSong, trackName);
                    trackList.Items.Add(new ListViewItem(new[] { trackName, trackFile }));

                    RadioButton trackRadioButton = new RadioButton()
                    {
                        Text = trackName,
                    };
                    trackRadioButton.CheckedChanged += TrackRadioButton_CheckedChanged;

                    mainTracks.Controls.Add(trackRadioButton);
                }

                SetupPlayer(selectedSong);

                ((RadioButton)mainTracks.Controls[0]).Checked = true;

                trackList.Sorting = SortOrder.Ascending;
                trackList.Sort();
            }
        }

        private void SetupPlayer(string songName)
        {
            bool isMSPInit = false;
            string[] tracks = Reader.GetAvailableTracks(songName);

            OutputDevice?.Stop();

            Environment.CurrentDirectory = @"F:\NieRAutomata Modding\NME2-0.5-alpha-x64\x64\separated\" + songName;

            VorbisWaveReaders = new List<VorbisWaveReader>();
            LoopStreams = new List<LoopStream>();
            VolumeSampleProviders = new List<KeyValuePair<string, VolumeSampleProvider>>();

            foreach (var track in tracks)
            {
                string trackFile = Reader.GetValue(songName, track);
                int loopStart = int.Parse(Reader.GetValue(songName, "loop_start"));
                int loopEnd = int.Parse(Reader.GetValue(songName, "loop_end"));

                VorbisWaveReaders.Add(new VorbisWaveReader(trackFile));
                LoopStreams.Add(new LoopStream(VorbisWaveReaders.Last(), loopStart, loopEnd));
                VolumeSampleProviders.Add(new KeyValuePair<string, VolumeSampleProvider>(track, new VolumeSampleProvider(LoopStreams.Last().ToSampleProvider())));

                if (!isMSPInit)
                {
                    MixingSampleProvider = new MixingSampleProvider(VorbisWaveReaders.First().WaveFormat);
                    isMSPInit = true;
                }

                MixingSampleProvider.AddMixerInput(VolumeSampleProviders.Last().Value);
            }

            RadioButton firstTrack = (RadioButton)mainTracks.Controls[0];
            foreach (var vsp in VolumeSampleProviders)
            {
                if (vsp.Key != firstTrack.Text)
                {
                    vsp.Value.Volume = 0.0f;
                }
                else
                    vsp.Value.Volume = 1.0f;
            }

            PositionUpdate.Enabled = true;

            OutputDevice.Init(MixingSampleProvider);
            OutputDevice.Play();
        }

        private void TrackRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            string selectedTrack = radioButton.Text;

            float speed = 0.025f;

            Timer timer = new Timer()
            {
                Interval = 33
            };

            VolumeSampleProvider fadeIn = null;
            VolumeSampleProvider fadeOut = null;

            foreach (var item in VolumeSampleProviders)
            {
                var vsp = item.Value;

                if (item.Key == selectedTrack)
                    fadeIn = vsp;

                if (vsp.Volume == 1.0f)
                    fadeOut = vsp;
            }

            fadeIn.Volume = 0.0f;
            fadeOut.Volume = 1.0f;

            timer.Tick += (sndr, evt) =>
            {
                fadeOut.Volume -= speed;
                fadeIn.Volume += speed;

                if (fadeOut.Volume <= 0.0f)
                {
                    fadeIn.Volume = 1.0f;
                    fadeOut.Volume = 0.0f;

                    timer.Enabled = false;
                }
            };

            timer.Enabled = true;
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
}