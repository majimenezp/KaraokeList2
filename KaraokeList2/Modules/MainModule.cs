using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
namespace KaraokeList2.Modules
{
    public class MainModule:NancyModule
    {
        public MainModule()
        {
            Get["/"] = x =>
            {
                string user = string.Empty;
                if (Request.Cookies.ContainsKey("username"))
                {
                    user = Request.Cookies["username"];
                }
                var model=new { 
                    title = "Welcome to karaokeList", 
                    username = user,
                    queue=DAL.Instance.GetQueue().OrderBy(y=>y.Date).ToList<Entities.KaraokeQueue>()
                };
                return View["Main/Index.cshtml",model ];
            };

            Post["/Search"] = x =>
            {
                var searchTerm=(string)Request.Form.searchText;
                var name = (string)Request.Form.username;
                if (string.IsNullOrEmpty(name))
                {
                    name = "Fulanito";
                }
                var results=DAL.Instance.GetSearchResults(searchTerm);
                var view=View["Main/results.cshtml", new { title = "Search result for " + searchTerm,Results=results,RequestedBy=name }];
                view.WithCookie(new Nancy.Cookies.NancyCookie("username", name));
                return view;
            };

            Get["/AddToQueue/{songId}/RequestedBy/{requestBy}/"] = x =>
            {
                var songid = (int)x.songId;
                var username = (string)x.requestBy;
                var newRequest=DAL.Instance.InsertQueueSlot(songid, username);
                MainWindow.NewRequest.OnNext(newRequest);
                return Response.AsJson(new { added=true});
            };
        }
    }
}
