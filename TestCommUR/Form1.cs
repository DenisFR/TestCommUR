using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using UR;


namespace TestCommUR
{
	public partial class Form1 : Form
	{
		URController urController;
		bool _OnLoading;

		#region Form Code
		public Form1()
		{
			InitializeComponent();
			urController = new URController
			{
				IP = "192.168.56.101"
			};
			urController.OnConnected += UrController_OnConnected;
			urController.OnDisconnected += UrController_OnDisconnected;
			urController.RecProtoVersionEvent += UrController_RecProtoVersionEvent;
			urController.RecURCtrlVersionEvent += UrController_RecURCtrlVersionEvent;
			urController.RecMessageEvent += UrController_RecMessageEvent;
			urController.RecCtrlPackSetupOut += UrController_RecCtrlPackSetupOut;
			urController.RecCtrlPackSetupIn += UrController_RecCtrlPackSetupIn;
			urController.RecDataPackage += UrController_RecDataPackage;
			urController.RecCtrlPackStartDone += UrController_RecCtrlPackStartDone;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			_OnLoading = true;

			PopulateListView(lstViewToRecv, RTDE_Package.ControllerOutput);
			PopulateListView(lstViewToSend, RTDE_Package.ControllerInput);

			Form1_Resize(sender, e);
			_OnLoading = false;
		}

		private void PopulateListView(ListView listView, List<RTDE_Data_Package.ControllerIOData> listIOData)
		{
			if (listView.IsDisposed || listView.Disposing) return;

			_OnLoading = true;
			listView.Clear();
			listView.CheckBoxes = true;
			listView.LabelEdit = true;
			listView.ShowItemToolTips = true;
			listView.View = View.Details;
			listView.Columns.Add("KeyValue", "Value");
			listView.Columns.Add("KeyName", "Name");
			listView.Columns.Add("KeyInfo", "Info");

			ListView.ListViewItemCollection lstvItemCollRecv = new ListView.ListViewItemCollection(listView);
			foreach (var item in listIOData)
			{
				bool bOK = false;
				string[] strCtrlVers = urController.URCtrlV.Split('.');
				if (strCtrlVers.Length >= 3)
				{
					foreach (string mainIntro in item.Intro.Split('/'))
					{
						string[] intros = mainIntro.Split('.');
						if (intros.Length >= 3)
						{
							if (intros[0] == strCtrlVers[0])
							{
								//Only check if main version are same
								int[] ctrlVers = new int[2];
								int[] itemVers = new int[2];
								if (int.TryParse(strCtrlVers[1], out ctrlVers[0]) && int.TryParse(strCtrlVers[2], out ctrlVers[1])
									&& int.TryParse(intros[1], out itemVers[0]) && int.TryParse(intros[2], out itemVers[1]))
								{
									if (itemVers[0] == ctrlVers[0])
										bOK = (itemVers[1] <= ctrlVers[1]);
									else
										bOK = (itemVers[0] < ctrlVers[0]);
								}
							}
						}
					}
				}
				if (bOK)
				{
					ListViewItem lstvItem = lstvItemCollRecv.Add(new ListViewItem());
					lstvItem.Name = item.Name;
					lstvItem.Text = "";
					lstvItem.Tag = item;
					lstvItem.SubItems.Add(item.Name).Name = item.Name;
					lstvItem.SubItems.Add(item.Comment).Name = item.Name;
				}
			}
			_OnLoading = false;
		}

		private void Form1_Resize(object sender, EventArgs e)
		{
			splitContainer1.Width = ClientSize.Width - 2 * splitContainer1.Left;
			splitContainer1.Height = ClientSize.Height - splitContainer1.Top - splitContainer1.Left;

			lstViewToRecv.Width = splitContainer1.SplitterDistance;
			lstViewToRecv.Height = splitContainer1.Height - lstViewToRecv.Top;
			lstViewToSend.Width = splitContainer1.Width - splitContainer1.SplitterDistance - splitContainer1.SplitterWidth;
			lstViewToSend.Height = splitContainer1.Height - lstViewToSend.Top;
		}

		private void SplitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
		{
			Form1_Resize(sender, e);
		}

		private void TxtbURCtrlIP_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			string[] strIPs = txtbURCtrlIP.Text.Split('.');
			if (strIPs.Length != 4)
			{
				e.Cancel = true;
				txtbURCtrlIP.Select(0, txtbURCtrlIP.Text.Length);

				// Set the ErrorProvider error with the text to display. 
				this.errorProvider1.SetError(txtbURCtrlIP, "Must be IP address (ww.xx.yy.zz).");
			}
			foreach (string strIPOct in strIPs)
			{
				if (!byte.TryParse(strIPOct, out byte bytIP))
				{
					e.Cancel = true;
					txtbURCtrlIP.Select(0, txtbURCtrlIP.Text.Length);

					// Set the ErrorProvider error with the text to display. 
					this.errorProvider1.SetError(txtbURCtrlIP, "Each value must be in range 0 to 255.");
				}
			}

		}

