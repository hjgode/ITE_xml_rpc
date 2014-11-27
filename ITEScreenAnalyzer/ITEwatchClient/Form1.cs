using System;

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ITEwatchClient
{
    public partial class Form1 : Form
    {
        msgqueue MsgQueue = null;
        public Form1()
        {
            InitializeComponent();
            MsgQueue = new msgqueue();
            
        }

        private void Form1_Closing(object sender, CancelEventArgs e)
        {
            if (MsgQueue != null)
                MsgQueue.Dispose();
        }
    }
}