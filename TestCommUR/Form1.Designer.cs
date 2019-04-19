namespace TestCommUR
{
	partial class Form1
	{
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.btnConnect = new System.Windows.Forms.Button();
			this.btnGetCtrlVer = new System.Windows.Forms.Button();
			this.lblRTDEVer = new System.Windows.Forms.Label();
			this.lblCtrlVer = new System.Windows.Forms.Label();
			this.lblRcvTxt = new System.Windows.Forms.Label();
			this.txtbMessToSend = new System.Windows.Forms.TextBox();
			this.btnSendTxt = new System.Windows.Forms.Button();
			this.btnDisconnect = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.txtbSourceToSend = new System.Windows.Forms.TextBox();
			this.cmbWarnLevelToSend = new System.Windows.Forms.ComboBox();
			this.nudRecFreq = new System.Windows.Forms.NumericUpDown();
			this.btnClearRcvTxt = new System.Windows.Forms.Button();
			this.lblURCtrlIP = new System.Windows.Forms.Label();
			this.txtbURCtrlIP = new System.Windows.Forms.TextBox();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.lblRecFreq = new System.Windows.Forms.Label();
			this.lstViewToRecv = new System.Windows.Forms.ListView();
			this.btnPauseReceive = new System.Windows.Forms.Button();
			this.btnStartReceive = new System.Windows.Forms.Button();
			this.lstViewToSend = new System.Windows.Forms.ListView();
			this.btnSend = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.nudRecFreq)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(260, 11);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(57, 25);
			this.btnConnect.TabIndex = 0;
			this.btnConnect.Text = "Connect";
			this.toolTip1.SetToolTip(this.btnConnect, "To Connect");
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
			// 
			// btnGetCtrlVer
			// 
			this.btnGetCtrlVer.Location = new System.Drawing.Point(260, 42);
			this.btnGetCtrlVer.Name = "btnGetCtrlVer";
			this.btnGetCtrlVer.Size = new System.Drawing.Size(91, 24);
			this.btnGetCtrlVer.TabIndex = 1;
			this.btnGetCtrlVer.Text = "Get Ctrl Version";
			this.toolTip1.SetToolTip(this.btnGetCtrlVer, "To Get Controller Version");
			this.btnGetCtrlVer.UseVisualStyleBackColor = true;
			this.btnGetCtrlVer.Click += new System.EventHandler(this.BtnGetCtrlVer_Click);
			// 
			// lblRTDEVer
			// 
			this.lblRTDEVer.AutoSize = true;
			this.lblRTDEVer.Location = new System.Drawing.Point(404, 17);
			this.lblRTDEVer.Name = "lblRTDEVer";
			this.lblRTDEVer.Size = new System.Drawing.Size(56, 13);
			this.lblRTDEVer.TabIndex = 2;
			this.lblRTDEVer.Text = "RTDE Ver";
			this.toolTip1.SetToolTip(this.lblRTDEVer, "RTDE Protocol version");
			// 
			// lblCtrlVer
			// 
			this.lblCtrlVer.AutoSize = true;
			this.lblCtrlVer.Location = new System.Drawing.Point(404, 48);
			this.lblCtrlVer.Name = "lblCtrlVer";
			this.lblCtrlVer.Size = new System.Drawing.Size(38, 13);
			this.lblCtrlVer.TabIndex = 3;
			this.lblCtrlVer.Text = "CtrlVer";
			this.toolTip1.SetToolTip(this.lblCtrlVer, "Current Controller Version");
			// 
			// lblRcvTxt
			// 
			this.lblRcvTxt.AutoSize = true;
			this.lblRcvTxt.Location = new System.Drawing.Point(72, 79);
			this.lblRcvTxt.Name = "lblRcvTxt";
			this.lblRcvTxt.Size = new System.Drawing.Size(77, 13);
			this.lblRcvTxt.TabIndex = 4;
			this.lblRcvTxt.Text = "Received Text";
			this.toolTip1.SetToolTip(this.lblRcvTxt, "Received Message from Controller");
			// 
			// txtbMessToSend
			// 
			this.txtbMessToSend.Location = new System.Drawing.Point(159, 103);
			this.txtbMessToSend.Name = "txtbMessToSend";
			this.txtbMessToSend.Size = new System.Drawing.Size(239, 20);
			this.txtbMessToSend.TabIndex = 5;
			this.toolTip1.SetToolTip(this.txtbMessToSend, "Message to send.");
			// 
			// btnSendTxt
			// 
			this.btnSendTxt.Location = new System.Drawing.Point(404, 101);
			this.btnSendTxt.Name = "btnSendTxt";
			this.btnSendTxt.Size = new System.Drawing.Size(75, 23);
			this.btnSendTxt.TabIndex = 6;
			this.btnSendTxt.Text = "Send";
			this.toolTip1.SetToolTip(this.btnSendTxt, "To Send message to Controller");
			this.btnSendTxt.UseVisualStyleBackColor = true;
			this.btnSendTxt.Click += new System.EventHandler(this.BtnSendTxt_Click);
			// 
			// btnDisconnect
			// 
			this.btnDisconnect.Location = new System.Drawing.Point(323, 12);
			this.btnDisconnect.Name = "btnDisconnect";
			this.btnDisconnect.Size = new System.Drawing.Size(75, 23);
			this.btnDisconnect.TabIndex = 10;
			this.btnDisconnect.Text = "Disconnect";
			this.toolTip1.SetToolTip(this.btnDisconnect, "To Disconnect");
			this.btnDisconnect.UseVisualStyleBackColor = true;
			this.btnDisconnect.Click += new System.EventHandler(this.BtnDisconnect_Click);
			// 
			// txtbSourceToSend
			// 
			this.txtbSourceToSend.Location = new System.Drawing.Point(75, 103);
			this.txtbSourceToSend.Name = "txtbSourceToSend";
			this.txtbSourceToSend.Size = new System.Drawing.Size(75, 20);
			this.txtbSourceToSend.TabIndex = 5;
			this.toolTip1.SetToolTip(this.txtbSourceToSend, "Source to send.");
			// 
			// cmbWarnLevelToSend
			// 
			this.cmbWarnLevelToSend.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbWarnLevelToSend.FormattingEnabled = true;
			this.cmbWarnLevelToSend.Items.AddRange(new object[] {
            "Exception",
            "Error",
            "Warning",
            "Info"});
			this.cmbWarnLevelToSend.Location = new System.Drawing.Point(12, 103);
			this.cmbWarnLevelToSend.Name = "cmbWarnLevelToSend";
			this.cmbWarnLevelToSend.Size = new System.Drawing.Size(52, 21);
			this.cmbWarnLevelToSend.TabIndex = 14;
			this.toolTip1.SetToolTip(this.cmbWarnLevelToSend, "Warning Level for message to send.");
			// 
			// nudRecFreq
			// 
			this.nudRecFreq.Location = new System.Drawing.Point(174, 3);
			this.nudRecFreq.Maximum = new decimal(new int[] {
            125,
            0,
            0,
            0});
			this.nudRecFreq.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudRecFreq.Name = "nudRecFreq";
			this.nudRecFreq.Size = new System.Drawing.Size(51, 20);
			this.nudRecFreq.TabIndex = 25;
			this.toolTip1.SetToolTip(this.nudRecFreq, "Output Frequency (1 to 125 Hz)");
			this.nudRecFreq.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.nudRecFreq.ValueChanged += new System.EventHandler(this.NudRecFreq_ValueChanged);
			// 
			// btnClearRcvTxt
			// 
			this.btnClearRcvTxt.Location = new System.Drawing.Point(12, 73);
			this.btnClearRcvTxt.Name = "btnClearRcvTxt";
			this.btnClearRcvTxt.Size = new System.Drawing.Size(50, 24);
			this.btnClearRcvTxt.TabIndex = 18;
			this.btnClearRcvTxt.Text = "Clear";
			this.toolTip1.SetToolTip(this.btnClearRcvTxt, "To Clear Received message");
			this.btnClearRcvTxt.UseVisualStyleBackColor = true;
			this.btnClearRcvTxt.Click += new System.EventHandler(this.BtnClearRcvTxt_Click);
			// 
			// lblURCtrlIP
			// 
			this.lblURCtrlIP.AutoSize = true;
			this.lblURCtrlIP.Location = new System.Drawing.Point(12, 17);
			this.lblURCtrlIP.Name = "lblURCtrlIP";
			this.lblURCtrlIP.Size = new System.Drawing.Size(138, 13);
			this.lblURCtrlIP.TabIndex = 19;
			this.lblURCtrlIP.Text = "Enter UR Controller IP here:";
			// 
			// txtbURCtrlIP
			// 
			this.txtbURCtrlIP.Location = new System.Drawing.Point(15, 45);
			this.txtbURCtrlIP.Name = "txtbURCtrlIP";
			this.txtbURCtrlIP.Size = new System.Drawing.Size(135, 20);
			this.txtbURCtrlIP.TabIndex = 20;
			this.txtbURCtrlIP.Text = "192.168.56.101";
			this.toolTip1.SetToolTip(this.txtbURCtrlIP, "Controller IP");
			this.txtbURCtrlIP.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TxtbURCtrlIP_KeyPress);
			this.txtbURCtrlIP.Validating += new System.ComponentModel.CancelEventHandler(this.TxtbURCtrlIP_Validating);
			this.txtbURCtrlIP.Validated += new System.EventHandler(this.TxtbURCtrlIP_Validated);
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Location = new System.Drawing.Point(12, 130);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.lblRecFreq);
			this.splitContainer1.Panel1.Controls.Add(this.nudRecFreq);
			this.splitContainer1.Panel1.Controls.Add(this.lstViewToRecv);
			this.splitContainer1.Panel1.Controls.Add(this.btnPauseReceive);
			this.splitContainer1.Panel1.Controls.Add(this.btnStartReceive);
			this.splitContainer1.Panel1MinSize = 260;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.lstViewToSend);
			this.splitContainer1.Panel2.Controls.Add(this.btnSend);
			this.splitContainer1.Panel2MinSize = 260;
			this.splitContainer1.Size = new System.Drawing.Size(760, 300);
			this.splitContainer1.SplitterDistance = 380;
			this.splitContainer1.TabIndex = 22;
			this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.SplitContainer1_SplitterMoved);
			// 
			// lblRecFreq
			// 
			this.lblRecFreq.AutoSize = true;
			this.lblRecFreq.Location = new System.Drawing.Point(231, 5);
			this.lblRecFreq.Name = "lblRecFreq";
			this.lblRecFreq.Size = new System.Drawing.Size(20, 13);
			this.lblRecFreq.TabIndex = 26;
			this.lblRecFreq.Text = "Hz";
			// 
			// lstViewToRecv
			// 
			this.lstViewToRecv.CheckBoxes = true;
			this.lstViewToRecv.Location = new System.Drawing.Point(0, 30);
			this.lstViewToRecv.Name = "lstViewToRecv";
			this.lstViewToRecv.Size = new System.Drawing.Size(377, 271);
			this.lstViewToRecv.TabIndex = 24;
			this.lstViewToRecv.UseCompatibleStateImageBehavior = false;
			this.lstViewToRecv.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LstViewToRecv_ItemChecked);
			// 
			// btnPauseReceive
			// 
			this.btnPauseReceive.Location = new System.Drawing.Point(93, 0);
			this.btnPauseReceive.Name = "btnPauseReceive";
			this.btnPauseReceive.Size = new System.Drawing.Size(75, 23);
			this.btnPauseReceive.TabIndex = 23;
			this.btnPauseReceive.Text = "Pause";
			this.toolTip1.SetToolTip(this.btnPauseReceive, "To Pause");
			this.btnPauseReceive.UseVisualStyleBackColor = true;
			this.btnPauseReceive.Click += new System.EventHandler(this.BtnPauseReceive_Click);
			// 
			// btnStartReceive
			// 
			this.btnStartReceive.Location = new System.Drawing.Point(0, 0);
			this.btnStartReceive.Name = "btnStartReceive";
			this.btnStartReceive.Size = new System.Drawing.Size(87, 23);
			this.btnStartReceive.TabIndex = 22;
			this.btnStartReceive.Text = "Start Receive";
			this.toolTip1.SetToolTip(this.btnStartReceive, "To Start receiving checked data from Controller");
			this.btnStartReceive.UseVisualStyleBackColor = true;
			this.btnStartReceive.Click += new System.EventHandler(this.BtnStartReceive_Click);
			// 
			// lstViewToSend
			// 
			this.lstViewToSend.CheckBoxes = true;
			this.lstViewToSend.Location = new System.Drawing.Point(0, 30);
			this.lstViewToSend.Name = "lstViewToSend";
			this.lstViewToSend.Size = new System.Drawing.Size(370, 271);
			this.lstViewToSend.TabIndex = 17;
			this.lstViewToSend.UseCompatibleStateImageBehavior = false;
			this.lstViewToSend.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.LstViewToSend_AfterLabelEdit);
			this.lstViewToSend.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.LstViewToSend_ItemChecked);
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(0, 0);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(83, 23);
			this.btnSend.TabIndex = 16;
			this.btnSend.Text = "Send";
			this.toolTip1.SetToolTip(this.btnSend, "To Send checked data values");
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 442);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.txtbURCtrlIP);
			this.Controls.Add(this.lblURCtrlIP);
			this.Controls.Add(this.btnClearRcvTxt);
			this.Controls.Add(this.cmbWarnLevelToSend);
			this.Controls.Add(this.btnDisconnect);
			this.Controls.Add(this.btnSendTxt);
			this.Controls.Add(this.txtbSourceToSend);
			this.Controls.Add(this.txtbMessToSend);
			this.Controls.Add(this.lblRcvTxt);
			this.Controls.Add(this.lblCtrlVer);
			this.Controls.Add(this.lblRTDEVer);
			this.Controls.Add(this.btnGetCtrlVer);
			this.Controls.Add(this.btnConnect);
			this.MinimumSize = new System.Drawing.Size(800, 480);
			this.Name = "Form1";
			this.Text = "Test UR Communication RTDE";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Resize += new System.EventHandler(this.Form1_Resize);
			((System.ComponentModel.ISupportInitialize)(this.nudRecFreq)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnConnect;
		private System.Windows.Forms.Button btnGetCtrlVer;
		private System.Windows.Forms.Label lblRTDEVer;
		private System.Windows.Forms.Label lblCtrlVer;
		private System.Windows.Forms.Label lblRcvTxt;
		private System.Windows.Forms.TextBox txtbMessToSend;
		private System.Windows.Forms.Button btnSendTxt;
		private System.Windows.Forms.Button btnDisconnect;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.TextBox txtbSourceToSend;
		private System.Windows.Forms.ComboBox cmbWarnLevelToSend;
		private System.Windows.Forms.Button btnClearRcvTxt;
		private System.Windows.Forms.Label lblURCtrlIP;
		private System.Windows.Forms.TextBox txtbURCtrlIP;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Label lblRecFreq;
		private System.Windows.Forms.NumericUpDown nudRecFreq;
		private System.Windows.Forms.ListView lstViewToRecv;
		private System.Windows.Forms.Button btnPauseReceive;
		private System.Windows.Forms.Button btnStartReceive;
		private System.Windows.Forms.ListView lstViewToSend;
		private System.Windows.Forms.Button btnSend;
	}
}

