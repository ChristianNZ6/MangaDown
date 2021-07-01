
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GetFunctions;
using ScrapySharp.Network;
using ScrapySharp.Extensions;
using GetFunctions;
using System.Threading;
using System.Net;

namespace WebScraper_Mangakakarot
{
    class Program
    {
        static void Main(string[] args)
        {

            DownManga("solo");
        }

        public static void DownManga(string nameManga, [Optional] int afterCap)
        {



            //nameManga = "Parallel Paradise";
            //afterCap = 0;
            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;
            var indexManga = 0;

            var baseUrl = "https://manganelo.com/getstorysearchjson";

            var http = (HttpWebRequest)WebRequest.Create(new Uri(baseUrl));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";

            string parsedContent = "searchword:solo";
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytess = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytess, 0, bytess.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();


            var json = browser.NavigateToPage(new Uri("https://unionleitor.top/assets/busca.php?q=" + nameManga), HttpVerb.Get).Content;

            //JavaScriptSerializer ser = new JavaScriptSerializer();

            //var jsonDTO = JsonConvert.DeserializeObject<List<JsonDTO>>(results.Content.ToString()); ;
            var instance = JsonConvert.DeserializeObject<JsonDTO>(json).Property1;

            if (instance.ToList().Count > 1)
            {
                var ind = 0;

                Console.WriteLine("Mangas encontrados: ");
                foreach (var item in instance)
                {
                    Console.WriteLine(ind + ":" + item.name);
                    ind++;
                }

                indexManga = int.Parse(Console.ReadLine());
            }

            if (instance.ToList().Count == 0)
            {
                Console.WriteLine("NADA ENCONTRADO");

                return;

            }

            var url = instance[indexManga].story_link;

            var namemaga = instance[indexManga].name;

            var capsPage = browser.NavigateToPage(new Uri("https://unionleitor.top/manga/" + url), HttpVerb.Get);

            HtmlDocument hdoc = new HtmlDocument();

            hdoc.LoadHtml(capsPage.Html.InnerHtml);


            string capXpath = "//div[@class ='col-xs-6 col-md-6']//a";

            List<string> nameCaps;


            nameCaps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().Select(x => x.InnerText).ToList();


            nameCaps.Reverse();
            



            var caps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().Select(node => node.GetAttributeValue("href")).ToList();

            //Collections.reverse(caps);

            caps.Reverse();

            //if (afterCap == '')
            //{
            //    afterCap = 0
            //}


            var nameCapsAndcaps = nameCaps.Zip(caps, (nc, c) => new { nameCaps = nc, caps = c });


            if (afterCap < nameCapsAndcaps.Count())
            {
                var jump = afterCap;
                nameCapsAndcaps = nameCapsAndcaps.Skip(jump - 1);
            }



            foreach (var cap in nameCapsAndcaps)
            {
                List<string> pages;

                var pagesPage = browser.NavigateToPage(new Uri(cap.caps), HttpVerb.Get);
                hdoc.LoadHtml(pagesPage.Html.InnerHtml);
                string pagesXpath = "//*[@id='leitor']/div[4]/div[4]/img";

                try
                {
                    pages = hdoc.DocumentNode.SelectNodes(pagesXpath).ToArray().Select(node => node.GetAttributeValue("src")).ToList();

                    pages.RemoveAll(u => u.Contains("banner"));

                }
                catch (Exception)
                {
                    return;

                }

                List<Byte[]> listbyte = new List<byte[]>();
                byte[] bytes = { };

                int ttt = 1;





                Console.WriteLine(cap.nameCaps);

                foreach (var page in pages)
                {

                    Path.GetExtension(page);

                    Console.WriteLine("page: " + ttt);

                    //Imgs.Add(localFileName);

                    //WebClient webClient = new WebClient();

                    //Directory.CreateDirectory(Path.GetDirectoryName(localFileName));
                    //webClient.DownloadFile(page, localFileName);

                    //FileInfo fileInfo = new FileInfo(localFileName);

                    //folderName = Path.GetDirectoryName(localFileName);




                    do
                    {
                        bytes = GetFunctions.GetFunctions.GetImg(page);

                        Thread.Sleep(5000);
                    } while (bytes == null);


                    if (bytes == null)
                    {
                        var strin = "stop";
                    }


                    listbyte.Add(bytes);

                    ttt++;
                }

                var targetFileData = MergeTiff.NET.TiffHelper.MergeTiff(listbyte.ToArray());

                var localFileName = @"F:\Mangas\" + namemaga + "\\" + cap.nameCaps + ".tiff";

                var DirName = Path.GetDirectoryName(localFileName);

                Directory.CreateDirectory(DirName);

                File.WriteAllBytes(localFileName, targetFileData);
                //ToTIFF(Imgs, folderName);










                //MemoryStream ms = ToPDF(listbyte);



                //Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
                //doc.LoadFromStream(ms);

                ////doc.PageSettings.SetMargins(0, 0, 0, 0);







                //var pathFile = "C:\\1\\" + namemaga + " " + cap.nameCaps + ".pdf";
                //var fileName = namemaga + " " + cap.nameCaps;

                //doc.SaveToFile(pathFile);



                //FileStream fileStream = new FileStream("C:\\1\\" + namemaga + " " + cap.nameCaps + ".pdf", FileMode.Open, FileAccess.Read);

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

        }

        public class JsonDTO
        {
            public Class1[] Property1 { get; set; }
        }

        public class Class1
        {
            public string id { get; set; }
            public string name { get; set; }
            public string nameunsigned { get; set; }
            public string lastchapter { get; set; }
            public string image { get; set; }
            public string author { get; set; }
            public string story_link { get; set; }
        }


    }

   

}
