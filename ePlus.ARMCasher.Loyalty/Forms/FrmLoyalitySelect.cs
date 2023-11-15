using ePlus.ARMCommon;
using ePlus.ARMCommon.Controls;
using ePlus.Loyalty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmLoyalitySelect : Form, IFrmLoyality, IBaseView
	{
		private readonly LinkedList<ArmRadioButton> _rbtns = new LinkedList<ArmRadioButton>();

		private IContainer components;

		private Button buttonAccept;

		private Button buttonCancel;

		private Panel panel1;

		private TableLayoutPanel tableLayoutPanel1;

		public FrmLoyalitySelect()
		{
			this.InitializeComponent();
		}

		public void Bind(Dictionary<LoyaltySettings, string> list)
		{
			this.tableLayoutPanel1.RowCount = list.Count;
			foreach (KeyValuePair<LoyaltySettings, string> keyValuePair in list)
			{
				ArmRadioButton armRadioButton = new ArmRadioButton()
				{
					Text = keyValuePair.Value,
					Tag = keyValuePair.Key,
					Dock = DockStyle.None,
					AutoSize = true
				};
				ArmRadioButton armRadioButton1 = armRadioButton;
				armRadioButton1.CheckedChanged += new EventHandler(this.rbCheckedChanged);
				this.tableLayoutPanel1.Controls.Add(armRadioButton1);
				this._rbtns.AddLast(armRadioButton1);
			}
			if (this._rbtns.Count > 0)
			{
				this._rbtns.First.Value.Checked = true;
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

		void ePlus.ARMCommon.IBaseView.Close()
		{
			base.Close();
		}

		System.Windows.Forms.DialogResult ePlus.ARMCommon.IBaseView.ShowDialog()
		{
			return base.ShowDialog();
		}

		private void InitializeComponent()
		{
			this.buttonAccept = new Button();
			this.buttonCancel = new Button();
			this.panel1 = new Panel();
			this.tableLayoutPanel1 = new TableLayoutPanel();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonAccept.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold);
			this.buttonAccept.Location = new Point(118, 235);
			this.buttonAccept.Name = "buttonAccept";
			this.buttonAccept.Size = new System.Drawing.Size(100, 28);
			this.buttonAccept.TabIndex = 5;
			this.buttonAccept.Text = "ОК";
			this.buttonAccept.UseVisualStyleBackColor = true;
			this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold);
			this.buttonCancel.Location = new Point(224, 235);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(100, 28);
			this.buttonCancel.TabIndex = 6;
			this.buttonCancel.Text = "Отмена";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.panel1.Controls.Add(this.tableLayoutPanel1);
			this.panel1.Location = new Point(12, 13);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(312, 212);
			this.panel1.TabIndex = 7;
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.tableLayoutPanel1.Dock = DockStyle.Top;
			this.tableLayoutPanel1.Location = new Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(312, 0);
			this.tableLayoutPanel1.TabIndex = 0;
			base.AcceptButton = this.buttonAccept;
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.ClientSize = new System.Drawing.Size(336, 271);
			base.Controls.Add(this.panel1);
			base.Controls.Add(this.buttonCancel);
			base.Controls.Add(this.buttonAccept);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(352, 310);
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(352, 310);
			base.Name = "FrmLoyalitySelect";
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Выбор программы лояльности";
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			base.ResumeLayout(false);
		}

		private void OnLoyaltyTypeSelected(LoyaltySettings obj)
		{
			Action<LoyaltySettings> action = this.LoyaltyTypeSelected;
			if (action != null)
			{
				action(obj);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Up)
			{
				LinkedListNode<ArmRadioButton> last = this._rbtns.Last;
				this._rbtns.RemoveLast();
				this._rbtns.AddFirst(last);
				this._rbtns.First.Value.Checked = true;
				return true;
			}
			if (keyData != Keys.Down)
			{
				return base.ProcessCmdKey(ref msg, keyData);
			}
			LinkedListNode<ArmRadioButton> first = this._rbtns.First;
			this._rbtns.RemoveFirst();
			this._rbtns.AddLast(first);
			this._rbtns.First.Value.Checked = true;
			return true;
		}

		private void rbCheckedChanged(object sender, EventArgs e)
		{
			ArmRadioButton armRadioButton = sender as ArmRadioButton;
			if (armRadioButton == null)
			{
				return;
			}
			if (!armRadioButton.Checked)
			{
				return;
			}
			this.OnLoyaltyTypeSelected((LoyaltySettings)armRadioButton.Tag);
		}

		public event Action<LoyaltySettings> LoyaltyTypeSelected;
	}
}