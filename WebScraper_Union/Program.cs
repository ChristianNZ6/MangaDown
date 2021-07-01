using Aspose.Imaging.FileFormats.Tiff;
using HtmlAgilityPack;

using Nancy.Json;
using Newtonsoft.Json;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GetFunctions;


namespace WebScraper_Union
{
   public class Program
    {
        public static EncoderParameters EncoderParams { get; private set; }

        static void Main(string[] args)
        {


            Console.WriteLine("0 : atualizar mangas existentes\r\n1 : pesquisar mangas");
            var chose = Console.ReadLine();
            if (chose=="0")
            {
               var dirs =   Directory.GetDirectories(@"F:\Mangas");


                foreach (var item in dirs)
                {
                  var Namemanga=   Path.GetFileName(item);
                    var ListMangas = Directory.GetFiles(item).OrderBy(x => new FileInfo(x).CreationTime);
                    var LastManga = Path.GetFileNameWithoutExtension(ListMangas.Last().ToString()).Replace("Cap. ", "");
                        var instance = GetList(Namemanga);

                    if (instance[0].capitulo != ListMangas.Count().ToString()&& instance[0].capitulo != LastManga)
                    {
                        Console.WriteLine(instance[0].titulo + " | " + LastManga + " | " + instance[0].capitulo);
                        DownManga(instance, 0,true, LastManga);

                        

                    }
              

                }
                Console.WriteLine("finalizado");
                Console.ReadKey();

            }
            else if (chose == "1")
            {
                string exit = null;
                do
                {
                    int indexManga = 0;
                    Console.WriteLine("Nome do Manga");
                    var mangaNmae = Console.ReadLine();
                    exit = mangaNmae;

                    var instance = GetList(mangaNmae);

                    if (instance.ToList().Count > 1)
                    {
                        var ind = 0;

                        Console.WriteLine("Mangas encontrados: ");
                        foreach (var item in instance)
                        {
                            Console.WriteLine(ind + ":" + item.titulo);
                            ind++;
                        }

                        indexManga = int.Parse(Console.ReadLine());
                    }

                    if (instance.ToList().Count == 0)
                    {
                        Console.WriteLine("NADA ENCONTRADO");

                        continue;
                    }

                    var manga = instance[indexManga];

                    Console.WriteLine("Nome: " + manga.titulo + "\r\n" + "Autor: " + manga.autor + "\r\n" + "Ult. Cap: " + manga.capitulo);


                    Console.WriteLine("1 : Desde o Inicio \r\n 2 : partir de um capitulo");

                    var ch = Console.ReadLine();



                    if (ch == "1")
                    {
                        DownManga(instance, indexManga);
                    }
                    else if (ch == "2")
                    {
                        Console.WriteLine("Qual ??");
                        var aftercap = Console.ReadLine();

                        DownManga(instance, indexManga,false, aftercap);
                    }
                    //else
                    //{
                    //    return;
                    //}

                } while (exit != "");
            }
           


           


           
            
           
 


             
        }

