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
        SfxPlayer SfxPlayer = new SfxPlayer();

        FileMapReader MapReader;

        float sfxVolume = 2.0f;

        string mapLocation = @"mapping.txt";

        MusicPlayer Player;

        public MainWindow()
        {
            InitializeComponent();

            Player = MusicPlayer.PlayerInstance;
            Player.SongDirectory = musicPath.Text;
            Player.SetLoop(loopCheckBox.Checked);

            SfxPlayer.Volume = sfxVolume;

            playButton.MouseEnter += MouseEnterSound;

            loopCheckBox.MouseEnter += MouseEnterSound;
            loopCheckBox.MouseDown += MouseDownSound2;

            pauseButton.MouseEnter += MouseEnterSound;
            pauseButton.MouseDown += MouseDownSound;
        }

        private void MouseEnterSound(object sender, EventArgs e)
        {
            SfxPlayer.PlaySfx(new MemoryStream(Properties.Resources.select));
        }

        private void MouseDownSound(object sender, EventArgs e)
        {
            SfxPlayer.PlaySfx(new MemoryStream(Properties.Resources.chosen));
        }

        private void MouseDownSound2(object sender, EventArgs e)
        {
            SfxPlayer.PlaySfx(new MemoryStream(Properties.Resources.chosen2));
        }

        private void MouseDownSoundInvalid(object sender, EventArgs e)
        {
            SfxPlayer.PlaySfx(new MemoryStream(Properties.Resources.invalid));
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
            long position = Player.PlaybackPosition;
            long length = Player.PlaybackLength;

            if (length > 0)
                playPosition.Value = (int)((double)playPosition.Maximum * ((double)position / (double)length));

            mainFadeProgress.Value = (int)(Player.MainTrackFadeVolume * 100.0f);
            overlayFadeProgress.Value = (int)(Player.OverlayTrackFadeVolume * 100.0f);

            //if (Player.CurrentMainTrackFadeVolume == 0.0f || Player.CurrentMainTrackFadeVolume == 1.0f)
            //    mainTracksPanel.Enabled = true;
            //else
            //    mainTracksPanel.Enabled = false;

            //if (Player.CurrentOverlayTrackFadeVolume == 0.0f || Player.CurrentOverlayTrackFadeVolume == 1.0f)
            //    overlayTracksPanel.Enabled = true;
            //else
            //    overlayTracksPanel.Enabled = false;

            if (Player.MainTrackFadeInProgress)
                mainTracksPanel.Enabled = false;
            else mainTracksPanel.Enabled = true;

            if (Player.OverlayTrackFadeInProgress)
                overlayTracksPanel.Enabled = false;
            else overlayTracksPanel.Enabled = true;

            //Console.WriteLine(Player.MainTrackFadeInProgress);
            //Console.WriteLine(Player.OverlayTrackFadeInProgress);
        }

        private void PlayPosition_Scroll(object sender, EventArgs e)
        {
            long length = Player.PlaybackLength;
            int newPos = (int)(length * ((double)playPosition.Value / (double)playPosition.Maximum));

            if (length > 0)
                Player.SetPlaybackPosition(newPos);
        }

        private void LoopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Player.SetLoop(loopCheckBox.Checked);
        }

        private void PlayButton_Click(object sender, EventArgs e)
        {
            RadioButton mainTrack = null;
            RadioButton overlayTrack = null;

            foreach (RadioButton item in mainTracksPanel.Controls)
            {
                if (item.Checked) mainTrack = item;
            }

            foreach (RadioButton item in overlayTracksPanel.Controls)
            {
                if (item.Checked) overlayTrack = item;
            }

            if (mainTrack != null)
            {
                MouseDownSound(sender, e);
                Player.Play(mainTrack.Name, overlayTrack?.Name);
            }
            else
            {
                MouseDownSoundInvalid(sender, e);
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            WaveOut tempPlayer = new WaveOut() { DesiredLatency = 100 };

            if (Player.PlaybackState == PlaybackState.Paused)
            {
                tempPlayer.Init(new VolumeSampleProvider(new VorbisWaveReader(new MemoryStream(Properties.Resources.resume))) { Volume = sfxVolume });
                tempPlayer.Play();

                pauseButton.Enabled = false;

                tempPlayer.PlaybackStopped += (snd, evt) =>
                {
                    Player.Pause();
                    Console.WriteLine(Player.PlaybackState);

                    pauseButton.Enabled = true;
                };
            }
            else
            {
                tempPlayer.Init(new VolumeSampleProvider(new VorbisWaveReader(new MemoryStream(Properties.Resources.pause))) { Volume = sfxVolume });
                tempPlayer.Play();

                pauseButton.Enabled = false;

                tempPlayer.PlaybackStopped += (snd, evt) =>
                {
                    pauseButton.Enabled = true;
                };

                Player.Pause();
                Console.WriteLine(Player.PlaybackState);
            }
        }

        private void SongList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapReader = new FileMapReader(mapLocation);

            Player.Stop();

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

                Player.InitTracks(selectedSong);

                //((RadioButton)mainTracksPanel.Controls[0]).Checked = true;

                //if (overlayTracksPanel.Controls.Count > 0)
                //    ((RadioButton)overlayTracksPanel.Controls[0]).Checked = true;

                trackList.Sorting = SortOrder.Ascending;
                trackList.Sort();
            }

            MapReader.Dispose();
        }

        private void OverlayRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            string selectedTrack = radioButton.Name;

            if (radioButton.Checked) Player.FadeTrackTo(MusicPlayer.TrackType.Overlay, selectedTrack);
        }

        private void NoneRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Player.FadeTrackTo(MusicPlayer.TrackType.Overlay, "");
        }

        private void TrackRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = (RadioButton)sender;
            string selectedTrack = radioButton.Name;

            if (radioButton.Checked) Player.FadeTrackTo(MusicPlayer.TrackType.Main, selectedTrack);
        }
    }
}