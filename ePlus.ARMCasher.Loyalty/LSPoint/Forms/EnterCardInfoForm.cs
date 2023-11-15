using ePlus.ARMCasher.Loyalty.LSPoint;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.LSPoint.Forms
{
	internal class EnterCardInfoForm : Form
	{
		private LSPointLoyaltyProgram _lsPoint;

		private IContainer components;

		public ComboBox ComboBoxCardType;

		public TextBox MagTrack1Field;

		public TextBox MagTrack2Field;

		public Label Label50;

		public Label Label49;

		public Label Label47;

		private Button InfoButton;

		private Button PromoButton;

		private Button buttonCancel;

		public EnterCardInfoForm(LSPointLoyaltyProgram lsPoint)
		{
			this._lsPoint = lsPoint;
			this.InitializeComponent();
			this.Label49.Visible = false;
			this.MagTrack1Field.Visible = false;
		}

		private void CancelButtonClick(object sender, EventArgs e)
		{
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

		private void EnterCardInfoFormLoad(object sender, EventArgs e)
		{
			base.AcceptButton = this.PromoButton;
		}

		private void InfoButtonClick(object sender, EventArgs e)
		{
			this.UpdateCardInfo();
			this._lsPoint.Info();
		}

		private void InitializeComponent()
		{
			this.ComboBoxCardType = new ComboBox();
			this.MagTrack1Field = new TextBox();
			this.MagTrack2Field = new TextBox();
			this.Label50 = new Label();
			this.Label49 = new Label();
			this.Label47 = new Label();
			this.InfoButton = new Button();
			this.PromoButton = new Button();
			this.buttonCancel = new Button();
			base.SuspendLayout();
			this.ComboBoxCardType.FormattingEnabled = true;
			ComboBox.ObjectCollection items = this.ComboBoxCardType.Items;
			object[] objArray = new object[] { "1 - Синхронная карта (ЛНР)", "2 - Карта Mifare (ЛНР)", "3 - Талон", "4 - Чиповая карта с приложением LifeStyle Point", "5 - Чиповая карта с приложением PetrolPlus", "6 - Чиповая карта с приложением МПС", "7 - Чиповая комбинированная карта (PetrolPlus + LifeStyle Point)", "8 - Чиповая кобрендинговая карта (LifeStyle Point + МПС)", "9 - Чиповая кобрендинговая карта (PetrolPlus + МПС)", "10 - Чиповая комбинированная кобрендинговая карта (PetrolPlus + LifeStyle Point + МПС)", "11 - Карта с магнитной полосой (только бонусная)", "12 - Карта с магнитной полосой (только МПС)", "13 - Кобрендинговая карта с магнитной полосой (LifeStyle Point + МПС)" };
			items.AddRange(objArray);
			this.ComboBoxCardType.Location = new Point(78, 6);
			this.ComboBoxCardType.MaxDropDownItems = 13;
			this.ComboBoxCardType.Name = "ComboBoxCardType";
			this.ComboBoxCardType.Size = new System.Drawing.Size(297, 21);
			this.ComboBoxCardType.TabIndex = 161;
			this.ComboBoxCardType.Text = "11 - Карта с магнитной полосой (только бонусная)";
			this.MagTrack1Field.AcceptsReturn = true;
			this.MagTrack1Field.BackColor = SystemColors.Window;
			this.MagTrack1Field.Cursor = Cursors.IBeam;
			this.MagTrack1Field.ForeColor = SystemColors.WindowText;
			this.MagTrack1Field.Location = new Point(78, 33);
			this.MagTrack1Field.MaxLength = 0;
			this.MagTrack1Field.Name = "MagTrack1Field";
			this.MagTrack1Field.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MagTrack1Field.Size = new System.Drawing.Size(297, 20);
			this.MagTrack1Field.TabIndex = 158;
			this.MagTrack2Field.AcceptsReturn = true;
			this.MagTrack2Field.BackColor = SystemColors.Window;
			this.MagTrack2Field.Cursor = Cursors.IBeam;
			this.MagTrack2Field.ForeColor = SystemColors.WindowText;
			this.MagTrack2Field.Location = new Point(78, 59);
			this.MagTrack2Field.MaxLength = 0;
			this.MagTrack2Field.Name = "MagTrack2Field";
			this.MagTrack2Field.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.MagTrack2Field.Size = new System.Drawing.Size(297, 20);
			this.MagTrack2Field.TabIndex = 156;
			this.Label50.Cursor = Cursors.Default;
			this.Label50.ForeColor = SystemColors.ControlText;
			this.Label50.Location = new Point(12, 9);
			this.Label50.Name = "Label50";
			this.Label50.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label50.Size = new System.Drawing.Size(69, 18);
			this.Label50.TabIndex = 160;
			this.Label50.Text = "Тип карты:";
			this.Label49.Cursor = Cursors.Default;
			this.Label49.ForeColor = SystemColors.ControlText;
			this.Label49.Location = new Point(12, 36);
			this.Label49.Name = "Label49";
			this.Label49.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label49.Size = new System.Drawing.Size(69, 20);
			this.Label49.TabIndex = 159;
			this.Label49.Text = "MagTrack1:";
			this.Label47.Cursor = Cursors.Default;
			this.Label47.ForeColor = SystemColors.ControlText;
			this.Label47.Location = new Point(12, 62);
			this.Label47.Name = "Label47";
			this.Label47.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Label47.Size = new System.Drawing.Size(69, 20);
			this.Label47.TabIndex = 157;
			this.Label47.Text = "Номер:";
			this.InfoButton.Location = new Point(175, 86);
			this.InfoButton.Name = "InfoButton";
			this.InfoButton.Size = new System.Drawing.Size(86, 23);
			this.InfoButton.TabIndex = 164;
			this.InfoButton.Text = "Инфо";
			this.InfoButton.UseVisualStyleBackColor = true;
			this.InfoButton.Click += new EventHandler(this.InfoButtonClick);
			this.PromoButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.PromoButton.Location = new Point(289, 86);
			this.PromoButton.Name = "PromoButton";
			this.PromoButton.Size = new System.Drawing.Size(86, 23);
			this.PromoButton.TabIndex = 165;
			this.PromoButton.Text = "Промо";
			this.PromoButton.UseVisualStyleBackColor = true;
			this.PromoButton.Click += new EventHandler(this.PromoButtonClick);
			this.buttonCancel.Location = new Point(59, 86);
			this.buttonCancel.Name = "CancelButton";
			this.buttonCancel.Size = new System.Drawing.Size(86, 23);
			this.buttonCancel.TabIndex = 163;
			this.buttonCancel.Text = "Отмена";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new EventHandler(this.CancelButtonClick);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(387, 122);
			base.Controls.Add(this.PromoButton);
			base.Controls.Add(this.InfoButton);
			base.Controls.Add(this.buttonCancel);
			base.Controls.Add(this.ComboBoxCardType);
			base.Controls.Add(this.MagTrack1Field);
			base.Controls.Add(this.MagTrack2Field);
			base.Controls.Add(this.Label50);
			base.Controls.Add(this.Label49);
			base.Controls.Add(this.Label47);
			this.MaximumSize = new System.Drawing.Size(403, 160);
			this.MinimumSize = new System.Drawing.Size(403, 160);
			base.Name = "EnterCardInfoForm";
			this.Text = "Ввод карты LSPoint";
			base.Load += new EventHandler(this.EnterCardInfoFormLoad);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Escape)
			{
				base.Close();
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void Promo()
		{
			this.UpdateCardInfo();
			if (this._lsPoint.Promo() && (this._lsPoint.PaidBonus != new decimal(0) || this._lsPoint.PaidCard != new decimal(0) || this._lsPoint.PaidCash != new decimal(0)))
			{
				base.DialogResult = System.Windows.Forms.DialogResult.OK;
				base.Close();
			}
		}

		private void PromoButtonClick(object sender, EventArgs e)
		{
			this.Promo();
		}

		private void UpdateCardInfo()
		{
			this._lsPoint.CardNumber = this.MagTrack2Field.Text;
			Bel.Instance.ComboBoxCardType.Text = this.ComboBoxCardType.Text;
			Bel.Instance.MagTrack1Field.Text = Utils.Str2Hex(this.MagTrack1Field.Text);
			Bel.Instance.MagTrack2Field.Text = Utils.Str2Hex(this.MagTrack2Field.Text);
			Bel.Instance.Check_TransmitCardData.Checked = true;
		}
	}
}