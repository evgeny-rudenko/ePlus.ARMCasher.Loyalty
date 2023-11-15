using ePlus.ARMCommon.Controls;
using ePlus.Loyalty;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Forms
{
	public class FrmBalanceDetails : Form
	{
		private IContainer components;

		private DataGridView dataGridViewDetails;

		private BindingSource iBalanceInfoRowBindingSource;

		private Panel panel1;

		private ARMButton armButtonOk;

		private DataGridViewTextBoxColumn BalanceTypeName;

		private DataGridViewTextBoxColumn amountDataGridViewTextBoxColumn;

		private DataGridViewTextBoxColumn expirationDateTimeDataGridViewTextBoxColumn;

		public FrmBalanceDetails()
		{
			this.InitializeComponent();
		}

		public void Bind(IEnumerable<IBalanceInfoRow> rows)
		{
			this.dataGridViewDetails.DataSource = rows;
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
			this.components = new System.ComponentModel.Container();
			DataGridViewCellStyle dataGridViewCellStyle = new DataGridViewCellStyle();
			this.dataGridViewDetails = new DataGridView();
			this.iBalanceInfoRowBindingSource = new BindingSource(this.components);
			this.panel1 = new Panel();
			this.armButtonOk = new ARMButton();
			this.BalanceTypeName = new DataGridViewTextBoxColumn();
			this.amountDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			this.expirationDateTimeDataGridViewTextBoxColumn = new DataGridViewTextBoxColumn();
			((ISupportInitialize)this.dataGridViewDetails).BeginInit();
			((ISupportInitialize)this.iBalanceInfoRowBindingSource).BeginInit();
			this.panel1.SuspendLayout();
			base.SuspendLayout();
			this.dataGridViewDetails.AllowUserToAddRows = false;
			this.dataGridViewDetails.AllowUserToDeleteRows = false;
			this.dataGridViewDetails.AutoGenerateColumns = false;
			this.dataGridViewDetails.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			DataGridViewColumnCollection columns = this.dataGridViewDetails.Columns;
			DataGridViewColumn[] balanceTypeName = new DataGridViewColumn[] { this.BalanceTypeName, this.amountDataGridViewTextBoxColumn, this.expirationDateTimeDataGridViewTextBoxColumn };
			columns.AddRange(balanceTypeName);
			this.dataGridViewDetails.DataSource = this.iBalanceInfoRowBindingSource;
			this.dataGridViewDetails.Dock = DockStyle.Fill;
			this.dataGridViewDetails.Location = new Point(0, 0);
			this.dataGridViewDetails.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.dataGridViewDetails.Name = "dataGridViewDetails";
			this.dataGridViewDetails.ReadOnly = true;
			this.dataGridViewDetails.RowHeadersVisible = false;
			this.dataGridViewDetails.RowTemplate.ReadOnly = true;
			this.dataGridViewDetails.RowTemplate.Resizable = DataGridViewTriState.False;
			this.dataGridViewDetails.Size = new System.Drawing.Size(354, 463);
			this.dataGridViewDetails.TabIndex = 0;
			this.iBalanceInfoRowBindingSource.DataSource = typeof(IBalanceInfoRow);
			this.panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			this.panel1.Controls.Add(this.dataGridViewDetails);
			this.panel1.Location = new Point(3, 3);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(354, 463);
			this.panel1.TabIndex = 2;
			this.armButtonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.armButtonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.armButtonOk.Font = new System.Drawing.Font("Arial", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.armButtonOk.Location = new Point(131, 481);
			this.armButtonOk.Name = "armButtonOk";
			this.armButtonOk.Size = new System.Drawing.Size(105, 28);
			this.armButtonOk.TabIndex = 3;
			this.armButtonOk.Text = "ОК";
			this.armButtonOk.UseVisualStyleBackColor = true;
			this.BalanceTypeName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.BalanceTypeName.DataPropertyName = "Name";
			this.BalanceTypeName.HeaderText = "Название";
			this.BalanceTypeName.Name = "BalanceTypeName";
			this.BalanceTypeName.ReadOnly = true;
			this.amountDataGridViewTextBoxColumn.DataPropertyName = "Amount";
			dataGridViewCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
			dataGridViewCellStyle.Format = "N2";
			this.amountDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle;
			this.amountDataGridViewTextBoxColumn.HeaderText = "Доступно";
			this.amountDataGridViewTextBoxColumn.Name = "amountDataGridViewTextBoxColumn";
			this.amountDataGridViewTextBoxColumn.ReadOnly = true;
			this.amountDataGridViewTextBoxColumn.Width = 150;
			this.expirationDateTimeDataGridViewTextBoxColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
			this.expirationDateTimeDataGridViewTextBoxColumn.DataPropertyName = "ExpirationDateTime";
			this.expirationDateTimeDataGridViewTextBoxColumn.HeaderText = "Дата сгорания";
			this.expirationDateTimeDataGridViewTextBoxColumn.Name = "expirationDateTimeDataGridViewTextBoxColumn";
			this.expirationDateTimeDataGridViewTextBoxColumn.ReadOnly = true;
			this.expirationDateTimeDataGridViewTextBoxColumn.Visible = false;
			base.AcceptButton = this.armButtonOk;
			base.AutoScaleDimensions = new SizeF(9f, 18f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.armButtonOk;
			base.ClientSize = new System.Drawing.Size(360, 521);
			base.Controls.Add(this.armButtonOk);
			base.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Arial", 12f, FontStyle.Regular, GraphicsUnit.Point, 204);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FrmBalanceDetails";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Детализация баланса";
			((ISupportInitialize)this.dataGridViewDetails).EndInit();
			((ISupportInitialize)this.iBalanceInfoRowBindingSource).EndInit();
			this.panel1.ResumeLayout(false);
			base.ResumeLayout(false);
		}
	}
}