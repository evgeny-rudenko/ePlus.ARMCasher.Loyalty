using ePlus.ARMUtils;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.AstraZeneca.Forms
{
	public class FormConfirmationCode : Form
	{
		private IContainer components;

		private Button buttonOK;

		private Button buttonCancel;

		private Label label1;

		private TextBox textBoxConfirmationCode;

		public string ConfirmationCode
		{
			get
			{
				return this.textBoxConfirmationCode.Text.Trim();
			}
		}

		public FormConfirmationCode()
		{
			this.InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FormConfirmationCode_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (base.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}
			if (!this.ValidateValues())
			{
				UtilsArm.ShowMessageInformationOK("Для продолжения необходимо ввести код подтверждения.");
				e.Cancel = true;
			}
		}

		private void InitializeComponent()
		{
			this.buttonOK = new Button();
			this.buttonCancel = new Button();
			this.label1 = new Label();
			this.textBoxConfirmationCode = new TextBox();
			base.SuspendLayout();
			this.buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new Point(39, 99);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(5);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(150, 45);
			this.buttonOK.TabIndex = 2;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new Point(198, 99);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(5);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(150, 45);
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
			this.textBoxConfirmationCode.Size = new System.Drawing.Size(334, 30);
			this.textBoxConfirmationCode.TabIndex = 1;
			base.AcceptButton = this.buttonOK;
			base.AutoScaleDimensions = new SizeF(12f, 24f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.ClientSize = new System.Drawing.Size(364, 161);
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
			base.Name = "FormConfirmationCode";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Введите код";
			base.FormClosing += new FormClosingEventHandler(this.FormConfirmationCode_FormClosing);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private bool ValidateValues()
		{
			if (string.IsNullOrWhiteSpace(this.ConfirmationCode))
			{
				return false;
			}
			return true;
		}
	}
}