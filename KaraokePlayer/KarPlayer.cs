using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CDGNet;
using NAudio;
using NAudio.Wave;
using System.IO;
namespace KaraokePlayer
{
    public class KarPlayer
    {
        IWavePlayer waveOutDevice;
        WaveStream mainOutputStream;
        WaveChannel32 volumeStream;
        CDGFile cdgFile;
        bool stopPlay=false;
        bool pausePlay=false;
        public KarPlayer()
        {
            waveOutDevice = new WaveOutEvent();
        }

        public void PlayMp3File(string fileName)
        {
            mainOutputStream = CreateMp3OutPut(fileName);
            waveOutDevice.Init(mainOutputStream);
            waveOutDevice.Play();
        }

        public void PlayCdg(string fileName,System.Windows.Forms.PictureBox canvas)
        {
            int frameCount=0;
            stopPlay = false;
            cdgFile = new CDGFile(fileName);
            var mp3FileName=fileName.Replace(Path.GetExtension(fileName),".mp3");
            var cdgLength = cdgFile.getTotalDuration();
            PlayMp3File(mp3FileName);
            var startTime = DateTime.Now;
            var endTime = startTime.AddMilliseconds(cdgLength);
            var msecsRemaining = cdgLength;
            while (msecsRemaining > 0)
            {
                if (stopPlay)
                {
                    break;
                }
                msecsRemaining = (long)endTime.Subtract(DateTime.Now).TotalMilliseconds;
                var pos = cdgLength - msecsRemaining;
                while (pausePlay)
                {
                    endTime = DateTime.Now.AddMilliseconds(msecsRemaining);
                    System.Windows.Forms.Application.DoEvents();
                }
                cdgFile.renderAtPosition(pos);
                ++frameCount;
                canvas.Image = cdgFile.get_RGBImage(false);
                canvas.BackColor = ((System.Drawing.Bitmap)canvas.Image).GetPixel(1, 1);
                canvas.Refresh();
                System.Windows.Forms.Application.DoEvents();
            }
            StopCdgFile();
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

        public void Close()
        {
            if (waveOutDevice != null)
            {
                waveOutDevice.Stop();
            }
            if (mainOutputStream != null)
            {
                volumeStream.Close();
                volumeStream = null;
                mainOutputStream.Close();
                mainOutputStream = null;
            }
            if (waveOutDevice != null)
            {
                waveOutDevice.Dispose();
                waveOutDevice = null;
            }
        }

        public void StopMp3File()
        {
            waveOutDevice.Stop();
        }

        public void StopCdgFile()
        {
            stopPlay = true;
            waveOutDevice.Stop();
            cdgFile.Dispose();
        }
    }
}
