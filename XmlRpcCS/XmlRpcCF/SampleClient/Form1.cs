using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Collections;
using System.Diagnostics;
using Nwc.XmlRpc;

namespace SampleClient
{
    public partial class Form1 : Form
    {
        private static String URL = "http://169.254.2.1:50023";// "http://192.168.0.44:50023";
        // localhost fails for unknown reason
        // 169.254.2.2 (ActiveSync Host)
        // 169.254.2.1 (ActiveSync Device)

        /// <summary><c>LoggerDelegate</c> compliant method that does logging to Console.
        /// This method filters out the <c>LogLevel.Information</c> chatter.</summary>
        static public void WriteEntry(String msg, LogLevel level)
        {
            if (level > LogLevel.Information) // ignore debug msgs
                System.Diagnostics.Debug.WriteLine(String.Format("{0}: {1}", level, msg));
        }
        /// <summary><c>LoggerDelegate</c> compliant method that does logging to Console.
        /// This method filters out the <c>LogLevel.Information</c> chatter.</summary>
        static public void WriteEntry(String msg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("{0}: {1}", "info: " , msg));
        }

        /// <summary>Main application method.</summary>
        /// <remarks> Simply sets up logging and then an <c>XmlRpcRequest</c> instance. Then
        /// Calls <c>sample.Ping</c>, <c>sample.Echo</c> and <c>sample.Broken</c> in sequence.
        /// The <c>XmlRpcResponse</c> from each call is then displayed. Faults are checked for.
        /// </remarks>
        public void Test()
        {
            WriteEntry("Server: " + URL);

            // Use the console logger above.
            Logger.Delegate = new Logger.LoggerDelegate(WriteEntry);

            // Send the sample.Ping RPC using the Send method which gives you a little more control...
            XmlRpcRequest client = new XmlRpcRequest();
            client.MethodName = "ITC.sendKeys"; //"TE2000.SendKeys"
            client.Params.Clear();
            client.Params.Add("1");
            try
            {
                WriteEntry("### Request: " + client);
                XmlRpcResponse response = client.Send(URL);
                WriteEntry("### Response: " + response);

                if (response.IsFault)
                {
                    WriteEntry(String.Format("Fault {0}: {1}", response.FaultCode, response.FaultString),LogLevel.Error);
                }
                else
                {
                    WriteEntry("### Returned: " + response.Value);
                }
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }

            // Invoke the sample.Echo RPC - Invoke more closely parallels a method invocation
            //client.MethodName = "sample.Echo";
            //client.Params.Clear();
            //client.Params.Add("Hello");
            client.MethodName = "ITC.sendKeys"; //"TE2000.SendKeys"
            client.Params.Clear();
            client.Params.Add(KeyStruct.getKeyStruct("1"));
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                String echo = (String)client.Invoke(URL);
                WriteEntry("### Returned: " + echo);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }


            // Invoke sample.Broken RPC - method that is not present on server.
            client.MethodName = "ITC.SendKeys";
            client.Params.Clear();
            myKeys = new KeysStruct[1];
            KeysStruct ks=new KeysStruct();
            ks.key="Alt"; ks.value="false";
            myKeys[0] = ks;
            client.Params.Add(myKeys);
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Invoke(URL);
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }

            //ITC.registerScreenContentsCallback
            client.MethodName = "ITC.registerScreenContentsCallback";
            client.Params.Clear();
            client.Params.Add("ITC.GetScreenContents");
            client.Params.Add("localhost");
            client.Params.Add(12345);
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Invoke(URL);
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }

            //ITC.stopScreenContentsResponse
            client.MethodName = "ITC.stopScreenContentsResponse";
            client.Params.Clear();
            client.Params.Add("");
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Invoke(URL);
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }

            //ITC.doWebBrowser
            client.MethodName = "ITC.doWebBrowser";
            client.Params.Clear();
            client.Params.Add("http://www.google.com");
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Invoke(URL);
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }

            System.Threading.Thread.Sleep(3000);

            //ITC.closeWebBrowser
            client.MethodName = "ITC.closeWebBrowser";
            client.Params.Clear();
            client.Params.Add("");
            try
            {
                WriteEntry("### Invoke: " + client.MethodName);
                Object response = client.Invoke(URL);
                WriteEntry("### Response: " + response);
            }
            catch (XmlRpcException serverException)
            {
                WriteEntry(String.Format("Fault {0}: {1}", serverException.FaultCode, serverException.FaultString));
            }
            catch (Exception e)
            {
                WriteEntry("Exception " + e + "\n" + e.StackTrace);
            }

            /*
            void SendKey ( int KeyValue) 
            {
                TCHAR aTCharString[2];
                char aCharString[2];
                XmlRpcValue charStruct,result;
                XmlRpcValue booleanFalse(false);
                XmlRpcValue booleanTrue(true);
                aTCharString[0] = KeyValue;
                aTCharString[1] = '\0';
                wcstombs(aCharString, aTCharString, 2);
                charStruct[0]["Alt"] = booleanFalse;
                charStruct[0]["Control"] = booleanFalse;
                charStruct[0]["Shift"] = booleanFalse;
                charStruct[0]["Special"] = booleanFalse;
                charStruct[0]["KeyValue"] = (int)aCharString[0];
                c->execute("ITC.sendKeys", charStruct, result);
            }

            ITC.doWebBrowser, string URL
            ITC.closeWebBrowser, ""
            */
        }

        struct KeysStruct
        {
            public string key;
            public object value;
        }

        KeysStruct[] myKeys;

        class KeyStruct{
            public static ArrayList getKeyStruct(String sKey)
            {
                ArrayList iList = new ArrayList();
                Dictionary<string, object> list = new Dictionary<string, object>();
                list.Add("Alt", "false");
                list.Add("Control", "false");
                list.Add("Shift", "false");
                list.Add("Special", "false");
                list.Add("KeyValue", Encoding.UTF8.GetBytes(sKey)[0]);

                iList.Add(list);
                return iList;
            }
        }

        public Form1()
        {
            InitializeComponent();
            Test();
        }
    }
}