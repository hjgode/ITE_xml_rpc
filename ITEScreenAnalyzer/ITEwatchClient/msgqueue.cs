using System;

using System.Collections.Generic;
using System.Text;

using System.Runtime.InteropServices;
using System.Threading;

using DWORD = System.UInt32;
using BOOL = System.Boolean;
using BYTE = System.Byte;
using HANDLE = System.IntPtr;
using USHORT = System.UInt16;
using UCHAR = System.Byte;

namespace ITEwatchClient
{
    class msgqueue:IDisposable
    {
        #region NativeStuff
        [DllImport("coredll.dll", SetLastError = true)]
        static extern UInt32 WaitForSingleObject(int hHandle, UInt32 dwMilliseconds);

        const UInt32 WAIT_INFINITE = 0xFFFFFFFF;
        enum Wait_Object
        {
            WAIT_ABANDONED = 0x00000080,
            WAIT_OBJECT_0 = 0x00000000,
            WAIT_TIMEOUT = 0x00000102,
        }

        [DllImport("coredll.dll")]
        static extern int CreateMsgQueue(string szName, ref MSGQUEUEOPTIONS pOptions);
        [DllImport("coredll.dll")]
        static extern IntPtr CreateMsgQueue(IntPtr hString, ref MSGQUEUEOPTIONS pOptions);

        [DllImport("coredll.dll", SetLastError = true)]
        internal static extern bool ReadMsgQueue(int hMsgQ, byte[] lpBuffer, int cbBufferSize, out int lpNumberOfBytesRead, int dwTimeout, out int pdwFlags);

        [DllImport("coredll.dll")]
        static extern BOOL CloseMsgQueue(int h);

        [DllImport("coredll")]
        static extern bool CloseHandle(IntPtr h);

        [StructLayout(LayoutKind.Sequential)]
        struct MSGQUEUEOPTIONS
        {
            public DWORD dwSize;
            public DWORD dwFlags;
            public DWORD dwMaxMessages;
            public DWORD cbMaxMessage;
            [MarshalAs(UnmanagedType.Bool)]
            public BOOL bReadAccess;
            //MSGQUEUEOPTIONS()
            //{
            //    dwSize = Marshal.SizeOf(MSGQUEUEOPTIONS);
            //    dwFlags = 0;
            //    dwMaxMessages = 10;
            //    cbMaxMessage = 0;
            //    bReadAccess = true;
            //}
        }
        // WINBASE.h header constants
        private const int MSGQUEUE_NOPRECOMMIT = 1;
        private const int MSGQUEUE_ALLOW_BROKEN = 2;

        // MSGQUEUEOPTIONS constants
        private const bool ACCESS_READWRITE = false;
        private const bool ACCESS_READONLY = true;
        #endregion

        System.Threading.Thread msgThread = null;
        bool bRunThread = true;

        class ITE_MESSAGE
        {
            byte[] m_data;

            public ITE_MESSAGE(int size)
            {
                m_size = size;
                m_data = new byte[size];
            }
            internal int m_size=0;
            public int size
            {
                get { return m_size; }
            }
//            [MarshalAs(UnmanagedType.LPTStr)] //if C/C++ is TCHAR*
            //public string msg;
            public byte[] msg
            {
                get { return m_data; }
                set { m_data = value; }
            }
            public override string ToString()
            {
                return Encoding.Unicode.GetString(msg, 0, msg.Length);
            }
        }
        const int ITE_MESSAGE_SIZE = 160;

        public msgqueue()
        {
            startThread();
        }

        void startThread()
        {
            bRunThread = true;
            msgThread = new Thread(new ThreadStart(MsgQueueThread));
            msgThread.Name = "btmon thread";
            msgThread.Start();
        }

        void stopThread()
        {
            bRunThread = false;
            Thread.Sleep(3000);
            if (msgThread != null)
            {
                msgThread.Abort();
            }
        }

        public void Dispose()
        {
            addLog(DateTime.Now.ToLongTimeString() + " " + "BTmon class Dispose()");
            stopThread();
        }

