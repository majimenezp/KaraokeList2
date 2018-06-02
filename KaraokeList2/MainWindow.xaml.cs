using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
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
using KaraokeList2.Entities;

namespace KaraokeList2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DAL dal = DAL.Instance;
        List<KaraokeFile> searchResults;
        ObservableCollection<KaraokeQueue> queue;
        KaraokeQueue current;
        KaraokeWindow karWindow;
        BackgroundWorker workerSearch;
        BackgroundWorker workerAddToQueue;

        public static readonly Subject<KaraokeQueue> NewRequest = new Subject<KaraokeQueue>();
        public static readonly Subject<CommandRequest> NewCommand = new Subject<CommandRequest>();
        public MainWindow()
        {
            InitializeComponent();
            workerSearch = new BackgroundWorker();
            workerAddToQueue = new BackgroundWorker();
            workerSearch.DoWork += workerSearch_DoWork;
            workerSearch.RunWorkerCompleted += workerSearch_RunWorkerCompleted;
            workerAddToQueue.DoWork += workerAddToQueue_DoWork;
            workerAddToQueue.RunWorkerCompleted += workerAddToQueue_RunWorkerCompleted;
        }

        private static IObservable<KaraokeQueue> OnNewRequest
        {
            get
            {
                return NewRequest;
            }
        }

        private static IObservable<CommandRequest> OnCommandRequest
        {
            get
            {
                return NewCommand;
            }
        }

        void workerSearch_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            searchResults = (List<KaraokeFile>)e.Result;
            GridResults.ItemsSource = searchResults;
        }

        void workerSearch_DoWork(object sender, DoWorkEventArgs e)
        {
            var results = dal.GetSearchResults((string)e.Argument);
            e.Result = results;
        }

        private void btnDbSetup_Click(object sender, RoutedEventArgs e)
        {
            DbSetup dbsetup = new DbSetup();
            var res = dbsetup.ShowDialog();

        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            var queue = dal.GetQueue();
            if (queue != null)
            {
                Gridqueque.ItemsSource = queue;
            }
            karWindow = new KaraokeWindow();
            karWindow.Show();
            karWindow.KaraokeEnded += karWindow_KaraokeEnded;
            NewRequest.Subscribe(newRequest =>
            {
                Gridqueque.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
                    Gridqueque.ItemsSource = dal.GetQueue();
                }));

            });
            NewCommand.Subscribe(newCommand =>
            {
                Gridqueque.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate ()
                {
                    switch (newCommand.Command)
                    {
                        case CommandTypes.Play:
                            PlaySong();
                            break;
                        case CommandTypes.Stop:
                            karWindow.Stop();
                            break;
                        case CommandTypes.Next:
                            NextSong();
                            break;
                        case CommandTypes.Remove:
                            RemoveFromQueue(Convert.ToInt32(newCommand.CommandText));
                            break;
                        case CommandTypes.Back:
                        case CommandTypes.Other:
                            break;
                    }
                    Gridqueque.ItemsSource = dal.GetQueue();
                }));
            });
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            workerSearch.RunWorkerAsync(txtSearchText.Text);
        }

        private void Click_RemoveFromQueue(object sender, RoutedEventArgs e)
        {
            var slot = (sender as Button).DataContext as Entities.KaraokeQueue;
            if (slot != null)
            {
                RemoveFromQueue(slot.Id);
                
            }
        }

        private void RemoveFromQueue(int id)
        {
            dal.DeleteSlotInQueue(id);
            Gridqueque.ItemsSource = dal.GetQueue();
        }
        private void Click_AddSongToQueue(object sender, RoutedEventArgs e)
        {
            var file = (sender as Button).DataContext as Entities.KaraokeFile;
            workerAddToQueue.RunWorkerAsync(file);

        }

        void workerAddToQueue_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (List<KaraokeQueue>)e.Result;
            Gridqueque.ItemsSource = result;
        }

        void workerAddToQueue_DoWork(object sender, DoWorkEventArgs e)
        {
            var file = (KaraokeFile)e.Argument;
            KaraokeQueue tmp = new KaraokeQueue();
            tmp.Date = DateTime.Now;
            tmp.FileName = file.Filename;
            tmp.FilePath = file.FullFilePath;
            tmp.UserName = "console";
            tmp.PlayOrder = 10;
            dal.InsertQueueSlot(tmp);
            e.Result = dal.GetQueue();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            PlaySong();
        }

        private void PlaySong()
        {
            var next = dal.GetNextFileInQueue();
            if (next != null)
            {
                try
                {
                    karWindow.Start(next.FilePath, next.Id, "Cancion pedida por:" + next.UserName, "Viene la siguiente cancion");
                    current = next;
                }
                catch (Exception ex1)
                {
                    current = null;
                    next = null;
                }                
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            karWindow.Stop();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NextSong();
        }

        private void NextSong()
        {
            if (karWindow.CurrentCdgId > 0)
            {
                karWindow.Stop();
                dal.SetKaraokePlayed(current.Id);
                current = dal.GetNextFileInQueue();
                if (current != null)
                {
                    karWindow.Start(current.FilePath, current.Id, "Cancion pedida por:" + current.UserName, "Viene la siguiente cancion");
                }
            }
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {

        }
        void karWindow_KaraokeEnded(int KaraokeId)
        {
            dal.SetKaraokePlayed(KaraokeId);
            Gridqueque.ItemsSource = dal.GetQueue();
            current = dal.GetNextFileInQueue();
            if (current != null)
            {
                karWindow.Start(current.FilePath, current.Id, "Cancion pedida por:" + current.UserName, "Viene la siguiente cancion");
            }
        }
    }
}
