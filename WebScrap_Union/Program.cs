using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nancy.Json;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace WebScrap_Union
{
    class Program
    {
        static void Main(string[] args)
        {
            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;


            var json = browser.NavigateToPage(new Uri("https://unionleitor.top/assets/busca.php?q=" + "spy"), HttpVerb.Get).Content;

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


            nameCaps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().OrderBy(x => x.InnerText).Select(x => x.InnerText).ToList();



            var caps = hdoc.DocumentNode.SelectNodes(capXpath).Select(node => node.GetAttributeValue("href")).ToArray();


            foreach (var cap in caps)
            {
                string[] pages = { };

                var pagesPage = browser.NavigateToPage(new Uri(cap), HttpVerb.Get);
                hdoc.LoadHtml(pagesPage.Html.InnerHtml);
                string pagesXpath = "//*[@id='leitor']/div[4]/div[4]/img";

                try
                {
                    pages = hdoc.DocumentNode.SelectNodes(pagesXpath).Select(node => node.GetAttributeValue("src")).ToArray();
                }
                catch (Exception)
                {
                    return;

                }

                List<Byte[]> listbyte = new List<byte[]>();
                byte[] bytes = { };
                foreach (var page in pages)
                {


                    bytes = GetImg(page);

                    var base64 = Convert.ToBase64String(bytes);

                    listbyte.Add(bytes);


                }

                

                MemoryStream ms = ToPDF(listbyte);
                Spire.Pdf.PdfDocument doc = new Spire.Pdf.PdfDocument();
                doc.LoadFromStream(ms);
                doc.SaveToFile("C:\\1\\output.pdf");

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
        //public static string SaveIntoPDf(Byte base64)
        //{



        //    byte[] pdfBytes;
        //    System.IO.FileStream stream = new FileStream(@"C:\file.pdf", FileMode.CreateNew);
        //    System.IO.BinaryWriter writer = new BinaryWriter(stream);
        //    writer.Write(bytes, 0, bytes.Length);
        //    writer.Close();


        //    File.WriteAllBytes(@"C:\testpdf.pdf", pdfBytes);
        //}



    }
}
