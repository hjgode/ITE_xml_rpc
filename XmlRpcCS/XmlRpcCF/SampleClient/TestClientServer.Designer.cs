﻿namespace SampleClient
{
    partial class TestClientServer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MainMenu mainMenu1;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClassStop = new System.Windows.Forms.Button();
            this.btnClass = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnClassStop);
            this.panel1.Controls.Add(this.btnClass);
            this.panel1.Controls.Add(this.btnTest);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(238, 52);
            // 
            // btnClassStop
            // 
            this.btnClassStop.Location = new System.Drawing.Point(145, 26);
            this.btnClassStop.Name = "btnClassStop";
            this.btnClassStop.Size = new System.Drawing.Size(84, 20);
            this.btnClassStop.TabIndex = 0;
            this.btnClassStop.Text = "StopScreen";
            this.btnClassStop.Click += new System.EventHandler(this.btnClassStop_Click);
            // 
            // btnClass
            // 
            this.btnClass.Location = new System.Drawing.Point(145, 3);
            this.btnClass.Name = "btnClass";
            this.btnClass.Size = new System.Drawing.Size(84, 20);
            this.btnClass.TabIndex = 0;
            this.btnClass.Text = "StartScreen";
            this.btnClass.Click += new System.EventHandler(this.btnClass_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(74, 3);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(65, 20);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "TestKey";
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(3, 26);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(65, 20);
            this.btnStop.TabIndex = 0;
            this.btnStop.Text = "StopWeb";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(3, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(65, 20);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "StartWeb";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtLog
            // 
            this.txtLog.AcceptsReturn = true;
            this.txtLog.AcceptsTab = true;
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular);
            this.txtLog.Location = new System.Drawing.Point(0, 52);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(238, 243);
            this.txtLog.TabIndex = 1;
            this.txtLog.WordWrap = false;
            // 
            // TestClientServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(238, 295);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.Menu = this.mainMenu1;
            this.MinimizeBox = false;
            this.Name = "TestClientServer";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnClass;
        private System.Windows.Forms.Button btnClassStop;
    }
}

