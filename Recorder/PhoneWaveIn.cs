using System;
using Microsoft.Xna.Framework.Audio;

using NAudio.Wave;

namespace Recorder
{
    public class PhoneWaveIn : IWaveIn
    {
        private readonly Microphone _mic;
        private byte[] _buffer;

        public event EventHandler<WaveInEventArgs> DataAvailable;

        public PhoneWaveIn()
        {
            _mic = Microphone.Default;
            _mic.BufferDuration = TimeSpan.FromMilliseconds(100);
        }

        public void StartRecording()
        {
            _buffer = new byte[_mic.GetSampleSizeInBytes(_mic.BufferDuration)];
            _mic.BufferReady += _mic_BufferReady;
            _mic.Start();
        }

        void _mic_BufferReady(object sender, EventArgs e)
        {
            _mic.GetData(_buffer);

            if (DataAvailable != null)
            {
                DataAvailable(this, new WaveInEventArgs(_buffer, _buffer.Length));
            }
        }

        public void StopRecording()
        {
            _mic.Stop();
            _mic.BufferReady -= _mic_BufferReady;
        }

        public void Dispose()
        {
            if (_mic.State == MicrophoneState.Started)
            {
                StopRecording();
            }
        }
    }
}
