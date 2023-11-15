using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty.Cotrols;
using ePlus.ARMCommon.Controls;
using ePlus.Loyalty;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmDebit : Form
	{
		private LoyaltyCardInfo _cardInfo;

		private IContainer components;

		private ePlus.ARMCasher.Loyalty.Cotrols.ucDebit ucDebit;

		private ARMButton btnOk;

		private ARMButton btnCancel;

		public decimal DiscountSum
		{
			get
			{
				if (base.DialogResult != System.Windows.Forms.DialogResult.OK)
				{
					return new decimal(0);
				}
				return this.ucDebit.DiscountSum;
			}
		}

		public string Email
		{
			set
			{
				this.ucDebit.Email = value;
			}
		}

		public decimal MaxAllowSum
		{
			get
			{
				return this.ucDebit.MaxAllowSum;
			}
		}

		public FrmDebit()
		{
			this.InitializeComponent();
			this.btnOk.Click += new EventHandler(this.Ok_Click);
			this.btnCancel.Click += new EventHandler(this.Cancel_Click);
			this.ucDebit.EmailEditEvent += new EventHandler(this.ucDebit_EmailEditEvent);
		}

		public void Bind(ILoyaltyProgram obj, decimal discountSum, CHEQUE cheque)
		{
			this._cardInfo = obj.GetLoyaltyCardInfo(false);
			this.btnOk.Enabled = (this._cardInfo.CardStatusId == LoyaltyCardStatus.Blocked ? false : this._cardInfo.CardStatusId != LoyaltyCardStatus.NotFound);
			this.ucDebit.Bind(obj, discountSum, cheque);
			this.Text = obj.GetDebitOperationDescription();
			if (obj.LoyaltyType == LoyaltyType.SailPlay)
			{
				this.ucDebit.EmailVisible = true;
			}
		}

		private void Cancel()
		{
			base.DialogResult = (this._cardInfo.CardStatusId == LoyaltyCardStatus.Blocked ? System.Windows.Forms.DialogResult.Abort : System.Windows.Forms.DialogResult.Cancel);
			base.Close();
		}

		private void Cancel_Click(object sender, EventArgs e)
		{
			this.Cancel();
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
			this.btnOk = new ARMButton();
			this.btnCancel = new ARMButton();
			this.ucDebit = new ePlus.ARMCasher.Loyalty.Cotrols.ucDebit();
			base.SuspendLayout();
			this.btnOk.Anchor = AnchorStyles.Bottom;
			this.btnOk.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.btnOk.Location = new Point(91, 242);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(108, 27);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "Применить";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnCancel.Anchor = AnchorStyles.Bottom;
			this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.btnCancel.Location = new Point(205, 242);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(108, 27);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Отмена";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.ucDebit.AutoSize = true;
			this.ucDebit.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ucDebit.Dock = DockStyle.Top;
			this.ucDebit.Location = new Point(0, 0);
			this.ucDebit.Name = "ucDebit";
			this.ucDebit.Size = new System.Drawing.Size(393, 194);
			this.ucDebit.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			base.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			base.ClientSize = new System.Drawing.Size(393, 281);
			base.ControlBox = false;
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.ucDebit);
			base.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(409, 320);
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(409, 295);
			base.Name = "FrmDebit";
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "FrmDebit";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void Ok_Click(object sender, EventArgs e)
		{
			if (this.ucDebit.Validate())
			{
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Return)
			{
				if (this.btnOk.Enabled)
				{
					base.DialogResult = System.Windows.Forms.DialogResult.OK;
					base.Close();
				}
			}
			else if (keyData == Keys.Escape)
			{
				this.Cancel();
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void ucDebit_EmailEditEvent(object sender, EventArgs e)
		{
			if (this.EmailEditEvent != null)
			{
				this.EmailEditEvent(this, e);
			}
		}

		public event EventHandler EmailEditEvent;
	}
}