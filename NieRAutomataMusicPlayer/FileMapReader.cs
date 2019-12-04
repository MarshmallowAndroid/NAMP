using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NieRAutomataMusicTest
{
    class FileMapReader : IDisposable
    {
        private readonly StreamReader reader;

        public FileMapReader(string filename)
        {
            reader = new StreamReader(filename);
        }

        private void Reset()
        {
            reader.BaseStream.Position = 0;
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public void GetMapping(string songName)
        {
            Reset();

            while (!reader.EndOfStream)
            {
                if (reader.ReadLine() == $"[{songName}]")
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!char.IsLetterOrDigit(line[0])) break;

                        Console.WriteLine(line);
                    }
                }
            }
        }

        public string GetValue(string songName, string key)
        {
            Reset();

            while (!reader.EndOfStream)
            {
                if (reader.ReadLine() == $"[{songName}]")
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.Length > 0)
                        {
                            if (line.StartsWith("[") || !char.IsLetterOrDigit(line[0])) break;
                            if (key == line.Substring(0, line.IndexOf('=')).Trim())
                            {
                                return line.Substring(line.IndexOf('=') + 1, line.Length - (line.IndexOf('=') + 1)).Trim();
                            }
                        }

                    }
                }
            }

            return "";
        }

        public string[] GetAvailableTracks(string songName)
        {
            List<string> trackList = new List<string>();

            Reset();

            while (!reader.EndOfStream)
            {
                if (reader.ReadLine() == $"[{songName}]")
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.Length > 0)
                        {
                            if (line.StartsWith("[") || !char.IsLetterOrDigit(line[0])) break;
                            if (!line.StartsWith("loop"))
                            {
                                string key = line.Substring(0, line.IndexOf('=')).Trim();
                                trackList.Add(key);
                            }
                        }
                    }
                }
            }

            return trackList.ToArray();
        }

        public string[] GetAvailableSongs()
        {
            List<string> songList = new List<string>();

            Reset();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string songName = line.Substring(line.IndexOf("[") + 1, line.LastIndexOf("]") - 1);

                    songList.Add(songName);
                }
            }

            return songList.ToArray();
        }
    }
}
