using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Conventions;
using Nancy.Diagnostics;
using Nancy.TinyIoc;

namespace KaraokeList2
{
    public class SelfHostBootstrapper: DefaultNancyBootstrapper
    {
        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get { return new DiagnosticsConfiguration { Password = @"karaoke" }; }
        }

        //protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        //{
        //    this.Conventions.ViewLocationConventions.Add((viewName, model, context) =>
        //    {
        //        return string.Concat("custom/", viewName);
        //    });
        //}

        protected override IRootPathProvider RootPathProvider
        {
            get { return new RoothPathProvider(); }
        }
    }
}
