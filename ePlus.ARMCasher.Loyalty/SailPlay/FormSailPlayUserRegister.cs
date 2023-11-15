using ePlus.ARMCasher.Loyalty.Properties;
using ePlus.ARMUtils;
using ePlus.Loyalty.SailPlay;
using ePlus.Loyalty.SailPlay.Wpf;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace ePlus.ARMCasher.Loyalty.SailPlay
{
	public class FormSailPlayUserRegister : Form
	{
		private MaskedTextProvider _maskedProvider = new MaskedTextProvider("+7 (###) ### ## ##");

		private IContainer components;

		private ElementHost elementHost;

		private ElementHost elementHostButtons;

		private OkCancelButtonsControl okCancelButtonsControl;

		private Label label1;

		private SailPlayUserRegControl sailPlayUserRegControl;

		public bool OnlyEmailEdit
		{
			get;
			set;
		}

		public UserInfoResult UserInfo
		{
			get
			{
				return (UserInfoResult)this.sailPlayUserRegControl.DataContext;
			}
			set
			{
				this.sailPlayUserRegControl.DataContext = value;
			}
		}

		public FormSailPlayUserRegister()
		{
			this.InitializeComponent();
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Width = this.BackgroundImage.Width;
			base.Height = this.BackgroundImage.Height;
			this.RefreshImage();
			base.TransparencyKey = Color.FromArgb(0, 0, 0);
			base.StartPosition = FormStartPosition.CenterScreen;
			this.BackColor = Color.Black;
			this.elementHost.BackColorTransparent = true;
			this.elementHostButtons.BackColorTransparent = true;
			this.okCancelButtonsControl.OkClick += new EventHandler(this.OkClick);
			this.okCancelButtonsControl.CancelClick += new EventHandler(this.CancelClick);
		}

		private void CancelClick(object sender, EventArgs e)
		{
			base.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
			ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof(FormSailPlayUserRegister));
			this.elementHost = new ElementHost();
			this.sailPlayUserRegControl = new SailPlayUserRegControl();
			this.elementHostButtons = new ElementHost();
			this.okCancelButtonsControl = new OkCancelButtonsControl();
			this.label1 = new Label();
			base.SuspendLayout();
			this.elementHost.Location = new System.Drawing.Point(66, 102);
			this.elementHost.Name = "elementHost";
			this.elementHost.Size = new System.Drawing.Size(430, 410);
			this.elementHost.TabIndex = 0;
			this.elementHost.Text = "elementHost1";
			this.elementHost.Child = this.sailPlayUserRegControl;
			this.elementHostButtons.Location = new System.Drawing.Point(125, 515);
			this.elementHostButtons.Name = "elementHostButtons";
			this.elementHostButtons.Size = new System.Drawing.Size(324, 48);
			this.elementHostButtons.TabIndex = 1;
			this.elementHostButtons.Text = "elementHost1";
			this.elementHostButtons.Child = this.okCancelButtonsControl;
			this.label1.AutoSize = true;
			this.label1.BackColor = Color.Transparent;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 204);
			this.label1.ForeColor = Color.FromArgb(1, 1, 1);
			this.label1.Location = new System.Drawing.Point(148, 566);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(273, 18);
			this.label1.TabIndex = 2;
			this.label1.Text = "* поля обязательные для заполнения";
			base.AutoScaleDimensions = new SizeF(6f, 13f);
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = Resources.UserRegForm;
			base.ClientSize = new System.Drawing.Size(600, 618);
			base.Controls.Add(this.label1);
			base.Controls.Add(this.elementHostButtons);
			base.Controls.Add(this.elementHost);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			base.Icon = (System.Drawing.Icon)componentResourceManager.GetObject("$this.Icon");
			this.MaximumSize = new System.Drawing.Size(600, 618);
			this.MinimumSize = new System.Drawing.Size(600, 618);
			base.Name = "FormSailPlayUserRegister";
			this.Text = "Регистрация нового пользователя";
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		private bool IsValid()
		{
			if (!this.OnlyEmailEdit)
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (string.IsNullOrEmpty(this.UserInfo.FirstName))
				{
					stringBuilder.AppendLine("Необходимо задать имя клиента");
				}
				if (!this._maskedProvider.Set(this.UserInfo.Phone) || !this._maskedProvider.MaskCompleted)
				{
					stringBuilder.AppendLine("Необходимо задать телефон клиента");
				}
				if (stringBuilder.Length > 0)
				{
					System.Windows.Forms.MessageBox.Show(stringBuilder.ToString(), "Не указаны обязательные данные", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return false;
				}
			}
			if (string.IsNullOrEmpty(this.UserInfo.EMail) || UtilsArm.IsValidMailAddress(this.UserInfo.EMail))
			{
				return true;
			}
			System.Windows.Forms.MessageBox.Show("Задан неправильный формат электронной почты");
			return false;
		}

		private void OkClick(object sender, EventArgs e)
		{
			if (!this.IsValid())
			{
				return;
			}
			if (!this.OnlyEmailEdit)
			{
				string str = this._maskedProvider.ToString(false, false);
				this.UserInfo.Phone = ((str.StartsWith("7") ? str : string.Concat("7", str))).Replace("+", "");
			}
			base.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			if (e.Button == System.Windows.Forms.MouseButtons.Left && e.X > 530 && e.Y < 60)
			{
				base.Close();
			}
		}

		public void RefreshImage()
		{
			Bitmap bitmap = new Bitmap(this.BackgroundImage);
			for (int i = 0; i < bitmap.Width; i++)
			{
				for (int j = 0; j < bitmap.Height; j++)
				{
					Color pixel = bitmap.GetPixel(i, j);
					if (pixel.B != 0 && pixel.B < 229)
					{
						bitmap.SetPixel(i, j, Color.Black);
					}
				}
			}
			this.BackgroundImage = bitmap;
		}

		public System.Windows.Forms.DialogResult ShowDialog(UserInfoResult userInfo)
		{
			this.UserInfo = userInfo;
			return base.ShowDialog();
		}
	}
}