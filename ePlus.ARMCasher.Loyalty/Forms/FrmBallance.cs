using ePlus.ARMCasher.Loyalty.Cotrols;
using ePlus.ARMCommon;
using ePlus.ARMCommon.Controls;
using ePlus.Loyalty;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmBallance : Form, IBallanceView, IBaseView
	{
		private IContainer components;

		private ucBallance ucBallance1;

		private ARMButton armButton1;

		public FrmBallance()
		{
			this.InitializeComponent();
		}

		public void Bind(LoyaltyCardInfo obj, string caption)
		{
			this.ucBallance1.Bind(obj);
			this.Text = caption;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		void ePlus.ARMCommon.IBaseView.Close()
		{
			base.Close();
		}

		System.Windows.Forms.DialogResult ePlus.ARMCommon.IBaseView.ShowDialog()
		{
			return base.ShowDialog();
		}

		private void FrmBallance_Shown(object sender, EventArgs e)
		{
			this.armButton1.Focus();
		}

		private void InitializeComponent()
		{
			this.armButton1 = new ARMButton();
			this.ucBallance1 = new ucBallance();
			base.SuspendLayout();
			this.armButton1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
			this.armButton1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.armButton1.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButton1.Location = new Point(131, 141);
			this.armButton1.Name = "armButton1";
			this.armButton1.Size = new System.Drawing.Size(105, 28);
			this.armButton1.TabIndex = 1;
			this.armButton1.Text = "ОК";
			this.armButton1.UseVisualStyleBackColor = true;
			this.ucBallance1.AutoSize = true;
			this.ucBallance1.Dock = DockStyle.Top;
			this.ucBallance1.Location = new Point(0, 0);
			this.ucBallance1.Name = "ucBallance1";
			this.ucBallance1.Size = new System.Drawing.Size(374, 90);
			this.ucBallance1.TabIndex = 0;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			base.ClientSize = new System.Drawing.Size(374, 181);
			base.Controls.Add(this.armButton1);
			base.Controls.Add(this.ucBallance1);
			base.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(390, 220);
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(390, 189);
			base.Name = "FrmBallance";
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Баланс";
			base.Shown += new EventHandler(this.FrmBallance_Shown);
			base.ResumeLayout(false);
			base.PerformLayout();
		}
	}
}