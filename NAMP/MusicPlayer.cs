using NAudio.Vorbis;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NAMP
{
    class MusicPlayer : IDisposable
    {
        private WaveOut outputDevice;
        private List<CustomVorbisWaveReader> readers;
        private List<LoopSampleProvider> loopSampleProviders;
        private List<KeyValuePair<string, VolumeSampleProvider>> volumeSampleProviders;

        private MixingSampleProvider MixingSampleProvider;

        private FileMapReader MapReader;

        float fadeSpeed = 0.0075f;
        int fadeInterval = 16;

        string mapLocation = @"mapping.txt";

        string currentSong = "";

        public MusicPlayer(string songDirectory = "")
        {
            SongDirectory = songDirectory;
        }

        public PlaybackState PlaybackState => outputDevice.PlaybackState;

        public bool Loop { get; set; }

        public float CurrentFadeVolume { get; private set; }

        public long PlaybackPosition
        {
            get
            {
                if (readers?.Count > 0) return readers[0].Position;
                else return 0;
            }
        }

        public long PlaybackLength
        {
            get
            {
                if (readers?.Count > 0) return readers[0].Length;
                else return 0;
            }
        }

        public string SongDirectory { get; set; }

        public enum FadeTrack
        {
            Main,
            Overlay
        }

        public void Play(string trackName, string overlayName = "")
        {
            if (outputDevice.PlaybackState != PlaybackState.Stopped)
            {
                outputDevice.Stop();
                outputDevice.Dispose();

                InitPlayer(currentSong);
            }

            InitVolumes(trackName, overlayName);

            if (MixingSampleProvider != null)
            {
                outputDevice.Init(MixingSampleProvider);
                outputDevice.Play();
            }
        }

        public void Pause()
        {
            if (outputDevice.PlaybackState == PlaybackState.Paused || outputDevice.PlaybackState == PlaybackState.Stopped)
            {
                outputDevice.Play();
            }
            else
            {
                outputDevice.Pause();
            }
        }

        public void Stop()
        {
            outputDevice.Stop();
        }

        public void InitPlayer(string songName)
        {
            if (SongDirectory != string.Empty)
            {
                MapReader = new FileMapReader(mapLocation);

                bool isMSPInit = false;
                string[] tracks = MapReader.GetAvailableTracks(songName);

                string musicDirectory = SongDirectory + "\\";

                readers = new List<CustomVorbisWaveReader>();
                loopSampleProviders = new List<LoopSampleProvider>();
                volumeSampleProviders = new List<KeyValuePair<string, VolumeSampleProvider>>();

                foreach (var track in tracks)
                {
                    string trackFile = MapReader.GetValue(songName, track);
                    int loopStart = int.Parse(MapReader.GetValue(songName, "loop_start"));
                    int loopEnd = int.Parse(MapReader.GetValue(songName, "loop_end"));

                    readers.Add(new CustomVorbisWaveReader(musicDirectory + trackFile));

                    loopSampleProviders.Add(new LoopSampleProvider(readers.Last(), loopStart, loopEnd) { Loop = Loop });
                    volumeSampleProviders.Add(new KeyValuePair<string, VolumeSampleProvider>(track, new VolumeSampleProvider(loopSampleProviders.Last())));

                    Console.WriteLine("Added LoopSampleProvider for song \"" + songName + "\", track " + track + ".");

                    if (!isMSPInit)
                    {
                        MixingSampleProvider = new MixingSampleProvider(readers.First().WaveFormat);
                        isMSPInit = true;
                    }

                    MixingSampleProvider.AddMixerInput(volumeSampleProviders.Last().Value);
                }

                currentSong = songName;

                MapReader.Dispose();
            }
        }

        public void InitVolumes(string trackName, string overlayName = "")
        {
            foreach (var vsp in volumeSampleProviders)
            {
                if (vsp.Key.StartsWith("ins"))
                {
                    if (vsp.Key == trackName)
                        vsp.Value.Volume = 1.0f;
                    else
                        vsp.Value.Volume = 0.0f;
                }

                if (overlayName != string.Empty)
                {
                    if (vsp.Key.StartsWith("voc"))
                    {
                        if (vsp.Key == overlayName)
                            vsp.Value.Volume = 1.0f;
                        else
                            vsp.Value.Volume = 0.0f;
                    }
                }
            }
        }

        public void SetLoop(bool value)
        {
            Loop = value;

            if (outputDevice.PlaybackState != PlaybackState.Stopped && loopSampleProviders.Count > 0)
            {
                foreach (var lsp in loopSampleProviders)
                {
                    lsp.Loop = value;
                }
            }
        }

        public long GetPlaybackPosition()
        {
            if (outputDevice.PlaybackState != PlaybackState.Stopped && readers?.Count > 0)
            {
                long currentPos = readers[0].Position;

                return currentPos;
            }
            else
                return 0;
        }

        public void SetPlaybackPosition(long position)
        {
            if (loopSampleProviders.Count > 0)
            {
                try
                {
                    long currentLength = loopSampleProviders[0].SourceStream.Length;

                    foreach (var lsp in loopSampleProviders)
                    {
                        lsp.Seek(position - (position % lsp.WaveFormat.Channels));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Position exceeded limits of LoopSampleProvider.");
                }
            }
        }

        public long GetPlaybackLength()
        {
            if (outputDevice.PlaybackState != PlaybackState.Stopped && readers?.Count > 0)
            {
                long length = readers[0].Length;

                return length;
            }
            else
                return 0;
        }

        public void FadeTrackTo(FadeTrack fadeTrack, string trackName)
        {
            if (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                float speed = fadeSpeed;

                Timer timer = new Timer()
                {
                    Interval = fadeInterval
                };

                VolumeSampleProvider fadeIn = null;
                VolumeSampleProvider fadeOut = null;

                foreach (var item in volumeSampleProviders)
                {
                    var vsp = item.Value;

                    if (item.Key.StartsWith(fadeTrack == FadeTrack.Main ? "ins" : "voc"))
                    {
                        if (item.Key == trackName)
                            fadeIn = vsp;

                        if (vsp.Volume > 0.0f)
                            fadeOut = vsp;
                    }
                }

                if (fadeIn != null)
                    fadeIn.Volume = 0.0f;

                if (fadeOut != null)
                    fadeOut.Volume = 1.0f;

                timer.Elapsed += (sndr, evt) =>
                {
                    if (fadeIn != null)
                        fadeIn.Volume += speed;

                    if (fadeOut != null)
                        fadeOut.Volume -= speed;

                    //CurrentFadeVolume = fadeIn.Volume;

                    if (fadeIn != null)
                    {
                        if (fadeIn.Volume >= 1.0f)
                        {
                            if (fadeIn != null)
                                fadeIn.Volume = 1.0f;

                            if (fadeOut != null)
                                fadeOut.Volume = 0.0f;

                            timer.Enabled = false;
                        }
                    }

                    if (fadeOut != null)
                    {
                        if (fadeOut.Volume <= 0.0f)
                        {
                            if (fadeIn != null)
                                fadeIn.Volume = 1.0f;

                            if (fadeOut != null)
                                fadeOut.Volume = 0.0f;

                            timer.Enabled = false;
                        }
                    }
                };

                if (fadeIn != null || fadeOut != null)
                {
                    timer.Enabled = true;
                }
            }
            else
            {
                InitVolumes(trackName);
            }
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static MusicPlayer PlayerInstance = new MusicPlayer();
    }
}
