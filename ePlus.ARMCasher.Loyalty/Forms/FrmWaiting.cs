using ePlus.ARMCommon.Log;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Timers;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmWaiting : Form
	{
		private IContainer components;

		private Label label1;

		private System.Timers.Timer timer = new System.Timers.Timer();

		private int secondsCounter;

		private bool callbackFileCheck;

		private static int waitingTimeout;

		private BackgroundWorker bkWorker = new BackgroundWorker();

		private System.Exception exception;

		public BackgroundWorker BkWorker
		{
			get
			{
				return this.bkWorker;
			}
		}

		public System.Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		public int WaitingTimeout
		{
			get
			{
				return FrmWaiting.waitingTimeout;
			}
			set
			{
				FrmWaiting.waitingTimeout = value;
			}
		}

		static FrmWaiting()
		{
			FrmWaiting.waitingTimeout = 1000;
		}

		public FrmWaiting()
		{
			this.InitializeComponent();
			this.timer.Interval = 1000;
			this.timer.Elapsed += new ElapsedEventHandler(this.timer_Elapsed);
			base.HandleDestroyed += new EventHandler(this.FrmWaiting_HandleDestroyed);
		}

		private void bkWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			this.StopTimeCounter(e.Error);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FrmScanIncomingFolder_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (base.DialogResult != System.Windows.Forms.DialogResult.Yes && base.DialogResult != System.Windows.Forms.DialogResult.No)
			{
				e.Cancel = true;
			}
		}

		private void FrmScanIncomingFolder_Load(object sender, EventArgs e)
		{
			this.secondsCounter = FrmWaiting.waitingTimeout;
			this.bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.bkWorker_RunWorkerCompleted);
			this.timer.Start();
			this.bkWorker.RunWorkerAsync();
			this.label1.Text = string.Format("До окончания времени ожидания осталось {0} секунд.", this.secondsCounter);
		}

		private void FrmWaiting_HandleDestroyed(object sender, EventArgs e)
		{
			if (this.timer.Enabled)
			{
				this.timer.Stop();
			}
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FrmWaiting));
			this.label1 = new Label();
			base.SuspendLayout();
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label1.Location = new Point(12, 20);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(419, 23);
			this.label1.TabIndex = 1;
			this.label1.Text = "label1";
			this.label1.TextAlign = ContentAlignment.TopCenter;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(443, 66);
			base.Controls.Add(this.label1);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmWaitingPCX";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Ожидание";
			base.FormClosing += new FormClosingEventHandler(this.FrmScanIncomingFolder_FormClosing);
			base.Load += new EventHandler(this.FrmScanIncomingFolder_Load);
			base.ResumeLayout(false);
		}

		private void SetLabelText(string text)
		{
			this.label1.Text = text;
		}

		public System.Windows.Forms.DialogResult ShowChildDialod(Form frm)
		{
			if (!base.InvokeRequired)
			{
				this.timer.Stop();
				System.Windows.Forms.DialogResult dialogResult = frm.ShowDialog(this);
				this.timer.Start();
				return dialogResult;
			}
			FrmWaiting.FormArgVoidReturnDelegate formArgVoidReturnDelegate = new FrmWaiting.FormArgVoidReturnDelegate(this.ShowChildDialod);
			object[] objArray = new object[] { frm };
			return (System.Windows.Forms.DialogResult)base.Invoke(formArgVoidReturnDelegate, objArray);
		}

		private void StopTimeCounter(System.Exception ex)
		{
			this.timer.Stop();
			if (ex == null)
			{
				base.DialogResult = System.Windows.Forms.DialogResult.Yes;
				return;
			}
			this.exception = ex;
			base.DialogResult = System.Windows.Forms.DialogResult.No;
		}

		private void timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (this.secondsCounter > 0 && this.timer.Enabled && !base.IsDisposed && base.IsHandleCreated)
			{
				TextSetter textSetter = new TextSetter(this.SetLabelText);
				object[] objArray = new object[1];
				FrmWaiting frmWaiting = this;
				int num = frmWaiting.secondsCounter - 1;
				int num1 = num;
				frmWaiting.secondsCounter = num;
				objArray[0] = string.Format("До окончания времени ожидания осталось {0} секунд.", num1);
				base.Invoke(textSetter, objArray);
			}
			if (!this.callbackFileCheck)
			{
				try
				{
					try
					{
						this.callbackFileCheck = true;
						if (this.secondsCounter <= 0)
						{
							this.timer.Stop();
							return;
						}
					}
					catch (System.Exception exception)
					{
						ARMLogger.InfoException("Ошибка при работе таймера", exception);
					}
				}
				finally
				{
					this.callbackFileCheck = false;
				}
			}
		}

		private delegate System.Windows.Forms.DialogResult FormArgVoidReturnDelegate(Form frm);
	}
}