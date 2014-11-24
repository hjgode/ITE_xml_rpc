using System;

using System.Collections;

using System.Collections.Generic;
using System.Text;

  using Nwc.XmlRpc;

namespace SampleClient
{
    class SampleServer
    {
        const int PORT = 12345;
        static XmlRpcServer server=null;

        /// <summary><c>LoggerDelegate</c> compliant method that does logging to Console.
        /// This method filters out the <c>LogLevel.Information</c> chatter.</summary>
        public static void WriteEntry(String msg, LogLevel level)
        {
	        if (level > LogLevel.Information) // ignore debug msgs
	            System.Diagnostics.Debug.WriteLine(String.Format("{0}: {1}", level, msg));
        }

        /// <summary>The application starts here.</summary>
        /// <remarks>This method instantiates an <c>XmlRpcServer</c> as an embedded XML-RPC server,
        /// then add this object to the server as an XML-RPC handler, and finally starts the server.</remarks>
        public static void Start() 
        {
	        // Use the console logger above.
	        Logger.Delegate = new Logger.LoggerDelegate(WriteEntry);

            if (server == null)
            {
                server = new XmlRpcServer(PORT);
                server.Add("ITC", new SampleServer());
                System.Diagnostics.Debug.WriteLine(string.Format("Web Server Running on port {0} ... Press ^C to Stop...", PORT));
                server.Start();
            }
        }

        //public object GetScreenContents(IList args)
        //{
        //    int iCnt = args.Count;
        //    System.Diagnostics.Debug.WriteLine("\n#########################\n" + iCnt.ToString() + "\n#########################\n");
        //    return "OK";// new XmlRpcResponse(0, "OK");
        //}

        string[] screen = new string[128];
        int maxRow = 0;
        void clrScreen()
        {
            screen = new string[128];
            for (int x = 0; x < 128; x++)
                screen[x] = "                                                                                                 ";
        }
        void dumpScreen()
        {
            for (int i = 0; i < maxRow; i++)
                System.Diagnostics.Debug.WriteLine(screen[i]);
        }
        public object GetScreenContents(IList parameters)
        {
            clrScreen();
            int iCnt = parameters.Count;
            System.Diagnostics.Debug.WriteLine("\n#########################\n" + iCnt.ToString() + "\n#########################\n");
            Object[] args = new Object[parameters.Count];
            int col=0, row=0;
            string field = "";
            foreach (Object arg in parameters)
            {
                try
                {
                    Hashtable ht = (Hashtable)arg;
                    foreach (DictionaryEntry de in ht)
                    {
                        if (de.Key.ToString() == "Column")
                            col = int.Parse(de.Value.ToString());
                        if (de.Key.ToString() == "Row")
                            row = int.Parse(de.Value.ToString());

                        if (de.Key.ToString() == "Field")
                            if (de.Value != null)
                                field = de.Value.ToString();
                            else
                                field = "";
                        //System.Diagnostics.Debug.WriteLine(de.Key.ToString() + "->" + de.Value.ToString());
                    }
                    screen[row] = screen[row].Insert(col, field);
                    if (row > maxRow)
                        maxRow = row;
                }
                catch (Exception) { }
            }
            dumpScreen();
            return "OK";// new XmlRpcResponse(0, "OK");
        }

        //will be called only if 13 args
        //public object GetScreenContents(
        //    Object args1, 
        //    Object args2,
        //    Object args3, 
        //    Object args4,
        //    Object args5, 
        //    Object args6,
        //    Object args7, 
        //    Object args8,
        //    Object args9, 
        //    Object args10,
        //    Object args11, 
        //    Object args12,
        //    Object args13
        //    )
        //{
        //    int iCnt = 13;
        //    System.Diagnostics.Debug.WriteLine("\n#########################\n" + iCnt.ToString() + "\n#########################\n");
        //    return "OK";// new XmlRpcResponse(0, "OK");
        //}

        /// <summary>A method that returns the current time.</summary>
        /// <return>The current <c>DateTime</c> of the server is returned.</return>
        public DateTime Ping()
        {
	        return DateTime.Now;
        }

        /// <summary>A method that echos back it's arguement.</summary>
        /// <param name="arg">A <c>String</c> to echo back to the caller.</param>
        /// <return>Return, as a <c>String</c>, the <paramref>arg</paramref> that was passed in.</return>
        public String Echo(String arg)
        {
	        return arg;
        }
        public static void Stop()
        {
            if (server != null)
                server.Stop();
        }
    }
}
