﻿using System;
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
        }

        private void Initialize()
        {
            reader = new StreamReader(filename);
        }

        private void Dispose()
        {
            reader.Dispose();
        }

        public void GetMapping(string songName)
        {
            Initialize();

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

            reader.Dispose();
        }

        public string GetValue(string songName, string key)
        {
            Initialize();

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

            reader.Dispose();

            return "";
        }

        public string[] GetAvailableTracks(string songName)
        {
            List<string> trackList = new List<string>();

            Initialize();

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

            reader.Dispose();

            return trackList.ToArray();
        }

        public string[] GetAvailableSongs()
        {
            List<string> songList = new List<string>();

            Initialize();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    string songName = line.Substring(line.IndexOf("[") + 1, line.LastIndexOf("]") - 1);

                    songList.Add(songName);
                }
            }

            reader.Dispose();

            return songList.ToArray();
        }
    }
}
