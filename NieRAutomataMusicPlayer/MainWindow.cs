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

        FileMapReader Reader = new FileMapReader(@"..\..\..\mapping.txt");

        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine(Environment.CurrentDirectory);
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
            if (VorbisWaveReaders.Count > 0)
            {
                VorbisWaveReader vorbisWaveReader = VorbisWaveReaders.First();
                playPosition.Value = (int)((double)playPosition.Maximum * (vorbisWaveReader.CurrentTime.TotalSeconds / vorbisWaveReader.TotalTime.TotalSeconds));
            }
        }

        private void PlayPosition_Scroll(object sender, EventArgs e)
        {
            if (VorbisWaveReaders?.Count > 0)
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

        private void PlayButton_Click(object sender, EventArgs e)
        {
            string selectedSong = songList.SelectedItem?.ToString();

            OutputDevice?.Stop();
            OutputDevice?.Dispose();

            InitPlayer(selectedSong);
            InitVolumes();

            if (MixingSampleProvider != null)
            {
                OutputDevice.Init(MixingSampleProvider);
                OutputDevice.Play();
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                pauseButton.Text = "Pause";
                OutputDevice.Resume();
            }
            else
            {
                pauseButton.Text = "Resume";
                OutputDevice.Pause();
            }
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
                mainTracksPanel.Controls.Clear();
                overlayTracksPanel.Controls.Clear();

                foreach (var trackName in trackNames)
                {
                    if (trackName.StartsWith("ins"))
                    {
                        string trackFile = Reader.GetValue(selectedSong, trackName);
                        trackList.Items.Add(new ListViewItem(new[] { trackName, trackFile }));

                        string friendlyTrackName = "";

                        if (trackName.Contains("qui"))
                            friendlyTrackName += "Quiet ";
                        if (trackName.Contains("nml"))
                            friendlyTrackName += "Normal ";
                        if (trackName.Contains("med"))
                            friendlyTrackName += "Medium ";
                        if (trackName.Contains("dyn"))
                            friendlyTrackName += "Dynamic ";
                        if (trackName.Contains("8bt"))
                            friendlyTrackName += "8-bit ";

                        if (trackName.Contains("voc"))
                            friendlyTrackName += "Vocals ";

                        RadioButton trackRadioButton = new RadioButton()
                        {
                            Text = friendlyTrackName.Trim(),
                            Name = trackName
                        };
                        trackRadioButton.CheckedChanged += TrackRadioButton_CheckedChanged;

                        mainTracksPanel.Controls.Add(trackRadioButton);
                    }
                    else
                    {
                        string trackFile = Reader.GetValue(selectedSong, trackName);
                        trackList.Items.Add(new ListViewItem(new[] { trackName, trackFile }));

                        CheckBox separatedVocalsCheckBox = new CheckBox()
                        {
                            Text = "Vocals",
                            Name = trackName
                        };
                        separatedVocalsCheckBox.CheckedChanged += SeparatedVocalsCheckBox_CheckedChanged;

                        overlayTracksPanel.Controls.Add(separatedVocalsCheckBox);
                    }
                }

                InitPlayer(selectedSong);

                ((RadioButton)mainTracksPanel.Controls[0]).Checked = true;

                InitVolumes();

                trackList.Sorting = SortOrder.Ascending;
                trackList.Sort();
            }
        }

        private void InitPlayer(string songName)
        {
            bool isMSPInit = false;
            string[] tracks = Reader.GetAvailableTracks(songName);

            OutputDevice?.Stop();

            string musicDirectory = musicPath.Text + "\\" + songName + "\\";

            VorbisWaveReaders = new List<VorbisWaveReader>();
            LoopStreams = new List<LoopStream>();
            VolumeSampleProviders = new List<KeyValuePair<string, VolumeSampleProvider>>();

            foreach (var track in tracks)
            {
                string trackFile = Reader.GetValue(songName, track);
                int loopStart = int.Parse(Reader.GetValue(songName, "loop_start"));
                int loopEnd = int.Parse(Reader.GetValue(songName, "loop_end"));

                VorbisWaveReaders.Add(new VorbisWaveReader(musicDirectory + trackFile));
                LoopStreams.Add(new LoopStream(VorbisWaveReaders.Last(), loopStart, loopEnd));
                VolumeSampleProviders.Add(new KeyValuePair<string, VolumeSampleProvider>(track, new VolumeSampleProvider(LoopStreams.Last().ToSampleProvider())));

                if (!isMSPInit)
                {
                    MixingSampleProvider = new MixingSampleProvider(VorbisWaveReaders.First().WaveFormat);
                    isMSPInit = true;
                }

                MixingSampleProvider.AddMixerInput(VolumeSampleProviders.Last().Value);
            }
        }

        private void InitVolumes()
        {
            RadioButton initialTrack = null;

            foreach (RadioButton control in mainTracksPanel.Controls)
            {
                if (control.Checked)
                    initialTrack = control;
            }

            foreach (var vsp in VolumeSampleProviders)
            {
                if (vsp.Key.StartsWith("ins"))
                {
                    if (vsp.Key == initialTrack.Name)
                        vsp.Value.Volume = 1.0f;
                    else
                        vsp.Value.Volume = 0.0f;
                }
                else
                    vsp.Value.Volume = 0.0f;
            }

            PositionUpdate.Enabled = true;
        }

        private void SeparatedVocalsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            string selectedTrack = checkBox.Name;

            float speed = 0.025f;
            Timer timer = new Timer()
            {
                Interval = 33
            };

            VolumeSampleProvider fade = null;

            foreach (var item in VolumeSampleProviders)
            {
                var vsp = item.Value;
                if (item.Key == selectedTrack)
                    fade = vsp;
            }

            if (!checkBox.Checked)
            {
                fade.Volume = 1.0f;
                timer.Tick += (sndr, evt) =>
                {
                    fade.Volume -= speed;
                    if (fade.Volume <= 0.0f)
                    {
                        fade.Volume = 0.0f;
                        timer.Enabled = false;
                    }
                };
            }
            else
            {
                fade.Volume = 0.0f;
                timer.Tick += (sndr, evt) =>
                {
                    fade.Volume += speed;
                    if (fade.Volume >= 1.0f)
                    {
                        fade.Volume = 1.0f;
                        timer.Enabled = false;
                    }
                };
            }

            timer.Enabled = true;
        }

        private void TrackRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            string selectedSong = songList.SelectedItem?.ToString();

            if (OutputDevice.PlaybackState != PlaybackState.Playing)
                InitPlayer(selectedSong);

            if (VolumeSampleProviders?.Count > 0)
            {
                RadioButton radioButton = (RadioButton)sender;
                string selectedTrack = radioButton.Name;

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
                    if (item.Key.StartsWith("ins"))
                    {
                        if (item.Key == selectedTrack)
                            fadeIn = vsp;

                        if (vsp.Volume > 0.0f)
                            fadeOut = vsp;
                    }
                }

                if (fadeIn != null)
                    fadeIn.Volume = 0.0f;

                if (fadeOut != null)
                    fadeOut.Volume = 1.0f;

                timer.Tick += (sndr, evt) =>
                {
                    fadeOut.Volume -= speed;
                    fadeIn.Volume += speed;

                    fadeProgress.Value = Math.Max((int)(fadeOut.Volume * 100.0f), 0);

                    if (fadeOut.Volume <= 0.0f)
                    {
                        fadeIn.Volume = 1.0f;
                        fadeOut.Volume = 0.0f;

                        fadeProgress.Visible = false;
                        timer.Enabled = false;
                    }
                };

                if (OutputDevice.PlaybackState != PlaybackState.Stopped)
                {
                    fadeProgress.Visible = true;
                }

                timer.Enabled = true;
            }
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