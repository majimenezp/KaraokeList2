using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaraokeList2.Entities
{
    public class KaraokeDirectory
    {
        public int Id { get; set; }
        public string Directory { get; set; }
        public override string ToString()
        {
            return this.Directory;
        }
    }
}
