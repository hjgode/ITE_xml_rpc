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
        string sHostITE = "127.0.0.1";
        int iPort = 12345;

        /// <summary>
        /// init a new ITE server object
        /// </summary>
        /// <param name="UrlITE">the URL of the running ITE (http://host_or_ip:50023)</param>
        public ITExmlrpcServer(string UrlITE)
        {
            sHostITE = UrlITE;
            client = new XmlRpcClient(UrlITE);
        }
        
        ITExmlrpcServer()
        {
            if(client==null)
                client = new XmlRpcClient(sHostITE);
        }

        public void Dispose()
        {
            if (server != null)
            {
                server.Stop();                
                server = null;
            }
        }

        /// <summary>
        /// start the server listening on port x
        /// </summary>
        void startServer()
        {
            if (server == null)
            {
                this.Start(iPort);
            }
        }

        /// <summary>
        /// send ITE the method registerScreenContentsCallback
        /// </summary>
        /// <param name="sHost">which server hosts the callback method</param>
        /// <param name="Port">which port is to be used for the callback method</param>
        /// <returns>the result of the method request</returns>
        public object registerScreenContentsCallback(string sHost, int Port)
        {
            iPort = Port;
            startServer();
            return client.clientSend("ITC.registerScreenContentsCallback", new object[] { "ITC.GetScreenContents", sHost, iPort });
        }
        /// <summary>
        /// send ITE the method registerScreenContentsCallback in async mode
        /// </summary>
        /// <param name="sHost">which server hosts the callback method</param>
        /// <param name="Port">which port is to be used for the callback method</param>
        /// <returns>results are reported via an eventhandler</returns>
        public void registerScreenContentsCallbackAsync(string sHost, int Port)
        {
            iPort = Port;
            startServer();
            client.clientSendAsync("ITC.registerScreenContentsCallback", new object[] { "ITC.GetScreenContents", sHost, iPort });
        }

        /// <summary>
        /// call the ITE stopScreenContentsResponse method to stop ITE to callback
        /// </summary>
        /// <returns>the result of the ITE method call</returns>
        public object stopScreenContentsResponse()
        {
            if (server != null)
                server.Stop();
            return client.clientSend("ITC.stopScreenContentsResponse", new object[] { "" });
        }
        /// <summary>
        /// call the ITE stopScreenContentsResponse method to stop ITE to callback in async mode
        /// results are reported via an eventhandler
        /// </summary>
        public void stopScreenContentsResponseAsync()
        {
            if (server != null)
                server.Stop();
            client.clientSendAsync("ITC.stopScreenContentsResponse", new object[] { "" });
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

                server.Add("ITC", new ITExmlrpcServer()); //using that constructor we will get a new server and event subscription will not work
                //server.Add("ITC", this);
                
                System.Diagnostics.Debug.WriteLine(string.Format("Web Server Running on port {0} ...", PORT));
                server.Start();
            }
        }

        /// <summary>
        /// this is the local server method called by ITE on Screen updates
        /// </summary>
        /// <param name="parameters">a list of arguments</param>
        /// <returns>always OK to the client (ITE)</returns>
        public object GetScreenContents(IList parameters)
        {
            clrScreen();
            int iCnt = parameters.Count;
            System.Diagnostics.Debug.WriteLine("\n#########################\n" + iCnt.ToString() + "\n#########################\n");
            Object[] args = new Object[parameters.Count];
            int col = 0, row = 0;
            string field = "", attribute="";
            foreach (Object arg in parameters)
            {
                try
                {
                    Hashtable ht = (Hashtable)arg;
                    foreach (DictionaryEntry de in ht)
                    {
                        if (de.Key != null)
                        {
                            if (de.Key.ToString() == "Attribute")
                                if (de.Value != null)
                                    attribute = de.Value.ToString();
                                else
                                    attribute = "";
                            if (de.Key.ToString() == "Column")
                                if (de.Value != null)
                                    col = int.Parse(de.Value.ToString());
                            if (de.Key.ToString() == "Row")
                                if (de.Value != null)
                                    row = int.Parse(de.Value.ToString());

                            if (de.Key.ToString() == "Field")
                                if (de.Value != null)
                                    field = de.Value.ToString();
                                else
                                    field = "";

                            /*
                             * 32=bold
                             *  2=underline
                             *  8=blink
                             *  4=reverse
                             *  0=normal
                            */
                            //if(de.Value!=null)
                            //    System.Diagnostics.Debug.WriteLine(de.Key.ToString() + "->" + de.Value.ToString());
                            //else
                            //    System.Diagnostics.Debug.WriteLine(de.Key.ToString() + "->" + "null");
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //screen[row] = screen[row].Insert(col, field);
                    char[] screenRow = screen[row].ToCharArray();
                    //will the field fit?
                    if (field.Length + col > screenRow.Length)
                        field = field.Substring(0, screenRow.Length - field.Length);
                    //copy field into screen var
                    field.CopyTo(0, screenRow, col, field.Length);
                    screen[row] = new string(screenRow);
                    if (row > maxRow)
                    {
                        maxRow = row;
                        System.Diagnostics.Debug.WriteLine("new MaxRow=" + maxRow.ToString());
                    }
                    if (col + field.Length > maxCol)
                    {
                        maxCol = col + field.Length;
                        System.Diagnostics.Debug.WriteLine("new MaxCol=" + maxCol.ToString());
                    }
                }
                catch (Exception) { }
            }
            dumpScreen();
            return "OK";// new XmlRpcResponse(0, "OK");
        }

        #region screen
        string[] screen = new string[128];
        int maxRow = 0;
        int maxCol = 0;
        const int DefColCount = 60;
        void clrScreen()
        {
            screen = new string[DefColCount];
            for (int x = 0; x < DefColCount; x++)
                screen[x] = " ".PadRight(DefColCount, ' ');
            maxRow = 0;
            maxCol = 0;
        }
        void dumpScreen()
        {
            string[] dump = new string[maxRow];
            for (int i = 0; i < maxRow; i++)
            {
                dump[i] = screen[i].TrimEnd(new char[] { ' ' });
                System.Diagnostics.Debug.WriteLine("'" + dump[i] + "'");
            }
            this.onUpdateHandler(new MyEventArgs(dump));

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

        /// <summary>
        /// stop the server
        /// </summary>
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
        public static updateEventHandler updateEvent; //static works as a new server is created for a call
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
