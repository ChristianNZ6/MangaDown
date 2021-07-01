using HtmlAgilityPack;
using Nancy.Json;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static WebScraper_Union.Program;

namespace WebScrapper_MangaHost
{
    public class Program
    {
        static void Main(string[] args)
        {
        }

        public static void DownManga(string nameManga, [Optional]int afterCap)
        {

            //nameManga = "Parallel Paradise";
            //afterCap = 0;
            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;
            var indexManga = 0;

            nameManga = nameManga.Replace(" ", "+");

            var retunr = browser.NavigateToPage(new Uri("https://mangahost2.com/find/spy" + nameManga), HttpVerb.Get).Content;

            HtmlDocument hdoc = new HtmlDocument();

            hdoc.LoadHtml(retunr);

            var ListMangaXpath = "//h3[@class='entry-title']/a";

            var listManga = hdoc.DocumentNode.SelectNodes(ListMangaXpath).ToArray().Select(x => new MangaModel {Name = x.InnerText,Url = x.GetAttributeValue("href") }).ToList();


            int inde = 0;
            foreach (var item in listManga)
            {
                Console.WriteLine(inde+" : " + item.Name);

                inde++;
            }

            int Choose =  int.Parse(Console.ReadLine());

            retunr = browser.NavigateToPage(new Uri(listManga[Choose].Url), HttpVerb.Get);

            var listCapXpath = "//a[@class='capitulo']";

            hdoc.LoadHtml(retunr);

            var listCap = hdoc.DocumentNode.SelectNodes(listCapXpath).ToArray().Select(x => new MangaModel { Name = x.GetAttributeValue("title"), Url = x.GetAttributeValue("href") }).ToList();

            foreach (var item in listCap)
            {
                var regex = new Regex(@"\s\#([\d.]+)\s-\s(.+)");

                var nameCap = regex.Match(item.Name).Groups[2]+" Cap. " +regex.Match(item.Name).Groups[1].ToString();

            }


            List<Byte[]> listbyte = new List<byte[]>();
                byte[] bytes = { };
                //foreach (var page in pages)
                //{


                //    bytes = GetImg(page);

                //    //var base64 = Convert.ToBase64String(bytes);

                //    listbyte.Add(bytes);


                //}



                //MemoryStream ms = ToPDF(listbyte);



                //Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
                //doc.LoadFromStream(ms);

                ////doc.PageSettings.SetMargins(0, 0, 0, 0);







                //var pathFile = "C:\\1\\" + namemaga + " " + cap.nameCaps + ".pdf";
                //var fileName = namemaga + " " + cap.nameCaps;

                //doc.SaveToFile(pathFile);



                ////FileStream fileStream = new FileStream("C:\\1\\" + namemaga + " " + cap.nameCaps + ".pdf", FileMode.Open, FileAccess.Read);

                //try
                //{
                //    EmailEnvio.Program.SendEmaail(pathFile, fileName);

                //}
                //catch (Exception)
                //{
                //    continue;

                //}


                //MemoryStream.CopyTo(fileStream);



            }

        public class MangaModel
        {
            public string Name { get; set; }
            public string Url { get; set; }
        }

    }
    }

