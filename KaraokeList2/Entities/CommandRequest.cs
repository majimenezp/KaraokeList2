using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KaraokeList2.Entities
{
    public class CommandRequest
    {
        public CommandTypes Command { get; set; }
        public string CommandText { get; set; }
    }
}
