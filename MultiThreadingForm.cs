//
// HALCON/.NET (C#) multithreading example
//
// ｩ 2007-2019 MVTec Software GmbH
//
// Purpose:
// This example program shows how to perform image acquisition, image
// processing, and image display in parallel by using two threads (besides
// the main thread), one for each task. The first thread grabs images, the
// second one performs image processing tasks, and the main thread is in
// charge of the HALCON window - it displays the image processed last and
// its results.
//
// MultiThreadingForm.cs: Defines the behavior of the application's GUI.
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using System.Runtime.InteropServices;

//using HalconDotNet;

namespace MultiThreading
{
    /// <summary>
    /// Summary description for MultiThreadExpDlg.
    /// </summary>

    public class MultiThreadingForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        public System.Windows.Forms.Label procTimeLabel;
        private System.ComponentModel.IContainer components;
        public WorkerThread workerObject;

        public ManualResetEvent stopEventHandle;
        public Thread threadAcq, threadIP;
        public PictureBox pictureBox1;
        private Button connectButton;
        public TextBox msgTextBox;
        private CheckBox drawCheckBox;
        private System.Windows.Forms.Label LabelPT;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
            HandleRef hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_SETREDRAW = 0x000B;



        public MultiThreadingForm()
        {

            // Required for Windows Form Designer support
            InitializeComponent();

            // set up eventhandle and instance of WorkerThread class, which
            // contains thread functions
            stopEventHandle = new ManualResetEvent(false);
            workerObject = new WorkerThread(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            // if threads are still running
            // abort them
            if (threadAcq != null) threadAcq.Abort();
            if (threadIP != null) threadIP.Abort();
            base.OnClosed(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.LabelPT = new System.Windows.Forms.Label();
            this.procTimeLabel = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.connectButton = new System.Windows.Forms.Button();
            this.msgTextBox = new System.Windows.Forms.TextBox();
            this.drawCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // startButton
            // 
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point(451, 124);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(96, 46);
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Start";
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(451, 198);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(96, 46);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Stop";
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // LabelPT
            // 
            this.LabelPT.Location = new System.Drawing.Point(19, 344);
            this.LabelPT.Name = "LabelPT";
            this.LabelPT.Size = new System.Drawing.Size(135, 28);
            this.LabelPT.TabIndex = 3;
            this.LabelPT.Text = "Processing time:";
            this.LabelPT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // procTimeLabel
            // 
            this.procTimeLabel.Location = new System.Drawing.Point(163, 344);
            this.procTimeLabel.Name = "procTimeLabel";
            this.procTimeLabel.Size = new System.Drawing.Size(87, 28);
            this.procTimeLabel.TabIndex = 5;
            this.procTimeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(33, 34);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(397, 267);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(451, 47);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(96, 46);
            this.connectButton.TabIndex = 7;
            this.connectButton.Text = "Connect";
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // msgTextBox
            // 
            this.msgTextBox.Location = new System.Drawing.Point(38, 308);
            this.msgTextBox.Name = "msgTextBox";
            this.msgTextBox.ReadOnly = true;
            this.msgTextBox.Size = new System.Drawing.Size(413, 22);
            this.msgTextBox.TabIndex = 8;
            // 
            // drawCheckBox
            // 
            this.drawCheckBox.AutoSize = true;
            this.drawCheckBox.Location = new System.Drawing.Point(457, 273);
            this.drawCheckBox.Name = "drawCheckBox";
            this.drawCheckBox.Size = new System.Drawing.Size(95, 21);
            this.drawCheckBox.TabIndex = 9;
            this.drawCheckBox.Text = "Draw Stop";
            this.drawCheckBox.UseVisualStyleBackColor = true;
            this.drawCheckBox.CheckedChanged += new System.EventHandler(this.drawCheckBox_CheckedChanged);
            // 
            // MultiThreadingForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(684, 428);
            this.Controls.Add(this.drawCheckBox);
            this.Controls.Add(this.msgTextBox);
            this.Controls.Add(this.connectButton);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.procTimeLabel);
            this.Controls.Add(this.LabelPT);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "MultiThreadingForm";
            this.Text = "Performing Image Acquisition, Processing, and Display in Multiple Threads";
            this.Move += new System.EventHandler(this.MultiThreadingForm_Move);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.Run(new MultiThreadingForm());
        }


        ////////////////////////////////////////////////////////////////////////////
        // startButton_Click - Every click on the Start button will create new
        //                     instances of the Thread class - threadFG handles the
        //                     image acquisition, whereas threadIP performs the image
        //                     processing. Since Start and Stop button can be
        //                     clicked consecutively, all handles used to synchronize
        //                     the threads need to be set/reset to their initial
        //                     states. Handles that are members of the WorkerThread
        //                     class are reset by calling the init() method. Last but
        //                     not least we need to call Thread.Start() to "start"
        //                     the thread functions.
        ////////////////////////////////////////////////////////////////////////////
        private void startButton_Click(object sender, System.EventArgs e)
        {
            stopEventHandle.Reset();

            threadAcq = new Thread(new ThreadStart(workerObject.ImgAcqRun));
            threadIP = new Thread(new ThreadStart(workerObject.IPRun));

            startButton.Enabled = false;
            stopButton.Enabled = true;

            threadAcq.Start();
            threadIP.Start();
        }

        ////////////////////////////////////////////////////////////////////////////
        // stopButton_Click - Once the Stop button is clicked, the stopEventHandle is
        //                    "turned on" - to signal the two threads to terminate.
        ////////////////////////////////////////////////////////////////////////////
        private void stopButton_Click(object sender, System.EventArgs e)
        {
            stopEventHandle.Set();
        }

        ////////////////////////////////////////////////////////////////////////////
        //public HWindow GetHalconWindow()
        public bool GetHalconWindow()
        {
            //return WindowControl.HalconWindow;
            return true;
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            workerObject.Init();

            msgTextBox.Text = "Init Done";
            startButton.Enabled = true;
        }

        ////////////////////////////////////////////////////////////////////////////
        // ResetControls - We used the means of delegates to prevent deadlocks
        //                 between the main thread and the IPthread. The delegate is
        //                 called to "clean" the display and the labels.
        ////////////////////////////////////////////////////////////////////////////
        public void ResetControls()
        {
            startButton.Enabled = true;
            stopButton.Enabled = false;
            //GetHalconWindow().ClearWindow();
            procTimeLabel.Text = " ";
            //imageDataLabel.Text = " ";
            components = null;
        }

        // https://dobon.net/vb/dotnet/control/beginupdate.html
        /// <summary>
        /// コントロールの再描画を停止させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void BeginControlUpdate(Control control)
        {
            SendMessage(new HandleRef(control, control.Handle),
                WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero);
        }

        /// <summary>
        /// コントロールの再描画を再開させる
        /// </summary>
        /// <param name="control">対象のコントロール</param>
        public static void EndControlUpdate(Control control)
        {
            SendMessage(new HandleRef(control, control.Handle),
                WM_SETREDRAW, new IntPtr(1), IntPtr.Zero);
            control.Invalidate();
        }

        private void MultiThreadingForm_Move(object sender, EventArgs e)
        {

        }

        protected override void OnMove(EventArgs e)
        {
/*            if(stopButton.Enabled)
            {
                BeginControlUpdate(pictureBox1);
                drawCheckBox.Checked = true;
            } */
        }

        private void drawCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (drawCheckBox.Checked)
            {
                BeginControlUpdate(pictureBox1);
            }
            else
            {
                EndControlUpdate(pictureBox1);
            }

        }

    } // end of class

} // end of using namespace