		private void TxtbURCtrlIP_Validated(object sender, EventArgs e)
		{
			this.errorProvider1.SetError(txtbURCtrlIP, "");

			urController.IP = txtbURCtrlIP.Text;
			btnConnect.Focus();
		}

		private void TxtbURCtrlIP_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == '\r')
				btnConnect.Focus();
		}
		#endregion
		#region UrController Events
		private void UrController_OnConnected(object sender, EventArgs e)
		{
			btnConnect.BackColor=SystemColors.ControlLightLight;
			btnDisconnect.BackColor = SystemColors.Control;
			btnGetCtrlVer.Focus();
		}

		private void UrController_OnDisconnected(object sender, EventArgs e)
		{
			btnConnect.BackColor = SystemColors.Control;
			btnDisconnect.BackColor = SystemColors.ControlLightLight;
		}

		private void UrController_RecProtoVersionEvent(object sender, EventArgs e)
		{
			if (lblRcvTxt.IsDisposed || lblRcvTxt.Disposing) return;
			lblRTDEVer.Invoke(new Action(() => lblRTDEVer.Text = ((URController)sender)?.ProtoVersion.ToString() ));
		}

		private void UrController_RecURCtrlVersionEvent(object sender, EventArgs e)
		{
			if (lblCtrlVer.IsDisposed || lblCtrlVer.Disposing) return;
			if (lstViewToRecv.IsDisposed || lstViewToRecv.Disposing) return;
			if (lstViewToSend.IsDisposed || lstViewToSend.Disposing) return;
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)delegate { this.UrController_RecURCtrlVersionEvent(sender, e); });
				return;
			}

			lblCtrlVer.Text = urController.URCtrlV;
			PopulateListView(lstViewToRecv, RTDE_Package.ControllerOutput);
			PopulateListView(lstViewToSend, RTDE_Package.ControllerInput);
		}

		private void UrController_RecMessageEvent(object sender, EventArgs e)
		{
			if (lblRcvTxt.IsDisposed || lblRcvTxt.Disposing) return;
			lblRcvTxt.Invoke(new Action(() => lblRcvTxt.Text = urController.LastMessageSource + ": " + urController.LastMessage ));
		}

		private void UrController_RecCtrlPackSetupOut(object sender, EventArgs e)
		{
			if (lstViewToRecv.IsDisposed || lstViewToRecv.Disposing) return;
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)delegate { this.UrController_RecCtrlPackSetupOut(sender, e); });
				return;
			}

			string[] varTypes = urController.LastOutSetupVarTypes.Split(',');
			if (lstViewToRecv.CheckedItems.Count == varTypes.Length)
			{
				int curs = 0;
				foreach (ListViewItem item in lstViewToRecv.CheckedItems)
				{
					RTDE_Package.DataType dtItem = ((RTDE_Package.ControllerIOData)item.Tag).Type;
					string sItemType = dtItem.GetAttribute<RTDE_Package.DataTypeAttribute>().Name;

					if (sItemType == varTypes[curs])
					{
						item.BackColor = Color.LightSeaGreen;
					}
					else
					{
						item.BackColor = Color.PaleVioletRed;
					}
					++curs;
				}
			}
			else
			{
				Debug.Print("Received Controller Package Setup Outputs returning wrong list lenght.");
			}
		}

		private void UrController_RecCtrlPackSetupIn(object sender, EventArgs e)
		{
			if (lstViewToSend.IsDisposed || lstViewToSend.Disposing) return;
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)delegate { this.UrController_RecCtrlPackSetupIn(sender, e); });
				return;
			}

			string[] varTypes = urController.LastInSetupVarTypes.Split(',');
			if (lstViewToSend.CheckedItems.Count == varTypes.Length)
			{
				int curs = 0;
				foreach (ListViewItem item in lstViewToSend.CheckedItems)
				{
					RTDE_Package.DataType dtItem = ((RTDE_Package.ControllerIOData)item.Tag).Type;
					string sItemType = dtItem.GetAttribute<RTDE_Package.DataTypeAttribute>().Name;

					if (sItemType == varTypes[curs])
					{
						item.BackColor = Color.LightSeaGreen;
					}
					else
					{
						item.BackColor = Color.PaleVioletRed;
					}
					++curs;
				}
			}
			else
			{
				Debug.Print("Received Controller Package Setup Inputs returning wrong list lenght.");
			}
		}

		private void UrController_RecDataPackage(object sender, EventArgs e)
		{
			if (lstViewToRecv.IsDisposed || lstViewToRecv.Disposing) return;
			if (InvokeRequired)
			{
				Invoke((MethodInvoker)delegate { this.UrController_RecDataPackage(sender, e); });
				return;
			}

			if (lstViewToRecv.CheckedItems.Count == urController.LastValidatedOutputs.Count)
			{
				foreach (ListViewItem item in lstViewToRecv.CheckedItems)
				{
					string sItemName = ((RTDE_Package.ControllerIOData)item.Tag).Name;
					RTDE_Package.ControllerIOData ioItem = urController.LastValidatedOutputs.Find(x => x.Name == sItemName);

					if (ioItem != null)
						item.Text = ioItem.Value;
				}
			}
			else
			{
				Debug.Print("Received Controller Data Package returning wrong list lenght.");
			}
		}

		private void UrController_RecCtrlPackStartDone(object sender, EventArgs e)
		{
			btnStartReceive.BackColor = urController.CtrlPackageRunning ? SystemColors.ControlLightLight : SystemColors.Control;
			btnPauseReceive.BackColor = !urController.CtrlPackageRunning ? SystemColors.ControlLightLight : SystemColors.Control;
		}

		#endregion
		#region Buttons
		private void BtnConnect_Click(object sender, EventArgs e)
		{
			lblRTDEVer.Text = "";
			urController.Connect();
			urController.AskProtoVersion();
		}

		private void BtnDisconnect_Click(object sender, EventArgs e)
		{
			urController.Disconnect();
		}

		private void BtnGetCtrlVer_Click(object sender, EventArgs e)
		{
			lblCtrlVer.Text = "";
			urController.AskCtrlVersion();
		}

		private void BtnClearRcvTxt_Click(object sender, EventArgs e)
		{
			lblRcvTxt.Text = "";
		}

		private void BtnSendTxt_Click(object sender, EventArgs e)
		{
			urController.SendMessage(txtbMessToSend.Text,txtbSourceToSend.Text,(byte)cmbWarnLevelToSend.SelectedIndex);
		}

		#endregion
		#region Outputs To Receive
		private void LstViewToRecv_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			SetupOutputs();

			if (!e.Item.Checked)
				e.Item.BackColor = SystemColors.Window;
		}

		private void NudRecFreq_ValueChanged(object sender, EventArgs e)
		{
			SetupOutputs();
		}

		private void SetupOutputs()
		{
			if (!_OnLoading)
			{
				string varNames = "";

				foreach (ListViewItem item in lstViewToRecv.CheckedItems)
					varNames += ((RTDE_Package.ControllerIOData)item.Tag).Name + ",";

				varNames = varNames.TrimEnd(',');
				urController.CtrlPackageSetupOutputs((double)nudRecFreq.Value, varNames);

			}
		}

		private void BtnStartReceive_Click(object sender, EventArgs e)
		{
			urController.AskCtrlStartOutput();
		}

		private void BtnPauseReceive_Click(object sender, EventArgs e)
		{
			urController.AskCtrlPauseOutput();
		}

		#endregion
		#region Inputs To Send
		private void LstViewToSend_AfterLabelEdit(object sender, LabelEditEventArgs e)
		{
			// Determine if label is changed by checking for null.
			if (e.Label == null)
				return;

			string strLabel = e.Label;
			ListViewItem item = lstViewToSend.Items[e.Item];
			RTDE_Package.ControllerIOData ctrlIOData = (RTDE_Package.ControllerIOData)item.Tag;
			if (RTDE_Data_Package.IsValueValid(ctrlIOData.Type, ref strLabel))
			{
				ctrlIOData.Value = strLabel;
				item.BackColor = SystemColors.Window;
				lstViewToSend.BeginInvoke(new Action(() => item.Text = strLabel));
				e.CancelEdit = false;
				return;
			}
			e.CancelEdit = true;
			
			lstViewToSend.BeginInvoke(new Action(() => item.Checked = false));
			lstViewToSend.BeginInvoke(new Action(() => item.BackColor = Color.PaleVioletRed));
			item.BeginEdit();
			toolTip1.Show("Value must be " + ctrlIOData.Type.GetAttribute<RTDE_Package.DataTypeAttribute>().Name, lstViewToSend,item.Position,3000);

		}

		private void LstViewToSend_ItemChecked(object sender, ItemCheckedEventArgs e)
		{
			if (!_OnLoading)
			{
				string varNames = "";

				foreach (ListViewItem item in lstViewToSend.CheckedItems)
				{
					if (e.Item.Text != "")
						varNames += ((RTDE_Package.ControllerIOData)item.Tag).Name + ",";
					else
						item.Checked = false;
				}

				varNames = varNames.TrimEnd(',');
				urController.CtrlPackageSetupInputs(varNames);

				if (!e.Item.Checked)
					e.Item.BackColor = SystemColors.Window;
			}
		}

		private void BtnSend_Click(object sender, EventArgs e)
		{
			List<RTDE_Package.ControllerIOData> ctrlIODatas = new List<RTDE_Package.ControllerIOData>();

			foreach (ListViewItem item in lstViewToSend.CheckedItems)
				ctrlIODatas.Add( (RTDE_Package.ControllerIOData)item.Tag );

			urController.CtrlPackageSendInputs(ctrlIODatas);

		}

		#endregion
	}
}
