using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using KaraokeList2.Entities;

namespace KaraokeList2.Modules
{
    public class MainModule : NancyModule
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
                var model = new
                {
                    title = "Welcome to karaokeList",
                    username = user,
                    queue = DAL.Instance.GetQueue().OrderBy(y => y.Date).ToList<Entities.KaraokeQueue>()
                };
                return View["Main/Index.cshtml", model];
            };

            Post["/Search"] = x =>
            {
                var searchTerm = (string)Request.Form.searchText;
                var name = (string)Request.Form.username;
                if (string.IsNullOrEmpty(name))
                {
                    name = "Fulanito";
                }
                var results = DAL.Instance.GetSearchResults(searchTerm);
                var view = View["Main/results.cshtml", new { title = "Search result for " + searchTerm, Results = results, RequestedBy = name }];
                view.WithCookie(new Nancy.Cookies.NancyCookie("username", name));
                return view;
            };

            Get["/All"] = x =>
            {
                Dictionary<string, KaraokeCatalog> catalog = new Dictionary<string, KaraokeCatalog>();
                var name = Request.Cookies["username"];
                if (string.IsNullOrEmpty(name))
                {
                    name = "Fulanito";
                }
                var results = DAL.Instance.GetAll();
                string alphabet = "abcdefghijklmnopqrstuvwxyz";
                foreach (char c in alphabet)
                {
                    catalog.Add(c.ToString().ToUpper(), new KaraokeCatalog() { Name = c.ToString().ToUpper() });
                }
                foreach (var file in results)
                {
                    string firstLetter = file.Filename[0].ToString().ToUpper();
                    if (catalog.ContainsKey(firstLetter))
                    {
                        catalog[firstLetter.ToString().ToUpper()].Files.Add(file);
                    }
                    else
                    {
                        var newEntry = new KaraokeCatalog() { Name = firstLetter };
                        newEntry.Files.Add(file);
                        catalog.Add(firstLetter, newEntry);
                    }
                    
                }
                
                var view = View["Main/bigresults.cshtml", new { title = "All Catalog ", Results = catalog.Values.ToList<KaraokeCatalog>(), RequestedBy = name }];
                view.WithCookie(new Nancy.Cookies.NancyCookie("username", name));
                return view;
            };

            Get["/AddToQueue/{songId}/RequestedBy/{requestBy}/"] = x =>
            {
                var songid = (int)x.songId;
                var username = (string)x.requestBy;
                var newRequest = DAL.Instance.InsertQueueSlot(songid, username);
                MainWindow.NewRequest.OnNext(newRequest);
                return Response.AsJson(new { added = true });
            };
        }
    }
}
