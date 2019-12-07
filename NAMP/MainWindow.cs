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

namespace NAMP
{
    public partial class MainWindow : Form
    {
        WaveOut OutputDevice = new WaveOut();

        List<CustomVorbisWaveReader> CustomVorbisWaveReaders;
        List<LoopSampleProvider> LoopSampleProviders;
        List<KeyValuePair<string, VolumeSampleProvider>> VolumeSampleProviders;

        MixingSampleProvider MixingSampleProvider;

        FileMapReader MapReader;

        float fadeSpeed = 0.0075f;
        int fadeInterval = 16;

        string mapLocation = @"mapping.txt";

        public MainWindow()
        {
            InitializeComponent();

            // OutputDevice.DesiredLatency = 100;
            OutputDevice.PlaybackStopped += (sndr, evt) =>
            {
                Flush();
            };
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            OutputDevice?.Stop();
            OutputDevice?.Dispose();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            MapReader = new FileMapReader(mapLocation);

            string[] availableSongs = MapReader.GetAvailableSongs();
            songList.Items.AddRange(availableSongs);

            MapReader.Dispose();
        }

        private void PositionUpdate_Tick(object sender, EventArgs e)
        {
            if (OutputDevice.PlaybackState != PlaybackState.Stopped)
            {
                CustomVorbisWaveReader customVorbisWaveReader = CustomVorbisWaveReaders.First();
                long currentPos = customVorbisWaveReader.Position;
                long currentLength = customVorbisWaveReader.Length;

                playPosition.Value = (int)((double)playPosition.Maximum * ((double)currentPos / (double)currentLength));
            }
        }

        private void PlayPosition_Scroll(object sender, EventArgs e)
        {
            if (LoopSampleProviders.Count > 0)
            {
                long currentLength = LoopSampleProviders.First().SourceStream.Length;
                int newPos = (int)(currentLength * ((double)playPosition.Value / (double)playPosition.Maximum));

                foreach (var lsp in LoopSampleProviders)
                {
                    lsp.Seek(newPos - (newPos % lsp.WaveFormat.Channels));
                }
            }
        }

        private void LoopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (OutputDevice.PlaybackState != PlaybackState.Stopped)
            {
                foreach (var lsp in LoopSampleProviders)
                {
                    lsp.Loop = loopCheckBox.Checked;
                }
            }
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            string selectedSong = songList.SelectedItem?.ToString();

            OutputDevice.Stop();
            OutputDevice.Dispose();

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
            WaveOut tempPlayer = new WaveOut();
            VorbisWaveReader pauseVorbis = new VorbisWaveReader("pause.ogg");
            VorbisWaveReader resumeVorbis = new VorbisWaveReader("resume.ogg");

            if (OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                pauseButton.Enabled = false;

                pauseButton.Text = "Pause";

                tempPlayer.Init(resumeVorbis);
                tempPlayer.Play();

                tempPlayer.PlaybackStopped += (snd, evt) =>
                {
                    tempPlayer.Dispose();
                    pauseVorbis.Dispose();
                    resumeVorbis.Dispose();

                    OutputDevice.Play();

                    pauseButton.Enabled = true;
                };

            }
            else
            {
                pauseButton.Enabled = false;

                pauseButton.Text = "Resume";

                tempPlayer.Init(pauseVorbis);
                tempPlayer.Play();

                tempPlayer.PlaybackStopped += (snd, evt) =>
                {
                    tempPlayer.Dispose();
                    pauseVorbis.Dispose();
                    resumeVorbis.Dispose();

                    pauseButton.Enabled = true;
                };

                OutputDevice.Pause();
            }

        }

        private void Flush()
        {
            if (LoopSampleProviders?.Count > 0 || VolumeSampleProviders?.Count > 0)
            {
                foreach (var item in LoopSampleProviders)
                {
                    item.SourceStream.Dispose();
                }

                foreach (var item in CustomVorbisWaveReaders)
                {
                    item.Dispose();
                }
            }
        }

        private void InitPlayer(string songName)
        {
            MapReader = new FileMapReader(mapLocation);

            OutputDevice.Stop();

            bool isMSPInit = false;
            string[] tracks = MapReader.GetAvailableTracks(songName);

            string musicDirectory = musicPath.Text + "\\";

            CustomVorbisWaveReaders = new List<CustomVorbisWaveReader>();
            LoopSampleProviders = new List<LoopSampleProvider>();
            VolumeSampleProviders = new List<KeyValuePair<string, VolumeSampleProvider>>();

            foreach (var track in tracks)
            {
                string trackFile = MapReader.GetValue(songName, track);
                int loopStart = int.Parse(MapReader.GetValue(songName, "loop_start"));
                int loopEnd = int.Parse(MapReader.GetValue(songName, "loop_end"));

                CustomVorbisWaveReaders.Add(new CustomVorbisWaveReader(musicDirectory + trackFile));

                Console.WriteLine("Add LoopStream for song " + songName + " track " + track + ".");

                LoopSampleProviders.Add(new LoopSampleProvider(CustomVorbisWaveReaders.Last(), loopStart, loopEnd) { Loop = true });
                VolumeSampleProviders.Add(new KeyValuePair<string, VolumeSampleProvider>(track, new VolumeSampleProvider(LoopSampleProviders.Last())));

                if (!isMSPInit)
                {
                    MixingSampleProvider = new MixingSampleProvider(CustomVorbisWaveReaders.First().WaveFormat);
                    isMSPInit = true;
                }

                MixingSampleProvider.AddMixerInput(VolumeSampleProviders.Last().Value);
            }

            MapReader.Dispose();
        }

