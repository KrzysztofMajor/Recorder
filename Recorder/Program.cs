using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Lame;
using NAudio.Wave;

namespace Recorder
{
    class Program
    {
        static void Main(string[] args)
        {
            var now = DateTime.Now;
            var machineName = Environment.MachineName.ToLower(CultureInfo.CurrentCulture);

            var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
            Directory.CreateDirectory(outputFolder);
            var outputFilepath = Path.Combine(outputFolder, $"output.wav");
            var mp3Filepath = Path.Combine(outputFolder, $"{machineName}{now:ddMMdyyyyHHmmss}.mp3");

            var waveIn = new WasapiCapture { };
            var writer = new WaveFileWriter(outputFilepath, waveIn.WaveFormat);

            waveIn.StartRecording();

            var tm = new System.Timers.Timer(10*1000);
            tm.Elapsed += (sender, eventArgs) => waveIn.StopRecording();
            tm.Start();

            waveIn.DataAvailable += (sender, eventArgs) =>
            {
                Console.Write(".");
                writer.Write(eventArgs.Buffer, 0, eventArgs.BytesRecorded);
            };

            var e = new ManualResetEvent(false);
            waveIn.RecordingStopped += (sender, eventArgs) =>
            {
                writer.Dispose();
                waveIn.Dispose();

                Console.WriteLine("writing mp3");
                using (var reader = new AudioFileReader(outputFilepath))
                using (var mp3Writer = new LameMP3FileWriter(mp3Filepath, reader.WaveFormat, 128))
                    reader.CopyTo(mp3Writer);

                Console.WriteLine("writing done");
                e.Set();
            };

            e.WaitOne();
        }
    }
}