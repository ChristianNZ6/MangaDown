using HtmlAgilityPack;
using Nancy.Json;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace WebScrap_Union_single
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DownMangaSingle("");


        }

        public static void DownMangaSingle(string nameManga)
        {

           nameManga = "spy x family"; 

            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;


            var json = browser.NavigateToPage(new Uri("https://unionleitor.top/assets/busca.php?q=" + nameManga), HttpVerb.Get).Content;

            JavaScriptSerializer ser = new JavaScriptSerializer();

            //var jsonDTO = JsonConvert.DeserializeObject<List<JsonDTO>>(results.Content.ToString()); ;
            var instance = JsonConvert.DeserializeObject<JsonDTO>(json).items;

            var url = instance[0].url;

            var namemaga = instance[0].titulo;



            var capsPage = browser.NavigateToPage(new Uri("https://unionleitor.top/manga/" + url), HttpVerb.Get);

            HtmlDocument hdoc = new HtmlDocument();

            hdoc.LoadHtml(capsPage.Html.InnerHtml);


            string capXpath = "//div[@class ='col-xs-6 col-md-6']//a";

            List<string> nameCaps;


            nameCaps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().Select(x => x.InnerText).ToList();


            nameCaps.Reverse();
            CustomComparer customComparer = new CustomComparer();



            var caps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().Select(node => node.GetAttributeValue("href")).ToList(); ;

            //Collections.reverse(caps);

            caps.Reverse();


            var nameCapsAndcaps = nameCaps.Zip(caps, (nc, c) => new { nameCaps = nc, caps = c })/*.Where( x => x.nameCaps == "Cap. 07")*/;

            DBModel.MangasDBEntities mangasDBEntities = new DBModel.MangasDBEntities();

            DBModel.Mangas mangasModel = new DBModel.Mangas();
            DBModel.Caps capsModel = new DBModel.Caps();

            mangasModel.NameManga = namemaga;
            mangasModel.NumberCaps = caps.Count().ToString();

            //mangasModel.DateLastUpdate = DateTime.Now;

            mangasDBEntities.Mangas.Add(mangasModel);

            mangasDBEntities.SaveChanges();





            foreach (var cap in nameCapsAndcaps)
            {
                List<string> pages;

                var pagesPage = browser.NavigateToPage(new Uri(cap.caps), HttpVerb.Get);
                hdoc.LoadHtml(pagesPage.Html.InnerHtml);
                string pagesXpath = "//*[@id='leitor']/div[4]/div[4]/img";

                capsModel.NumberCap = cap.nameCaps;
                //capsModel.SendToKindle = false;
                capsModel.ID_Manga = mangasDBEntities.Mangas.Where(z => z.NameManga == namemaga).Select(x => x.ID).FirstOrDefault();
                capsModel.LastUpdate = DateTime.Now;

                mangasDBEntities.SaveChanges();


                //try
                //{
                //    pages = hdoc.DocumentNode.SelectNodes(pagesXpath).ToArray().Select(node => node.GetAttributeValue("src")).ToList();

                //    pages.RemoveAll(u => u.Contains("banner"));

                //}
                //catch (Exception)
                //{
                //    return;

                //}

                //List<Byte[]> listbyte = new List<byte[]>();
                //byte[] bytes = { };
                //foreach (var page in pages)
                //{


                //    bytes = GetImg(page);

                //    //var base64 = Convert.ToBase64String(bytes);

                //    listbyte.Add(bytes);


                //}



                //MemoryStream ms = ToPDF(listbyte);
                //Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
                //doc.LoadFromStream(ms);

                //doc.PageScaling = PdfPrintPageScaling.ShrinkOversized;







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

        }
        public class JsonDTO
        {
            public Item[] items { get; set; }
        }

        public class Item
        {
            public string imagem { get; set; }
            public string titulo { get; set; }
            public string url { get; set; }
            public string autor { get; set; }
            public string artista { get; set; }
            public string capitulo { get; set; }
        }

        public static byte[] GetImg(string url)
        {
            //create a stream object and initialize it to null
            Stream stream = null;
            //create a byte[] object. It serves as a buffer.
            byte[] buf;
            try
            {
                //Create a new WebProxy object.
                WebProxy myProxy = new WebProxy();
                //create a HttpWebRequest object and initialize it by passing the colleague api url to a create method.
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                //Create a HttpWebResponse object and initilize it
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                //get the response stream
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    //get the content length in integer
                    int len = (int)(response.ContentLength);
                    //Read bytes
                    buf = br.ReadBytes(len);
                    //close the binary reader
                    br.Close();
                }
                //close the stream object
                stream.Close();
                //close the response object 
                response.Close();
            }
            catch (Exception exp)
            {
                //set the buffer to null
                buf = null;
            }
            //return the buffer
            return (buf);
        }


        public static MemoryStream ToPDF(List<byte[]> input)
        {
            try
            {
                MemoryStream memoryStream = new MemoryStream();

                using (Spire.Pdf.PdfDocument document = new Spire.Pdf.PdfDocument())
                {
                    foreach (byte[] element in input)
                    {
                        MemoryStream imageMemoryStream = new MemoryStream(element);

                        Spire.Pdf.PdfPageBase page = document.Pages.Add();
                        Spire.Pdf.Graphics.PdfImage image = Spire.Pdf.Graphics.PdfImage.FromStream(imageMemoryStream);
                        //page.PageInfo.Margin.Bottom = 0;
                        //page.PageInfo.Margin.Top = 0;
                        //page.PageInfo.Margin.Left = 0;
                        //page.PageInfo.Margin.Right = 0;
                        //page.PageInfo.IsLandscape = false;

                        float widthFitRate = image.PhysicalDimension.Width / page.Canvas.ClientSize.Width;
                        float heightFitRate = image.PhysicalDimension.Height / page.Canvas.ClientSize.Height;
                        float fitRate = Math.Max(widthFitRate, heightFitRate);
                        float fitWidth = image.PhysicalDimension.Width / fitRate;
                        float fitHeight = image.PhysicalDimension.Height / fitRate;

                        page.Canvas.DrawImage(image, 0, 0, fitWidth, fitHeight);
                    }

                    //document.PDFStandard.CompressionLevel(new Document.ConvertOptions()
                    //{
                    //    LinkDuplcateStreams = true,
                    //    RemoveUnusedObjects = true,
                    //    RemoveUnusedStreams = true,
                    //    CompressImages = true //IMP
                    //});

                    document.SaveToStream(memoryStream);
                }

                return memoryStream;
            }
            catch
            {
                throw;
            }
        }

        public class CustomComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var regex = new Regex("^(d+)");

                // run the regex on both strings
                var xRegexResult = regex.Match(x);
                var yRegexResult = regex.Match(y);

                // check if they are both numbers
                if (xRegexResult.Success && yRegexResult.Success)
                {
                    return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
                }

                // otherwise return as string comparison
                return x.CompareTo(y);
            }
        }
    }
}
