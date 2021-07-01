using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using ActiveUp.Net.Mail;

namespace EmailEnvio
{
    public class Program
    {
        static void Main(string[] args)
        {
            ReadImap();
        }

        public static void SendEmaail(string filepath, string fileName)
        {
            System.Net.Mail.SmtpClient client = new System.Net.Mail.SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.Timeout = 360000;
            client.EnableSsl = true;
            client.Credentials = new System.Net.NetworkCredential("christianjorge2009@gmail.com", "Tiocris1");
            MailMessage mail = new MailMessage();


            mail.Sender = new System.Net.Mail.MailAddress("christianjorge2009+EnvioManga@gmail.com", "ENVIADOR");
            mail.From = new MailAddress("christianjorge2009+EnvioManga@gmail.com", "Manga Adicionado");
            mail.To.Add(new MailAddress("CHRISTIANJORGE2009_6ULABI@kindle.com", "RECEBEDOR"));
            mail.Subject = "convert";
            //mail.Body = "teste";
            mail.Attachments.Add(new Attachment(filepath));
            mail.IsBodyHtml = true;
            mail.Priority = MailPriority.High;


            var contue = true;
            while (contue)
            {
                try
                {
                    client.Send(mail);
                    Console.WriteLine(fileName + " enviado");


                    contue = false;
                }
                catch (System.Exception erro)
                {

                    Console.WriteLine(fileName + " " + erro.Message + " não enviado, tentando novamente");

                    Thread.Sleep(5000);



                    //trata erro
                }
            }

            //finally
            //{
            //    mail = null;
            //}
        }


        public static void ReadImap()
        {
            System.Net.WebClient objClient = new System.Net.WebClient();
            string response;
            string title;
            string summary;

            //Creating a new xml document
            XmlDocument doc = new XmlDocument();

            //Logging in Gmail server to get data
            objClient.Credentials = new System.Net.NetworkCredential("christianjorge2009@gmail.com", "Tiocris1");
            //reading data and converting to string
            response = Encoding.UTF8.GetString(
                       objClient.DownloadData(@"https://mail.google.com/mail/feed/atom"));

            response = response.Replace(
                 @"<feed version=""0.3"" xmlns=""http://purl.org/atom/ns#"">", @"<feed>");

            //loading into an XML so we can get information easily
            doc.LoadXml(response);

            //nr of emails
            var nr = doc.SelectSingleNode(@"/feed/fullcount").InnerText;

            //Reading the title and the summary for every email
            foreach (XmlNode node in doc.SelectNodes(@"/feed/entry"))
            {
                title = node.SelectSingleNode("title").InnerText;
                summary = node.SelectSingleNode("summary").InnerText;
            }
        }
    }
}
