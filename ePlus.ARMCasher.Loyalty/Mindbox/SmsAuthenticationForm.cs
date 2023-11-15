using ePlus.ARMCommon.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	public class SmsAuthenticationForm : Form
	{
		private IContainer components;

		private ARMTextBox armTextBoxCode;

		private ARMButton armButtonOk;

		private ARMButton armButtonCancel;

		private ARMLabel armLabel1;

		private ARMButton armButtonResend;

		public string Code
		{
			get
			{
				return this.armTextBoxCode.Text;
			}
		}

		public SmsAuthenticationForm()
		{
			this.InitializeComponent();
			base.FormClosing += new FormClosingEventHandler(this.SmsAuthenticationForm_FormClosing);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(SmsAuthenticationForm));
			this.armTextBoxCode = new ARMTextBox();
			this.armButtonOk = new ARMButton();
			this.armButtonCancel = new ARMButton();
			this.armLabel1 = new ARMLabel();
			this.armButtonResend = new ARMButton();
			base.SuspendLayout();
			this.armTextBoxCode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.armTextBoxCode.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armTextBoxCode.Location = new Point(15, 39);
			this.armTextBoxCode.Margin = new System.Windows.Forms.Padding(6);
			this.armTextBoxCode.Name = "armTextBoxCode";
			this.armTextBoxCode.Size = new System.Drawing.Size(387, 23);
			this.armTextBoxCode.TabIndex = 0;
			this.armButtonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.armButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.armButtonOk.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButtonOk.Location = new Point(252, 88);
			this.armButtonOk.Margin = new System.Windows.Forms.Padding(6);
			this.armButtonOk.Name = "armButtonOk";
			this.armButtonOk.Size = new System.Drawing.Size(150, 42);
			this.armButtonOk.TabIndex = 1;
			this.armButtonOk.Text = "ОК";
			this.armButtonOk.UseVisualStyleBackColor = true;
			this.armButtonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.armButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.armButtonCancel.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButtonCancel.Location = new Point(3, 74);
			this.armButtonCancel.Margin = new System.Windows.Forms.Padding(6);
			this.armButtonCancel.Name = "armButtonCancel";
			this.armButtonCancel.Size = new System.Drawing.Size(150, 42);
			this.armButtonCancel.TabIndex = 2;
			this.armButtonCancel.Text = "Отмена";
			this.armButtonCancel.UseVisualStyleBackColor = true;
			this.armButtonCancel.Visible = false;
			this.armLabel1.AutoSize = true;
			this.armLabel1.CharacterCasing = CharacterCasing.Normal;
			this.armLabel1.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel1.Location = new Point(15, 9);
			this.armLabel1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.armLabel1.Name = "armLabel1";
			this.armLabel1.Size = new System.Drawing.Size(181, 16);
			this.armLabel1.TabIndex = 3;
			this.armLabel1.Text = "Код из СМС сообщения:";
			this.armButtonResend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.armButtonResend.DialogResult = System.Windows.Forms.DialogResult.Retry;
			this.armButtonResend.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButtonResend.Location = new Point(15, 88);
			this.armButtonResend.Margin = new System.Windows.Forms.Padding(6);
			this.armButtonResend.Name = "armButtonResend";
			this.armButtonResend.Size = new System.Drawing.Size(225, 42);
			this.armButtonResend.TabIndex = 4;
			this.armButtonResend.Text = "Повторить отправку кода";
			this.armButtonResend.UseVisualStyleBackColor = true;
			base.AcceptButton = this.armButtonOk;
			base.AutoScaleDimensions = new SizeF(10f, 19f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.armButtonCancel;
			base.ClientSize = new System.Drawing.Size(417, 145);
			base.Controls.Add(this.armButtonResend);
			base.Controls.Add(this.armLabel1);
			base.Controls.Add(this.armButtonCancel);
			base.Controls.Add(this.armButtonOk);
			base.Controls.Add(this.armTextBoxCode);
			this.Font = new System.Drawing.Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Point, 204);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Margin = new System.Windows.Forms.Padding(6);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "SmsAuthenticationForm";
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Необходимо ввести код подтверждения";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void SmsAuthenticationForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (base.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}
			if (string.IsNullOrEmpty(this.armTextBoxCode.Text))
			{
				e.Cancel = true;
			}
		}
	}
}