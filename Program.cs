using System;
using System.Text;
using System.Net;
using System.Linq;
using System.IO;
using System.Threading;
using Memory;

namespace Locator
{
    class Program
    {
        static string userpath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        static string uuid = "";
        static string player = getPlayerName();
        static string domain = "haxsys.net:1337";

        static int Main(string[] args)
        {
            // check for debug data.
            bool debug = false;
            if (args.Length > 0 )
            {
                if (args[0] == "-d")
                {
                    Console.WriteLine("Debug Mode!\n");
                    debug = true;
                }
            }

            string url;
            // Check for configuration file.
            if (File.Exists(userpath + "\\AppData\\armaTracker.conf"))
            {
                StreamReader reader = new StreamReader(userpath + "\\AppData\\armaTracker.conf");
                uuid = reader.ReadLine();
                reader.Close();
            }
            else
            {
                url = "http://" + domain + "/id?name=" + player;
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                if (debug)
                {
                    Console.WriteLine(url);
                }

                try
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    StreamReader objReader = new StreamReader(httpResponse.GetResponseStream());
                    uuid = objReader.ReadLine();
                }
                catch (WebException we)
                {
                    Console.WriteLine("Web Error: " + ((HttpWebResponse)we.Response).StatusCode);
                    return 1;
                }
                
                if (debug)
                {
                    Console.WriteLine(uuid);
                }

                StreamWriter writer = new StreamWriter(userpath + "\\AppData\\armaTracker.conf");
                writer.WriteLine(uuid);
            }


            // This might change with patches
            // 0x0DE20A8] +0x13A4] +0x4] +0x18] +0x28
            int basePtr = 0xDE1FF8;
            int workingPtr;
            int len = 4;
            int errorCount = 0;
            double x, y, z;
            byte[] mem4 = new byte[len];
            byte[] memLarge = new byte[12];

            Console.WriteLine("UUID: " + uuid);

            // init the memory reader.
            MemoryTool memObj = new MemoryTool("arma2oa");
            Console.WriteLine("Everything is working, start reading memory");

            while (true)
            {
                // Grab the proper pointer depth
                // TODO: move this into it's own function
                try
                {
                    mem4 = memObj.read(basePtr, len);
                    workingPtr = BitConverter.ToInt32(mem4, 0);
                    workingPtr += 0x13A4;

                    mem4 = memObj.read(workingPtr, len);
                    workingPtr = BitConverter.ToInt32(mem4, 0);
                    workingPtr += 0x4;

                    mem4 = memObj.read(workingPtr, len);
                    workingPtr = BitConverter.ToInt32(mem4, 0);
                    workingPtr += 0x18;

                    mem4 = memObj.read(workingPtr, len);
                    workingPtr = BitConverter.ToInt32(mem4, 0);
                    workingPtr += 0x28;

                    memLarge = memObj.read(workingPtr, 12);
                }
                catch (System.Exception)
                {
                    // Make sure memory read errors are caught but just dont send the NULLs.
                    // Also include a max number of times it can error in a row. 
                    if (errorCount > 5)
                    {
                        return 1;
                    }
                    errorCount++;
                    continue;
                }
                errorCount = 0;

                // Have to do a few tricks for it to work with Chicken's map
                x = System.BitConverter.ToSingle(memLarge, 0) * 0.001;
                z = System.BitConverter.ToSingle(memLarge, 4) * 0.001;
                y = 14.524823 - (System.BitConverter.ToSingle(memLarge, 8) * 0.001) + 0.85;
                
                // Not sure why C# hates me so much but recreateing the object seems to be
                url = String.Format("http://{0}/track?lat={1}&lng={2}&name={3}&id={4}", domain, y.ToString(), x.ToString(), player, uuid);

                if (debug)
                {
                    Console.WriteLine(url);
                }

                try
                {
                    HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(url);
                    HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    httpResponse.Close();
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.Message);
                }

                Thread.Sleep(5000);
            }
        }

        static string getPlayerName()
        {
            // Look in C:\users\USERNAME\Documents\ArmA 2\ for a file that ends in *.ARMA2PROFILE.
            string name = "";

            // Get current user
            string[] filePaths = Directory.GetFiles(userpath + "\\Documents\\ArmA 2\\");

            foreach (string path in filePaths)
            {
                if (path.IndexOf(".ArmA2OAProfile") != -1)
                {
                    // grab the username out of the filename.
                    name = path.Split('.')[0];
                    name = name.Split('\\').Last();
                }
            }
            return name;
        }

        public static string hexlify(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        
    }
}
