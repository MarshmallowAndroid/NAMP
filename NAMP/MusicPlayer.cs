using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace NAMP
{
    class MusicPlayer : IDisposable
    {
        private WaveOut outputDevice;

        private List<LoopSampleProvider> loopSampleProviders;
        private Dictionary<string, VolumeSampleProvider> volumeSampleProviders;
        private MixingSampleProvider mixingSampleProvider;

        private FileMapReader MapReader;

        readonly float fadeSpeed = 0.0075f;
        readonly int fadeInterval = 16;

        string currentSong = "";

        public MusicPlayer(string songDirectory = "")
        {
            outputDevice = new WaveOut() { DesiredLatency = 100 };

            outputDevice.PlaybackStopped += (s, e) =>
            {
                Flush();
            };

            SongDirectory = songDirectory;
        }

        public bool MainTrackFadeInProgress { get; set; }

        public bool OverlayTrackFadeInProgress { get; set; }

        public bool Loop { get; set; }

        public enum TrackType
        {
            Main,
            Overlay
        }

        public float MainTrackFadeVolume { get; private set; }

        public float OverlayTrackFadeVolume { get; private set; }

        public long PlaybackLength
        {
            get
            {
                if (loopSampleProviders?.Count > 0) return loopSampleProviders.First().Length;
                else return 0;
            }
        }

        public long PlaybackPosition
        {
            get
            {
                if (loopSampleProviders?.Count > 0) return loopSampleProviders.First().Position;
                else return 0;
            }
        }

        public PlaybackState PlaybackState => outputDevice.PlaybackState;

        public string SongDirectory { get; set; }

        public void InitTracks(string songName)
        {
            if (!string.IsNullOrWhiteSpace(SongDirectory))
            {
                MapReader = new FileMapReader(SongDirectory + "\\mapping.txt");

                bool isMSPInit = false;
                string[] tracks = MapReader.GetAvailableTracks(songName);

                string musicDirectory = SongDirectory + "\\";

                loopSampleProviders = new List<LoopSampleProvider>();
                volumeSampleProviders = new Dictionary<string, VolumeSampleProvider>();

                foreach (var track in tracks)
                {
                    string trackFile = MapReader.GetValue(songName, track);
                    int loopStart = int.Parse(MapReader.GetValue(songName, "loop_start"));
                    int loopEnd = int.Parse(MapReader.GetValue(songName, "loop_end"));

                    int.TryParse(MapReader.GetValue(songName, "dly_" + track), out int startSample);

                    var reader = new CustomVorbisWaveReader(musicDirectory + trackFile);
                    loopSampleProviders.Add(new LoopSampleProvider(reader, loopStart, loopEnd, startSample) { Loop = Loop });
                    volumeSampleProviders.Add(track, new VolumeSampleProvider(loopSampleProviders.Last()) { Volume = 0.0f });

                    Console.WriteLine($"Added track \"{track}\" for song \"{songName}\".");
                    Console.WriteLine();
                    Console.WriteLine($"Start offset is {startSample}.");
                    Console.WriteLine();

                    if (!isMSPInit)
                    {
                        mixingSampleProvider = new MixingSampleProvider(loopSampleProviders.First().WaveFormat);
                        isMSPInit = true;
                    }

                    mixingSampleProvider.AddMixerInput(volumeSampleProviders.Last().Value);
                }

                currentSong = songName;

                MapReader.Dispose();
            }
        }

        public void InitVolumes(string mainTrackName, string overlayTrackName = "")
        {
            if (volumeSampleProviders?.Count > 0)
            {
                foreach (var item in volumeSampleProviders)
                {
                    if (item.Key.StartsWith("ins"))
                    {
                        if (!string.IsNullOrWhiteSpace(mainTrackName))
                        {
                            if (item.Key == mainTrackName)
                            {
                                if (item.Value.Volume < 1.0f)
                                    item.Value.Volume = 1.0f;
                            }
                            else item.Value.Volume = 0.0f;
                        }
                    }

                    if (item.Key.StartsWith("voc"))
                    {
                        if (!string.IsNullOrWhiteSpace(overlayTrackName))
                        {
                            if (item.Key == overlayTrackName)
                            {
                                if (item.Value.Volume < 1.0f)
                                    item.Value.Volume = 1.0f;
                            }
                            else item.Value.Volume = 0.0f;
                        }
                    }
                }
            }
        }

        public void Play(string mainTrackName, string overlayTrackName = "")
        {
            if (outputDevice.PlaybackState != PlaybackState.Stopped)
            {
                outputDevice.Stop();

                InitTracks(currentSong);
            }

            InitVolumes(mainTrackName, overlayTrackName);

            if (mixingSampleProvider != null)
            {
                outputDevice.Init(mixingSampleProvider);
                outputDevice.Play();
            }
        }

        public void Pause()
        {
            if (outputDevice?.PlaybackState == PlaybackState.Paused)
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

        public void FadeTrackTo(TrackType trackType, string trackName)
        {
            if ((!MainTrackFadeInProgress && trackType == TrackType.Main)
                || (!OverlayTrackFadeInProgress && trackType == TrackType.Overlay))
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

                    string[] trackTypes =
                    {
                        "ins",
                        "voc"
                    };

                    foreach (var item in volumeSampleProviders)
                    {
                        var vsp = item.Value;

                        if (item.Key.StartsWith(trackTypes[(int)trackType]))
                        {
                            if (item.Key == trackName)
                            {
                                if (vsp.Volume < 1.0f)
                                    fadeIn = vsp;
                                else break;
                            }

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
                        {
                            fadeIn.Volume += speed;

                            if (fadeIn.Volume >= 1.0f) fadeIn.Volume = 1.0f;

                            if (trackType == TrackType.Main)
                            {
                                MainTrackFadeVolume = fadeIn.Volume;
                                MainTrackFadeInProgress = true;
                            }
                            else if (trackType == TrackType.Overlay)
                            {
                                OverlayTrackFadeVolume = fadeIn.Volume;
                                OverlayTrackFadeInProgress = true;
                            }
                        }

                        if (fadeOut != null)
                        {
                            fadeOut.Volume -= speed;

                            if (fadeOut.Volume <= 0.0f) fadeOut.Volume = 0.0f;

                            if (trackType == TrackType.Main)
                            {
                                MainTrackFadeVolume = fadeOut.Volume;
                                MainTrackFadeInProgress = true;
                            }
                            else if (trackType == TrackType.Overlay)
                            {
                                OverlayTrackFadeVolume = fadeOut.Volume;
                                OverlayTrackFadeInProgress = true;
                            }
                        }

                        if (fadeIn?.Volume == 1.0f || fadeOut?.Volume == 0.0f)
                        {
                            if (trackType == TrackType.Main)
                                MainTrackFadeInProgress = false;
                            else if (trackType == TrackType.Overlay)
                                OverlayTrackFadeInProgress = false;

                            timer.Enabled = false;
                            timer.Dispose();
                        }
                    };

                    if (fadeIn != null || fadeOut != null)
                    {
                        timer.Enabled = true;
                    }
                }
                else InitVolumes(trackName, trackName);
            }
        }

        public void Flush()
        {
            Console.WriteLine("Flushing resources...");

            loopSampleProviders?.ForEach(x =>
            {
                x.SourceStream.Dispose();
            });

            loopSampleProviders?.Clear();
            volumeSampleProviders?.Clear();
            mixingSampleProvider?.RemoveAllMixerInputs();
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

        public void SetPlaybackPosition(long position)
        {
            if (loopSampleProviders.Count > 0)
            {
                try
                {
                    long currentLength = loopSampleProviders[0].SourceStream.Length;

                    foreach (var lsp in loopSampleProviders)
                    {
                        lsp.Seek(position);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static MusicPlayer PlayerInstance = new MusicPlayer();
    }
}
