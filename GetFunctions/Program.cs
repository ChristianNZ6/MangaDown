using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetFunctions
{
   public class GetFunctions
    {
        static void Main(string[] args)
        {
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
    }

}
