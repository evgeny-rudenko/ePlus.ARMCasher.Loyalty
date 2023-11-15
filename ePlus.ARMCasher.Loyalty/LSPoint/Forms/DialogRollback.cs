using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.LSPoint.Forms
{
	internal class DialogRollback : Form
	{
		private IContainer components;

		internal TextBox ECROpIdOrig;

		internal Button Cancel_Button;

		internal Button OK_Button;

		internal TableLayoutPanel TableLayoutPanel1;

		internal TextBox BpSIdOrig;

		internal Label Label2;

		internal Label Label1;

		public DialogRollback()
		{
			this.InitializeComponent();
		}

		private void Cancel_Button_Click(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
			this.ECROpIdOrig = new TextBox();
			this.Cancel_Button = new Button();
			this.OK_Button = new Button();
			this.TableLayoutPanel1 = new TableLayoutPanel();
			this.BpSIdOrig = new TextBox();
			this.Label2 = new Label();
			this.Label1 = new Label();
			this.TableLayoutPanel1.SuspendLayout();
			base.SuspendLayout();
			this.ECROpIdOrig.Location = new Point(87, 31);
			this.ECROpIdOrig.MaxLength = 5;
			this.ECROpIdOrig.Name = "ECROpIdOrig";
			this.ECROpIdOrig.Size = new System.Drawing.Size(131, 20);
			this.ECROpIdOrig.TabIndex = 8;
			this.Cancel_Button.Anchor = AnchorStyles.None;
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = new Point(76, 3);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = new System.Drawing.Size(67, 22);
			this.Cancel_Button.TabIndex = 1;
			this.Cancel_Button.Text = "Cancel";
			this.Cancel_Button.Click += new EventHandler(this.Cancel_Button_Click);
			this.OK_Button.Anchor = AnchorStyles.None;
			this.OK_Button.Location = new Point(3, 3);
			this.OK_Button.Name = "OK_Button";
			this.OK_Button.Size = new System.Drawing.Size(67, 22);
			this.OK_Button.TabIndex = 0;
			this.OK_Button.Text = "OK";
			this.OK_Button.Click += new EventHandler(this.OK_Button_Click);
			this.TableLayoutPanel1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.TableLayoutPanel1.ColumnCount = 2;
			this.TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.TableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
			this.TableLayoutPanel1.Controls.Add(this.Cancel_Button, 1, 0);
			this.TableLayoutPanel1.Controls.Add(this.OK_Button, 0, 0);
			this.TableLayoutPanel1.Location = new Point(85, 64);
			this.TableLayoutPanel1.Name = "TableLayoutPanel1";
			this.TableLayoutPanel1.RowCount = 1;
			this.TableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
			this.TableLayoutPanel1.Size = new System.Drawing.Size(146, 28);
			this.TableLayoutPanel1.TabIndex = 7;
			this.BpSIdOrig.Location = new Point(87, 4);
			this.BpSIdOrig.MaxLength = 20;
			this.BpSIdOrig.Name = "BpSIdOrig";
			this.BpSIdOrig.Size = new System.Drawing.Size(131, 20);
			this.BpSIdOrig.TabIndex = 11;
			this.Label2.AutoSize = true;
			this.Label2.Location = new Point(7, 34);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(74, 13);
			this.Label2.TabIndex = 10;
			this.Label2.Text = "ECROpIdOrig:";
			this.Label1.AutoSize = true;
			this.Label1.Location = new Point(7, 7);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(58, 13);
			this.Label1.TabIndex = 9;
			this.Label1.Text = "BpSIdOrig:";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(238, 96);
			base.Controls.Add(this.ECROpIdOrig);
			base.Controls.Add(this.TableLayoutPanel1);
			base.Controls.Add(this.BpSIdOrig);
			base.Controls.Add(this.Label2);
			base.Controls.Add(this.Label1);
			base.Name = "DialogRollback";
			this.Text = "Rollback";
			this.TableLayoutPanel1.ResumeLayout(false);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void OK_Button_Click(object sender, EventArgs e)
		{
			string str = null;
			int num = Strings.Len(this.BpSIdOrig.Text);
			if (num == 20)
			{
				if (Strings.Len(this.ECROpIdOrig.Text) == 0)
				{
					Interaction.MsgBox("Введите номер оригинальной операции (ECROpIdOrig)!", MsgBoxStyle.OkOnly, null);
					return;
				}
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
				base.Close();
				return;
			}
			Interaction.MsgBox("Длина BpSIdOrig должна быть равна 20. Проверьте правильность ввода BpSIdOrig!", MsgBoxStyle.OkOnly, null);
			if (num < 20)
			{
				string str1 = "00000000000000000000";
				num = 20 - num;
				str = Strings.Mid(str1, 1, num);
				this.BpSIdOrig.Text = string.Concat(str, this.BpSIdOrig.Text);
			}
		}
	}
}