using System;

using System.Collections.Generic;
using System.Text;

using Nwc.XmlRpc;
using System.Collections;

namespace ITExmlrpc
{
    public class ITExmlrpcServer:IDisposable
    {
        XmlRpcServer server = null;
        XmlRpcClient client = null;
        string sHost = "127.0.0.1";

        public ITExmlrpcServer(string UrlITE)
        {
            client = new XmlRpcClient(UrlITE);
        }
        
        ITExmlrpcServer()
        {
        }

        public void Dispose()
        {
            if (server != null)
            {
                server.Stop();                
                server = null;
            }
        }

        public object registerScreenContentsCallback(string sHost, int iPort)
        {
            if (server == null)
            {
                this.Start(iPort);
            }

            return client.clientSend("ITC.registerScreenContentsCallback", new object[] { "ITC.GetScreenContents", sHost, iPort });
        }

        public object stopScreenContentsResponse()
        {
            if (server != null)
                server.Stop();
            return client.clientSend("ITC.stopScreenContentsResponse", new object[] { "" });
        }


        /// <summary><c>LoggerDelegate</c> compliant method that does logging to Console.
        /// This method filters out the <c>LogLevel.Information</c> chatter.</summary>
        static void WriteEntry(String msg, LogLevel level)
        {
            if (level > LogLevel.Information) // ignore debug msgs
                System.Diagnostics.Debug.WriteLine(String.Format("{0}: {1}", level, msg));
        }

        /// <summary>The application starts here.</summary>
        /// <remarks>This method instantiates an <c>XmlRpcServer</c> as an embedded XML-RPC server,
        /// then add this object to the server as an XML-RPC handler, and finally starts the server.</remarks>
        public void Start(int PORT)
        {
            // Use the console logger above.
            Logger.Delegate = new Logger.LoggerDelegate(WriteEntry);

            if (server == null)
            {
                server = new XmlRpcServer(PORT);
                server.Add("ITC", new ITExmlrpcServer());
                System.Diagnostics.Debug.WriteLine(string.Format("Web Server Running on port {0} ...", PORT));
                server.Start();
            }
        }

        public object GetScreenContents(IList parameters)
        {
            clrScreen();
            int iCnt = parameters.Count;
            System.Diagnostics.Debug.WriteLine("\n#########################\n" + iCnt.ToString() + "\n#########################\n");
            Object[] args = new Object[parameters.Count];
            int col = 0, row = 0;
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
                    //screen[row] = screen[row].Insert(col, field);
                    char[] screenRow = screen[row].ToCharArray();
                    //will the field fit?
                    if (field.Length + col > maxCol)
                        field.Substring(0, maxCol - field.Length);
                    //copy field into screen var
                    field.CopyTo(0, screenRow, col, field.Length);
                    screen[row] = new string(screenRow);
                    if (row > maxRow)
                        maxRow = row;
                }
                catch (Exception) { }
            }
            dumpScreen();
            return "OK";// new XmlRpcResponse(0, "OK");
        }
        #region screen
        string[] screen = new string[128];
        int maxRow = 0;
        int maxCol = 128;
        void clrScreen()
        {
            screen = new string[128];
            for (int x = 0; x < 128; x++)
                screen[x] = " ".PadRight(128, ' ');
        }
        void dumpScreen()
        {
            for (int i = 0; i < maxRow; i++)
                System.Diagnostics.Debug.WriteLine(screen[i]);
            this.onUpdateHandler(new MyEventArgs(screen));

        }
        #endregion

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


        public void Stop()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }
        }

        #region DelegateEvent

        public delegate void updateEventHandler(object sender, MyEventArgs eventArgs);
        public updateEventHandler updateEvent;
        protected virtual void onUpdateHandler(MyEventArgs args)
        {
            //anyone listening?
            updateEventHandler handler = updateEvent;
            MyEventArgs a = args;
            if (handler == null)
            {
                System.Diagnostics.Debug.WriteLine("onUpdateHandler: no subscription");
                return;
            }
            handler(this, a);
        }
        #endregion
    }
    public class MyEventArgs : EventArgs
    {
        //fields
        public string[] msg { get; set; }
        public MyEventArgs(string[] s)
        {
            msg = s;
        }
    }
}
