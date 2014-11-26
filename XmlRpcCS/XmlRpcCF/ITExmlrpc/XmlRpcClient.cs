using System;

using System.Collections.Generic;
using System.Text;

using Nwc.XmlRpc;
using System.Threading;

namespace ITExmlrpc
{
    public class XmlRpcClient:IDisposable
    {
        XmlRpcRequest client = null;
        string URL = "http://169.254.2.1:50023";
        
        public ITExmlrpcServer server = null;

        public XmlRpcClient(string sUrl)
        {
            URL = sUrl;
            client = new XmlRpcRequest();
        }

        public void Dispose()
        {
            if (server != null)
            {
                server.Stop();
                server = null;
            }
        }

        public struct keyStruct
        {
            public bool Alt;
            public bool Control;
            public bool Shift;
            public bool Special;
            public char Key;
            public keyStruct(char c)
            {
                Alt = false;
                Control = false;
                Shift = false;
                Special = false;
                Key = c;
            }
        }

        public void sendKeysAsync(keyStruct key)
        {
            object oResponse = new object();
            clientSendAsync("ITC.sendKeys", new object[] { key });
        }

        public object sendKeys(keyStruct key)
        {
            object oResponse = new object();
            return clientSend("ITC.sendKeys", new object[]{key});

        }

        public void doWebBrowserAsync(string sUrl)
        {
            object[] args = new object[] { sUrl };
            clientSendAsync("ITC.doWebBrowser", args);
        }
        public void closeWebBrowserAsync()
        {
            clientSendAsync("ITC.closeWebBrowser", new object[] { "" });
        }

        public object doWebBrowser(string sUrl)
        {
            object[] args = new object[] { sUrl };
            return clientInvoke("ITC.doWebBrowser", args);
        }
        
        public object closeWebBrowser()
        {
            //return clientInvoke("ITC.closeWebBrowser", null);
            return clientSend("ITC.closeWebBrowser", new object[]{""});
        }


        internal object clientSend(String methodName, object[] args)
        {
            object oResponse = new object();
            client.MethodName = methodName;// "ITC.doWebBrowser";
            client.Params.Clear();
            if (args != null && args.Length > 0)
            {
                foreach (object o in args)
                    client.Params.Add(o);
            }
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Send(URL);
                oResponse = response;
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (System.Net.WebException ex)
            {
                WriteEntry("WebException");
            }
            catch (Exception e)
            {
                //WriteEntry("Exception " + e + "\n" + e.StackTrace);
                WriteEntry("Exception " + e);
            }

            return oResponse;
        }

        /// <summary>
        /// invoke the server method
        /// </summary>
        /// <param name="methodName">which method to invoke</param>
        /// <param name="args">which args for the server method</param>
        /// <returns>the server response</returns>
        object clientInvoke(String methodName, object[] args)
        {
            object oResponse = new object();
            client.MethodName = methodName;// "ITC.doWebBrowser";
            client.Params.Clear();
            if (args!=null && args.Length > 0)
            {
                foreach(object o in args)
                    client.Params.Add(o);
            }
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Invoke(URL);
                oResponse = response;
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (System.Net.WebException ex)
            {
                WriteEntry("WebException");
            }
            catch (Exception e)
            {
                //WriteEntry("Exception " + e + "\n" + e.StackTrace);
                WriteEntry("Exception " + e);
            }

            return oResponse;
        }

        /// <summary>
        /// simply write messages to debug out
        /// </summary>
        /// <param name="msg"></param>
        public void WriteEntry(String msg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("{0}", msg));
        }

        #region async calls
        public event EventHandler<WorkerEventArgs> OnDone;
        public class WorkerEventArgs : EventArgs
        {
            public object m_object;
            public string m_methodName;
            public WorkerEventArgs(object o, string sMethodName)
            {
                m_object = o;
                m_methodName = sMethodName;
            }
        }

        /// <summary>
        /// Wrapper object for worker ThreadStart delegate
        /// </summary>
        class ThreadStartDelegateWrapper
        {

            EventHandler<WorkerEventArgs> m_OnDone;

            /// <summary>
            /// Number of worker iterations
            /// </summary>
            private string m_Method;
            private object[] m_Args;
            private XmlRpcRequest m_client = null;
            private string m_URL = "127.0.0.1";

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="iterations>
            /// Number of worker iterations
            /// </param>
            public ThreadStartDelegateWrapper(ref EventHandler<WorkerEventArgs> OnDone, ref XmlRpcRequest client, string URL, string method, object[] args)
            {
                this.m_Method = method;
                this.m_Args = args;
                this.m_client = client;
                this.m_URL = URL;
                m_OnDone = OnDone;
            }

            void WriteEntry(string s)
            {
                System.Diagnostics.Debug.WriteLine(s);
            }

            /// <summary>
            /// Worker thread delegate
            /// </summary>
            public void Worker()
            {
                object oResponse = new object();
                m_client.MethodName = m_Method;// "ITC.doWebBrowser";
                m_client.Params.Clear();
                if (this.m_Args != null && this.m_Args.Length > 0)
                {
                    foreach (object o in m_Args)
                        m_client.Params.Add(o);
                }
                try
                {
                    WriteEntry("### Invoke: " + m_client.MethodName);
                    Object response = m_client.Send(m_URL);
                    oResponse = response;
                    WriteEntry("### Response: " + response);
                }
                catch (XmlRpcException serverException)
                {
                    WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
                }
                catch (System.Net.WebException ex)
                {
                    WriteEntry("WebException");
                }
                catch (Exception e)
                {
                    //WriteEntry("Exception " + e + "\n" + e.StackTrace);
                    WriteEntry("Exception " + e);
                }

                on_Completed(oResponse, m_Method);
            }

            void on_Completed(object o, string sMethod)
            {
                EventHandler<WorkerEventArgs> handler=m_OnDone;
                if (handler != null)
                    handler(this, new WorkerEventArgs(o, sMethod));
            }
        } // end of ThreadStartDelegateWrapper class
        /*
        // create the wrapper object, passing in the desired number of iterations
        ThreadStartDelegateWrapper wrapper = new ThreadStartDelegateWrapper(arguments);

        // create and start the thread
        ThreadStart ts = new ThreadStart(wrapper.Worker);
        Thread t = new Thread(ts);
        t.Start();
         * or
        // create and start the thread
        new Thread(new ThreadStart(new ThreadStartDelegateWrapper(arguments).Worker)).Start();
        */

        internal void clientSendAsync(String methodName, object[] args)
        {
            ThreadStartDelegateWrapper wrapper = new ThreadStartDelegateWrapper(ref this.OnDone, ref this.client, this.URL, methodName, args);
            ThreadStart ts = new ThreadStart(wrapper.Worker);
            //wrapper.OnDone += new EventHandler<ThreadStartDelegateWrapper.WorkerEventArgs>(wrapper_OnDone);
            Thread t = new Thread(ts);
            t.Start();
        }

        void wrapper_OnDone(object sender, WorkerEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.m_object.ToString());
        }
        #endregion

    }
}
