using System;
using System.IO;
using System.IO.IsolatedStorage;

using NAudio.Wave;

namespace VoiceRecorder
{
    class Recorder : IDisposable
    {
        private IWaveIn _waveIn;
        private MemoryStream _recordedStream;
        private WaveFileWriter _writer;

        public bool IsRecording
        {
            get;
            private set;
        }

        public WaveFormat Format
        {
            get;
            private set;
        }

        public event EventHandler<SampleEventArgs> OnSample;

        public Recorder(WaveFormat format)
        {
            Format = format;
            IsRecording = false;
        }

        public void Start()
        {
            if (IsRecording)
            {
                return;
            }
            IsRecording = true;

            _recordedStream = new MemoryStream();
            _writer = new WaveFileWriter(new IgnoreDisposeStream(_recordedStream), Format);

            _waveIn = new PhoneWaveIn();
            _waveIn.DataAvailable += captureDevice_DataAvailable;
            _waveIn.StartRecording();
        }

        void captureDevice_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (!IsRecording)
            {
                return;
            }

            byte[] buffer = e.Buffer;
            _writer.WriteData(buffer, 0, e.BytesRecorded);

            if (OnSample != null)
            {
                for (int index = 0; index < e.BytesRecorded; index += 2)
                {
                    short sample = (short)((buffer[index + 1] << 8) | buffer[index + 0]);
                    float sample32 = sample / 32768f;
                    OnSample(this, new SampleEventArgs(sample32, 0));
                }
            }
        }

        public MemoryStream Stop()
        {
            if (!IsRecording)
            {
                return null;
            }
            IsRecording = false;

            _waveIn.StopRecording();
            _waveIn.DataAvailable -= captureDevice_DataAvailable;
            _waveIn = null;

            _writer.Close();
            _writer = null;
            
            var bytes = _recordedStream;
            _recordedStream = null;

            return bytes;
        }

        public void Dispose()
        {
            if (IsRecording)
            {
                Stop();
            }
        }
    }
}
