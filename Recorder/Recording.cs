using System;

using NAudio.Wave;

namespace Recorder
{
    public class Recording
    {
        public string FileName
        {
            get;
            private set;
        }

        public WaveFormat Format
        {
            get;
            private set;
        }

        public DateTime DateCreated
        {
            get;
            private set;
        }

        public Recording(string fileName, WaveFormat format, DateTime dateCreated)
        {
            FileName = fileName;
            Format = format;
            DateCreated = dateCreated;
        }
    }
}