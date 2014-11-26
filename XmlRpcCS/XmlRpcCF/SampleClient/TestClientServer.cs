using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.Collections;
using System.Diagnostics;

namespace SampleClient
{
    public partial class TestClientServer : Form
    {
        private static String URL = "http://169.254.2.1:50023";//"http://199.64.70.96:50023";// "http://169.254.2.1:50023";// "http://192.168.0.44:50023";
        static string URLhost = "169.254.2.2";
        static int PortHost = 12345;
        ITExmlrpc.XmlRpcClient ite = null;
        ITExmlrpc.ITExmlrpcServer server = null;

        // localhost fails for unknown reason
        // 169.254.2.2 (ActiveSync Host)
        // 169.254.2.1 (ActiveSync Device)

        public void WriteEntry(String msg)
        {
            System.Diagnostics.Debug.WriteLine(String.Format("{0}: {1}", "info: " , msg));
            addLog(String.Format("{0}: {1}", "info: ", msg));
        }


        delegate void SetTextCallback(string text);
        public void addLog(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.txtLog.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(addLog);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                if (txtLog.Text.Length > 20000)
                    txtLog.Text = "";
                txtLog.Text += text + "\r\n";
                txtLog.SelectionLength = 0;
                txtLog.SelectionStart = txtLog.Text.Length - 1;
                txtLog.ScrollToCaret();
            }
        }

        public TestClientServer()
        {
            InitializeComponent();

            //Test();
            //this.TopMost = true;
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            this.TopMost = false;
            if(server!=null)
                server.Dispose();
            if(ite!=null)
                ite.Dispose();
        }

        //start web browser
        private void btnStart_Click(object sender, EventArgs e)
        {
            startITEclient();
            
            addLog("doWebBrowser..."); Application.DoEvents();
            ite.doWebBrowserAsync("google.com");    //async call

            //object oRes = null;
            //oRes = ite.doWebBrowser("google.com");    //synced blocking call
            //addLog("DONE: " + getString(oRes)); Application.DoEvents();
        }

        //test sendKeys
        private void btnTest_Click(object sender, EventArgs e)
        {
            startITEclient();

            addLog("sendKeys..."); Application.DoEvents();
            ite.sendKeysAsync(new ITExmlrpc.XmlRpcClient.keyStruct('1'));
            addLog("Done"); Application.DoEvents();
            //addLog("DONE: " + getString(oRes)); Application.DoEvents();
        }

        //stop web browser
        private void btnStop_Click(object sender, EventArgs e)
        {
            object oRes = null;
            startITEclient();

            addLog("Close Web browser..."); Application.DoEvents();
            oRes = ite.closeWebBrowser();
            addLog(oRes.ToString());
            addLog("DONE: " + getString(oRes)); Application.DoEvents();
        }

        private void btnClass_Click(object sender, EventArgs e)
        {

            startITEclient();

            //start server
            addLog("Create server object..."); Application.DoEvents();
            if(server==null)
                server = new ITExmlrpc.ITExmlrpcServer(URL);

            ITExmlrpc.ITExmlrpcServer.updateEvent = server_updateEvent;
            //server.updateEvent = server_updateEvent;

            //async mode
            addLog("register callback in assync mode..."); Application.DoEvents();
            server.registerScreenContentsCallbackAsync(URLhost, PortHost);

            //synced, possibly blocking mode
            //object oRes = null;
            //addLog("register callback in synced mode..."); Application.DoEvents();
            //oRes = server.registerScreenContentsCallback(URLhost, PortHost);
            //addLog("DONE: " +getString(oRes)); Application.DoEvents();
        }

        void server_updateEvent(object sender, ITExmlrpc.MyEventArgs eventArgs)
        {
            for (int i = 0; i < eventArgs.msg.Length; i++)
            {
                addLog(eventArgs.msg[i]);
                Application.DoEvents();
            }
        }

        private void btnClassStop_Click(object sender, EventArgs e)
        {
            addLog("stop getScreenResponse..."); Application.DoEvents();
            startITEclient();

            //synced, possibly blocking mode
            //object oRes = server.stopScreenContentsResponse();
            //addLog("DONE: " + getString(oRes)); Application.DoEvents();

            //asynced call to stop request
            server.stopScreenContentsResponseAsync();
            addLog("DONE: async mode");

            //dispose objects
            addLog("Disposing objects..."); Application.DoEvents();
            if(server!=null)
                server.Dispose();
            ite.Dispose();
            addLog("ALL DONE"); Application.DoEvents();
        }

        string getString(object o)
        {
            string s = "n/a";
            try
            {
                s = (String)o;
            }
            catch (Exception)
            {
            }
            return s;
        }

        void startITEclient()
        {
            if (ite == null)
            {
                addLog("Create new client..."); Application.DoEvents();
                ite = new ITExmlrpc.XmlRpcClient(URL);
                ite.OnDone += new EventHandler<ITExmlrpc.XmlRpcClient.WorkerEventArgs>(ite_OnDone);
            }
        }

        void ite_OnDone(object sender, ITExmlrpc.XmlRpcClient.WorkerEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("ite.onDone: " + e.m_methodName + "\n" + e.m_object.ToString());
            addLog(getString(e.m_methodName + "/" + e.m_object));
        }
    }
}