        private void InitVolumes()
        {
            RadioButton initialMainTrack = null;
            RadioButton initialOverlayTrack = null;

            foreach (RadioButton control in mainTracksPanel.Controls)
            {
                if (control.Checked)
                    initialMainTrack = control;
            }

            foreach (RadioButton control in overlayTracksPanel.Controls)
            {
                if (control.Checked)
                    initialOverlayTrack = control;
            }

            foreach (var vsp in VolumeSampleProviders)
            {
                if (vsp.Key.StartsWith("ins"))
                {
                    if (vsp.Key == initialMainTrack.Name)
                        vsp.Value.Volume = 1.0f;
                    else
                        vsp.Value.Volume = 0.0f;
                }

                if (initialOverlayTrack != null)
                {
                    if (vsp.Key.StartsWith("voc"))
                    {
                        if (vsp.Key == initialOverlayTrack.Name)
                            vsp.Value.Volume = 1.0f;
                        else
                            vsp.Value.Volume = 0.0f;
                    }
                }
            }

            PositionUpdate.Enabled = true;
        }

        private void SongList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapReader = new FileMapReader(mapLocation);

            OutputDevice.Stop();

            string selectedSong = songList.SelectedItem?.ToString();

            if (selectedSong != null)
            {
                bool offRadioButtonAdded = false;
                string[] trackNames = MapReader.GetAvailableTracks(selectedSong);

                trackList.Items.Clear();
                mainTracksPanel.Controls.Clear();
                overlayTracksPanel.Controls.Clear();

                foreach (var trackName in trackNames)
                {
                    if (trackName.StartsWith("ins"))
                    {
                        string trackFile = MapReader.GetValue(selectedSong, trackName);
                        trackList.Items.Add(new ListViewItem(new[] { trackName, trackFile }));

                        string friendlyTrackName = "";

                        if (trackName.Contains("qui"))
                            friendlyTrackName += "Quiet ";
                        if (trackName.Contains("nml"))
                            friendlyTrackName += "Normal ";
                        if (trackName.Contains("med"))
                            friendlyTrackName += "Medium ";
                        if (trackName.Contains("8bt"))
                            friendlyTrackName += "8-bit ";

                        if (trackName.Contains("voc"))
                            friendlyTrackName += "Vocals ";

                        if (int.TryParse(trackName.Split('_').Last(), out int number))
                        {
                            friendlyTrackName += number.ToString();
                        }

                        RadioButton trackRadioButton = new RadioButton()
                        {
                            Text = friendlyTrackName.Trim(),
                            Name = trackName
                        };
                        trackRadioButton.CheckedChanged += TrackRadioButton_CheckedChanged;

                        mainTracksPanel.Controls.Add(trackRadioButton);
                    }
                    else if (trackName.StartsWith("voc"))
                    {
                        if (!offRadioButtonAdded)
                        {
                            RadioButton noneRadioButton = new RadioButton()
                            {
                                Text = "None",
                                Name = "none"
                            };
                            noneRadioButton.CheckedChanged += NoneRadioButton_CheckedChanged;

                            overlayTracksPanel.Controls.Add(noneRadioButton);

                            offRadioButtonAdded = true;
                        }

                        string trackFile = MapReader.GetValue(selectedSong, trackName);
                        trackList.Items.Add(new ListViewItem(new[] { trackName, trackFile }));

                        string numbering = "";

                        if (int.TryParse(trackName.Split('_').Last(), out int number))
                        {
                            numbering = " " + number.ToString();
                        }

                        RadioButton overlayRadioButton = new RadioButton()
                        {
                            Text = "Vocals" + numbering,
                            Name = trackName
                        };
                        overlayRadioButton.CheckedChanged += OverlayRadioButton_CheckedChanged;

                        overlayTracksPanel.Controls.Add(overlayRadioButton);
                    }
                }

                ((RadioButton)mainTracksPanel.Controls[0]).Checked = true;

                if (overlayTracksPanel.Controls.Count > 0)
                    ((RadioButton)overlayTracksPanel.Controls[0]).Checked = true;

                //InitPlayer(selectedSong);
                InitVolumes();

                trackList.Sorting = SortOrder.Ascending;
                trackList.Sort();
            }

            MapReader.Dispose();
        }

        private void OverlayRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            string selectedTrack = radioButton.Name;

            float speed = fadeSpeed;

            Timer timer = new Timer()
            {
                Interval = fadeInterval
            };

            VolumeSampleProvider fade = null;

            foreach (var item in VolumeSampleProviders)
            {
                var vsp = item.Value;

                if (item.Key == selectedTrack)
                    fade = vsp;
            }

            if (!radioButton.Checked)
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

        private void NoneRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;

            float speed = fadeSpeed;

            Timer timer = new Timer()
            {
                Interval = fadeInterval
            };

            VolumeSampleProvider fade = null;

            foreach (var vsp in VolumeSampleProviders)
            {
                if (vsp.Key.StartsWith("voc"))
                {
                    if (vsp.Value.Volume > 0.0f)
                        fade = vsp.Value;
                }
            }

            if (fade != null)
            {
                if (radioButton.Checked)
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

                timer.Enabled = true;
            }
        }

        private void TrackRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            string selectedSong = songList.SelectedItem?.ToString();

            if (OutputDevice.PlaybackState == PlaybackState.Playing || OutputDevice.PlaybackState == PlaybackState.Paused)
            {
                RadioButton radioButton = (RadioButton)sender;
                string selectedTrack = radioButton.Name;

                float speed = fadeSpeed;

                Timer timer = new Timer()
                {
                    Interval = fadeInterval
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

                if (fadeIn != null && fadeOut != null)
                {
                    timer.Enabled = true;
                }
            }
            else
            {
                InitPlayer(selectedSong);
                InitVolumes();
            }
        }
    }
}