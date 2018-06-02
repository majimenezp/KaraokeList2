using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using KaraokeList2.Entities;

namespace KaraokeList2.Modules
{
    public class ControlModule : NancyModule
    {
        public ControlModule()
        {
            Get["/Control"] = x =>
            {
                string user = string.Empty;

                var model = new
                {
                    title = "Control List",
                    queue = DAL.Instance.GetQueue().OrderBy(y => y.Date).ToList<Entities.KaraokeQueue>()
                };
                return View["Main/ControlIndex.cshtml", model];
            };
            Post["/Control"] = x =>
            {
                var command = (string)Request.Form.command;
                CommandRequest newRequest = new CommandRequest();
                var commandParts=command.Split('|');
                newRequest.Command=(CommandTypes) Enum.Parse(typeof(CommandTypes), commandParts[0]);
                newRequest.CommandText = commandParts.Length>1 ? commandParts[0]: string.Empty;
                MainWindow.NewCommand.OnNext(newRequest);
                var model = new
                {
                    title = "Control List",
                    queue = DAL.Instance.GetQueue().OrderBy(y => y.Date).ToList<Entities.KaraokeQueue>()
                };
                return Response.AsRedirect("/Control");
            };
        }
    }
}
