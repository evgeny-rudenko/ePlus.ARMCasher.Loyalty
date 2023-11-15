using ePlus.CommonEx.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.DiscountMobile.Forms
{
	internal class FrmAddPaymentDiscountMobile : Form
	{
		private decimal _maxSum;

		private decimal _chequeSum;

		private bool _blockForm;

		private decimal _scorePerSum;

		private IContainer components;

		private ePlusNumericBox txtSum;

		private Label label1;

		private Button btnOk;

		private Button btnCancel;

		private Label label2;

		private ErrorProvider errorProvider1;

		private TextBox txtMaxSum;

		private TextBox txtChequeSum;

		private Label label3;

		private TextBox txtMaxSumRub;

		private Label label5;

		private ePlusNumericBox txtSumRub;

		private Label label4;

		public bool BlockForm
		{
			get
			{
				return this._blockForm;
			}
			set
			{
				this._blockForm = value;
			}
		}

		public decimal ChequeSum
		{
			get
			{
				return this._chequeSum;
			}
			set
			{
				this._chequeSum = value;
			}
		}

		public decimal MaxSum
		{
			get
			{
				return this._maxSum;
			}
			set
			{
				this._maxSum = value;
			}
		}

		public decimal ScorePerSum
		{
			get
			{
				return this._scorePerSum;
			}
			set
			{
				this._scorePerSum = value;
			}
		}

		public decimal Sum
		{
			get
			{
				return this.txtSum.Value;
			}
			set
			{
				this.txtSum.Value = (int)value;
			}
		}

		public FrmAddPaymentDiscountMobile()
		{
			this.InitializeComponent();
		}

		private void BtnCancelClick(object sender, EventArgs e)
		{
			if (this._blockForm)
			{
				return;
			}
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		private void BtnOkClick(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.errorProvider1.GetError(this.txtSum)))
			{
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
				return;
			}
			base.ActiveControl = this.txtSum;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FrmAddPCXPaymentLoad(object sender, EventArgs e)
		{
			decimal maxSum = this.MaxSum / this.ScorePerSum;
			this.txtMaxSum.Text = this.MaxSum.ToString("#0.00");
			this.txtMaxSumRub.Text = maxSum.ToString("#0.00");
			this.txtChequeSum.Text = this.ChequeSum.ToString("#0.00");
			bool flag = !this._blockForm;
			bool flag1 = flag;
			this.btnCancel.Enabled = flag;
			base.ControlBox = flag1;
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FrmAddPaymentDiscountMobile));
			this.txtSum = new ePlusNumericBox();
			this.label1 = new Label();
			this.btnOk = new Button();
			this.btnCancel = new Button();
			this.label2 = new Label();
			this.errorProvider1 = new ErrorProvider(this.components);
			this.label3 = new Label();
			this.txtChequeSum = new TextBox();
			this.txtMaxSum = new TextBox();
			this.label4 = new Label();
			this.txtSumRub = new ePlusNumericBox();
			this.label5 = new Label();
			this.txtMaxSumRub = new TextBox();
			((ISupportInitialize)this.errorProvider1).BeginInit();
			base.SuspendLayout();
			this.txtSum.DecimalPlaces = 0;
			this.txtSum.DecimalSeparator = '.';
			this.txtSum.Font = new System.Drawing.Font("Arial", 20.25f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtSum.Location = new Point(198, 45);
			this.txtSum.MaxLength = 18;
			this.txtSum.Name = "txtSum";
			this.txtSum.Positive = true;
			this.txtSum.Size = new System.Drawing.Size(133, 39);
			this.txtSum.TabIndex = 0;
			this.txtSum.Text = "0";
			this.txtSum.TextAlign = HorizontalAlignment.Right;
			this.txtSum.ThousandSeparator = ' ';
			this.txtSum.TypingMode = TypingMode.Replace;
			this.txtSum.Value = new decimal(new int[4]);
			this.txtSum.TextChanged += new EventHandler(this.TxtSumValueChanged);
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label1.Location = new Point(20, 55);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(148, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Сумма для списания:";
			this.btnOk.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.btnOk.Location = new Point(81, 124);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(91, 24);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "ОК";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new EventHandler(this.BtnOkClick);
			this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.btnCancel.Location = new Point(178, 124);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(91, 24);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Отмена";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new EventHandler(this.BtnCancelClick);
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label2.ForeColor = SystemColors.GrayText;
			this.label2.Location = new Point(20, 91);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(172, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Максимально возможно:";
			this.errorProvider1.ContainerControl = this;
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label3.Location = new Point(20, 18);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(164, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = "Всего к оплате по чеку:";
			this.txtChequeSum.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtChequeSum.Location = new Point(198, 17);
			this.txtChequeSum.Name = "txtChequeSum";
			this.txtChequeSum.ReadOnly = true;
			this.txtChequeSum.Size = new System.Drawing.Size(133, 22);
			this.txtChequeSum.TabIndex = 3;
			this.txtChequeSum.TabStop = false;
			this.txtChequeSum.TextAlign = HorizontalAlignment.Right;
			this.txtMaxSum.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtMaxSum.Location = new Point(198, 90);
			this.txtMaxSum.Name = "txtMaxSum";
			this.txtMaxSum.ReadOnly = true;
			this.txtMaxSum.Size = new System.Drawing.Size(133, 22);
			this.txtMaxSum.TabIndex = 3;
			this.txtMaxSum.TabStop = false;
			this.txtMaxSum.TextAlign = HorizontalAlignment.Right;
			this.label4.AutoSize = true;
			this.label4.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label4.Location = new Point(538, 55);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(35, 16);
			this.label4.TabIndex = 4;
			this.label4.Text = "руб.";
			this.txtSumRub.DecimalPlaces = 2;
			this.txtSumRub.DecimalSeparator = '.';
			this.txtSumRub.Font = new System.Drawing.Font("Arial", 20.25f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtSumRub.Location = new Point(399, 45);
			this.txtSumRub.MaxLength = 18;
			this.txtSumRub.Name = "txtSumRub";
			this.txtSumRub.Positive = true;
			this.txtSumRub.Size = new System.Drawing.Size(133, 39);
			this.txtSumRub.TabIndex = 5;
			this.txtSumRub.Text = "0.00";
			this.txtSumRub.TextAlign = HorizontalAlignment.Right;
			this.txtSumRub.ThousandSeparator = ' ';
			this.txtSumRub.TypingMode = TypingMode.Replace;
			ePlusNumericBox num = this.txtSumRub;
			int[] numArray = new int[] { 0, 0, 0, 131072 };
			num.Value = new decimal(numArray);
			this.txtSumRub.TextChanged += new EventHandler(this.TxtSumRubValueChanged);
			this.label5.AutoSize = true;
			this.label5.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label5.Location = new Point(337, 55);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(56, 16);
			this.label5.TabIndex = 6;
			this.label5.Text = "баллов";
			this.txtMaxSumRub.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtMaxSumRub.Location = new Point(399, 90);
			this.txtMaxSumRub.Name = "txtMaxSumRub";
			this.txtMaxSumRub.ReadOnly = true;
			this.txtMaxSumRub.Size = new System.Drawing.Size(133, 22);
			this.txtMaxSumRub.TabIndex = 7;
			this.txtMaxSumRub.TabStop = false;
			this.txtMaxSumRub.TextAlign = HorizontalAlignment.Right;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(576, 158);
			base.Controls.Add(this.txtMaxSumRub);
			base.Controls.Add(this.label5);
			base.Controls.Add(this.txtSumRub);
			base.Controls.Add(this.label4);
			base.Controls.Add(this.txtMaxSum);
			base.Controls.Add(this.txtChequeSum);
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.label3);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.txtSum);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmAddPaymentDiscountMobile";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Платеж картой ПЦ";
			base.Load += new EventHandler(this.FrmAddPCXPaymentLoad);
			((ISupportInitialize)this.errorProvider1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Keys key = keyData;
			if (key == Keys.Return)
			{
				this.BtnOkClick(null, null);
			}
			else if (key == Keys.Escape)
			{
				this.BtnCancelClick(null, null);
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void TxtSumRubValueChanged(object sender, EventArgs e)
		{
			this.txtSum.Value = this.txtSumRub.Value * this.ScorePerSum;
		}

		private void TxtSumValueChanged(object sender, EventArgs e)
		{
			this.txtSumRub.Value = this.txtSum.Value / this.ScorePerSum;
			if (this.Sum > this.MaxSum)
			{
				this.errorProvider1.SetError(this.txtSum, "Сумма больше максимально возможной");
				return;
			}
			if (this.Sum == new decimal(0))
			{
				this.errorProvider1.SetError(this.txtSum, "Сумма нулевая");
				return;
			}
			this.errorProvider1.SetError(this.txtSum, "");
		}
	}
}