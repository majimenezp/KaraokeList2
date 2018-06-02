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
        public static SelfHostBootstrapper bootstrapper;
        public App() : base()
        {
            string httpPort = System.Configuration.ConfigurationManager.AppSettings.Get("httpport") ?? "80";
            bootstrapper = new SelfHostBootstrapper();
            Host = new NancyHost(bootstrapper, new Uri[] { new Uri($"http://localhost:{httpPort}") });
            Host.Start();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Host.Stop();
        }
    }
}
