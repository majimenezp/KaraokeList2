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
using Ookii.Dialogs.Wpf;
using System.IO;
using System.Threading.Tasks;
using KaraokeList2.Entities;
using System.Text.RegularExpressions;

namespace KaraokeList2
{
    /// <summary>
    /// Interaction logic for DbSetup.xaml
    /// </summary>
    public partial class DbSetup : Window
    {
        DAL dal;
        public DbSetup()
        {
            InitializeComponent();
            dal = DAL.Instance;
        }

        private void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            var res = dialog.ShowDialog();
            if (res ?? false)
            {
                var karDir = new KaraokeDirectory() { Id = 0, Directory = dialog.SelectedPath };
                dal.InsertDirectory(karDir);
                lstFolders.Items.Add(karDir);
            }
        }

        private void btnDelFolder_Click(object sender, RoutedEventArgs e)
        {
            var item = lstFolders.SelectedItem;
            lstFolders.Items.Remove(item);
            dal.DeleteDirectory((KaraokeDirectory)item);
        }

        private void btnScan_Click(object sender, RoutedEventArgs e)
        {
            Regex removeNumbers = new Regex(@"^[\d-]*\s*", RegexOptions.Singleline | RegexOptions.Compiled);
            var dal = DAL.Instance;
            dal.ClearKaraokeFile();
            foreach (var item in lstFolders.Items)
            {
                var folderPath = item.ToString();
                DirectoryInfo dir = new DirectoryInfo(folderPath);
                var files = dir.EnumerateFiles("*.cdg", SearchOption.AllDirectories);
                Parallel.ForEach(files, file =>
                {
                    var dirName = System.IO.Path.GetFileNameWithoutExtension(file.DirectoryName);
                    var fileName = System.IO.Path.GetFileNameWithoutExtension(file.Name);
                    fileName = removeNumbers.Replace(fileName, "");
                    dirName = removeNumbers.Replace(dirName, "");
                    //fileName =Regex.Replace(fileName, @"^[\d-]*\s*","",RegexOptions.IgnoreCase)
                    var insFile = new KaraokeFile();
                    insFile.Filename = dirName + " " +fileName;
                    insFile.FullFilePath = file.FullName;
                    dal.InsertFile(insFile);
                });

            }
            dal.CommitScan();
            MessageBox.Show("Scanning finished", "Message", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            var dirs = dal.GetDirectories();
            if (dirs != null)
            {
                foreach (var dir in dirs)
                {
                    lstFolders.Items.Add(dir);
                }
            }
        }
    }
}
