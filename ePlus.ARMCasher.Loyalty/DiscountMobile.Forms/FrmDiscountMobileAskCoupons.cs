using ePlus.ARMCasher.Loyalty.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.DiscountMobile.Forms
{
	internal class FrmDiscountMobileAskCoupons : Form
	{
		public List<long> SelectedCoupons = new List<long>();

		private IContainer components;

		private System.Windows.Forms.Button okButton;

		private System.Windows.Forms.Button cancelButton;

		private CheckedListBox checkedBoxCoupons;

		private System.Windows.Forms.Label label1;

		public FrmDiscountMobileAskCoupons()
		{
			this.InitializeComponent();
			this.SelectedCoupons.Clear();
		}

		private void CancelButtonClick(object sender, EventArgs e)
		{
			this.SelectedCoupons.Clear();
			base.Close();
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
			this.okButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.checkedBoxCoupons = new CheckedListBox();
			this.label1 = new System.Windows.Forms.Label();
			base.SuspendLayout();
			this.okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.okButton.Location = new Point(711, 398);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "ОК";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new EventHandler(this.OkButtonClick);
			this.cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.cancelButton.Location = new Point(609, 398);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Отмена";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new EventHandler(this.CancelButtonClick);
			this.checkedBoxCoupons.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.checkedBoxCoupons.FormattingEnabled = true;
			this.checkedBoxCoupons.HorizontalScrollbar = true;
			this.checkedBoxCoupons.Location = new Point(16, 36);
			this.checkedBoxCoupons.Name = "checkedBoxCoupons";
			this.checkedBoxCoupons.ScrollAlwaysVisible = true;
			this.checkedBoxCoupons.Size = new System.Drawing.Size(801, 349);
			this.checkedBoxCoupons.TabIndex = 2;
			this.checkedBoxCoupons.ThreeDCheckBoxes = true;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(13, 13);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(115, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Использовать купон:";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(837, 434);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.checkedBoxCoupons);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.okButton);
			base.Name = "FrmDiscountMobileAskCoupons";
			this.Text = "Найдены цифровые купоны";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void OkButtonClick(object sender, EventArgs e)
		{
			long num;
			CheckedListBox.CheckedItemCollection checkedItems = this.checkedBoxCoupons.CheckedItems;
			this.SelectedCoupons.Clear();
			foreach (ListItem checkedItem in checkedItems)
			{
				long.TryParse(checkedItem.Value, out num);
				this.SelectedCoupons.Add(num);
			}
			base.Close();
		}

		public void SetCoupons(List<DiscountMobileCouponItem> coupons)
		{
			this.checkedBoxCoupons.Items.Clear();
			foreach (DiscountMobileCouponItem coupon in coupons)
			{
				string[] number = new string[] { coupon.Number, " ", coupon.OfferName, " ", coupon.CouponCondition };
				ListItem listItem = new ListItem(string.Concat(number), coupon.Id.ToString(CultureInfo.InvariantCulture));
				this.checkedBoxCoupons.Items.Add(listItem, false);
			}
		}
	}
}