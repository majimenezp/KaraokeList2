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
using System.Windows.Media.Effects;

namespace KaraokeList2
{
    /// <summary>
    /// Interaction logic for KaraokeWindow.xaml
    /// </summary>
    public partial class KaraokeWindow : Window
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
        bool firstFrame = false;
        bool step1,step2;
        string messageEnd;
        int currentId;
        public delegate void KaraokeEndHandler(int KaraokeId);
        public event KaraokeEndHandler KaraokeEnded;

        public KaraokeWindow()
        {
            InitializeComponent();
            waveOutDevice = new WaveOutEvent();
            player = new KarPlayer();
        }

        public int CurrentCdgId { get { return currentId; } }

        public void Start(string fileName, int KaraokeId, string message, string messageAtEnd)
        {
            stopPlay = false;
            cdgFile = new CDGFile(fileName);
            this.currentId = KaraokeId;
            var mp3FileName = fileName.Replace(System.IO.Path.GetExtension(fileName), ".mp3");
            cdgLength = cdgFile.getTotalDuration();
            PlayMp3File(mp3FileName);
            startTime = DateTime.Now;
            firstFrame = false;
            endTime = startTime.AddMilliseconds(cdgLength);
            msecsRemaining = cdgLength;
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(TickTimer);
            txtAnuncio.Text = message;
            messageEnd = messageAtEnd;
            step1 = true;
            step2 = false;
            

            txtAnuncio.Visibility = System.Windows.Visibility.Visible;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();
        }

        private void TickTimer(object sender, EventArgs e)
        {
            if (msecsRemaining > 0)
            {
                msecsRemaining = (long)endTime.Subtract(DateTime.Now).TotalMilliseconds;
                var pos = cdgLength - msecsRemaining;
                cdgFile.renderAtPosition(pos);
                var image = (System.Drawing.Bitmap)cdgFile.get_RGBImage();
                if (!firstFrame)
                {
                    ChangeAnnounceColor(image);
                }
                if (step1 && pos > 10000)
                {
                    txtAnuncio.Visibility = System.Windows.Visibility.Hidden;
                    step1 = false;
                    step2 = true;
                }
                if (step2 && msecsRemaining < 15000)
                {
                    txtAnuncio.Visibility = System.Windows.Visibility.Visible;
                    txtAnuncio.Text = messageEnd;
                    step2 = false;
                }
                karImage.Source = loadBitmap(image);
            }
            else
            {
                waveOutDevice.Stop();
                KaraokeEnded(currentId);
            }
        }

        private void ChangeAnnounceColor(System.Drawing.Bitmap image)
        {
            var pixel = image.GetPixel(1, 1);
            var brightness = pixel.GetBrightness();
            if (brightness > 0.5)
            {
                txtAnuncio.Foreground = Brushes.Black;
            }
            else
            {
                txtAnuncio.Foreground = Brushes.White;
            }
            firstFrame = true;
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
            waveOutDevice.Stop();
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
