using System;

using System.Collections.Generic;
using System.Text;

using Nwc.XmlRpc;

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
        /// <summary>
        /// currently not working
        /// </summary>
        /// <param name="o"></param>
        public object sendKeys(keyStruct key)
        {
            object oResponse = new object();
            return clientSend("ITC.sendKeys", new object[]{key});

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

        /// <summary><c>LoggerDelegate</c> compliant method that does logging to Console.
        /// This method filters out the <c>LogLevel.Information</c> chatter.</summary>
        public void WriteEntry(String msg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("{0}", msg));
        }
    }
}
