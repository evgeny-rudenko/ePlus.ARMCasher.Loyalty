using ePlus.Loyalty;
using ePlus.Loyalty.Interfaces;
using ePlus.Loyalty.SailPlay;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.SailPlay
{
	public class UserRegisterPresenter : IDisposable
	{
		private FormSailPlayUserRegister View
		{
			get;
			set;
		}

		public UserRegisterPresenter(FormSailPlayUserRegister view)
		{
			this.View = view;
		}

		public void Dispose()
		{
			if (this.View != null)
			{
				this.View.Dispose();
			}
		}

		public UserInfoResult ShowView()
		{
			if (this.View.ShowDialog() != DialogResult.OK)
			{
				return null;
			}
			return this.View.UserInfo;
		}

		public UserInfoResult ShowView(string clientId, PublicIdType clientIdType)
		{
			string str;
			string str1;
			UserInfoResult userInfoResult = new UserInfoResult();
			UserInfoResult userInfoResult1 = userInfoResult;
			if (clientIdType == PublicIdType.CardNumber)
			{
				str = clientId;
			}
			else
			{
				str = null;
			}
			userInfoResult1.ID = str;
			UserInfoResult userInfoResult2 = userInfoResult;
			if (clientIdType == PublicIdType.Phone)
			{
				str1 = string.Concat("+", clientId);
			}
			else
			{
				str1 = null;
			}
			userInfoResult2.Phone = str1;
			userInfoResult.Sex = "1";
			userInfoResult.AgeTag = "35-45";
			if (this.View.ShowDialog(userInfoResult) != DialogResult.OK)
			{
				return null;
			}
			return this.View.UserInfo;
		}

		public UserInfoResult ShowView(IUserInfo userInfo)
		{
			if (this.View.ShowDialog((UserInfoResult)userInfo) != DialogResult.OK)
			{
				return null;
			}
			return this.View.UserInfo;
		}
	}
}