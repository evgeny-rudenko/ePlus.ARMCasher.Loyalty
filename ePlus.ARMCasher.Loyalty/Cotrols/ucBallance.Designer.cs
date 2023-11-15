using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCasher.Loyalty.Forms;
using ePlus.ARMCommon.Controls;
using ePlus.Loyalty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Cotrols
{
	public class ucBallance : UserControl
	{
		private LoyaltyCardInfo m_cardInfo;

		private IContainer components;

		private ARMTextBox txtStatus;

		private ARMTextBox txtBallance;

		private ARMTextBox txtClientId;

		private ARMLabel armLabel3;

		private ARMLabel armLabel2;

		private ARMLabel lbClientId;

		private LinkLabel linkLabelDetails;

		private Panel panelClientName;

		private ARMTextBox txtClientName;

		private ARMLabel armLabel1;

		private ARMLabel lbEmail;

		private Panel panelCardNumber;

		private Panel panelBalance;

		private Panel panelStatus;

		private Panel panelEmail;

		private Panel panel1;

		private ARMTextBox txtEmail;

		public Button buttonEmailEdit;

		public string Email
		{
			set
			{
				this.txtEmail.Text = value;
			}
		}

		public bool EmailVisible
		{
			get
			{
				return this.panelEmail.Visible;
			}
			set
			{
				this.panelEmail.Visible = value;
			}
		}

		public ucBallance()
		{
			this.InitializeComponent();
		}

		public void Bind(LoyaltyCardInfo obj)
		{
			this.lbClientId.Text = LoyaltyProgManager.GetPublicIdTypeTitle(obj.ClientIdType);
			this.txtClientId.Text = LoyaltyProgManager.FormatPublicId(obj.GetPublicId(), obj.ClientIdType);
			this.txtBallance.DataBindings.Add("Text", obj, "Balance", true, DataSourceUpdateMode.OnPropertyChanged, string.Empty, "N2");
			this.txtStatus.DataBindings.Add("Text", obj, "CardStatus", false, DataSourceUpdateMode.OnPropertyChanged);
			if (!string.IsNullOrEmpty(obj.ClientName))
			{
				this.panelClientName.Visible = true;
				this.txtClientName.DataBindings.Add("Text", obj, "ClientName", false, DataSourceUpdateMode.OnPropertyChanged);
			}
			this.linkLabelDetails.Visible = (obj.BalanceDetails == null ? false : obj.BalanceDetails.Count > 0);
			this.m_cardInfo = obj;
			this.txtEmail.Text = obj.ClientEmail;
		}

		private void buttonEmailEdit_Click(object sender, EventArgs e)
		{
			if (this.EmailEditEvent != null)
			{
				this.EmailEditEvent(this, e);
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

		private void InitializeComponent()
		{
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(ucBallance));
			this.linkLabelDetails = new LinkLabel();
			this.panelClientName = new Panel();
			this.panelCardNumber = new Panel();
			this.panelBalance = new Panel();
			this.panelStatus = new Panel();
			this.panelEmail = new Panel();
			this.panel1 = new Panel();
			this.buttonEmailEdit = new Button();
			this.armLabel3 = new ARMLabel();
			this.txtStatus = new ARMTextBox();
			this.armLabel2 = new ARMLabel();
			this.txtBallance = new ARMTextBox();
			this.txtClientName = new ARMTextBox();
			this.armLabel1 = new ARMLabel();
			this.txtEmail = new ARMTextBox();
			this.lbEmail = new ARMLabel();
			this.lbClientId = new ARMLabel();
			this.txtClientId = new ARMTextBox();
			this.panelClientName.SuspendLayout();
			this.panelCardNumber.SuspendLayout();
			this.panelBalance.SuspendLayout();
			this.panelStatus.SuspendLayout();
			this.panelEmail.SuspendLayout();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.linkLabelDetails.AutoSize = true;
			this.linkLabelDetails.Location = new Point(118, 8);
			this.linkLabelDetails.Name = "linkLabelDetails";
			this.linkLabelDetails.Size = new System.Drawing.Size(57, 13);
			this.linkLabelDetails.TabIndex = 9;
			this.linkLabelDetails.TabStop = true;
			this.linkLabelDetails.Text = "Подробно";
			this.linkLabelDetails.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabelDetails_LinkClicked);
			this.panelClientName.Controls.Add(this.txtClientName);
			this.panelClientName.Controls.Add(this.armLabel1);
			this.panelClientName.Dock = DockStyle.Top;
			this.panelClientName.Location = new Point(0, 60);
			this.panelClientName.Name = "panelClientName";
			this.panelClientName.Size = new System.Drawing.Size(461, 30);
			this.panelClientName.TabIndex = 10;
			this.panelClientName.Visible = false;
			this.panelCardNumber.Controls.Add(this.lbClientId);
			this.panelCardNumber.Controls.Add(this.txtClientId);
			this.panelCardNumber.Dock = DockStyle.Top;
			this.panelCardNumber.Location = new Point(0, 0);
			this.panelCardNumber.Name = "panelCardNumber";
			this.panelCardNumber.Padding = new System.Windows.Forms.Padding(3);
			this.panelCardNumber.Size = new System.Drawing.Size(461, 30);
			this.panelCardNumber.TabIndex = 11;
			this.panelBalance.Controls.Add(this.armLabel2);
			this.panelBalance.Controls.Add(this.txtBallance);
			this.panelBalance.Controls.Add(this.linkLabelDetails);
			this.panelBalance.Dock = DockStyle.Top;
			this.panelBalance.Location = new Point(0, 90);
			this.panelBalance.Name = "panelBalance";
			this.panelBalance.Size = new System.Drawing.Size(461, 30);
			this.panelBalance.TabIndex = 12;
			this.panelStatus.Controls.Add(this.armLabel3);
			this.panelStatus.Controls.Add(this.txtStatus);
			this.panelStatus.Dock = DockStyle.Top;
			this.panelStatus.Location = new Point(0, 120);
			this.panelStatus.Name = "panelStatus";
			this.panelStatus.Size = new System.Drawing.Size(461, 30);
			this.panelStatus.TabIndex = 13;
			this.panelEmail.Controls.Add(this.panel1);
			this.panelEmail.Controls.Add(this.lbEmail);
			this.panelEmail.Dock = DockStyle.Top;
			this.panelEmail.Location = new Point(0, 30);
			this.panelEmail.Name = "panelEmail";
			this.panelEmail.Padding = new System.Windows.Forms.Padding(3);
			this.panelEmail.Size = new System.Drawing.Size(461, 30);
			this.panelEmail.TabIndex = 11;
			this.panelEmail.Visible = false;
			this.panel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.panel1.Controls.Add(this.txtEmail);
			this.panel1.Controls.Add(this.buttonEmailEdit);
			this.panel1.Location = new Point(182, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(262, 22);
			this.panel1.TabIndex = 10;
			this.buttonEmailEdit.Dock = DockStyle.Right;
			this.buttonEmailEdit.Image = (Image)componentResourceManager.GetObject("buttonEmailEdit.Image");
			this.buttonEmailEdit.Location = new Point(239, 0);
			this.buttonEmailEdit.Name = "buttonEmailEdit";
			this.buttonEmailEdit.Size = new System.Drawing.Size(23, 22);
			this.buttonEmailEdit.TabIndex = 10;
			this.buttonEmailEdit.UseVisualStyleBackColor = true;
			this.buttonEmailEdit.Click += new EventHandler(this.buttonEmailEdit_Click);
			this.armLabel3.AutoSize = true;
			this.armLabel3.CharacterCasing = CharacterCasing.Normal;
			this.armLabel3.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel3.Location = new Point(6, 6);
			this.armLabel3.Name = "armLabel3";
			this.armLabel3.Size = new System.Drawing.Size(55, 16);
			this.armLabel3.TabIndex = 3;
			this.armLabel3.Text = "Статус:";
			this.txtStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtStatus.BackColor = SystemColors.Control;
			this.txtStatus.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtStatus.Location = new Point(182, 3);
			this.txtStatus.Name = "txtStatus";
			this.txtStatus.ReadOnly = true;
			this.txtStatus.Size = new System.Drawing.Size(262, 22);
			this.txtStatus.TabIndex = 6;
			this.armLabel2.AutoSize = true;
			this.armLabel2.CharacterCasing = CharacterCasing.Normal;
			this.armLabel2.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel2.Location = new Point(6, 6);
			this.armLabel2.Name = "armLabel2";
			this.armLabel2.Size = new System.Drawing.Size(104, 16);
			this.armLabel2.TabIndex = 4;
			this.armLabel2.Text = "Баланс карты:";
			this.txtBallance.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtBallance.BackColor = SystemColors.Control;
			this.txtBallance.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtBallance.Location = new Point(182, 3);
			this.txtBallance.Name = "txtBallance";
			this.txtBallance.ReadOnly = true;
			this.txtBallance.Size = new System.Drawing.Size(263, 22);
			this.txtBallance.TabIndex = 7;
			this.txtClientName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtClientName.BackColor = SystemColors.Control;
			this.txtClientName.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtClientName.Location = new Point(182, 3);
			this.txtClientName.Name = "txtClientName";
			this.txtClientName.ReadOnly = true;
			this.txtClientName.Size = new System.Drawing.Size(263, 22);
			this.txtClientName.TabIndex = 8;
			this.armLabel1.AutoSize = true;
			this.armLabel1.CharacterCasing = CharacterCasing.Normal;
			this.armLabel1.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armLabel1.Location = new Point(6, 6);
			this.armLabel1.Name = "armLabel1";
			this.armLabel1.Size = new System.Drawing.Size(54, 16);
			this.armLabel1.TabIndex = 7;
			this.armLabel1.Text = "Клиент";
			this.txtEmail.BackColor = SystemColors.Control;
			this.txtEmail.Dock = DockStyle.Fill;
			this.txtEmail.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtEmail.Location = new Point(0, 0);
			this.txtEmail.Name = "txtEmail";
			this.txtEmail.ReadOnly = true;
			this.txtEmail.Size = new System.Drawing.Size(239, 22);
			this.txtEmail.TabIndex = 11;
			this.lbEmail.AutoSize = true;
			this.lbEmail.CharacterCasing = CharacterCasing.Normal;
			this.lbEmail.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.lbEmail.Location = new Point(6, 7);
			this.lbEmail.Name = "lbEmail";
			this.lbEmail.Size = new System.Drawing.Size(75, 16);
			this.lbEmail.TabIndex = 5;
			this.lbEmail.Text = "Эл. почта:";
			this.lbClientId.AutoSize = true;
			this.lbClientId.CharacterCasing = CharacterCasing.Normal;
			this.lbClientId.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.lbClientId.Location = new Point(6, 7);
			this.lbClientId.Name = "lbClientId";
			this.lbClientId.Size = new System.Drawing.Size(98, 16);
			this.lbClientId.TabIndex = 5;
			this.lbClientId.Text = "Номер карты:";
			this.txtClientId.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.txtClientId.BackColor = SystemColors.Control;
			this.txtClientId.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.txtClientId.Location = new Point(182, 4);
			this.txtClientId.Name = "txtClientId";
			this.txtClientId.ReadOnly = true;
			this.txtClientId.Size = new System.Drawing.Size(263, 22);
			this.txtClientId.TabIndex = 8;
			this.txtClientId.Text = "01234567890123456789";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(this.panelStatus);
			base.Controls.Add(this.panelBalance);
			base.Controls.Add(this.panelClientName);
			base.Controls.Add(this.panelEmail);
			base.Controls.Add(this.panelCardNumber);
			base.Name = "ucBallance";
			base.Size = new System.Drawing.Size(461, 154);
			base.Load += new EventHandler(this.ucBallance_Load);
			this.panelClientName.ResumeLayout(false);
			this.panelClientName.PerformLayout();
			this.panelCardNumber.ResumeLayout(false);
			this.panelCardNumber.PerformLayout();
			this.panelBalance.ResumeLayout(false);
			this.panelBalance.PerformLayout();
			this.panelStatus.ResumeLayout(false);
			this.panelStatus.PerformLayout();
			this.panelEmail.ResumeLayout(false);
			this.panelEmail.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}

		private void linkLabelDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (FrmBalanceDetails frmBalanceDetail = new FrmBalanceDetails())
			{
				frmBalanceDetail.Bind(this.m_cardInfo.BalanceDetails);
				frmBalanceDetail.ShowDialog(this);
			}
		}

		private void ucBallance_Load(object sender, EventArgs e)
		{
		}

		public event EventHandler EmailEditEvent;
	}
}