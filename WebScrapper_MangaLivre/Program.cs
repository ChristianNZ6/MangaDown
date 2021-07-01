using Newtonsoft.Json;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapper_MangaLivre
{
    public class Program
    {
        static void Main(string[] args)
        {
            DownManga("solo");
        }

        public static void DownManga(string nameManga, [Optional]int afterCap)
        {

            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;


            string myJson = "{'search': 'solo'}";


            //string albumurl = Uri.EscapeUriString("https://mangalivre.net/lib/search/series.json");
            //string doc = "";
            //using (System.Net.WebClient client = new System.Net.WebClient()) // WebClient class inherits IDisposable
            //{
            //    doc = client.DownloadString(albumurl);
            //}

            //using (var client = new httpcl())
            //{
            //    var response = await client.PostAsync(
            //        "https://mangalivre.net/lib/search/series.json",
            //         new StringContent(myJson, Encoding.UTF8, "application/json"));
            //}


            var indexManga = 0;

            var page = browser.NavigateToPage(new Uri("https://mangalivre.net/lib/search/series.json"), HttpVerb.Get);
            var json = browser.NavigateToPage(new Uri("https://mangalivre.com/"), HttpVerb.Get).Content;

            var instance = JsonConvert.DeserializeObject<JsonDTO>(json).series;

        }



        public class JsonDTO
        {
            public Series[] series { get; set; }
            public object[] categories { get; set; }
            public object[] groups { get; set; }
        }

        public class Series
        {
            public int id_serie { get; set; }
            public string name { get; set; }
            public string label { get; set; }
            public string score { get; set; }
            public string value { get; set; }
            public string author { get; set; }
            public string artist { get; set; }
            public Category[] categories { get; set; }
            public string cover { get; set; }
            public string link { get; set; }
            public bool is_complete { get; set; }
        }

        public class Category
        {
            public int id_category { get; set; }
            public object id_sub_domain { get; set; }
            public string domain { get; set; }
            public string name { get; set; }
            public _Joindata _joinData { get; set; }
        }

        public class _Joindata
        {
            public int id_serie_category { get; set; }
            public int id_serie { get; set; }
            public int id_category { get; set; }
        }

    }
}