        public static void UpdateRegister(string nameManga)
        {
            DBModel.MangasDBEntities mangasDBEntities = new DBModel.MangasDBEntities();

            //nameManga = "spy x family";

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


            DBModel.Mangas mangasModel = new DBModel.Mangas();
            DBModel.Caps capsModel = new DBModel.Caps();

            mangasModel.NameManga = namemaga;
            mangasModel.NumberCaps = caps.Count().ToString();

            //mangasModel.DateLastUpdate = DateTime.Now;

            var existM = mangasDBEntities.Mangas.Where(x => x.NameManga == namemaga).ToList().Count > 0;


            if (existM)
            {

            }
            else
            {
                mangasDBEntities.Mangas.Add(mangasModel);

            }

            mangasDBEntities.SaveChanges();





            foreach (var cap in nameCapsAndcaps)
            {
                List<string> pages;

                var pagesPage = browser.NavigateToPage(new Uri(cap.caps), HttpVerb.Get);
                hdoc.LoadHtml(pagesPage.Html.InnerHtml);
                string pagesXpath = "//*[@id='leitor']/div[4]/div[4]/img";

                capsModel.NumberCap = cap.nameCaps;
                //capsModel.SendToKindle = false;

                var idManga = mangasDBEntities.Mangas.Where(z => z.NameManga == namemaga).Select(x => x.ID).FirstOrDefault();
                capsModel.ID_Manga = idManga;
                capsModel.LastUpdate = DateTime.Now;

                var existC = mangasDBEntities.Caps.Where(x => x.NumberCap == cap.nameCaps && x.ID_Manga == idManga).ToList().Count > 0;


                if (existC)
                {

                }
                else
                {
                    mangasDBEntities.Caps.Add(capsModel);

                }

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

        public static Item[] GetList(string nameManga) 
        {
            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;
            var indexManga = 0;

            var json = browser.NavigateToPage(new Uri("https://unionleitor.top/assets/busca.php?q=" + nameManga), HttpVerb.Get).Content;

            //JavaScriptSerializer ser = new JavaScriptSerializer();

            //var jsonDTO = JsonConvert.DeserializeObject<List<JsonDTO>>(results.Content.ToString()); ;
            var instance = JsonConvert.DeserializeObject<JsonDTO>(json).items;


            return instance;
            //if (instance.ToList().Count > 1)
            //{
            //    var ind = 0;

            //    Console.WriteLine("Mangas encontrados: ");
            //    foreach (var item in instance)
            //    {
            //        Console.WriteLine(ind + ":" + item.titulo);
            //        ind++;
            //    }

            //    indexManga = int.Parse(Console.ReadLine());
            //}

            //if (instance.ToList().Count == 0)
            //{
            //    Console.WriteLine("NADA ENCONTRADO");

            //    return;

            //}



        }


        public static void DownManga(Item[] instance,int indexManga, [Optional] bool Last,[Optional]string afterCap)
        {



            //nameManga = "Parallel Paradise";
            //afterCap = 0;
            var browser = new ScrapingBrowser();

            browser.IgnoreCookies = true;
            //var indexManga = 0;

            //var json = browser.NavigateToPage(new Uri("https://unionleitor.top/assets/busca.php?q=" + nameManga), HttpVerb.Get).Content;

            ////JavaScriptSerializer ser = new JavaScriptSerializer();

            ////var jsonDTO = JsonConvert.DeserializeObject<List<JsonDTO>>(results.Content.ToString()); ;
            //var instance = JsonConvert.DeserializeObject<JsonDTO>(json).items;

            //if (instance.ToList().Count > 1)
            //{
            //    var ind = 0;

            //    Console.WriteLine("Mangas encontrados: ");
            //    foreach (var item in instance)
            //    {
            //        Console.WriteLine(ind + ":" + item.titulo);
            //        ind++;
            //    }

            //    indexManga = int.Parse(Console.ReadLine());
            //}

            //if (instance.ToList().Count == 0)
            //{
            //    Console.WriteLine("NADA ENCONTRADO");

            //    return;

            //}

            var url = instance[indexManga].url;

            var namemaga = instance[indexManga].titulo;

            var capsPage = browser.NavigateToPage(new Uri("https://unionleitor.top/manga/" + url), HttpVerb.Get);

            HtmlDocument hdoc = new HtmlDocument();

            hdoc.LoadHtml(capsPage.Html.InnerHtml);


            string capXpath = "//div[@class ='col-xs-6 col-md-6']//a";

            List<string> nameCaps;


            nameCaps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().Select(x => x.InnerText).ToList();


            nameCaps.Reverse();
            CustomComparer customComparer = new CustomComparer();



            var caps = hdoc.DocumentNode.SelectNodes(capXpath).ToArray().Select(node => node.GetAttributeValue("href")).ToList();

            //Collections.reverse(caps);

            caps.Reverse();

            //if (afterCap == '')
            //{
            //    afterCap = 0
            //}


            var nameCapsAndcaps = nameCaps.Zip(caps, (nc, c) => new { nameCaps = nc, caps = c });

          
                //if (afterCap< nameCapsAndcaps.Count())
                //{
                //    var jump = afterCap;
                //    nameCapsAndcaps = nameCapsAndcaps.Skip(jump-1);
                //}
                if (afterCap!=""|Last)
                {
                var index = Array.FindIndex(nameCapsAndcaps.ToArray(), r => r.nameCaps == "Cap. " + afterCap);

                nameCapsAndcaps = nameCapsAndcaps.Skip(index);
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

                    Console.WriteLine("page: "+ttt);

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

                var localFileName = @"F:\Mangas\" + namemaga + "\\" + cap.nameCaps+ ".tiff";

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

        

        //public static void ToTIFF(List<string> imagesPath,string folderName) 
        //{
        //    System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;
        //    EncoderParameters ep = new EncoderParameters(1);
        //    ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);





        //    Bitmap pages = null;

        //    File.MemoryStream ms = new MemoryStream(File.ReadAllBytes());
        //    pages = (Bitmap)Image.FromStream(ms);

        //    int frame = 0;

        //    foreach (var item in imagesPath)
        //    {

        //        MemoryStream mss = new MemoryStream(File.ReadAllBytes(item));
        //        Bitmap bm = (Bitmap)Image.FromStream(mss);
        //        int frameCount = bm.GetFrameCount(FrameDimension.Page);
        //        for (int i = 0; i < frameCount; i++)
        //        {
        //            bm.SelectActiveFrame(FrameDimension.Page, i);
        //            pages.SaveAdd(bm, ep);
        //        }
        //    }

        //    pages.Save(@"C:\1\TiffFile.tif");
            
            







        //}

        //public static class TiffHelper
        //{
        //    /// <summary>
        //    /// Merges multiple TIFF files (including multipage TIFFs) into a single multipage TIFF file.
        //    /// </summary>
        //    public static byte[] MergeTiff(params byte[][] tiffFiles)
        //    {
        //        byte[] tiffMerge = null;
        //        using (var msMerge = new MemoryStream())
        //        {
        //            //get the codec for tiff files
        //            ImageCodecInfo ici = null;
        //            foreach (ImageCodecInfo i in ImageCodecInfo.GetImageEncoders())
        //                if (i.MimeType == "image/tiff")
        //                    ici = i;

        //            System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;
        //            EncoderParameters ep = new EncoderParameters(1);

        //            Bitmap pages = null;
        //            int frame = 0;

        //            foreach (var tiffFile in tiffFiles)
        //            {
        //                using (var imageStream = new MemoryStream(tiffFile))
        //                {
        //                    using (Image tiffImage = Image.FromStream(imageStream))
        //                    {
        //                        foreach (Guid guid in tiffImage.FrameDimensionsList)
        //                        {
        //                            //create the frame dimension 
        //                            FrameDimension dimension = new FrameDimension(guid);
        //                            //Gets the total number of frames in the .tiff file 
        //                            int noOfPages = tiffImage.GetFrameCount(dimension);

        //                            for (int index = 0; index < noOfPages; index++)
        //                            {
        //                                FrameDimension currentFrame = new FrameDimension(guid);
        //                                tiffImage.SelectActiveFrame(currentFrame, index);
        //                                using (MemoryStream tempImg = new MemoryStream())
        //                                {
        //                                    tiffImage.Save(tempImg, ImageFormat.Tiff);
        //                                    {
        //                                        if (frame == 0)
        //                                        {
        //                                            //save the first frame
        //                                            pages = (Bitmap)Image.FromStream(tempImg);
        //                                            ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);
        //                                            pages.Save(msMerge, ici, ep);
        //                                        }
        //                                        else
        //                                        {
        //                                            //save the intermediate frames
        //                                            ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);
        //                                            pages.SaveAdd((Bitmap)Image.FromStream(tempImg), ep);
        //                                        }
        //                                    }
        //                                    frame++;
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if (frame > 0)
        //            {
        //                //flush and close.
        //                ep.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
        //                pages.SaveAdd(ep);
        //            }

        //            msMerge.Position = 0;
        //            tiffMerge = msMerge.ToArray();
        //        }
        //        return tiffMerge;
        //    }
        //}

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

        //public static byte[] GetImg(string url)
        //{
        //    //create a stream object and initialize it to null
        //    Stream stream = null;
        //    //create a byte[] object. It serves as a buffer.
        //    byte[] buf;
        //    try
        //    {
        //        //Create a new WebProxy object.
        //        WebProxy myProxy = new WebProxy();
        //        //create a HttpWebRequest object and initialize it by passing the colleague api url to a create method.
        //        HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
        //        //Create a HttpWebResponse object and initilize it
        //        HttpWebResponse response = (HttpWebResponse)req.GetResponse();
        //        //get the response stream
        //        stream = response.GetResponseStream();

        //        using (BinaryReader br = new BinaryReader(stream))
        //        {
        //            //get the content length in integer
        //            int len = (int)(response.ContentLength);
        //            //Read bytes
        //            buf = br.ReadBytes(len);
        //            //close the binary reader
        //            br.Close();
        //        }
        //        //close the stream object
        //        stream.Close();
        //        //close the response object 
        //        response.Close();
        //    }
        //    catch (Exception exp)
        //    {
        //        //set the buffer to null
        //        buf = null;
        //    }
        //    //return the buffer
        //    return (buf);
        //}


        //public static MemoryStream ToPDF(List<byte[]> input)
        //{
        //    try
        //    {
        //        MemoryStream memoryStream = new MemoryStream();

        //        using (Spire.Pdf.PdfDocument document = new Spire.Pdf.PdfDocument())
        //        {
        //            foreach (byte[] element in input)
        //            {
        //                MemoryStream imageMemoryStream = new MemoryStream(element);

        //                Spire.Pdf.PdfPageBase page = document.Pages.Add();
        //                Spire.Pdf.Graphics.PdfImage image = Spire.Pdf.Graphics.PdfImage.FromStream(imageMemoryStream);
        //                //page.PageInfo.Margin.Bottom = 0;
        //                //page.PageInfo.Margin.Top = 0;
        //                //page.PageInfo.Margin.Left = 0;
        //                //page.PageInfo.Margin.Right = 0;
        //                //page.PageInfo.IsLandscape = false;

        //                float widthFitRate = image.PhysicalDimension.Width / page.Canvas.ClientSize.Width;
        //                float heightFitRate = image.PhysicalDimension.Height / page.Canvas.ClientSize.Height;
        //                float fitRate = Math.Max(widthFitRate, heightFitRate);
        //                float fitWidth = image.PhysicalDimension.Width / fitRate;
        //                float fitHeight = image.PhysicalDimension.Height / fitRate;

        //                page.Canvas.DrawImage(image, 0, 0, fitWidth, fitHeight);
        //            }

        //            //document.PDFStandard.CompressionLevel(new Document.ConvertOptions()
        //            //{
        //            //    LinkDuplcateStreams = true,
        //            //    RemoveUnusedObjects = true,
        //            //    RemoveUnusedStreams = true,
        //            //    CompressImages = true //IMP
        //            //});

        //            document.SaveToStream(memoryStream);
        //        }

        //        return memoryStream;
        //    }
        //    catch
        //    {
        //        throw;
        //    }
        //}

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
