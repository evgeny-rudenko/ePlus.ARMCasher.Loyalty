using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCommon.Controls;
using ePlus.ARMUtils;
using ePlus.CommonEx.Controls;
using ePlus.Loyalty;
using ePlus.Loyalty.Domestic;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Cotrols
{
	public class ucDebit : UserControl
	{
		private IContainer components;

		private ucBallance ucBallance1;

		private ARMLabel armLabel1;

		private ARMLabel armLabel2;

		private ARMLabel armLabel3;

		private ePlusNumericBox chequeTotal;

		private ePlusNumericBox maxAllowSum;

		private ePlusNumericBox txtSum;

		private Panel panel1;

		public decimal DiscountSum
		{
			get
			{
				return this.txtSum.Value;
			}
		}

		public string Email
		{
			set
			{
				this.ucBallance1.Email = value;
			}
		}

		public bool EmailVisible
		{
			get
			{
				return this.ucBallance1.EmailVisible;
			}
			set
			{
				this.ucBallance1.EmailVisible = value;
			}
		}

		public decimal MaxAllowSum
		{
			get
			{
				return this.maxAllowSum.Value;
			}
		}

		public ucDebit()
		{
			this.InitializeComponent();
			this.ucBallance1.EmailEditEvent += new EventHandler(this.ucBallance1_EmailEditEvent);
		}

		public void Bind(ILoyaltyProgram obj, decimal discountSum, CHEQUE cheque)
		{
			decimal num;
			decimal num1;
			Params loyaltyParams = null;
			if (obj.LoyaltyType != LoyaltyType.Domestic)
			{
				this.txtSum.DecimalPlaces = 0;
				this.txtSum.Text = "0";
			}
			else
			{
				this.txtSum.DecimalPlaces = 2;
				this.txtSum.Text = "0,00";
				loyaltyParams = obj.GetLoyaltyParams() as Params;
			}
			LoyaltyCardInfo loyaltyCardInfo = obj.GetLoyaltyCardInfo(false);
			this.ucBallance1.Bind(loyaltyCardInfo);
			decimal num2 = obj.CalculateMaxSumBonus(cheque);
			this.maxAllowSum.Value = num2;
			this.chequeTotal.Value = cheque.SUMM + obj.CalculateDiscountSum(cheque);
			if (obj.LoyaltyType == LoyaltyType.Domestic && loyaltyParams != null && loyaltyParams.ApplyDiscountAsPrepayment)
			{
				ePlusNumericBox _ePlusNumericBox = this.txtSum;
				if (discountSum == new decimal(0))
				{
					num1 = num2;
				}
				else
				{
					num1 = (discountSum < num2 ? discountSum : num2);
				}
				_ePlusNumericBox.Value = num1;
				return;
			}
			ePlusNumericBox _ePlusNumericBox1 = this.txtSum;
			if (discountSum == new decimal(0))
			{
				num = num2;
			}
			else
			{
				num = (discountSum < num2 ? discountSum : num2);
			}
			_ePlusNumericBox1.Value = Math.Truncate(num);
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
			this.armLabel1 = new ARMLabel();
			this.armLabel2 = new ARMLabel();
			this.armLabel3 = new ARMLabel();
			this.chequeTotal = new ePlusNumericBox();
			this.maxAllowSum = new ePlusNumericBox();
			this.txtSum = new ePlusNumericBox();
			this.panel1 = new Panel();
			this.ucBallance1 = new ucBallance();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.armLabel1.AutoSize = true;
			this.armLabel1.CharacterCasing = CharacterCasing.Normal;
			this.armLabel1.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel1.Location = new Point(5, 9);
			this.armLabel1.Name = "armLabel1";
			this.armLabel1.Size = new System.Drawing.Size(164, 16);
			this.armLabel1.TabIndex = 1;
			this.armLabel1.Text = "Всего к оплате по чеку:";
			this.armLabel2.AutoSize = true;
			this.armLabel2.CharacterCasing = CharacterCasing.Normal;
			this.armLabel2.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel2.Location = new Point(5, 49);
			this.armLabel2.Name = "armLabel2";
			this.armLabel2.Size = new System.Drawing.Size(148, 16);
			this.armLabel2.TabIndex = 1;
			this.armLabel2.Text = "Сумма для списания:";
			this.armLabel3.AutoSize = true;
			this.armLabel3.CharacterCasing = CharacterCasing.Normal;
			this.armLabel3.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel3.Location = new Point(5, 82);
			this.armLabel3.Name = "armLabel3";
			this.armLabel3.Size = new System.Drawing.Size(172, 16);
			this.armLabel3.TabIndex = 1;
			this.armLabel3.Text = "Максимально возможно:";
			this.chequeTotal.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.chequeTotal.DecimalPlaces = 2;
			this.chequeTotal.DecimalSeparator = ',';
			this.chequeTotal.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold);
			this.chequeTotal.Location = new Point(181, 6);
			this.chequeTotal.MaxLength = 18;
			this.chequeTotal.Name = "chequeTotal";
			this.chequeTotal.Nullable = false;
			this.chequeTotal.Positive = true;
			this.chequeTotal.ReadOnly = true;
			this.chequeTotal.Size = new System.Drawing.Size(468, 22);
			this.chequeTotal.TabIndex = 3;
			this.chequeTotal.Text = "0,00";
			this.chequeTotal.TextAlign = HorizontalAlignment.Right;
			this.chequeTotal.ThousandSeparator = ' ';
			this.chequeTotal.TypingMode = TypingMode.Replace;
			ePlusNumericBox num = this.chequeTotal;
			int[] numArray = new int[] { 0, 0, 0, 131072 };
			num.Value = new decimal(numArray);
			this.maxAllowSum.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.maxAllowSum.DecimalPlaces = 2;
			this.maxAllowSum.DecimalSeparator = ',';
			this.maxAllowSum.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold);
			this.maxAllowSum.Location = new Point(181, 79);
			this.maxAllowSum.MaxLength = 18;
			this.maxAllowSum.Name = "maxAllowSum";
			this.maxAllowSum.Nullable = false;
			this.maxAllowSum.Positive = true;
			this.maxAllowSum.ReadOnly = true;
			this.maxAllowSum.Size = new System.Drawing.Size(468, 22);
			this.maxAllowSum.TabIndex = 3;
			this.maxAllowSum.Text = "0,00";
			this.maxAllowSum.TextAlign = HorizontalAlignment.Right;
			this.maxAllowSum.ThousandSeparator = ' ';
			this.maxAllowSum.TypingMode = TypingMode.Replace;
			ePlusNumericBox _ePlusNumericBox = this.maxAllowSum;
			int[] numArray1 = new int[] { 0, 0, 0, 131072 };
			_ePlusNumericBox.Value = new decimal(numArray1);
			this.txtSum.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtSum.DecimalPlaces = 0;
			this.txtSum.DecimalSeparator = ',';
			this.txtSum.Font = new System.Drawing.Font("Arial", 20.25f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtSum.Location = new Point(181, 34);
			this.txtSum.MaxLength = 18;
			this.txtSum.Name = "txtSum";
			this.txtSum.Nullable = false;
			this.txtSum.Positive = true;
			this.txtSum.Size = new System.Drawing.Size(468, 39);
			this.txtSum.TabIndex = 3;
			this.txtSum.Text = "0";
			this.txtSum.TextAlign = HorizontalAlignment.Right;
			this.txtSum.ThousandSeparator = ' ';
			this.txtSum.TypingMode = TypingMode.Replace;
			this.txtSum.Value = new decimal(new int[4]);
			this.panel1.AutoSize = true;
			this.panel1.Controls.Add(this.armLabel1);
			this.panel1.Controls.Add(this.maxAllowSum);
			this.panel1.Controls.Add(this.armLabel2);
			this.panel1.Controls.Add(this.chequeTotal);
			this.panel1.Controls.Add(this.armLabel3);
			this.panel1.Controls.Add(this.txtSum);
			this.panel1.Dock = DockStyle.Top;
			this.panel1.Location = new Point(0, 90);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(665, 104);
			this.panel1.TabIndex = 4;
			this.ucBallance1.AutoSize = true;
			this.ucBallance1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ucBallance1.Dock = DockStyle.Top;
			this.ucBallance1.Location = new Point(0, 0);
			this.ucBallance1.Name = "ucBallance1";
			this.ucBallance1.Size = new System.Drawing.Size(665, 90);
			this.ucBallance1.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			base.Controls.Add(this.panel1);
			base.Controls.Add(this.ucBallance1);
			base.Name = "ucDebit";
			base.Size = new System.Drawing.Size(665, 232);
			base.Load += new EventHandler(this.ucDebit_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Return && !this.Validate())
			{
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void ucBallance1_EmailEditEvent(object sender, EventArgs e)
		{
			if (this.EmailEditEvent != null)
			{
				this.EmailEditEvent(this, e);
			}
		}

		private void ucDebit_Load(object sender, EventArgs e)
		{
			this.txtSum.Select();
			this.txtSum.SelectAll();
		}

		public new bool Validate()
		{
			decimal num = (this.maxAllowSum.Value > this.chequeTotal.Value ? this.chequeTotal.Value : this.maxAllowSum.Value);
			if (this.txtSum.Value <= num)
			{
				return true;
			}
			UtilsArm.ShowMessageExclamationOK(string.Format("Сумма для списания не может превышать {0:#,##0.00}", num), "Ошибка");
			this.txtSum.Select();
			this.txtSum.SelectAll();
			return false;
		}

		public event EventHandler EmailEditEvent;
	}
}