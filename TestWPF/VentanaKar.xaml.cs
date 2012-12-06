using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KaraokePlayer;
using CDGNet;
using NAudio;
using NAudio.Wave;
using System.IO;
using System.Runtime.InteropServices;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for VentanaKar.xaml
    /// </summary>
    public partial class VentanaKar : Window
    {
        KarPlayer player;
        IWavePlayer waveOutDevice;
        WaveStream mainOutputStream;
        WaveChannel32 volumeStream;
        CDGFile cdgFile;
        long msecsRemaining, cdgLength;
        bool stopPlay = false;
        bool pausePlay = false;
        System.Windows.Threading.DispatcherTimer timer;
        DateTime startTime, endTime;

        public VentanaKar()
        {
            InitializeComponent();
            waveOutDevice = new WaveOutEvent();
            player = new KarPlayer();

        }
        public void Start(string fileName)
        {

            int frameCount = 0;
            stopPlay = false;
            cdgFile = new CDGFile(fileName);
            
            var mp3FileName = fileName.Replace(System.IO.Path.GetExtension(fileName), ".mp3");
            cdgLength = cdgFile.getTotalDuration();
            PlayMp3File(mp3FileName);
            startTime = DateTime.Now;
            endTime = startTime.AddMilliseconds(cdgLength);
            msecsRemaining = cdgLength;
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(TickTimer);

            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();
            //while (msecsRemaining > 0)
            //{
            //    if (stopPlay)
            //    {
            //        break;
            //    }
            //    msecsRemaining = (long)endTime.Subtract(DateTime.Now).TotalMilliseconds;
            //    var pos = cdgLength - msecsRemaining;
            //    while (pausePlay)
            //    {
            //        endTime = DateTime.Now.AddMilliseconds(msecsRemaining);
                    
            //        System.Windows.Forms.Application.DoEvents();
            //    }
            //    cdgFile.renderAtPosition(pos);
            //    ++frameCount;
            //    canvas.Image = cdgFile.get_RGBImage(false);
            //    canvas.BackColor = ((System.Drawing.Bitmap)canvas.Image).GetPixel(1, 1);
            //    canvas.Refresh();
            //    System.Windows.Forms.Application.DoEvents();
            //}
            //StopCdgFile();
        }
        private void TickTimer(object sender, EventArgs e)
        {
            if (msecsRemaining > 0)
            {
                msecsRemaining = (long)endTime.Subtract(DateTime.Now).TotalMilliseconds;
                var pos = cdgLength - msecsRemaining;
                cdgFile.renderAtPosition(pos);
                var image=cdgFile.get_RGBImage();
                imagen.Source = loadBitmap((System.Drawing.Bitmap)image);
            }
        }

        public void PlayMp3File(string fileName)
        {
            mainOutputStream = CreateMp3OutPut(fileName);
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();
        }

        public void Stop()
        {
            timer.Stop();
        }

        public WaveStream CreateMp3OutPut(string fileName)
        {
            WaveChannel32 inputStream = null;
            if (fileName.EndsWith(".mp3"))
            {
                WaveStream reader = new Mp3FileReader(fileName);
                inputStream = new WaveChannel32(reader);
            }
            return inputStream;
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }
    }


}
