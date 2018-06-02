using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaraokeList2.Entities
{
    public class KaraokeCatalog
    {
        public string Name { get; set; }
        public List<KaraokeFile> Files { get; set; }
        public KaraokeCatalog()
        {
            this.Files = new List<KaraokeFile>();
        }
        public KaraokeCatalog(string Name)
        {
            this.Name = Name;
            this.Files = new List<KaraokeFile>();
        }
    }
}
