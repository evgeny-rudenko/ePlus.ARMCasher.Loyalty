using ePlus.ARMCommon.Controls;
using ePlus.CommonEx.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmScanBarcodeEx : Form
	{
		private IContainer components;

		private ARMBarcodeTextBox armBarcodeTextBoxCardNumber;

		private ARMButton armButtonOk;

		private ARMButton armButtonCancel;

		private ARMMaskedTextBox armMaskedTextBoxPhone;

		private ArmRadioButton armRadioButtonCardNumber;

		private ArmRadioButton armRadioButtonPhone;

		private ToolTip toolTipError;

		private ARMBarcodeTextBox armBarcodeTextBoxPromocode;

		private ARMLabel labelPromocodeHeader;

		public string Barcode
		{
			get
			{
				if (!this.armRadioButtonPhone.Checked || !this.armMaskedTextBoxPhone.MaskFull)
				{
					if (!this.armRadioButtonCardNumber.Checked)
					{
						return string.Empty;
					}
					return this.armBarcodeTextBoxCardNumber.Text;
				}
				return this.armMaskedTextBoxPhone.Text.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("-", "");
			}
		}

		public string Promocode
		{
			get
			{
				return this.armBarcodeTextBoxPromocode.Text;
			}
		}

		public FrmScanBarcodeEx(bool allowPromocode)
		{
			this.InitializeComponent();
			this.armBarcodeTextBoxPromocode.Enabled = allowPromocode;
			this.armBarcodeTextBoxCardNumber.Focus();
		}

		private void armBarcodeTextBoxCardNumber_Enter(object sender, EventArgs e)
		{
			this.armRadioButtonCardNumber.Checked = true;
		}

		private void armBarcodeTextBoxCardNumber_KeyUp(object sender, KeyEventArgs e)
		{
			this.CheckForEnterPressed(e);
		}

		private void armBarcodeTextBoxPromocode_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				this.armButtonOk_Click(null, null);
			}
			if (e.KeyCode == Keys.Escape)
			{
				this.armButtonCancel_Click(null, null);
			}
		}

		private void armButtonCancel_Click(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			base.Close();
		}

		private void armButtonOk_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(this.armBarcodeTextBoxPromocode.Text))
			{
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
				base.Close();
				return;
			}
			if (this.armRadioButtonPhone.Checked && !this.armMaskedTextBoxPhone.MaskFull)
			{
				base.DialogResult = System.Windows.Forms.DialogResult.None;
				this.toolTipError.Show("Некорректно заполнен номер телефона!", this.armMaskedTextBoxPhone, 0, -40, 5000);
				this.armMaskedTextBoxPhone.Focus();
				return;
			}
			if (!this.armRadioButtonCardNumber.Checked || !string.IsNullOrEmpty(this.armBarcodeTextBoxCardNumber.Text))
			{
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
				base.Close();
				return;
			}
			base.DialogResult = System.Windows.Forms.DialogResult.None;
			this.toolTipError.Show("Необходимо ввести штрих код карты лояльности!", this.armBarcodeTextBoxCardNumber, 0, -40, 5000);
			this.armBarcodeTextBoxCardNumber.Focus();
		}

		private void armMaskedTextBoxPhone_Enter(object sender, EventArgs e)
		{
			this.armRadioButtonPhone.Checked = true;
		}

		private void armMaskedTextBoxPhone_KeyUp(object sender, KeyEventArgs e)
		{
			this.CheckForEnterPressed(e);
		}

		private void armRadioButtonCardNumber_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.armRadioButtonCardNumber.Checked || this.armBarcodeTextBoxCardNumber.Focused)
			{
				return;
			}
			this.armBarcodeTextBoxCardNumber.Focus();
		}

		private void armRadioButtonPhone_CheckedChanged(object sender, EventArgs e)
		{
			if (!this.armRadioButtonPhone.Checked || this.armMaskedTextBoxPhone.Focused)
			{
				return;
			}
			this.armMaskedTextBoxPhone.Focus();
			this.armMaskedTextBoxPhone.Select(4, 0);
		}

		private void CheckForEnterPressed(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				if (!this.armBarcodeTextBoxPromocode.Enabled)
				{
					this.armButtonOk_Click(null, null);
				}
				else
				{
					this.armBarcodeTextBoxPromocode.Focus();
				}
			}
			if (e.KeyCode == Keys.Escape)
			{
				this.armButtonCancel_Click(null, null);
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

		private void FrmScanBarcodeEx_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (base.DialogResult == System.Windows.Forms.DialogResult.None)
			{
				e.Cancel = true;
			}
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FrmScanBarcodeEx));
			this.armButtonCancel = new ARMButton();
			this.armButtonOk = new ARMButton();
			this.armBarcodeTextBoxCardNumber = new ARMBarcodeTextBox();
			this.armMaskedTextBoxPhone = new ARMMaskedTextBox();
			this.armRadioButtonCardNumber = new ArmRadioButton();
			this.armRadioButtonPhone = new ArmRadioButton();
			this.toolTipError = new ToolTip(this.components);
			this.armBarcodeTextBoxPromocode = new ARMBarcodeTextBox();
			this.labelPromocodeHeader = new ARMLabel();
			base.SuspendLayout();
			this.armButtonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.armButtonCancel.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButtonCancel.Location = new Point(267, 253);
			this.armButtonCancel.Margin = new System.Windows.Forms.Padding(6);
			this.armButtonCancel.Name = "armButtonCancel";
			this.armButtonCancel.Size = new System.Drawing.Size(150, 42);
			this.armButtonCancel.TabIndex = 8;
			this.armButtonCancel.TabStop = false;
			this.armButtonCancel.Text = "Отмена";
			this.armButtonCancel.UseVisualStyleBackColor = true;
			this.armButtonCancel.Click += new EventHandler(this.armButtonCancel_Click);
			this.armButtonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.armButtonOk.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButtonOk.Location = new Point(105, 253);
			this.armButtonOk.Margin = new System.Windows.Forms.Padding(6);
			this.armButtonOk.Name = "armButtonOk";
			this.armButtonOk.Size = new System.Drawing.Size(150, 42);
			this.armButtonOk.TabIndex = 7;
			this.armButtonOk.TabStop = false;
			this.armButtonOk.Text = "ОК";
			this.armButtonOk.UseVisualStyleBackColor = true;
			this.armButtonOk.Click += new EventHandler(this.armButtonOk_Click);
			this.armBarcodeTextBoxCardNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.armBarcodeTextBoxCardNumber.AutoValidating = false;
			this.armBarcodeTextBoxCardNumber.BarcodeType = BarcodeType.Other;
			this.armBarcodeTextBoxCardNumber.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armBarcodeTextBoxCardNumber.Location = new Point(15, 50);
			this.armBarcodeTextBoxCardNumber.Margin = new System.Windows.Forms.Padding(6);
			this.armBarcodeTextBoxCardNumber.Name = "armBarcodeTextBoxCardNumber";
			this.armBarcodeTextBoxCardNumber.SendDataRecivedOnEnter = true;
			this.armBarcodeTextBoxCardNumber.Size = new System.Drawing.Size(402, 23);
			this.armBarcodeTextBoxCardNumber.TabIndex = 2;
			this.armBarcodeTextBoxCardNumber.UseBarcodeValidation = false;
			this.armBarcodeTextBoxCardNumber.Enter += new EventHandler(this.armBarcodeTextBoxCardNumber_Enter);
			this.armBarcodeTextBoxCardNumber.KeyUp += new KeyEventHandler(this.armBarcodeTextBoxCardNumber_KeyUp);
			this.armMaskedTextBoxPhone.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.armMaskedTextBoxPhone.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armMaskedTextBoxPhone.Location = new Point(15, 137);
			this.armMaskedTextBoxPhone.Margin = new System.Windows.Forms.Padding(6);
			this.armMaskedTextBoxPhone.Mask = "+7 (000) 000-0000";
			this.armMaskedTextBoxPhone.Name = "armMaskedTextBoxPhone";
			this.armMaskedTextBoxPhone.PromptChar = '*';
			this.armMaskedTextBoxPhone.Size = new System.Drawing.Size(402, 23);
			this.armMaskedTextBoxPhone.TabIndex = 11;
			this.armMaskedTextBoxPhone.Enter += new EventHandler(this.armMaskedTextBoxPhone_Enter);
			this.armMaskedTextBoxPhone.KeyUp += new KeyEventHandler(this.armMaskedTextBoxPhone_KeyUp);
			this.armRadioButtonCardNumber.AutoSize = true;
			this.armRadioButtonCardNumber.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armRadioButtonCardNumber.Location = new Point(15, 15);
			this.armRadioButtonCardNumber.Margin = new System.Windows.Forms.Padding(6);
			this.armRadioButtonCardNumber.Name = "armRadioButtonCardNumber";
			this.armRadioButtonCardNumber.Size = new System.Drawing.Size(105, 20);
			this.armRadioButtonCardNumber.TabIndex = 12;
			this.armRadioButtonCardNumber.Text = "Штрих код:";
			this.armRadioButtonCardNumber.UseVisualStyleBackColor = true;
			this.armRadioButtonCardNumber.CheckedChanged += new EventHandler(this.armRadioButtonCardNumber_CheckedChanged);
			this.armRadioButtonPhone.AutoSize = true;
			this.armRadioButtonPhone.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armRadioButtonPhone.Location = new Point(15, 102);
			this.armRadioButtonPhone.Margin = new System.Windows.Forms.Padding(6);
			this.armRadioButtonPhone.Name = "armRadioButtonPhone";
			this.armRadioButtonPhone.Size = new System.Drawing.Size(152, 20);
			this.armRadioButtonPhone.TabIndex = 13;
			this.armRadioButtonPhone.Text = "Номер телефона:";
			this.armRadioButtonPhone.UseVisualStyleBackColor = true;
			this.armRadioButtonPhone.CheckedChanged += new EventHandler(this.armRadioButtonPhone_CheckedChanged);
			this.toolTipError.AutomaticDelay = 2000;
			this.toolTipError.ToolTipIcon = ToolTipIcon.Error;
			this.toolTipError.ToolTipTitle = "Ошибка";
			this.armBarcodeTextBoxPromocode.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.armBarcodeTextBoxPromocode.AutoValidating = false;
			this.armBarcodeTextBoxPromocode.BarcodeType = BarcodeType.Other;
			this.armBarcodeTextBoxPromocode.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armBarcodeTextBoxPromocode.Location = new Point(15, 207);
			this.armBarcodeTextBoxPromocode.Margin = new System.Windows.Forms.Padding(6);
			this.armBarcodeTextBoxPromocode.Name = "armBarcodeTextBoxPromocode";
			this.armBarcodeTextBoxPromocode.SendDataRecivedOnEnter = true;
			this.armBarcodeTextBoxPromocode.Size = new System.Drawing.Size(402, 23);
			this.armBarcodeTextBoxPromocode.TabIndex = 14;
			this.armBarcodeTextBoxPromocode.UseBarcodeValidation = false;
			this.armBarcodeTextBoxPromocode.KeyUp += new KeyEventHandler(this.armBarcodeTextBoxPromocode_KeyUp);
			this.labelPromocodeHeader.AutoSize = true;
			this.labelPromocodeHeader.CharacterCasing = CharacterCasing.Normal;
			this.labelPromocodeHeader.Font = new System.Drawing.Font("Arial", 10f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.labelPromocodeHeader.Location = new Point(12, 182);
			this.labelPromocodeHeader.Name = "labelPromocodeHeader";
			this.labelPromocodeHeader.Size = new System.Drawing.Size(81, 16);
			this.labelPromocodeHeader.TabIndex = 15;
			this.labelPromocodeHeader.Text = "Промокод";
			base.AutoScaleDimensions = new SizeF(10f, 19f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(432, 310);
			base.Controls.Add(this.labelPromocodeHeader);
			base.Controls.Add(this.armBarcodeTextBoxPromocode);
			base.Controls.Add(this.armRadioButtonPhone);
			base.Controls.Add(this.armRadioButtonCardNumber);
			base.Controls.Add(this.armMaskedTextBoxPhone);
			base.Controls.Add(this.armButtonCancel);
			base.Controls.Add(this.armButtonOk);
			base.Controls.Add(this.armBarcodeTextBoxCardNumber);
			this.Font = new System.Drawing.Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Point, 204);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.Margin = new System.Windows.Forms.Padding(6);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmScanBarcodeEx";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "FrmScanBarcodeEx";
			base.FormClosing += new FormClosingEventHandler(this.FrmScanBarcodeEx_FormClosing);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}