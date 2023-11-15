using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.Loyalty;
using ePlus.CommonEx;
using ePlus.Loyalty;
using ePlus.Loyalty.PharmacyWallet;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using winpcxLib;

namespace ePlus.ARMCasher.Loyalty.PCX
{
	internal class PharmacyWalletLoyaltyProgram : PCXLoyaltyProgramEx
	{
		private readonly static Guid _chequeOperTypeCharge;

		private readonly static Guid _chequeOperTypeDebit;

		private readonly static Guid _chequeOperTypeRefundCharge;

		private readonly static Guid _chequeOperTypeRefundDebit;

		private static string _name;

		private static Guid _id;

		private static bool _isSettingsInitialized;

		private static Dictionary<Guid, DataRowItem> ExcludedPrograms;

		private static ePlus.Loyalty.PharmacyWallet.Certificate Certificate
		{
			get;
			set;
		}

		protected override Guid ChequeOperTypeCharge
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._chequeOperTypeCharge;
			}
		}

		protected override Guid ChequeOperTypeDebit
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._chequeOperTypeDebit;
			}
		}

		protected override Guid ChequeOperTypeRefundCharge
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._chequeOperTypeRefundCharge;
			}
		}

		protected override Guid ChequeOperTypeRefundDebit
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._chequeOperTypeRefundDebit;
			}
		}

		protected override decimal Devider
		{
			get
			{
				return PharmacyWalletLoyaltyProgram.Params.Devider;
			}
			set
			{
				PharmacyWalletLoyaltyProgram.Params.Devider = value;
			}
		}

		public override Guid IdGlobal
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._id;
			}
		}

		private static bool IscompatibilityEnabled
		{
			get;
			set;
		}

		protected override bool IsSettingsInitialized
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._isSettingsInitialized;
			}
			set
			{
				PharmacyWalletLoyaltyProgram._isSettingsInitialized = value;
			}
		}

		protected override decimal MinChequeSumForCharge
		{
			get
			{
				return PharmacyWalletLoyaltyProgram.Params.MinChequeSumForCharge;
			}
			set
			{
				PharmacyWalletLoyaltyProgram.Params.MinChequeSumForCharge = value;
			}
		}

		protected override decimal MinPayPercent
		{
			get
			{
				return PharmacyWalletLoyaltyProgram.Params.MinPayPercent;
			}
			set
			{
				PharmacyWalletLoyaltyProgram.Params.MinPayPercent = value;
			}
		}

		public override string Name
		{
			get
			{
				return PharmacyWalletLoyaltyProgram._name;
			}
		}

		protected override decimal OfflineChargePercent
		{
			get
			{
				return new decimal(20, 0, 0, false, 1);
			}
		}

		private static ePlus.Loyalty.PharmacyWallet.Params Params
		{
			get;
			set;
		}

		private static ePlus.Loyalty.PharmacyWallet.Settings Settings
		{
			get;
			set;
		}

		protected override string UnitName
		{
			get
			{
				return PharmacyWalletLoyaltyProgram.Params.UnitName;
			}
			set
			{
				PharmacyWalletLoyaltyProgram.Params.UnitName = value;
			}
		}

		static PharmacyWalletLoyaltyProgram()
		{
			PharmacyWalletLoyaltyProgram._chequeOperTypeCharge = new Guid("C65D562E-F17E-4A5A-A851-966DE23BA7D6");
			PharmacyWalletLoyaltyProgram._chequeOperTypeDebit = new Guid("05F3045F-7F74-48D4-AA79-EF276C8A5A21");
			PharmacyWalletLoyaltyProgram._chequeOperTypeRefundCharge = new Guid("CDA64C3B-A3FC-4271-B706-5105F4003DA2");
			PharmacyWalletLoyaltyProgram._chequeOperTypeRefundDebit = new Guid("942CCDC8-3578-445A-8659-1F5FB67D2D53");
			PharmacyWalletLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public PharmacyWalletLoyaltyProgram(string clientId) : base(ePlus.Loyalty.LoyaltyType.PharmacyWallet, clientId, clientId, "PH_WL")
		{
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (!PharmacyWalletLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !PharmacyWalletLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		protected override void OnInitSettings()
		{
			SettingsModel settingsModel = new SettingsModel();
			LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
			PharmacyWalletLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.PharmacyWallet.Settings>(loyaltySetting.SETTINGS, "Settings");
			PharmacyWalletLoyaltyProgram.Certificate = settingsModel.Deserialize<ePlus.Loyalty.PharmacyWallet.Certificate>(loyaltySetting.SETTINGS, "Certificate");
			PharmacyWalletLoyaltyProgram.Params = settingsModel.Deserialize<ePlus.Loyalty.PharmacyWallet.Params>(loyaltySetting.PARAMS, "Params");
			PharmacyWalletLoyaltyProgram.Params.Devider = (PharmacyWalletLoyaltyProgram.Params.Devider == new decimal(0) ? new decimal(1) : PharmacyWalletLoyaltyProgram.Params.Devider);
			PharmacyWalletLoyaltyProgram.Params.ScorePerRub = (PharmacyWalletLoyaltyProgram.Params.ScorePerRub == new decimal(0) ? new decimal(1) : PharmacyWalletLoyaltyProgram.Params.ScorePerRub);
			base.SendRecvTimeout = (PharmacyWalletLoyaltyProgram.Settings.SendReciveTimeout == 0 ? 30 : PharmacyWalletLoyaltyProgram.Settings.SendReciveTimeout);
			PharmacyWalletLoyaltyProgram._id = loyaltySetting.ID_LOYALITY_PROGRAM_GLOBAL;
			PharmacyWalletLoyaltyProgram._name = loyaltySetting.NAME;
			PharmacyWalletLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
			if (PharmacyWalletLoyaltyProgram.IscompatibilityEnabled)
			{
				PharmacyWalletLoyaltyProgram.ExcludedPrograms.Add(this.IdGlobal, null);
				foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
				{
					PharmacyWalletLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
				}
				foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
				{
					PharmacyWalletLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
				}
				foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
				{
					PharmacyWalletLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
				}
			}
		}

		protected override void OnPCXSettings()
		{
			base.PcxObject.ConnectionString = PharmacyWalletLoyaltyProgram.Settings.Url;
			base.PcxObject.ConnectTimeout = PharmacyWalletLoyaltyProgram.Settings.ConnectionTimeout;
			base.PcxObject.SendRecvTimeout = PharmacyWalletLoyaltyProgram.Settings.SendReciveTimeout;
			base.PcxObject.Location = PharmacyWalletLoyaltyProgram.Settings.Location;
			base.PcxObject.PartnerID = PharmacyWalletLoyaltyProgram.Settings.PartnerId;
			base.PcxObject.BackgndFlushPeriod = PharmacyWalletLoyaltyProgram.Settings.BkgndFlushPeriod;
			if (PharmacyWalletLoyaltyProgram.Settings.Proxy.Use)
			{
				base.PcxObject.ProxyHost = PharmacyWalletLoyaltyProgram.Settings.Proxy.Address;
				base.PcxObject.ProxyPort = PharmacyWalletLoyaltyProgram.Settings.Proxy.Port;
				base.PcxObject.ProxyUserId = PharmacyWalletLoyaltyProgram.Settings.Proxy.User;
				base.PcxObject.ProxyUserPass = PharmacyWalletLoyaltyProgram.Settings.Proxy.Password;
			}
			base.PcxObject.Terminal = PharmacyWalletLoyaltyProgram.Settings.Terminal;
			if (!PharmacyWalletLoyaltyProgram.Certificate.SertInStorage)
			{
				base.PcxObject.CertFilePath = PharmacyWalletLoyaltyProgram.Certificate.SertFilePath;
				base.PcxObject.KeyFilePath = PharmacyWalletLoyaltyProgram.Certificate.KeyFilePath;
				base.PcxObject.KeyPassword = PharmacyWalletLoyaltyProgram.Certificate.CertPassword;
			}
			else
			{
				base.PcxObject.CertSubjectName = PharmacyWalletLoyaltyProgram.Certificate.SertName;
			}
			if (!LoyaltyProgManager.IsLoyalityProgramEnabled(ePlus.Loyalty.LoyaltyType.Svyaznoy) && !AppConfigurator.EnableSberbank)
			{
				return;
			}
			int num = base.PcxObject.Init();
			if (num != 0)
			{
				throw new LoyaltyException(this, string.Concat("Объект PCX, не создан  возможно ActiveX компонент не установлен", Environment.NewLine, ErrorMessage.GetErrorMessage(num, base.PcxObject.GetErrorMessage(num))));
			}
		}
	}
}