        void MsgQueueThread()
        {
            //only BTE_DISCONNECTION and BTE_CONNECTION change this state!
            addLog("thread about to start");
            int hMsgQueue = 0;
//            IntPtr hBTevent = IntPtr.Zero;
            // allocate space to store the received messages
            byte[] msgBuffer = new byte[160];// Marshal.AllocHGlobal(160);// ITE_MESSAGE_SIZE);

            ITE_MESSAGE ite_msg;
            try
            {
                //create msgQueueOptions
                MSGQUEUEOPTIONS msgQueueOptions = new MSGQUEUEOPTIONS();
                msgQueueOptions.dwSize = (DWORD)Marshal.SizeOf(msgQueueOptions);
                msgQueueOptions.dwFlags = 0;// MSGQUEUE_NOPRECOMMIT;
                msgQueueOptions.dwMaxMessages = 10;
                msgQueueOptions.cbMaxMessage = 160;// (DWORD)Marshal.SizeOf(ite_msg);
                msgQueueOptions.bReadAccess = ACCESS_READONLY;

                hMsgQueue = CreateMsgQueue("ITESCREENS", ref msgQueueOptions);
                addLog("CreateMsgQueue=" + Marshal.GetLastWin32Error().ToString()); //6 = InvalidHandle

                if (hMsgQueue == 0)
                {
                    addLog("Create MsgQueue failed");
                    throw new Exception("Create MsgQueue failed");
                }

                Wait_Object waitRes = 0;
                //create a msg queue
                while (bRunThread)
                {
                    // initialise values returned by ReadMsgQueue
                    int bytesRead = 0;
                    int msgProperties = 0;
                    //block until message
                    waitRes = (Wait_Object)WaitForSingleObject(hMsgQueue, 5000);
                    if ((int)waitRes == -1)
                    {
                        int iErr = Marshal.GetLastWin32Error();
                        addLog("error in WaitForSingleObject=" + iErr.ToString()); //6 = InvalidHandle
                        Thread.Sleep(1000);
                    }
                    switch (waitRes)
                    {
                        case Wait_Object.WAIT_OBJECT_0:
                            //signaled
                            //check event type and fire event
                            //ReadMsgQueue entry
                            ite_msg = new ITE_MESSAGE(160);
                            bool success = ReadMsgQueue(hMsgQueue,   // the open message queue
                                                        ite_msg.msg,// msgBuffer,        // buffer to store msg
                                                        ite_msg.size, //ITE_MESSAGE_SIZE,   // size of the buffer
                                                        out bytesRead,    // bytes stored in buffer
                                                        -1,         // wait forever
                                                        out msgProperties);
                            if (success)
                            {
                                // marshal the data read from the queue into a structure
                                //ite_msg = (ITE_MESSAGE)Marshal.PtrToStructure(msgBuffer, typeof(ITE_MESSAGE));
                                addLog("msgqueue read: " + ite_msg.ToString());//Encoding.Unicode.GetString(ite_msg.msg, 0, bytesRead));
                            }
                            else
                            {
                                addLog("ReadMsgQueue error: " + Marshal.GetLastWin32Error().ToString());
                                continue; //start a new while cirlce
                            }
                            //addLog("message received: " + ite_msg.ToString());
                            break;
                        case Wait_Object.WAIT_ABANDONED:
                            //wait has abandoned
                            addLog("msg queue thread: WAIT_ABANDONED");
                            break;
                        case Wait_Object.WAIT_TIMEOUT:
                            //timed out
                            addLog("msg queue thread: WAIT_TIMEOUT");
                            break;
                    }//WaitRes
                }//while bRunThread
            }
            catch (ThreadAbortException ex)
            {
                addLog("msg queue thread ThreadAbortException: " + ex.Message + "\r\n" + ex.StackTrace);
            }
            catch (Exception ex)
            {
                addLog("msg queue thread exception: " + ex.Message + "\r\n" + ex.StackTrace);
            }
            finally
            {
                //Marshal.FreeHGlobal(msgBuffer);
                CloseMsgQueue(hMsgQueue);
            }
            addLog("btmon thread ended");
        }

        #region logging
        static string logFile = "\\ITEwatch.log";
        static object lockFile = new object();
        static void addLog(string s)
        {
            System.Diagnostics.Debug.WriteLine(s);
            try
            {
                lock (lockFile)
                {
                    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logFile, true))
                    {
                        sw.WriteLine(s);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("addLog: " + ex.Message);
            }
        }
        #endregion

    }
}
