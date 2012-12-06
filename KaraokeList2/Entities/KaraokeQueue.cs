using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaraokeList2.Entities
{
    public class KaraokeQueue
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }
        public int PlayOrder { get; set; }
        public Boolean Played { get; set; }
    }
}
