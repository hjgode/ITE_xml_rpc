using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Nwc.XmlRpc;

namespace SampleServer
{
    public partial class SampeServerFrm : Form
    {
        class SampleServer
        {
            const int PORT = 5050;

            /// <summary><c>LoggerDelegate</c> compliant method that does logging to Console.
            /// This method filters out the <c>LogLevel.Information</c> chatter.</summary>
            static public void WriteEntry(String msg, LogLevel level)
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

                XmlRpcServer server = new XmlRpcServer(PORT);
                server.Add("sample", new SampleServer());
                Console.WriteLine("Web Server Running on port {0} ... Press ^C to Stop...", PORT);
                server.Start();
            }

            public static void Stop()
            {
            }

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
        }
        public SampeServerFrm()
        {
            InitializeComponent();
            SampleServer.Start();
        }
    }
}