using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using KaraokePlayer;
namespace TestProject
{
    public partial class Form1 : Form
    {
        KarPlayer player;
        public Form1()
        {
            InitializeComponent();
            player = new KarPlayer();
        }

        private void txtiniciar_Click(object sender, EventArgs e)
        {
            
            //player.PlayMp3File(@"d:\Musica\01 Karma Police.mp3");
            player.PlayCdg(@"G:\Karaoke\U2\U2 - Elevation.cdg", pBox);
        }

        private void btnDetener_Click(object sender, EventArgs e)
        {
            //player.StopMp3File();
            player.StopCdgFile();
        }
    }
}
