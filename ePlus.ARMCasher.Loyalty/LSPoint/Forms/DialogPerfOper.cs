using BELLib;
using ePlus.ARMCasher.Loyalty.LSPoint;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.LSPoint.Forms
{
	internal class DialogPerfOper : Form
	{
		private const short OPERATION_PAYMENT = 3;

		private const short OPERATION_CLOSE_CHEQUE = 4;

		private const short OPERATION_MODIFY_CHEQUE = 6;

		private const short OPERATION_PREPAID = 7;

		private const short OPERATION_CARD_LINKING = 8;

		private const short OPERATION_CARD_SUBSTITUTION = 9;

		private const short OPERATION_BPRRN_CANCEL = 10;

		private const short OPERATION_CHANGE_PIN = 11;

		private const short OPERATION_EMV_PAYMENT_CANCEL = 12;

		private const short OPERATION_FORM_APPLIANCE = 13;

		private const short OPERATION_GET_INFO = 14;

		private const short OPERATION_ROLLBACK = 15;

		private const short OPERATION_PROMO = 16;

		private const byte EM_TYPE_INPUTBOX = 1;

		private const byte EM_TYPE_INPUTBOX_NUMBER = 2;

		private const byte EM_TYPE_INPUTBOX_BOOLEAN = 3;

		private const byte EM_TYPE_INPUT_CARDDATA = 4;

		private const long EM_FLAG_MANDATORY = 1L;

		private const short DLG_CONTROL_MODE_HIDE = 0;

		private const short DLG_CONTROL_MODE_EM_01 = 1;

		private const short DLG_CONTROL_MODE_EM_02 = 2;

		private const short DLG_CONTROL_MODE_EM_03 = 3;

		private const short DLG_CONTROL_MODE_EM_04 = 4;

		private const int MAX_NUM_BUTTONS = 8;

		private bool bCancelOperation;

		private bool bItegralProtocol;

		private bool bPushNext;

		private bool bPushSet;

		private bool IsStrFormatRequest;

		private int IndexButtonSelect = -1;

		private Bel pBEL;

		private string strMagTrack;

		private List<Button> listDynBtn;

		private Label captionBtn;

		public int OperationID;

		private Thread tRecvThread;

		public bool IsCancelled;

		private IContainer components;

		internal TextBox Label_EMBody;

		internal Button Break_Button;

		internal CheckBox CheckBoxDataInHexString;

		internal Label LabelDataCardSet;

		internal Button ButtonSetDataCardFromMainForm;

		public GroupBox Frame_EM;

		internal ComboBox ComboBoxSelectCardType;

		internal Button Button_EMNext;

		internal Button Button_EMSet;

		internal TextBox TextBoxTrack2;

		internal TextBox TextBoxTrack1;

		internal TextBox TextBox_EMValue;

		internal Button Reset_Button;

		internal Label Label_ParformOperMessage;

		internal System.Windows.Forms.Timer Timer1;

		public Bel pBELForm
		{
			get
			{
				return this.pBEL;
			}
			set
			{
				this.pBEL = value;
			}
		}

		public DialogPerfOper()
		{
			this.InitializeComponent();
			this.ThreadComplete += new DialogPerfOper.ThreadCompleteEventHandler(this.DialogPerfOperEventHandler);
			this.IsCancelled = false;
		}

		private void Button_EMNext_Click(object sender, EventArgs e)
		{
			this.SetExtraMessVisibble(0);
			this.bPushNext = true;
		}

		private void Button_EMSet_Click(object sender, EventArgs e)
		{
		}

		private void Button_EMSet_Click_1(object sender, EventArgs e)
		{
			Bel.Instance.Bpecr1.ExtraMessageCardType = this.ComboBoxSelectCardType.SelectedIndex + 1;
			if (!this.CheckBoxDataInHexString.Checked)
			{
				Bel.Instance.Bpecr1.ExtraMessageTrack1 = this.TextBoxTrack1.Text;
				Bel.Instance.Bpecr1.ExtraMessageTrack2 = this.TextBoxTrack2.Text;
			}
			else
			{
				string text = this.TextBoxTrack1.Text;
				Bel.Instance.Bpecr1.ExtraMessageTrack1 = this.ConverHexString(ref text);
				string str = this.TextBoxTrack2.Text;
				Bel.Instance.Bpecr1.ExtraMessageTrack2 = this.ConverHexString(ref str);
			}
			this.SetExtraMessVisibble(0);
			this.bPushSet = true;
		}

		private void ButtonSetDataCardFromMainForm_Click(object sender, EventArgs e)
		{
			this.ComboBoxSelectCardType.SelectedIndex = this.pBELForm.IndexCardTypeComboBox;
			this.TextBoxTrack1.Text = this.pBELForm.Track1StrValue;
			this.TextBoxTrack2.Text = this.pBELForm.Track2StrValue;
		}

		private void Cancel_Button_Click(object sender, EventArgs e)
		{
			this.Reset_Button.Enabled = false;
			Bel.Instance.Bpecr1.AbortOperation();
			this.tRecvThread.Abort();
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.IsCancelled = true;
			base.Close();
		}

		private void CancelOperation()
		{
			if (!this.bItegralProtocol)
			{
				base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				base.Close();
				return;
			}
			Bel.Instance.Bpecr1.CancelOperation();
			this.Reset_Button.Text = "Сбросить принудительно";
			this.Break_Button.Enabled = false;
			this.Label_ParformOperMessage.Text = "Подождите, операция прерывается...";
			this.bCancelOperation = true;
		}

		private string ConverHexString(ref string strSource)
		{
			string str = null;
			byte num = 0;
			int length = 0;
			string str1 = null;
			str = "";
			length = strSource.Length;
			for (int i = 1; i <= length / 2; i++)
			{
				str1 = strSource.Substring((i - 1) * 2, 2);
				num = Convert.ToByte(str1, 16);
				str = string.Concat(str, Strings.ChrW((int)num));
			}
			return str;
		}

		private void CreateButtonList()
		{
			Button button = null;
			for (int i = 0; i <= 8; i++)
			{
				button = new Button()
				{
					Name = string.Concat("dynBtn_", i.ToString()),
					Size = new System.Drawing.Size(250, 30),
					Location = new Point(60, 90 + i * 40),
					Font = new System.Drawing.Font("Microsoft Sans Serif", 10f, FontStyle.Regular, GraphicsUnit.Point, Convert.ToByte(204)),
					ForeColor = Color.Navy,
					Text = ""
				};
				button.Click += new EventHandler(this.DynBtn_MouseClk);
				base.Controls.Add(button);
				this.listDynBtn.Add(button);
			}
			for (int j = 0; j <= 8; j++)
			{
				this.listDynBtn[j].Visible = false;
				this.listDynBtn[j].Enabled = false;
			}
		}

		private void DialogPerfOper_Load(object sender, EventArgs e)
		{
			bool useContextlessProtocol = false;
			bool useIntegralProtocol = false;
			try
			{
				useContextlessProtocol = Bel.Instance.Bpecr1.UseContextlessProtocol == 1;
				useIntegralProtocol = Bel.Instance.Bpecr1.UseIntegralProtocol == 1;
				this.bCancelOperation = false;
				this.bItegralProtocol = true;
				if (!useContextlessProtocol || !useIntegralProtocol)
				{
					this.Break_Button.Visible = false;
					this.Reset_Button.Visible = false;
					this.bItegralProtocol = false;
				}
				this.IsStrFormatRequest = false;
				this.captionBtn = new Label()
				{
					Size = new System.Drawing.Size(300, 40),
					Location = new Point(35, 30)
				};
				base.Controls.Add(this.captionBtn);
				this.captionBtn.Visible = false;
				this.captionBtn.Enabled = false;
				this.captionBtn.BorderStyle = BorderStyle.FixedSingle;
				this.captionBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, Convert.ToByte(204));
				this.captionBtn.TextAlign = ContentAlignment.MiddleCenter;
				this.captionBtn.ForeColor = Color.Maroon;
				this.listDynBtn = new List<Button>();
				this.CreateButtonList();
				this.SetExtraMessVisibble(0);
				Control.CheckForIllegalCrossThreadCalls = false;
				this.tRecvThread = new Thread(new ThreadStart(this.RecvProcess));
				this.tRecvThread.Start();
			}
			catch (Exception exception)
			{
				base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				base.Close();
			}
		}

		public void DialogPerfOperEventHandler(int RetCode)
		{
			this.EndHandling(RetCode);
		}

		private void DialogPerfOperFormClosed(object sender, FormClosedEventArgs e)
		{
			if (this.tRecvThread.ThreadState == ThreadState.Running)
			{
				this.tRecvThread.Abort();
			}
		}

		private void DialogPerfOperFormClosing(object sender, FormClosingEventArgs e)
		{
			this.CancelOperation();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		public void DynBtn_MouseClk(object sender, EventArgs e)
		{
			Button button = (Button)sender;
			if (this.IsStrFormatRequest)
			{
				for (int i = 0; i <= 8; i++)
				{
					if (button.Equals(this.listDynBtn[i]))
					{
						this.IndexButtonSelect = i;
					}
				}
			}
			this.SetExtraMessVisibble(0);
			for (int j = 0; j <= 8; j++)
			{
				this.listDynBtn[j].Visible = false;
				this.listDynBtn[j].Enabled = false;
			}
			this.captionBtn.Enabled = false;
			this.captionBtn.Visible = false;
			base.Update();
			this.bPushSet = true;
		}

		private void EndHandling(int RetCode)
		{
			Bel.Instance.Bpecr1.RetCode = RetCode;
			if (RetCode != 0 || !this.bCancelOperation)
			{
				Bel.Instance.Bpecr1.RetCode = RetCode;
			}
			else
			{
				Bel.Instance.Bpecr1.RetCode = 12;
			}
			base.DialogResult = System.Windows.Forms.DialogResult.OK;
			base.Close();
		}

		private void GroupBox_EM_Enter(object sender, EventArgs e)
		{
		}

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.Label_EMBody = new TextBox();
			this.Break_Button = new Button();
			this.CheckBoxDataInHexString = new CheckBox();
			this.LabelDataCardSet = new Label();
			this.ButtonSetDataCardFromMainForm = new Button();
			this.Frame_EM = new GroupBox();
			this.ComboBoxSelectCardType = new ComboBox();
			this.Button_EMNext = new Button();
			this.Button_EMSet = new Button();
			this.TextBoxTrack2 = new TextBox();
			this.TextBoxTrack1 = new TextBox();
			this.TextBox_EMValue = new TextBox();
			this.Reset_Button = new Button();
			this.Label_ParformOperMessage = new Label();
			this.Timer1 = new System.Windows.Forms.Timer(this.components);
			this.Frame_EM.SuspendLayout();
			base.SuspendLayout();
			this.Label_EMBody.BackColor = SystemColors.Window;
			this.Label_EMBody.Location = new Point(9, 21);
			this.Label_EMBody.Multiline = true;
			this.Label_EMBody.Name = "Label_EMBody";
			this.Label_EMBody.ReadOnly = true;
			this.Label_EMBody.ScrollBars = ScrollBars.Vertical;
			this.Label_EMBody.Size = new System.Drawing.Size(326, 131);
			this.Label_EMBody.TabIndex = 12;
			this.Break_Button.Anchor = AnchorStyles.None;
			this.Break_Button.Location = new Point(76, 411);
			this.Break_Button.Name = "Break_Button";
			this.Break_Button.Size = new System.Drawing.Size(85, 24);
			this.Break_Button.TabIndex = 69;
			this.Break_Button.Text = "Прервать";
			this.Break_Button.Click += new EventHandler(this.OK_Button_Click);
			this.CheckBoxDataInHexString.AutoSize = true;
			this.CheckBoxDataInHexString.Location = new Point(11, 300);
			this.CheckBoxDataInHexString.Name = "CheckBoxDataInHexString";
			this.CheckBoxDataInHexString.Size = new System.Drawing.Size(225, 17);
			this.CheckBoxDataInHexString.TabIndex = 11;
			this.CheckBoxDataInHexString.Text = "Данные треков в формате HEX-строки";
			this.CheckBoxDataInHexString.UseVisualStyleBackColor = true;
			this.LabelDataCardSet.AutoSize = true;
			this.LabelDataCardSet.Location = new Point(15, 192);
			this.LabelDataCardSet.Name = "LabelDataCardSet";
			this.LabelDataCardSet.Size = new System.Drawing.Size(165, 13);
			this.LabelDataCardSet.TabIndex = 10;
			this.LabelDataCardSet.Text = "Установить из главной формы";
			this.ButtonSetDataCardFromMainForm.Location = new Point(233, 186);
			this.ButtonSetDataCardFromMainForm.Name = "ButtonSetDataCardFromMainForm";
			this.ButtonSetDataCardFromMainForm.Size = new System.Drawing.Size(102, 24);
			this.ButtonSetDataCardFromMainForm.TabIndex = 9;
			this.ButtonSetDataCardFromMainForm.Text = "Установить";
			this.ButtonSetDataCardFromMainForm.UseVisualStyleBackColor = true;
			this.ButtonSetDataCardFromMainForm.Click += new EventHandler(this.ButtonSetDataCardFromMainForm_Click);
			this.Frame_EM.BackColor = SystemColors.Control;
			this.Frame_EM.Controls.Add(this.Label_EMBody);
			this.Frame_EM.Controls.Add(this.CheckBoxDataInHexString);
			this.Frame_EM.Controls.Add(this.LabelDataCardSet);
			this.Frame_EM.Controls.Add(this.ButtonSetDataCardFromMainForm);
			this.Frame_EM.Controls.Add(this.ComboBoxSelectCardType);
			this.Frame_EM.Controls.Add(this.Button_EMNext);
			this.Frame_EM.Controls.Add(this.Button_EMSet);
			this.Frame_EM.Controls.Add(this.TextBoxTrack2);
			this.Frame_EM.Controls.Add(this.TextBoxTrack1);
			this.Frame_EM.Controls.Add(this.TextBox_EMValue);
			this.Frame_EM.ForeColor = SystemColors.ControlText;
			this.Frame_EM.Location = new Point(7, 35);
			this.Frame_EM.Name = "Frame_EM";
			this.Frame_EM.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.Frame_EM.Size = new System.Drawing.Size(341, 362);
			this.Frame_EM.TabIndex = 71;
			this.Frame_EM.TabStop = false;
			this.Frame_EM.Text = "Экстра сообщение";
			this.ComboBoxSelectCardType.FormattingEnabled = true;
			ComboBox.ObjectCollection items = this.ComboBoxSelectCardType.Items;
			object[] objArray = new object[] { "1 - Синхронная карта (ЛНР)", "2 - Карта Mifare (ЛНР)", "3 - Талон", "4 - Чиповая карта с приложением LifeStyle Point", "5 - Чиповая карта с приложением PetrolPlus", "6 - Чиповая карта с приложением МПС", "7 - Чиповая комбинированная карта (PetrolPlus + LifeStyle Point)", "8 - Чиповая кобрендинговая карта (LifeStyle Point + МПС)", "9 - Чиповая кобрендинговая карта (PetrolPlus + МПС)", "10 - Чиповая комбинированная кобрендинговая карта (PetrolPlus + LifeStyle Point + МПС)", "11 - Карта с магнитной полосой (только бонусная)", "12 - Карта с магнитной полосой (только МПС)", "13 - Кобрендинговая карта с магнитной полосой (LifeStyle Point + МПС)" };
			items.AddRange(objArray);
			this.ComboBoxSelectCardType.Location = new Point(9, 216);
			this.ComboBoxSelectCardType.MaxDropDownItems = 13;
			this.ComboBoxSelectCardType.Name = "ComboBoxSelectCardType";
			this.ComboBoxSelectCardType.Size = new System.Drawing.Size(326, 21);
			this.ComboBoxSelectCardType.TabIndex = 8;
			this.Button_EMNext.Location = new Point(183, 323);
			this.Button_EMNext.Name = "Button_EMNext";
			this.Button_EMNext.Size = new System.Drawing.Size(85, 24);
			this.Button_EMNext.TabIndex = 7;
			this.Button_EMNext.Text = "Пропустить";
			this.Button_EMNext.UseVisualStyleBackColor = true;
			this.Button_EMNext.Click += new EventHandler(this.Button_EMNext_Click);
			this.Button_EMSet.Location = new Point(69, 323);
			this.Button_EMSet.Name = "Button_EMSet";
			this.Button_EMSet.Size = new System.Drawing.Size(85, 24);
			this.Button_EMSet.TabIndex = 0;
			this.Button_EMSet.Text = "Ответить";
			this.Button_EMSet.UseVisualStyleBackColor = true;
			this.Button_EMSet.Click += new EventHandler(this.Button_EMSet_Click_1);
			this.TextBoxTrack2.Location = new Point(9, 273);
			this.TextBoxTrack2.Name = "TextBoxTrack2";
			this.TextBoxTrack2.Size = new System.Drawing.Size(326, 20);
			this.TextBoxTrack2.TabIndex = 5;
			this.TextBoxTrack1.Location = new Point(9, 243);
			this.TextBoxTrack1.Name = "TextBoxTrack1";
			this.TextBoxTrack1.Size = new System.Drawing.Size(326, 20);
			this.TextBoxTrack1.TabIndex = 5;
			this.TextBox_EMValue.Location = new Point(9, 161);
			this.TextBox_EMValue.Name = "TextBox_EMValue";
			this.TextBox_EMValue.Size = new System.Drawing.Size(326, 20);
			this.TextBox_EMValue.TabIndex = 5;
			this.Reset_Button.Anchor = AnchorStyles.None;
			this.Reset_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Reset_Button.Location = new Point(190, 411);
			this.Reset_Button.Name = "Reset_Button";
			this.Reset_Button.Size = new System.Drawing.Size(85, 24);
			this.Reset_Button.TabIndex = 70;
			this.Reset_Button.Text = "Сбросить";
			this.Reset_Button.Click += new EventHandler(this.Cancel_Button_Click);
			this.Label_ParformOperMessage.AutoSize = true;
			this.Label_ParformOperMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75f, FontStyle.Bold, GraphicsUnit.Point, 204);
			this.Label_ParformOperMessage.Location = new Point(9, 5);
			this.Label_ParformOperMessage.Name = "Label_ParformOperMessage";
			this.Label_ParformOperMessage.Size = new System.Drawing.Size(275, 16);
			this.Label_ParformOperMessage.TabIndex = 68;
			this.Label_ParformOperMessage.Text = "Операция выполняется. Ожидайте...";
			this.Timer1.Tick += new EventHandler(this.Timer1_Tick);
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.ClientSize = new System.Drawing.Size(355, 441);
			base.ControlBox = false;
			base.Controls.Add(this.Break_Button);
			base.Controls.Add(this.Frame_EM);
			base.Controls.Add(this.Reset_Button);
			base.Controls.Add(this.Label_ParformOperMessage);
			base.MaximizeBox = false;
			this.MaximumSize = new System.Drawing.Size(371, 479);
			base.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(371, 479);
			base.Name = "DialogPerfOper";
			base.ShowIcon = false;
			this.Text = "Выполнение операции...";
			base.Load += new EventHandler(this.DialogPerfOper_Load);
			base.FormClosed += new FormClosedEventHandler(this.DialogPerfOperFormClosed);
			base.KeyPress += new KeyPressEventHandler(this.OnKeyPressForm);
			base.FormClosing += new FormClosingEventHandler(this.DialogPerfOperFormClosing);
			this.Frame_EM.ResumeLayout(false);
			this.Frame_EM.PerformLayout();
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private void Label_EMBody_Click(object sender, EventArgs e)
		{
		}

		private void OK_Button_Click(object sender, EventArgs e)
		{
			this.IsCancelled = true;
			this.CancelOperation();
		}

		private void OnKeyPressForm(object sender, KeyPressEventArgs eventArgs)
		{
			int num = Strings.Asc(eventArgs.KeyChar);
			string str = null;
			int num1 = 0;
			string str1 = null;
			string str2 = null;
			str1 = "йЙцЦуУкКеЕнНгГшШщЩзЗхХъЪфФыЫвВаАпПрРоОлЛдДжЖэЭяЯчЧсСмМиИтТьЬбБюЮ.,";
			str2 = "qQwWeErRtTyYuUiIoOpP[{]}aAsSdDfFgGhHjJkKlL;;'_zZxXcCvVbBnNmM,<.>/?";
			str = Strings.Chr(num).ToString();
			num1 = Strings.InStr(str1, str, CompareMethod.Text);
			if (num1 != 0)
			{
				str = Strings.Mid(str2, num1, 1);
			}
			if (this.Timer1.Enabled)
			{
				this.strMagTrack = string.Concat(this.strMagTrack, str);
				num = 0;
				this.Timer1.Enabled = false;
				this.Timer1.Enabled = true;
			}
			else if (str == "%" || str == ";")
			{
				this.strMagTrack = str;
				this.Timer1.Enabled = true;
				num = 0;
			}
			eventArgs.KeyChar = Strings.Chr(num);
			if (num == 0)
			{
				eventArgs.Handled = true;
			}
		}

		public bool RadioButtonMessageShow(ref string ExtraMessageStrFull)
		{
			bool flag;
			string str = null;
			string extraMessageStrFull = null;
			string str1 = null;
			List<string> strs = null;
			int num = 0;
			int num1 = 0;
			try
			{
				strs = new List<string>();
				num = 0;
				num1 = 0;
				str = ":";
				num = ExtraMessageStrFull.IndexOf(str);
				if (num == -1)
				{
					extraMessageStrFull = ExtraMessageStrFull;
				}
				else
				{
					this.captionBtn.Text = ExtraMessageStrFull.Substring(0, num);
					extraMessageStrFull = ExtraMessageStrFull.Substring(num + 1);
				}
				num = 0;
				num1 = 0;
				str = "/";
				num1 = extraMessageStrFull.IndexOf(str);
				if (num1 != -1)
				{
					this.captionBtn.Enabled = true;
					this.captionBtn.Visible = true;
					while (num1 != -1)
					{
						str1 = "";
						str1 = extraMessageStrFull.Substring(num, num1 - num);
						num = num1 + 1;
						if (string.IsNullOrEmpty(str1))
						{
							break;
						}
						strs.Add(str1);
						num1 = extraMessageStrFull.IndexOf(str, num);
					}
					str1 = extraMessageStrFull.Substring(num);
					strs.Add(str1);
					if (strs.Count > 0)
					{
						this.SetExtraMessVisibble(0);
						for (int i = 0; i <= strs.Count - 1 && this.listDynBtn.Count != i; i++)
						{
							this.listDynBtn[i].Text = strs[i];
							this.listDynBtn[i].Enabled = true;
							this.listDynBtn[i].Visible = true;
							this.listDynBtn[i].Focus();
						}
					}
					base.Update();
					flag = true;
				}
				else
				{
					flag = false;
				}
			}
			catch (Exception exception)
			{
				MessageBox.Show(exception.Message);
				flag = false;
			}
			return flag;
		}

		private void RecvProcess()
		{
			bool useContextlessProtocol = false;
			bool useIntegralProtocol = false;
			int retCode = 0;
			ushort extraMessageCount = 0;
			int num = 0;
			bool flag = false;
			try
			{
				useContextlessProtocol = false;
				switch (this.OperationID)
				{
					case 3:
					{
						Bel.Instance.Bpecr1.PerformPayment();
						break;
					}
					case 4:
					{
						Bel.Instance.Bpecr1.CloseCheque();
						break;
					}
					case 5:
					{
						useContextlessProtocol = true;
						break;
					}
					case 6:
					{
						Bel.Instance.Bpecr1.GetDiscount();
						break;
					}
					case 7:
					{
						Bel.Instance.Bpecr1.PerformPrepaidCharge();
						break;
					}
					case 8:
					{
						Bel.Instance.Bpecr1.PerformCardLinking();
						break;
					}
					case 9:
					{
						Bel.Instance.Bpecr1.PerformCardSubstitution();
						break;
					}
					case 10:
					{
						Bel.Instance.Bpecr1.PerformBprrnCancel();
						break;
					}
					case 11:
					{
						Bel.Instance.Bpecr1.PerformChangePIN();
						break;
					}
					case 12:
					{
						Bel.Instance.Bpecr1.PerformRRNCancel();
						break;
					}
					case 13:
					{
						Bel.Instance.Bpecr1.PerformFormAppliance();
						break;
					}
					case 14:
					{
						Bel.Instance.Bpecr1.GetCardInfo();
						break;
					}
					case 15:
					{
						Bel.Instance.Bpecr1.PerformRollback();
						break;
					}
					case 16:
					{
						Bel.Instance.Bpecr1.PerformPromo();
						break;
					}
					default:
					{
						goto case 5;
					}
				}
				if (!useContextlessProtocol)
				{
					retCode = Bel.Instance.Bpecr1.RetCode;
					useContextlessProtocol = Bel.Instance.Bpecr1.UseContextlessProtocol == 1;
					useIntegralProtocol = Bel.Instance.Bpecr1.UseIntegralProtocol == 1;
					if (retCode == 0 && useContextlessProtocol && useIntegralProtocol)
					{
						num = 0;
						this.bPushSet = false;
						this.bPushNext = false;
						flag = true;
						for (useContextlessProtocol = Bel.Instance.Bpecr1.IsOperationRunning == 1; useContextlessProtocol; useContextlessProtocol = Bel.Instance.Bpecr1.IsOperationRunning == 1)
						{
							extraMessageCount = Bel.Instance.Bpecr1.ExtraMessageCount;
							if (extraMessageCount > num && (this.bPushNext || this.bPushSet || flag))
							{
								if (this.bPushSet)
								{
									if (!this.IsStrFormatRequest)
									{
										Bel.Instance.Bpecr1.ExtraMessageValue = this.TextBox_EMValue.Text;
										Bel.Instance.Bpecr1.SetExtraMessage(Convert.ToUInt16(num));
									}
									else
									{
										int indexButtonSelect = this.IndexButtonSelect + 1;
										Bel.Instance.Bpecr1.ExtraMessageValue = indexButtonSelect.ToString();
										Bel.Instance.Bpecr1.SetExtraMessage(Convert.ToUInt16(num));
									}
									this.bPushNext = true;
								}
								if (this.bPushNext && extraMessageCount > num)
								{
									num++;
									flag = true;
								}
								this.bPushSet = false;
								this.bPushNext = false;
								if (flag && extraMessageCount > num)
								{
									flag = false;
									Bel.Instance.Bpecr1.GetExtraMessage(Convert.ToUInt16(num));
									if (Bel.Instance.Bpecr1.RetCode != 0)
									{
										this.bPushNext = true;
									}
									else
									{
										this.ShowCurrentExtraMess();
									}
								}
							}
							Thread.Sleep(100);
						}
						retCode = Bel.Instance.Bpecr1.RetCode;
						if (this.ThreadComplete != null)
						{
							this.ThreadComplete(retCode);
						}
					}
					else if (this.ThreadComplete != null)
					{
						this.ThreadComplete(retCode);
					}
				}
				else if (this.ThreadComplete != null)
				{
					this.ThreadComplete(18);
				}
			}
			catch (Exception exception)
			{
				if (this.ThreadComplete != null)
				{
					this.ThreadComplete(2);
				}
			}
		}

		private void SetExtraMessVisibble(short ModeShowControls)
		{
			switch (ModeShowControls)
			{
				case 0:
				{
					this.Frame_EM.Visible = false;
					this.Button_EMSet.Visible = false;
					this.Button_EMNext.Visible = false;
					this.TextBox_EMValue.Visible = false;
					this.Label_EMBody.Visible = false;
					this.LabelDataCardSet.Visible = false;
					this.ButtonSetDataCardFromMainForm.Visible = false;
					this.ComboBoxSelectCardType.Visible = false;
					this.TextBoxTrack1.Visible = false;
					this.TextBoxTrack2.Visible = false;
					this.CheckBoxDataInHexString.Visible = false;
					return;
				}
				case 1:
				{
					this.Frame_EM.Visible = true;
					this.Button_EMSet.Visible = true;
					this.Button_EMNext.Visible = true;
					this.TextBox_EMValue.Visible = true;
					this.Label_EMBody.Visible = true;
					this.Frame_EM.Select();
					this.Button_EMSet.Select();
					this.Button_EMSet.Focus();
					return;
				}
				case 2:
				{
					this.Frame_EM.Visible = true;
					this.Button_EMSet.Visible = true;
					this.Button_EMNext.Visible = true;
					this.TextBox_EMValue.Visible = true;
					this.Label_EMBody.Visible = true;
					this.Frame_EM.Select();
					this.Button_EMSet.Select();
					this.Button_EMSet.Focus();
					return;
				}
				case 3:
				{
					this.Frame_EM.Visible = true;
					this.Button_EMSet.Visible = true;
					this.Button_EMNext.Visible = true;
					this.TextBox_EMValue.Visible = true;
					this.Label_EMBody.Visible = true;
					this.Frame_EM.Select();
					this.Button_EMSet.Select();
					this.Button_EMSet.Focus();
					return;
				}
				case 4:
				{
					this.Frame_EM.Visible = true;
					this.Button_EMSet.Visible = true;
					this.Button_EMNext.Visible = true;
					this.TextBox_EMValue.Visible = true;
					this.Label_EMBody.Visible = true;
					this.LabelDataCardSet.Visible = true;
					this.ButtonSetDataCardFromMainForm.Visible = true;
					this.ComboBoxSelectCardType.Visible = true;
					this.TextBoxTrack1.Visible = true;
					this.TextBoxTrack2.Visible = true;
					this.CheckBoxDataInHexString.Visible = true;
					this.Frame_EM.Select();
					this.Button_EMSet.Select();
					this.Button_EMSet.Focus();
					return;
				}
				default:
				{
					return;
				}
			}
		}

		private void ShowCurrentExtraMess()
		{
			this.bPushSet = false;
			this.bPushNext = false;
			if (Bel.Instance.Bpecr1.ExtraMessageType != 1 && Bel.Instance.Bpecr1.ExtraMessageType != 2 && Bel.Instance.Bpecr1.ExtraMessageType != 3)
			{
				if (Bel.Instance.Bpecr1.ExtraMessageType != 4)
				{
					this.SetExtraMessVisibble(0);
					this.bPushNext = true;
					return;
				}
				this.TextBox_EMValue.Text = Bel.Instance.Bpecr1.ExtraMessageValue as string;
				this.Label_EMBody.Text = Bel.Instance.Bpecr1.ExtraMessageBody as string;
				if (Bel.Instance.Bpecr1.ExtraMessageFlags != 1)
				{
					this.Button_EMNext.Enabled = true;
				}
				else
				{
					this.Button_EMNext.Enabled = false;
				}
				this.ComboBoxSelectCardType.SelectedIndex = 12;
				this.SetExtraMessVisibble(4);
				return;
			}
			this.TextBox_EMValue.Text = Bel.Instance.Bpecr1.ExtraMessageValue as string;
			this.Label_EMBody.Text = Bel.Instance.Bpecr1.ExtraMessageBody as string;
			if (Bel.Instance.Bpecr1.ExtraMessageFlags != 1)
			{
				this.Button_EMNext.Enabled = true;
			}
			else
			{
				this.Button_EMNext.Enabled = false;
			}
			string text = this.Label_EMBody.Text;
			if (!this.RadioButtonMessageShow(ref text))
			{
				this.IsStrFormatRequest = false;
				this.SetExtraMessVisibble(1);
				return;
			}
			this.Label_EMBody.Text = text;
			this.IsStrFormatRequest = true;
		}

		private void TextBox_EMValue_TextChanged(object sender, EventArgs e)
		{
		}

		private void Timer1_Tick(object sender, EventArgs e)
		{
			string str = null;
			string str1 = null;
			string str2 = null;
			str = "";
			str1 = "";
			str2 = "";
			this.Timer1.Enabled = false;
			if (this.pBELForm.ConvertMagTrack(ref this.strMagTrack, ref str, ref str1, ref str2))
			{
				if (this.CheckBoxDataInHexString.Checked)
				{
					this.TextBoxTrack1.Text = Utils.Str2Hex(str);
					this.TextBoxTrack2.Text = Utils.Str2Hex(str1);
					return;
				}
				this.TextBoxTrack1.Text = str;
				this.TextBoxTrack2.Text = str1;
			}
		}

		public event DialogPerfOper.ThreadCompleteEventHandler ThreadComplete;

		public delegate void EndHandlingCallback(int RetCode);

		public delegate void ThreadCompleteEventHandler(int RetCode);
	}
}