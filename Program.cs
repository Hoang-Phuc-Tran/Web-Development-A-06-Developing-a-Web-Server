/*  
*  FILE          : Program.cs 
*  PROJECT       : PROG2001 - Assignment #6
*  PROGRAMMER    : BUMSU YI, Hoang phuc tran
*  FIRST VERSION : 2022-11-23 
*  DESCRIPTION   : this is to create the local host server that is capable of retriving data of various file extention for one client
*    
*/

using LoggingLibrary;
using System;
using System.Net.Sockets;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace Assignment6
{
    /*  NAME        : Program
     *  DESCRIPTION : This is to get cmd arguments and generate a locally hosted webserver to retrive info from clients
     */
    class Program
    {
        //declaring all the file type and mimetype
        private static readonly string[] jpeg = { "jpeg", "image/jpeg" };
        private static readonly string[] jpg = { "jpg", "image/jpeg" };
        private static readonly string[] jpe = { "jpe", "image/jpeg" };
        private static readonly string[] jfif = { "jfif", "image/jpeg" };
        private static readonly string[] log = { "log", "text/plain" };
        private static readonly string[] gif = { "gif", "image/gif" };
        private static readonly string[] jfif_tbnl = { "jfif-tbnl", "image/jpeg" };
        private static readonly string[] html = { "html", "text/html" };
        private static readonly string[] htm = { "htm", "text/html" };
        private static readonly string[] htmls = { "htmls", "text/html" };
        private static readonly string[] htx = { "htx", "text/html" };
        private static readonly string[] shtml = { "shtml", "text/html" };
        private static readonly string[] txt = { "txt", "text/plain" };

        private const int dataSize = 8192;
        private static readonly string curDate = string.Format("Current date: {0}\n", DateTime.Now.ToString("r"));

        static Dictionary<string, string> mimeDic = new Dictionary<string, string>{
            { jpeg[0], jpeg[1] },
            { jpg[0], jpg[1] },
            { jpe[0], jpe[1] },
            { jfif[0], jfif[1] },
            { shtml[0], shtml[1] },
            { txt[0], txt[1] },
            { log[0], log[1] },
            { gif[0], gif[1] },
            { jfif_tbnl[0], jfif_tbnl[1] },
            { html[0], html[1] },
            { htm[0], htm[1] },
            { htmls[0], htmls[1] },
            { htx[0], htx[1] },
        };

        static void Main(string[] args)
        {
            string webRoot = null;
            string webIpStr = null;
            string webPortStr = null;
            IPAddress webIP = null;
            int webPort = 0;

            if (args.Length == 3)   // check if three arguments are input
            {
                if (args[0].Substring(0, 8) == "-webRoot" && args[0].Length >= 11)
                {
                    webRoot = args[0].Substring(9);
                    if (args[1].Substring(0, 6) == "-webIP" && args[1].Length >= 9)
                    {
                        webIpStr = args[1].Substring(7);
                        if (args[2].Substring(0, 8) == "-webPort" && args[2].Length >= 11)
                        {
                            webPortStr = args[2].Substring(9);
                        }
                        else
                        {
                            Log.LogToFile(string.Format("the command line argument you have provided {0} is invalid!. please input the valid argument!: ", args[2]));
                        }
                    }
                    else
                    {
                        Log.LogToFile(string.Format("the command line argument you have provided {0} is invalid!. please input the valid argument!: ", args[1]));
                    }
                }
                else
                {
                    Log.LogToFile(string.Format("the command line argument you have provided {0} is invalid!. please input the valid argument!: ", args[0]));
                }


                if (webRoot != null && webIpStr != null && webPortStr != null && IPAddress.TryParse(webIpStr, out webIP) && Int32.TryParse(webPortStr, out webPort))
                {
                    StartServer(webRoot, webIP, webPort);
                }
                else
                {
                    Log.LogToFile("error!! invalid command like argument is provided");
                }
            }
            else
            {
                Log.LogToFile(string.Format("wrong arugment given please provide the appropriate arugments"));
            }
        }

        /*  FUNCTION        : StartServer
         *  DESCRIPTION     : This method is to start the server with given ip, port, path
         *  PARAMETERS      :
         *      string webRoot      : the root for the local host to run the server on
         *      IPAddress webIP     : the locally hosted ip
         *      int webPort         : The port in which to host the server.
         *  RETURNS         :
         *      byte[]  : page's body to send after the header.
         */
        private static void StartServer(string webRoot, IPAddress webIP, int webPort)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(webIP, webPort);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(ipEndPoint);
                socket.Listen(1);
                socket = socket.Accept();
            }
            catch (SocketException exception)
            {
                Log.LogToFile(string.Format("failed to connect, exception: {0}", exception.Message));
                return;
            }

            int receiver = 0;
            byte[] data = new byte[dataSize];
            while (true)
            {
                if ((receiver = socket.Receive(data)) > 0)
                {
                    string stringData = Encoding.ASCII.GetString(data, 0, receiver);

                    Log.LogToFile(stringData.Trim('\r', '\n') + "\r\n");

                    string header = "";
                    byte[] body = new byte[1];

                    try
                    {
                        string nameOfFile = stringData.Split('/')[1].Split(' ')[0].ToLower();
                        body = Get404ErrorPage(out header);

                        if (nameOfFile.Contains("."))
                        {
                            string extensionOfFile = nameOfFile.Split('.')[1];
                            if (mimeDic.ContainsKey(extensionOfFile))
                            {
                                body = GetPage(webRoot, nameOfFile, mimeDic[extensionOfFile], out header);
                            }
                        }

                        Log.LogToFile(string.Format("Resources requested: {0}", nameOfFile));
                    }
                    catch (Exception exception)
                    {
                        body = Get500ErrorPage(out header);
                        Log.LogToFile(string.Format("500 error!!, exception: {0}", exception.Message));
                    }

                    // logging after trimming for the log file
                    Log.LogToFile(header.Trim('\r', '\n') + "\r\n");

                    socket.Send(Encoding.ASCII.GetBytes(header));
                    socket.Send(body);
                }
            }
        }

        /*  FUNCTION        : Get404ErrorPage
         *  DESCRIPTION     : This method is to make 404 non found error page to send to client.
         *  PARAMETERS      : out string header   : the external header to send to client
         *  RETURNS         :
         *      byte[]  : The body of the page to be sent after the header of the page.
         */
        private static byte[] Get404ErrorPage(out string header)
        {
            string error404NotFound = "<html><body><h1>Error 404 not found please check the url</h1</html>";

            header = "HTTP/1.1 404 PAGE NOT FOUND ERROR!\r+\n" 
                + curDate
                + string.Format("Content-Length: {0}\r\n", Encoding.ASCII.GetByteCount(error404NotFound))
                + "Content-Type: text/html\r\n\r\n";
            return Encoding.ASCII.GetBytes(error404NotFound);
        }

        /*  FUNCTION        : GetError500Page
         *  DESCRIPTION     : This is to make 500 error page to send to client.
         *  PARAMETERS      : out string header   : the external header to send to client      
         *  RETURNS         : byte[]  : The bdy of the page to be sent after the header of the page.
         */
        private static byte[] Get500ErrorPage(out string header)
        {
            string error500Page = "<html><body><h1>500: Server Error!!, internal error in the server has occured.</body></html>";

            header = "HTTP/1.1 500 Server Error\r\n"
            + curDate  // the current date string
            + "Content-Type: text/html\r\n"
            + string.Format("Content-Length: {0}\r\n", Encoding.ASCII.GetByteCount(error500Page));
             

            return Encoding.ASCII.GetBytes(error500Page);
        }

        /*  FUNCTION        : GetPage
         *  DESCRIPTION     : This is to retrieve the givin resources, and makes 200 error code header to display correct file to client
         *  PARAMETERS      :
         *      string webRoot      : the root for the local host to run the server on
         *      nameOfFile          : name of the file being sent to the client
         *      typeOfContent       : type of content which is used.
         *      out string header   : the external header to send to client 
         *      RETURNS             : byte[]  : body of web.  
         */
        private static byte[] GetPage(string webRoot, string nameOfFile, string typeOfContent, out string header)
        {
            string pathOfFile = string.Format("{0}/{1}", webRoot, nameOfFile);  // build the file path
            if (File.Exists(pathOfFile))
            {
                byte[] resource = File.ReadAllBytes(pathOfFile);  // get the file from the path in a byte array to send

                header = "HTTP/1.1 200 OK\r\n"
                + curDate
                + string.Format("Content-Type: {0}\r\n", typeOfContent)
                + string.Format("Content-Length: {0}\r\n\r\n", resource.LongLength);  
                       
                return resource;
            }
            else
            {
                return Get404ErrorPage(out header);
            }
        }
    }
}