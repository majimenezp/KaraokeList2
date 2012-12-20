using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
namespace KaraokeList2
{
    public class RoothPathProvider : IRootPathProvider
    {

        public string GetRootPath()
        {
            return
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
           //@"C:\Users\majimenezp\Documents\Visual Studio 2012\Projects\KaraokeList\KaraokeList2"; 
        }
    }
}
