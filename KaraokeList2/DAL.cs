using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Simple.Data;
using KaraokeList2.Entities;
using System.Data;
using System.Data.SQLite;
namespace KaraokeList2
{
    public sealed class DAL
    {
        private string currentPath;
        private string dbFilePath;
        dynamic db;
        dynamic mem;
        IDbConnection memConn;
        private static readonly Lazy<DAL> instance = new Lazy<DAL>(() => new DAL());
        private DAL()
        {
            currentPath=System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            dbFilePath=Path.Combine(currentPath, "db.sqlite");
            db = Database.OpenFile(dbFilePath);
            mem=Database.OpenConnection("data source=:memory:");
            memConn = ((Simple.Data.Ado.AdoAdapter)(mem.GetAdapter())).ConnectionProvider.CreateConnection();
            memConn.Open();
            var createDbCmd = memConn.CreateCommand();
            createDbCmd.CommandText = "CREATE TABLE 'KaraokeFiles' ('Id' INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 'Filename' TEXT NOT NULL DEFAULT '', 'FullFilePath' TEXT NOT NULL)";
            createDbCmd.ExecuteNonQuery();
            createDbCmd.CommandText=@"ATTACH '"+dbFilePath+"' AS disk";
            createDbCmd.ExecuteNonQuery();
            createDbCmd.CommandText = @"insert into KaraokeFiles select * from disk.KaraokeFiles";
            createDbCmd.ExecuteNonQuery();
        }

        public static DAL Instance { get { return instance.Value; } } 

        public bool InsertFile(KaraokeFile file)
        {
            var result = mem.KaraokeFiles.Insert(file);
            
            return true;
        }

        public List<KaraokeDirectory> GetDirectories()
        {
            List<KaraokeDirectory> results=db.Directories.All().ToList<KaraokeDirectory>();
            return results;
        }

        public void InsertDirectory(KaraokeDirectory karDir)
        {
            db.Directories.Insert(karDir);
        }

        public void DeleteDirectory(KaraokeDirectory karaokeDirectory)
        {
            db.Directories.DeleteById(karaokeDirectory.Id);
        }

        public void ClearKaraokeFile()
        {
            mem.KaraokeFiles.DeleteAll();
            db.KaraokeFiles.DeleteAll();
        }

        public void ClearKaraokeQueue()
        {
            db.Queue.DeleteAll();
        }

        public List<KaraokeQueue> GetQueue()
        {
            List<KaraokeQueue> results = db.Queue.All().Where(db.Queue.Played == false).ToList<KaraokeQueue>();
            return results;
        }

        internal List<KaraokeFile> GetSearchResults(string searchString)
        {
            List < KaraokeFile > results = db.KaraokeFiles.FindAll(db.KaraokeFiles.Filename.Like("%" + searchString + "%")).ToList<KaraokeFile>();
            return results;
        }

        internal bool InsertQueueSlot(KaraokeQueue tmp)
        {
            var result = db.Queue.Insert(tmp);
            return true;
        }

        internal void DeleteSlotInQueue(int Id)
        {
            db.Queue.DeleteById(Id);
        }

        internal KaraokeQueue GetNextFileInQueue()
        {
            var listQueue =(IList<KaraokeQueue>) db.Queue.All().Where(db.Queue.Played == false).OrderBy(db.Queue.Date).ToList<KaraokeQueue>();
            return listQueue.OrderBy(y => y.PlayOrder).FirstOrDefault<KaraokeQueue>();
        }

        internal void SetKaraokePlayed(int Id)
        {
            db.Queue.UpdateById(Id: Id, Played: true);
        }

        internal void CommitScan()
        {
            var cmd=memConn.CreateCommand();            
            cmd.CommandText = "delete from disk.KaraokeFiles;update sqlite_sequence set seq=0 where name='KaraokeFiles';insert into disk.KaraokeFiles select * from  KaraokeFiles";
            cmd.ExecuteNonQuery();
        }

        internal KaraokeQueue InsertQueueSlot(int songid, string username)
        {
            var file=(KaraokeFile)db.KaraokeFiles.FindById(songid);
            var slot = new KaraokeQueue();
            slot.Date = DateTime.Now;
            slot.FileName = file.Filename;
            slot.FilePath = file.FullFilePath;
            slot.Played = false;
            slot.PlayOrder = 10;
            slot.UserName = username;
            db.Queue.Insert(slot);
            return slot;
        }
    }
}
