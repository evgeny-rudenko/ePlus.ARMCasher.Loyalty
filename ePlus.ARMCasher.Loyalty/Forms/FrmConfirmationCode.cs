using ePlus.ARMUtils;
using ePlus.Loyalty;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmConfirmationCode : Form
	{
		private ILoyaltyProgram loyaltyProgram;

		private System.Windows.Forms.Timer timer;

		private int counter;

		private IContainer components;

		private Button buttonOK;

		private Button buttonCancel;

		private Label label1;

		private TextBox textBoxConfirmationCode;

		private Button buttonGetCode;

		private StatusStrip statusStrip1;

		private ToolStripStatusLabel messageLabel;

		public string ConfirmationCode
		{
			get
			{
				return this.textBoxConfirmationCode.Text.Trim();
			}
		}

		public FrmConfirmationCode()
		{
			this.InitializeComponent();
		}

		public FrmConfirmationCode(ILoyaltyProgram loyaltyProgram, bool codeRequested = false) : this()
		{
			this.loyaltyProgram = loyaltyProgram;
			this.InitTimer();
			if (codeRequested)
			{
				this.StartTimer();
			}
		}

		private void buttonGetCode_Click(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.None;
			if (this.GetCodeRequestEvent != null)
			{
				this.StartTimer();
				this.GetCodeRequestEvent(this.loyaltyProgram, null);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FrmConfirmationCode_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (base.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}
			if (!this.ValidateValues())
			{
				UtilsArm.ShowMessageInformationOK("Введенный код некорректен. Пожалуйста, повторите попытку.");
				e.Cancel = true;
			}
		}

		private void InitializeComponent()
		{
			this.buttonOK = new Button();
			this.buttonCancel = new Button();
			this.label1 = new Label();
			this.textBoxConfirmationCode = new TextBox();
			this.buttonGetCode = new Button();
			this.statusStrip1 = new StatusStrip();
			this.messageLabel = new ToolStripStatusLabel();
			this.statusStrip1.SuspendLayout();
			base.SuspendLayout();
			this.buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new Point(165, 79);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(5);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(100, 38);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new Point(275, 79);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(5);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(100, 38);
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Отмена";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(14, 11);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(216, 24);
			this.label1.TabIndex = 0;
			this.label1.Text = "Код подтверждения:";
			this.textBoxConfirmationCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.textBoxConfirmationCode.Location = new Point(14, 39);
			this.textBoxConfirmationCode.Margin = new System.Windows.Forms.Padding(4);
			this.textBoxConfirmationCode.Name = "textBoxConfirmationCode";
			this.textBoxConfirmationCode.Size = new System.Drawing.Size(361, 30);
			this.textBoxConfirmationCode.TabIndex = 1;
			this.buttonGetCode.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonGetCode.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonGetCode.Font = new System.Drawing.Font("Arial", 10.2f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.buttonGetCode.Location = new Point(14, 79);
			this.buttonGetCode.Margin = new System.Windows.Forms.Padding(5);
			this.buttonGetCode.Name = "buttonGetCode";
			this.buttonGetCode.Size = new System.Drawing.Size(141, 38);
			this.buttonGetCode.TabIndex = 4;
			this.buttonGetCode.Text = "Получить sms";
			this.buttonGetCode.UseVisualStyleBackColor = true;
			this.buttonGetCode.Click += new EventHandler(this.buttonGetCode_Click);
			this.statusStrip1.Items.AddRange(new ToolStripItem[] { this.messageLabel });
			this.statusStrip1.Location = new Point(0, 125);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(391, 25);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 5;
			this.messageLabel.ForeColor = SystemColors.Highlight;
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.messageLabel.Size = new System.Drawing.Size(319, 20);
			this.messageLabel.Text = "Получить sms повторно можно через 45 сек";
			base.AcceptButton = this.buttonOK;
			base.AutoScaleDimensions = new SizeF(12f, 24f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.ClientSize = new System.Drawing.Size(391, 150);
			base.Controls.Add(this.statusStrip1);
			base.Controls.Add(this.buttonGetCode);
			base.Controls.Add(this.textBoxConfirmationCode);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.buttonCancel);
			base.Controls.Add(this.buttonOK);
			this.Font = new System.Drawing.Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Point, 204);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.KeyPreview = true;
			base.Margin = new System.Windows.Forms.Padding(5);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmConfirmationCode";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Введите код";
			base.FormClosing += new FormClosingEventHandler(this.FrmConfirmationCode_FormClosing);
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void InitTimer()
		{
			this.timer = new System.Windows.Forms.Timer()
			{
				Interval = 1000
			};
			this.timer.Tick += new EventHandler(this.timer_Tick);
		}

		private void SetButtonText()
		{
			this.messageLabel.Text = string.Format("Получить sms повторно через {0} сек", this.counter);
		}

		private void StartTimer()
		{
			this.counter = 60;
			this.buttonGetCode.Enabled = false;
			this.messageLabel.Visible = true;
			this.SetButtonText();
			this.timer.Start();
		}

		private void StopTimer()
		{
			this.buttonGetCode.Enabled = true;
			this.messageLabel.Visible = false;
			this.timer.Stop();
		}

		private void timer_Tick(object sender, EventArgs e)
		{
			if (this.counter > 0)
			{
				this.SetButtonText();
			}
			this.counter--;
			if (this.counter < 0)
			{
				this.StopTimer();
			}
		}

		private bool ValidateValues()
		{
			if (string.IsNullOrWhiteSpace(this.ConfirmationCode))
			{
				return false;
			}
			return true;
		}

		public event EventHandler GetCodeRequestEvent;
	}
}