using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon.Log;
using ePlus.ARMUtils;
using ePlus.Loyalty;
using ePlus.Loyalty.AstraZeneca;
using ePlus.Loyalty.AstraZeneca.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.AstraZeneca.Forms
{
	public class FormAccountInfo : Form
	{
		private IContainer components;

		private Button buttonOK;

		private Button buttonCancel;

		private Label label1;

		private TextBox textBoxCardNumber;

		private Label label2;

		private MaskedTextBox textBoxPhone;

		public string CardNumber
		{
			get
			{
				return this.textBoxCardNumber.Text.Trim();
			}
		}

		public string ClientId
		{
			get
			{
				string cardNumber = this.CardNumber;
				if (string.IsNullOrWhiteSpace(this.CardNumber))
				{
					cardNumber = this.PhoneNumber;
				}
				return cardNumber;
			}
		}

		private string PhoneEnteredString
		{
			get
			{
				return this.textBoxPhone.MaskedTextProvider.ToString(false, false);
			}
		}

		public string PhoneNumber
		{
			get
			{
				if (!this.textBoxPhone.MaskFull)
				{
					return null;
				}
				return string.Concat("+7", this.PhoneEnteredString);
			}
		}

		public FormAccountInfo()
		{
			this.InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void FormAccountInfo_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (base.DialogResult != System.Windows.Forms.DialogResult.OK)
			{
				return;
			}
			if (this.textBoxCardNumber.Focused)
			{
				this.textBoxPhone.Select();
				e.Cancel = true;
				return;
			}
			if (!this.ValidateValues())
			{
				e.Cancel = true;
				return;
			}
			string phoneNumber = this.PhoneNumber;
			string cardNumber = this.CardNumber;
			if (!string.IsNullOrWhiteSpace(phoneNumber) && !string.IsNullOrWhiteSpace(cardNumber))
			{
				ARMLogger.Info("АстраЗенека: регистрация карты");
				AstraZenecaWebApi astraZenecaWebApi = new AstraZenecaWebApi((new SettingsModel()).Deserialize<Settings>(LoyaltyProgManager.LoyaltySettings[LoyaltyType.AstraZeneca].First<LoyaltySettings>().SETTINGS, "Settings"));
				RequestRegister requestRegister = astraZenecaWebApi.CreateRequest<RequestRegister>();
				requestRegister.CardNumber = cardNumber;
				requestRegister.PhoneNumber = phoneNumber;
				Response response = astraZenecaWebApi.Register(requestRegister);
				if (!response.IsSuccess)
				{
					MessageBox.Show(this, response.Message, "Ошибка регистрации карты", MessageBoxButtons.OK, MessageBoxIcon.Hand);
					e.Cancel = true;
				}
				else
				{
					ARMLogger.Info("АстраЗенека: запрашиваем код подтверждения");
					using (FormConfirmationCode formConfirmationCode = new FormConfirmationCode())
					{
						if (formConfirmationCode.ShowDialog() != System.Windows.Forms.DialogResult.OK)
						{
							e.Cancel = true;
						}
						else
						{
							RequestConfirmCode confirmationCode = astraZenecaWebApi.CreateRequest<RequestConfirmCode>();
							confirmationCode.Code = formConfirmationCode.ConfirmationCode;
							Response response1 = astraZenecaWebApi.ConfirmCode(confirmationCode);
							if (!response1.IsSuccess)
							{
								MessageBox.Show(this, response1.Message, "Ошибка ввода кода подтверждения", MessageBoxButtons.OK, MessageBoxIcon.Hand);
								e.Cancel = true;
							}
						}
					}
				}
			}
		}

		private void InitializeComponent()
		{
			this.buttonOK = new Button();
			this.buttonCancel = new Button();
			this.label1 = new Label();
			this.textBoxCardNumber = new TextBox();
			this.label2 = new Label();
			this.textBoxPhone = new MaskedTextBox();
			base.SuspendLayout();
			this.buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new Point(149, 169);
			this.buttonOK.Margin = new System.Windows.Forms.Padding(5);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(150, 45);
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new Point(308, 169);
			this.buttonCancel.Margin = new System.Windows.Forms.Padding(5);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(150, 45);
			this.buttonCancel.TabIndex = 5;
			this.buttonCancel.Text = "Отмена";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.label1.AutoSize = true;
			this.label1.Location = new Point(14, 11);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(101, 19);
			this.label1.TabIndex = 0;
			this.label1.Text = "Штрих-код:";
			this.textBoxCardNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.textBoxCardNumber.Location = new Point(14, 39);
			this.textBoxCardNumber.Margin = new System.Windows.Forms.Padding(4);
			this.textBoxCardNumber.Name = "textBoxCardNumber";
			this.textBoxCardNumber.Size = new System.Drawing.Size(444, 26);
			this.textBoxCardNumber.TabIndex = 1;
			this.label2.AutoSize = true;
			this.label2.Location = new Point(14, 77);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(150, 19);
			this.label2.TabIndex = 2;
			this.label2.Text = "Номер телефона:";
			this.textBoxPhone.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			this.textBoxPhone.Location = new Point(14, 105);
			this.textBoxPhone.Margin = new System.Windows.Forms.Padding(4);
			this.textBoxPhone.Mask = "+7 (999) 000-00-00";
			this.textBoxPhone.Name = "textBoxPhone";
			this.textBoxPhone.Size = new System.Drawing.Size(444, 26);
			this.textBoxPhone.TabIndex = 3;
			base.AcceptButton = this.buttonOK;
			base.AutoScaleDimensions = new SizeF(10f, 19f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.CancelButton = this.buttonCancel;
			base.ClientSize = new System.Drawing.Size(474, 231);
			base.Controls.Add(this.textBoxPhone);
			base.Controls.Add(this.label2);
			base.Controls.Add(this.textBoxCardNumber);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.buttonCancel);
			base.Controls.Add(this.buttonOK);
			this.Font = new System.Drawing.Font("Arial", 12f, FontStyle.Bold, GraphicsUnit.Point, 204);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.KeyPreview = true;
			base.Margin = new System.Windows.Forms.Padding(5);
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "FormAccountInfo";
			base.ShowIcon = false;
			base.ShowInTaskbar = false;
			base.StartPosition = FormStartPosition.CenterParent;
			this.Text = "Введите данные";
			base.FormClosing += new FormClosingEventHandler(this.FormAccountInfo_FormClosing);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private bool ValidateValues()
		{
			if (!string.IsNullOrWhiteSpace(this.PhoneEnteredString) && !this.textBoxPhone.MaskFull)
			{
				UtilsArm.ShowMessageInformationOK("Введен некорректный номер телефона.");
				return false;
			}
			if (!string.IsNullOrWhiteSpace(this.ClientId))
			{
				return true;
			}
			UtilsArm.ShowMessageInformationOK("Для продолжения необходимо ввести ШК или номер телефона.");
			return false;
		}
	}
}