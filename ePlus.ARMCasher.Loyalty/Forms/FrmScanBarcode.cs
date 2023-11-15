using ePlus.ARMCasherNew.Controls;
using ePlus.CommonEx.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmScanBarcode : Form
	{
		private IContainer components;

		private Button btnCancel;

		private Button btnOk;

		private Label lbTitle;

		private ARMPCXBarcodeTextBox txtBarcode;

		public string Barcode
		{
			get
			{
				return this.txtBarcode.Text;
			}
		}

		public string Title
		{
			get
			{
				return this.lbTitle.Text;
			}
			set
			{
				this.lbTitle.Text = value;
			}
		}

		public FrmScanBarcode()
		{
			this.InitializeComponent();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.OK;
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
			this.btnCancel = new Button();
			this.btnOk = new Button();
			this.lbTitle = new Label();
			this.txtBarcode = new ARMPCXBarcodeTextBox();
			base.SuspendLayout();
			this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnCancel.Location = new Point(287, 83);
			this.btnCancel.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(150, 45);
			this.btnCancel.TabIndex = 3;
			this.btnCancel.Text = "Отмена";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			this.btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnOk.Enabled = false;
			this.btnOk.Location = new Point(125, 83);
			this.btnOk.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(150, 45);
			this.btnOk.TabIndex = 2;
			this.btnOk.Text = "ОК";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new EventHandler(this.btnOk_Click);
			this.lbTitle.AutoSize = true;
			this.lbTitle.Location = new Point(15, 9);
			this.lbTitle.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
			this.lbTitle.Name = "lbTitle";
			this.lbTitle.Size = new System.Drawing.Size(122, 24);
			this.lbTitle.TabIndex = 0;
			this.lbTitle.Text = "Штрих-код:";
			this.txtBarcode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtBarcode.AutoValidating = false;
			this.txtBarcode.BarcodeType = BarcodeType.Other;
			this.txtBarcode.Location = new Point(15, 39);
			this.txtBarcode.Margin = new System.Windows.Forms.Padding(6);
			this.txtBarcode.Name = "txtBarcode";
			this.txtBarcode.SendDataRecivedOnEnter = true;
			this.txtBarcode.Size = new System.Drawing.Size(422, 30);
			this.txtBarcode.TabIndex = 1;
			this.txtBarcode.UseBarcodeValidation = false;
			this.txtBarcode.TextChanged += new EventHandler(this.txtBarcode_TextChanged);
			base.AutoScaleDimensions = new SizeF(12f, 24f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(452, 143);
			base.Controls.Add(this.txtBarcode);
			base.Controls.Add(this.lbTitle);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.btnCancel);
			this.Font = new System.Drawing.Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Point, 204);
			base.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmScanBarcode";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Введите ШК карты программы лояльности";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Keys key = keyData;
			if (key != Keys.Return)
			{
				if (key == Keys.Escape)
				{
					this.btnCancel_Click(null, null);
				}
			}
			else if (this.btnOk.Enabled)
			{
				this.btnOk_Click(null, null);
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void txtBarcode_TextChanged(object sender, EventArgs e)
		{
			this.btnOk.Enabled = !string.IsNullOrEmpty(this.txtBarcode.Text.Trim());
		}
	}
}