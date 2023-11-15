using ePlus.CommonEx.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.PCX.Forms
{
	internal class FrmAddPCXPayment : Form
	{
		private decimal maxSum;

		private decimal chequeSum;

		private bool blockForm;

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

		public bool BlockForm
		{
			get
			{
				return this.blockForm;
			}
			set
			{
				this.blockForm = value;
			}
		}

		public decimal ChequeSum
		{
			get
			{
				return this.chequeSum;
			}
			set
			{
				this.chequeSum = value;
			}
		}

		public decimal MaxSum
		{
			get
			{
				return this.maxSum;
			}
			set
			{
				this.maxSum = value;
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
				this.txtSum.Value = value;
			}
		}

		public FrmAddPCXPayment()
		{
			this.InitializeComponent();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			if (this.blockForm)
			{
				return;
			}
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		private void btnOk_Click(object sender, EventArgs e)
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

		private void FrmAddPCXPayment_Load(object sender, EventArgs e)
		{
			this.txtMaxSum.Text = this.MaxSum.ToString("#0.00");
			this.txtChequeSum.Text = this.ChequeSum.ToString("#0.00");
			bool flag = !this.blockForm;
			bool flag1 = flag;
			this.btnCancel.Enabled = flag;
			base.ControlBox = flag1;
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FrmAddPCXPayment));
			this.txtSum = new ePlusNumericBox();
			this.label1 = new Label();
			this.btnOk = new Button();
			this.btnCancel = new Button();
			this.label2 = new Label();
			this.errorProvider1 = new ErrorProvider(this.components);
			this.label3 = new Label();
			this.txtChequeSum = new TextBox();
			this.txtMaxSum = new TextBox();
			((ISupportInitialize)this.errorProvider1).BeginInit();
			base.SuspendLayout();
			this.txtSum.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtSum.DecimalPlaces = 2;
			this.txtSum.DecimalSeparator = '.';
			this.txtSum.Font = new System.Drawing.Font("Arial", 20.25f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtSum.Location = new Point(203, 40);
			this.txtSum.MaxLength = 18;
			this.txtSum.Name = "txtSum";
			this.txtSum.Positive = true;
			this.txtSum.Size = new System.Drawing.Size(145, 39);
			this.txtSum.TabIndex = 0;
			this.txtSum.Text = "0.00";
			this.txtSum.TextAlign = HorizontalAlignment.Right;
			this.txtSum.ThousandSeparator = ' ';
			this.txtSum.TypingMode = TypingMode.Replace;
			ePlusNumericBox num = this.txtSum;
			int[] numArray = new int[] { 0, 0, 0, 131072 };
			num.Value = new decimal(numArray);
			this.txtSum.TextChanged += new EventHandler(this.txtSum_ValueChanged);
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label1.Location = new Point(12, 51);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(148, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Сумма для списания:";
			this.btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnOk.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.btnOk.Location = new Point(162, 131);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(90, 28);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "ОК";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new EventHandler(this.btnOk_Click);
			this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.btnCancel.Location = new Point(258, 131);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(90, 28);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Отмена";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label2.ForeColor = SystemColors.GrayText;
			this.label2.Location = new Point(12, 88);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(172, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Максимально возможно:";
			this.errorProvider1.ContainerControl = this;
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.label3.Location = new Point(12, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(164, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = "Всего к оплате по чеку:";
			this.txtChequeSum.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtChequeSum.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtChequeSum.Location = new Point(203, 12);
			this.txtChequeSum.Name = "txtChequeSum";
			this.txtChequeSum.ReadOnly = true;
			this.txtChequeSum.Size = new System.Drawing.Size(145, 22);
			this.txtChequeSum.TabIndex = 3;
			this.txtChequeSum.TabStop = false;
			this.txtChequeSum.TextAlign = HorizontalAlignment.Right;
			this.txtMaxSum.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtMaxSum.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtMaxSum.Location = new Point(203, 85);
			this.txtMaxSum.Name = "txtMaxSum";
			this.txtMaxSum.ReadOnly = true;
			this.txtMaxSum.Size = new System.Drawing.Size(145, 22);
			this.txtMaxSum.TabIndex = 3;
			this.txtMaxSum.TabStop = false;
			this.txtMaxSum.TextAlign = HorizontalAlignment.Right;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(360, 171);
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
			base.Name = "FrmAddPCXPayment";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterScreen;
			this.Text = "Платеж картой ПЦ";
			base.Load += new EventHandler(this.FrmAddPCXPayment_Load);
			((ISupportInitialize)this.errorProvider1).EndInit();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			Keys key = keyData & Keys.KeyCode;
			if (keyData == Keys.Return)
			{
				this.btnOk_Click(null, null);
			}
			else if (keyData == Keys.Escape)
			{
				this.btnCancel_Click(null, null);
			}
			else if (key == Keys.F4 && (keyData & Keys.Alt) != Keys.None && this.blockForm)
			{
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void txtSum_ValueChanged(object sender, EventArgs e)
		{
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