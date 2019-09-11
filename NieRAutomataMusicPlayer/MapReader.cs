using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NieRAutomataMusicTest
{
    class FileMapReader
    {
        private readonly string filename;
        private StreamReader reader;

        public FileMapReader(string filename)
        {
            this.filename = filename;
            reader = new StreamReader(filename);
        }

        private void Reinitialize()
        {
            reader = new StreamReader(filename);
        }

        public void GetMapping(string songName)
        {
            Reinitialize();

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
            Reinitialize();

            while (!reader.EndOfStream)
            {
                if (reader.ReadLine() == $"[{songName}]")
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!char.IsLetterOrDigit(line[0])) break;

                        if (key == line.Substring(0, line.IndexOf('=')).Trim())
                        {
                            return line.Substring(line.IndexOf('=') + 1, line.Length - (line.IndexOf('=') + 1)).Trim();
                        }
                    }
                }
            }

            return "";
        }

        public string[] GetAvailableTracks(string songName)
        {
            List<string> trackList = new List<string>();

            Reinitialize();

            while (!reader.EndOfStream)
            {
                if (reader.ReadLine() == $"[{songName}]")
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (!char.IsLetterOrDigit(line[0])) break;
                        if (!line.StartsWith("loop"))
                        {
                            string key = line.Substring(0, line.IndexOf('=')).Trim();
                            trackList.Add(key);
                        }
                    }
                }
            }

            return trackList.ToArray();
        }

        public string[] GetAvailableSongs()
        {
            List<string> songList = new List<string>();

            Reinitialize();

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
