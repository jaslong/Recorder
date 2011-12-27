using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Audio;

using NAudio.Wave;

namespace Recorder
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const int NUM_SAMPLES_PER_UPDATE = 20;
        private const string STORAGE_DIRECTORY = "recorder";

        private Recorder _recorder;
        private IsolatedStorageFile _storage;
        private MemoryStream _pendingSave;

        public MainPage()
        {
            InitializeComponent();

            _recorder = new Recorder(new WaveFormat());
            _storage = IsolatedStorageFile.GetUserStoreForApplication();

            if (!_storage.DirectoryExists(STORAGE_DIRECTORY))
            {
                _storage.CreateDirectory(STORAGE_DIRECTORY);
            }

            UpdateRecordings();
        }

        private void StartRecording()
        {
            _recorder.Start();
            RecordButton.Content = "Stop Recording";
        }

        private void StopRecording(object obj)
        {
            _pendingSave = _recorder.Stop();

            var defaultFileName = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            Dispatcher.BeginInvoke(delegate()
            {
                FileNameTextBox.Text = defaultFileName;
                FileNameTextBox.Focus();
                FileNameTextBox.SelectAll();

                ToggleVisibility();
                RecordButton.Content = "Start Recording";
            });
        }

        private void SaveRecording(object sender, RoutedEventArgs e)
        {
            var fileName = AddDirectory(FileNameTextBox.Text);
            if (_storage.FileExists(fileName))
            {
                MessageBox.Show(String.Format("A recording with the name \"{0}\" already exists.", fileName));
                return;
            }

            Stream fileStream;
            try
            {
                fileStream = _storage.CreateFile(fileName);
            }
            catch
            {
                MessageBox.Show(String.Format("The name \"{0}\" contains illegal characters.", fileName));
                return;
            }
            using (fileStream) {
                _pendingSave.WriteTo(fileStream);
            }
            _pendingSave.Close();
            _pendingSave = null;

            UpdateRecordings();
            ToggleVisibility();
        }

        private void DiscardRecording(object sender, RoutedEventArgs e)
        {
            ToggleVisibility();
        }

        private void UpdateRecordings()
        {
            RecordingsListBox.DataContext = _storage.GetFileNames(STORAGE_DIRECTORY + "/*");
        }

        private void ToggleVisibility()
        {
            RecordButton.Visibility = RecordButton.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            SavePanel.Visibility = SavePanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            if (!_recorder.IsRecording)
            {
                StartRecording();
            }
            else
            {
                // Microphone returns samples every 100ms, so delay this call 100ms to make sure no audio is cut off.
                new Timer(StopRecording, this, 100, System.Threading.Timeout.Infinite);
            }
        }

        private void FileNameTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SaveRecording(sender, e);
            }
        }

        private void StackPanel_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var fileName = (string)((StackPanel)sender).DataContext;
            var recording = AddDirectory(fileName);

            var fileStream = _storage.OpenFile(recording, FileMode.Open);
            if (fileStream.CanRead)
            {
                MemoryStream memoryStream;
                using (fileStream)
                {
                    memoryStream = new MemoryStream();
                    fileStream.CopyTo(memoryStream);
                }
                new SoundEffect(memoryStream.ToArray(), Microphone.Default.SampleRate, AudioChannels.Mono).Play();
            }
        }

        private void StackPanel_Hold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var fileName = (string)((StackPanel)sender).DataContext;
            var recording = AddDirectory(fileName);
            var result = MessageBox.Show(String.Format("Delete recording \"{0}\"?", fileName), "Delete", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                _storage.DeleteFile(recording);
                UpdateRecordings();
            }
        }

        private string AddDirectory(string fileName)
        {
            return STORAGE_DIRECTORY + "/" + fileName;
        }
    }
}