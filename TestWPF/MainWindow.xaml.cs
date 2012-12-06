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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            VentanaKar ventana = new VentanaKar();
            ventana.Show();
            ventana.Start(@"D:\Karaoke\KAR-4454 Exitos del Rock 4\KAR-4454 Exitos del Rock 4\09 Obsesion - Miguel Mateos.cdg");
        }
    }
}
