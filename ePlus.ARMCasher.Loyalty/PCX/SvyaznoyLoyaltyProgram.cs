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
	internal class SvyaznoyLoyaltyProgram : PCXLoyaltyProgramEx
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
				return SvyaznoyLoyaltyProgram._chequeOperTypeCharge;
			}
		}

		protected override Guid ChequeOperTypeDebit
		{
			get
			{
				return SvyaznoyLoyaltyProgram._chequeOperTypeDebit;
			}
		}

		protected override Guid ChequeOperTypeRefundCharge
		{
			get
			{
				return SvyaznoyLoyaltyProgram._chequeOperTypeRefundCharge;
			}
		}

		protected override Guid ChequeOperTypeRefundDebit
		{
			get
			{
				return SvyaznoyLoyaltyProgram._chequeOperTypeRefundDebit;
			}
		}

		protected override decimal Devider
		{
			get
			{
				return SvyaznoyLoyaltyProgram.Params.Devider;
			}
			set
			{
				SvyaznoyLoyaltyProgram.Params.Devider = value;
			}
		}

		public override Guid IdGlobal
		{
			get
			{
				return SvyaznoyLoyaltyProgram._id;
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
				return SvyaznoyLoyaltyProgram._isSettingsInitialized;
			}
			set
			{
				SvyaznoyLoyaltyProgram._isSettingsInitialized = value;
			}
		}

		protected override decimal MinChequeSumForCharge
		{
			get
			{
				return SvyaznoyLoyaltyProgram.Params.MinChequeSumForCharge;
			}
			set
			{
				SvyaznoyLoyaltyProgram.Params.MinChequeSumForCharge = value;
			}
		}

		protected override decimal MinPayPercent
		{
			get
			{
				return SvyaznoyLoyaltyProgram.Params.MinPayPercent;
			}
			set
			{
				SvyaznoyLoyaltyProgram.Params.MinPayPercent = value;
			}
		}

		public override string Name
		{
			get
			{
				return SvyaznoyLoyaltyProgram._name;
			}
		}

		protected override decimal OfflineChargePercent
		{
			get
			{
				return new decimal(10, 0, 0, false, 1);
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
				return SvyaznoyLoyaltyProgram.Params.UnitName;
			}
			set
			{
				SvyaznoyLoyaltyProgram.Params.UnitName = value;
			}
		}

		static SvyaznoyLoyaltyProgram()
		{
			SvyaznoyLoyaltyProgram._chequeOperTypeCharge = new Guid("D4D0A8BF-36E0-49EB-BE2D-E4EC6A64E1A9");
			SvyaznoyLoyaltyProgram._chequeOperTypeDebit = new Guid("52AE7BC7-A480-4B69-BA2F-D4AD7E60EAFC");
			SvyaznoyLoyaltyProgram._chequeOperTypeRefundCharge = new Guid("738AB360-B233-4719-A51D-E052E6ED1C70");
			SvyaznoyLoyaltyProgram._chequeOperTypeRefundDebit = new Guid("09432046-CAA6-4102-9FC6-241BCAF83B95");
			SvyaznoyLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public SvyaznoyLoyaltyProgram(string clientId) : base(ePlus.Loyalty.LoyaltyType.Svyaznoy, clientId, clientId, "SVYAZ")
		{
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (!SvyaznoyLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !SvyaznoyLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		protected override void OnInitSettings()
		{
			SettingsModel settingsModel = new SettingsModel();
			LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
			SvyaznoyLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.PharmacyWallet.Settings>(loyaltySetting.SETTINGS, "Settings");
			SvyaznoyLoyaltyProgram.Certificate = settingsModel.Deserialize<ePlus.Loyalty.PharmacyWallet.Certificate>(loyaltySetting.SETTINGS, "Certificate");
			SvyaznoyLoyaltyProgram.Params = settingsModel.Deserialize<ePlus.Loyalty.PharmacyWallet.Params>(loyaltySetting.PARAMS, "Params");
			SvyaznoyLoyaltyProgram.Params.Devider = (SvyaznoyLoyaltyProgram.Params.Devider == new decimal(0) ? new decimal(1) : SvyaznoyLoyaltyProgram.Params.Devider);
			SvyaznoyLoyaltyProgram.Params.ScorePerRub = (SvyaznoyLoyaltyProgram.Params.ScorePerRub == new decimal(0) ? new decimal(1) : SvyaznoyLoyaltyProgram.Params.ScorePerRub);
			base.SendRecvTimeout = (SvyaznoyLoyaltyProgram.Settings.SendReciveTimeout == 0 ? 30 : SvyaznoyLoyaltyProgram.Settings.SendReciveTimeout);
			SvyaznoyLoyaltyProgram._id = loyaltySetting.ID_LOYALITY_PROGRAM_GLOBAL;
			SvyaznoyLoyaltyProgram._name = loyaltySetting.NAME;
			SvyaznoyLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
			if (SvyaznoyLoyaltyProgram.IscompatibilityEnabled)
			{
				SvyaznoyLoyaltyProgram.ExcludedPrograms.Add(this.IdGlobal, null);
				foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
				{
					SvyaznoyLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
				}
				foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
				{
					SvyaznoyLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
				}
				foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
				{
					SvyaznoyLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
				}
			}
		}

		protected override void OnPCXSettings()
		{
			base.PcxObject.ConnectionString = SvyaznoyLoyaltyProgram.Settings.Url;
			base.PcxObject.ConnectTimeout = SvyaznoyLoyaltyProgram.Settings.ConnectionTimeout;
			base.PcxObject.SendRecvTimeout = SvyaznoyLoyaltyProgram.Settings.SendReciveTimeout;
			base.PcxObject.Location = SvyaznoyLoyaltyProgram.Settings.Location;
			base.PcxObject.PartnerID = SvyaznoyLoyaltyProgram.Settings.PartnerId;
			base.PcxObject.BackgndFlushPeriod = SvyaznoyLoyaltyProgram.Settings.BkgndFlushPeriod;
			if (SvyaznoyLoyaltyProgram.Settings.Proxy.Use)
			{
				base.PcxObject.ProxyHost = SvyaznoyLoyaltyProgram.Settings.Proxy.Address;
				base.PcxObject.ProxyPort = SvyaznoyLoyaltyProgram.Settings.Proxy.Port;
				base.PcxObject.ProxyUserId = SvyaznoyLoyaltyProgram.Settings.Proxy.User;
				base.PcxObject.ProxyUserPass = SvyaznoyLoyaltyProgram.Settings.Proxy.Password;
			}
			base.PcxObject.Terminal = SvyaznoyLoyaltyProgram.Settings.Terminal;
			if (!SvyaznoyLoyaltyProgram.Certificate.SertInStorage)
			{
				base.PcxObject.CertFilePath = SvyaznoyLoyaltyProgram.Certificate.SertFilePath;
				base.PcxObject.KeyFilePath = SvyaznoyLoyaltyProgram.Certificate.KeyFilePath;
				base.PcxObject.KeyPassword = SvyaznoyLoyaltyProgram.Certificate.CertPassword;
			}
			else
			{
				base.PcxObject.CertSubjectName = SvyaznoyLoyaltyProgram.Certificate.SertName;
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