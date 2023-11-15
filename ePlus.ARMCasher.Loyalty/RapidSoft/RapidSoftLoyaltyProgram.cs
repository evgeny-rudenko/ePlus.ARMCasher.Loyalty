using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon.Config;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.Loyalty.RapidSoft;
using ePlus.MetaData.Client;
using RapidSoft.Loyalty.PosConnector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.RapidSoft
{
	internal sealed class RapidSoftLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private const string Currency = "RUR";

		private readonly Guid ID_DISCOUNT2_CARD_GLOBAL = new Guid("D04FD3C9-82D9-46B1-BDA8-9A728AB5E7C1");

		private static bool _isSettingsInit;

		private static Dictionary<Guid, DataRowItem> _excludedPrograms;

		private static string _name;

		private static Guid _idGlobal;

		private Guid? _lastTranRequestId;

		private Guid? _lastTranChequeId;

		private GetBalanceResponse _balance;

		protected string CardNumber
		{
			get;
			set;
		}

		private static ePlus.Loyalty.RapidSoft.Certificate Certificate
		{
			get;
			set;
		}

		public override Guid IdGlobal
		{
			get
			{
				return RapidSoftLoyaltyProgram._idGlobal;
			}
		}

		private static bool IscompatibilityEnabled
		{
			get;
			set;
		}

		private bool IsSettingsInit
		{
			get
			{
				return RapidSoftLoyaltyProgram._isSettingsInit;
			}
			set
			{
				RapidSoftLoyaltyProgram._isSettingsInit = value;
			}
		}

		public override string Name
		{
			get
			{
				return RapidSoftLoyaltyProgram._name;
			}
		}

		private static string PosId
		{
			get;
			set;
		}

		private static ePlus.Loyalty.RapidSoft.Settings Settings
		{
			get;
			set;
		}

		static RapidSoftLoyaltyProgram()
		{
			RapidSoftLoyaltyProgram._excludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public RapidSoftLoyaltyProgram(string PublicId, string cardNumber) : base(ePlus.Loyalty.LoyaltyType.RapidSoft, PublicId, cardNumber, "")
		{
			this.CardNumber = cardNumber;
			this.LoadSettings();
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			return new decimal(0);
		}

		private PosConnectorClient Connect()
		{
			this.LoadSettings();
			if (RapidSoftLoyaltyProgram.Settings == null)
			{
				throw new Exception("Не задана конфигурация модуля RapidSoft.");
			}
			EndpointAddress endpointAddress = new EndpointAddress(RapidSoftLoyaltyProgram.Settings.Url);
			PosConnectorClient posConnectorClient = new PosConnectorClient(this.CreateBasicHttpBinding(), endpointAddress);
			BasicHttpBinding binding = posConnectorClient.Endpoint.Binding as BasicHttpBinding;
			binding.set_UseDefaultWebProxy(false);
			binding.set_BypassProxyOnLocal(false);
			if (RapidSoftLoyaltyProgram.Settings.Proxy.Use)
			{
				binding.Security.Mode = BasicHttpSecurityMode.Transport;
				binding.set_ProxyAddress(new Uri(string.Concat(RapidSoftLoyaltyProgram.get_Settings().get_Proxy().get_Address(), ":", RapidSoftLoyaltyProgram.get_Settings().get_Proxy().get_Port())));
				binding.set_UseDefaultWebProxy(false);
				binding.set_BypassProxyOnLocal(false);
				binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.Basic;
				posConnectorClient.ClientCredentials.UserName.UserName = RapidSoftLoyaltyProgram.Settings.Proxy.User;
				posConnectorClient.ClientCredentials.UserName.Password = RapidSoftLoyaltyProgram.Settings.Proxy.Password;
			}
			X509Store x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			x509Store.Open(OpenFlags.ReadOnly);
			X509Certificate2Collection certificates = x509Store.Certificates;
			X509Certificate2Collection x509Certificate2Collection = certificates.Find(X509FindType.FindByIssuerName, RapidSoftLoyaltyProgram.Certificate.CenterName, false);
			x509Store.Close();
			if (x509Certificate2Collection.Count > 1)
			{
				throw new Exception("Обнаружено более одного сертификата RapidSoft. Исправьте конфигурацию.");
			}
			if (x509Certificate2Collection == null || x509Certificate2Collection.Count <= 0)
			{
				throw new Exception("Не установлен сертификат клиента для работы с RapidSoft.");
			}
			posConnectorClient.ClientCredentials.ClientCertificate.Certificate = x509Certificate2Collection[0];
			return posConnectorClient;
		}

		private void CreateAndSavePCXChequeItemList(IEnumerable<CHEQUE_ITEM> chequeItemList, decimal totalSum, decimal pcxSumMoney, Guid transactionId, string operType)
		{
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			foreach (CHEQUE_ITEM cHEQUEITEM in chequeItemList)
			{
				decimal sUMM = cHEQUEITEM.SUMM;
				foreach (DISCOUNT2_MAKE_ITEM discount2MakeItemList in cHEQUEITEM.Discount2MakeItemList)
				{
					if (discount2MakeItemList.TYPE != "RAPID")
					{
						continue;
					}
					sUMM += discount2MakeItemList.AMOUNT;
				}
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, sUMM);
			}
			IDictionary<Guid, decimal> guids1 = LoyaltyProgManager.Distribute(guids, totalSum, Math.Abs(pcxSumMoney), false);
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM1 in chequeItemList)
			{
				PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
				{
					TRANSACTION_ID = transactionId.ToString(),
					CLIENT_ID = base.ClientId,
					CLIENT_ID_TYPE = (int)base.LoyaltyType,
					OPER_TYPE = operType
				};
				PCX_CHEQUE_ITEM item = pCXCHEQUEITEM;
				pCXCHEQUEITEMs.Add(item);
				item.SUMM = guids1[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL];
				item.ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL;
			}
			if (pCXCHEQUEITEMs.Count > 0)
			{
				(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
			}
		}

		private BasicHttpBinding CreateBasicHttpBinding()
		{
			BasicHttpBinding basicHttpBinding = new BasicHttpBinding()
			{
				Name = "BasicHttpsBinding_PosConnector"
			};
			basicHttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
			basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;
			return basicHttpBinding;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			throw new NotImplementedException();
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			throw new NotImplementedException();
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			PosConnectorClient posConnectorClient = this.Connect();
			if (posConnectorClient == null)
			{
				throw new Exception("Не удалось получить доступ к сервису RapidSoft");
			}
			this._balance = new GetBalanceResponse();
			GetBalanceRequest getBalanceRequest = new GetBalanceRequest()
			{
				CardData = base.ClientId,
				PartnerId = RapidSoftLoyaltyProgram.Settings.PartnerId,
				PosId = RapidSoftLoyaltyProgram.PosId,
				RequestDateTime = DateTime.Now,
				TerminalId = RapidSoftLoyaltyProgram.Settings.Terminal,
				RequestId = Guid.NewGuid().ToString()
			};
			this._balance = posConnectorClient.GetBalance(getBalanceRequest);
			string empty = string.Empty;
			int cardStatus = this._balance.CardStatus;
			if (cardStatus <= 20)
			{
				switch (cardStatus)
				{
					case 0:
					{
						empty = "Не найдена";
						break;
					}
					case 1:
					{
						empty = "Активна";
						break;
					}
					default:
					{
						if (cardStatus == 20)
						{
							empty = "Лимитирована";
							break;
						}
						else
						{
							break;
						}
					}
				}
			}
			else if (cardStatus == 30)
			{
				empty = "Заблокирована";
			}
			else if (cardStatus == 40)
			{
				empty = "Не активирована";
			}
			else if (cardStatus == 50)
			{
				empty = "Истёк срок действия";
			}
			LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
			{
				Balance = this._balance.MoneyBalance,
				Points = this._balance.PointsBalance,
				CardNumber = this.CardNumber,
				CardStatus = empty,
				ClientId = base.ClientId
			};
			return loyaltyCardInfo;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			this.LoadSettings();
			if (!RapidSoftLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !RapidSoftLoyaltyProgram._excludedPrograms.ContainsKey(discountId);
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
			throw new NotImplementedException();
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
			throw new NotImplementedException();
		}

		protected override void DoRollback(out string slipCheque)
		{
			slipCheque = null;
			throw new NotImplementedException();
		}

		private void LoadSettings()
		{
			if (this.IsSettingsInit)
			{
				return;
			}
			try
			{
				using (SqlConnection sqlConnection = new SqlConnection(MultiServerBL.ClientConnectionString))
				{
					using (SqlCommand sqlCommand = new SqlCommand("SELECT TOP 1 VALUE FROM SYS_OPTION WHERE CODE = 'A_COD_CONTRACTOR_SELF'", sqlConnection))
					{
						sqlConnection.Open();
						RapidSoftLoyaltyProgram.PosId = (string)sqlCommand.ExecuteScalar();
					}
				}
			}
			catch (Exception exception)
			{
				throw new Exception("Не удалось получить код контрагента из базы АРМ", exception);
			}
			SettingsModel settingsModel = new SettingsModel();
			LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
			RapidSoftLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.RapidSoft.Settings>(loyaltySetting.SETTINGS, "Settings");
			RapidSoftLoyaltyProgram.Settings.Terminal = Convert.ToString(AppConfigManager.IdCashRegister);
			RapidSoftLoyaltyProgram.Certificate = settingsModel.Deserialize<ePlus.Loyalty.RapidSoft.Certificate>(loyaltySetting.SETTINGS, "Certificate");
			RapidSoftLoyaltyProgram._idGlobal = loyaltySetting.ID_LOYALITY_PROGRAM_GLOBAL;
			RapidSoftLoyaltyProgram._name = loyaltySetting.NAME;
			RapidSoftLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
			if (RapidSoftLoyaltyProgram.IscompatibilityEnabled)
			{
				RapidSoftLoyaltyProgram._excludedPrograms.Add(this.IdGlobal, null);
				foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
				{
					RapidSoftLoyaltyProgram._excludedPrograms.Add(excludeList.Guid, excludeList);
				}
				foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
				{
					RapidSoftLoyaltyProgram._excludedPrograms.Add(dataRowItem.Guid, dataRowItem);
				}
				foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
				{
					RapidSoftLoyaltyProgram._excludedPrograms.Add(excludeList1.Guid, excludeList1);
				}
			}
			this.IsSettingsInit = true;
		}

		protected override void OnInitInternal()
		{
			throw new NotImplementedException();
		}

		protected override void OnInitSettings()
		{
			throw new NotImplementedException();
		}

		private void RollbackLastTransaction(out string slipCheque)
		{
			if (!this._lastTranRequestId.HasValue || !this._lastTranChequeId.HasValue)
			{
				throw new Exception("Нет данных о последней транзакции");
			}
			slipCheque = string.Empty;
			DateTime now = DateTime.Now;
			Guid guid = Guid.NewGuid();
			PosConnectorClient posConnectorClient = this.Connect();
			if (posConnectorClient == null)
			{
				throw new Exception("Не удалось получить доступ к сервису RapidSoft");
			}
			RollbackRequest rollbackRequest = new RollbackRequest()
			{
				PartnerId = RapidSoftLoyaltyProgram.Settings.PartnerId,
				PosId = RapidSoftLoyaltyProgram.PosId,
				TerminalId = RapidSoftLoyaltyProgram.Settings.Terminal,
				RequestDateTime = now,
				RequestId = guid.ToString(),
				OriginalRequestId = this._lastTranRequestId.Value.ToString()
			};
			RollbackResponse rollbackResponse = null;
			try
			{
				try
				{
					rollbackResponse = posConnectorClient.Rollback(rollbackRequest);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (!(exception is FaultException) || !(exception.Message == "Forbidden"))
					{
						throw exception;
					}
					throw new Exception("Процессинговый центр отклонил запрос по причине ошибки аутентификации.");
				}
			}
			finally
			{
				posConnectorClient.Close();
			}
			if (rollbackResponse.Status == 0)
			{
				StringBuilder stringBuilder = new StringBuilder("ОТМЕНА");
				stringBuilder.AppendLine("Точка обслуживания ").Append(RapidSoftLoyaltyProgram.PosId);
				stringBuilder.AppendLine("Дата и время ").Append(now);
				stringBuilder.AppendLine(RapidSoftHelper.OperationStatusText((RollbackStatus)rollbackResponse.Status));
				stringBuilder.AppendLine();
				slipCheque = stringBuilder.ToString();
			}
			string str = RapidSoftHelper.OperationStatusText((RollbackStatus)rollbackResponse.Status);
			if (rollbackResponse.Status != 0)
			{
				throw new Exception(string.Concat("Ошибка удаления скидки: ", str));
			}
			BaseLoyaltyProgramEx.QueryLogBl.ReverseQuery(this._lastTranChequeId.Value.ToString());
			string str1 = string.Format("TRANSACTION_ID = '{0}'", this._lastTranChequeId.Value);
			BaseLoyaltyProgramEx.PCXChequeItemLoader.Delete(str1);
			BaseLoyaltyProgramEx.PCXChequeLoader.Delete(str1);
		}
	}
}