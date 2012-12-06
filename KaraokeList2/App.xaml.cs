using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Nancy.Hosting.Self;
namespace KaraokeList2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static NancyHost Host;
        public App():base()
        {
            Host = new NancyHost(new Uri[] { new Uri("http://localhost:9090") });
            Host.Start();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Host.Stop();
        }
    }
}
