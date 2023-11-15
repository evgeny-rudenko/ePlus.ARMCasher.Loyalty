using ePlus.ARMBusinessLogic;
using ePlus.ARMBusinessLogic.Caches.BarcodesCaches;
using ePlus.ARMCacheManager;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCasher.Loyalty.AstraZeneca.Forms;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.Loyalty.AstraZeneca;
using ePlus.Loyalty.AstraZeneca.API;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.AstraZeneca
{
	internal class AstraZenecaLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private static Guid _id;

		private static string DiscountType;

		private static Dictionary<Guid, DataRowItem> ExcludedPrograms;

		private AstraZenecaWebApi azAPI;

		private string name = "АстраЗенека";

		private Dictionary<string, List<long>> extBarcodeCache;

		private RequestGetDiscount latestRequest;

		private ResponseGetDiscount latestResponse;

		public override Guid IdGlobal
		{
			get
			{
				return AstraZenecaLoyaltyProgram._id;
			}
		}

		private static bool IscompatibilityEnabled
		{
			get;
			set;
		}

		public override string Name
		{
			get
			{
				return this.name;
			}
		}

		protected override bool OnIsExplicitDiscount
		{
			get
			{
				return false;
			}
		}

		private string PosId
		{
			get;
			set;
		}

		private static ePlus.Loyalty.AstraZeneca.Settings Settings
		{
			get;
			set;
		}

		static AstraZenecaLoyaltyProgram()
		{
			AstraZenecaLoyaltyProgram._id = new Guid("647190D8-DC71-46FB-9112-5C3021256404");
			AstraZenecaLoyaltyProgram.DiscountType = "AZ_EX";
			AstraZenecaLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public AstraZenecaLoyaltyProgram(string publicId, string posId) : base(ePlus.Loyalty.LoyaltyType.AstraZeneca, publicId, publicId, "LP_AZ")
		{
			base.SendRecvTimeout = 30;
			this.PosId = posId;
		}

		private void CalculateDiscount(CHEQUE cheque)
		{
			string clientPublicId;
			string str;
			this.latestRequest = null;
			if (cheque.CHEQUE_ITEMS != null && cheque.CHEQUE_ITEMS.Count > 0)
			{
				LoyaltyCard loyaltyCard = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is AstraZenecaCard) as LoyaltyCard;
				base.GetLoyaltyCardInfo(false);
				RequestGetDiscount requestGetDiscount = this.azAPI.CreateRequest<RequestGetDiscount>(this.PosId);
				PublicIdType publicIdType = this.GetPublicIdType(base.ClientPublicId);
				RequestGetDiscount requestGetDiscount1 = requestGetDiscount;
				if (publicIdType == PublicIdType.CardNumber)
				{
					clientPublicId = base.ClientPublicId;
				}
				else
				{
					clientPublicId = null;
				}
				requestGetDiscount1.CardNumber = clientPublicId;
				string str1 = null;
				if (!string.IsNullOrEmpty(base.ClientPublicId))
				{
					str1 = (base.ClientPublicId.Contains("+") ? base.ClientPublicId : string.Concat("+", base.ClientPublicId));
				}
				RequestGetDiscount requestGetDiscount2 = requestGetDiscount;
				if (publicIdType == PublicIdType.Phone)
				{
					str = str1;
				}
				else
				{
					str = null;
				}
				requestGetDiscount2.PhoneNumber = str;
				requestGetDiscount.Orders = this.CreateOrderItems(cheque);
				if (!requestGetDiscount.Orders.Any<Order>())
				{
					return;
				}
				this.OnTraceMessage("отправляем запрос на получение скидки.", new object[0]);
				ResponseGetDiscount responseGetDiscount = null;
				if (requestGetDiscount.Orders.Any<Order>())
				{
					this.latestRequest = requestGetDiscount;
					responseGetDiscount = this.azAPI.CheckDiscount(requestGetDiscount);
					this.latestResponse = responseGetDiscount;
				}
				this.OnTraceMessage("обрабатываем результат.", new object[0]);
				loyaltyCard.ExtraDiscounts.Clear();
				cheque.CHEQUE_ITEMS.ForEach((CHEQUE_ITEM ci) => {
					ci.Transaction = null;
					ci.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.TYPE == AstraZenecaLoyaltyProgram.DiscountType);
				});
				if (responseGetDiscount != null)
				{
					ILoyaltyMessageList loyaltyMessageList = loyaltyCard as ILoyaltyMessageList;
					if (loyaltyMessageList != null && responseGetDiscount.Description != null)
					{
						string str2 = string.Format("{0}{2}{1}", responseGetDiscount.Description, responseGetDiscount.Message, Environment.NewLine);
						loyaltyMessageList.Add(new LoyaltyMessage(LoyaltyMessageType.Service, str2));
					}
					foreach (OrderResponse order in responseGetDiscount.Orders)
					{
						if (order.Discount <= new decimal(0))
						{
							continue;
						}
						CHEQUE_ITEM cHEQUEITEM = cheque.CHEQUE_ITEMS.FirstOrDefault<CHEQUE_ITEM>((CHEQUE_ITEM ci) => {
							if (order.AnyData != ci.ID_LOT_GLOBAL.ToString())
							{
								return false;
							}
							return ci.Transaction == null;
						});
						if (cHEQUEITEM == null)
						{
							continue;
						}
						object[] barcode = new object[] { this.GetBarcode(cHEQUEITEM), cHEQUEITEM.ID_LOT_GLOBAL };
						this.OnTraceMessage("найдена позиция чека [BARCODE={0}, LOT_ID={1}]", barcode);
						this.OnTraceMessage("номер транзакции [Transaction={0}]", new object[] { order.Transaction });
						decimal sUMM = cHEQUEITEM.SUMM - order.Value;
						CHEQUE_ITEM_TRANSACTION cHEQUEITEMTRANSACTION = new CHEQUE_ITEM_TRANSACTION()
						{
							TRANSACTION_ID = order.Transaction,
							ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL,
							SUM_DISCOUNT = sUMM,
							BARCODE = order.Barcode,
							CLIENT_ID = order.CardNumber
						};
						cHEQUEITEM.Transaction = cHEQUEITEMTRANSACTION;
						DISCOUNT_VALUE_INFO dISCOUNTVALUEINFO = new DISCOUNT_VALUE_INFO()
						{
							BARCODE = loyaltyCard.BARCODE,
							ID_DISCOUNT2_GLOBAL = ARM_DISCOUNT2_PROGRAM.AstraZenecaDiscountGUID,
							DISCOUNT2_NAME = order.Message,
							TYPE = AstraZenecaLoyaltyProgram.DiscountType,
							ID_LOT_GLOBAL = cHEQUEITEM.ID_LOT_GLOBAL,
							VALUE = sUMM
						};
						DISCOUNT_VALUE_INFO dISCOUNTVALUEINFO1 = dISCOUNTVALUEINFO;
						if (dISCOUNTVALUEINFO1.VALUE >= new decimal(0))
						{
							loyaltyCard.ExtraDiscounts.Add(dISCOUNTVALUEINFO1);
						}
						else
						{
							object[] lOTPRICEVATORIGINAL = new object[] { cHEQUEITEM.LOT_PRICE_VAT_ORIGINAL, cHEQUEITEM.QUANTITY, order.Value };
							this.OnErrorMessage("Ошибка! Рассчетная скидка меньше нуля: LOT_PRICE_VAT_ORIGINAL={0}, QUANTITY={1}, AZ_ORDER_VALUE={2}", lOTPRICEVATORIGINAL);
						}
					}
				}
				cheque.CalculateFields();
			}
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			this.CalculateDiscount(cheque);
			return new decimal(0);
		}

		private bool CardPrefixFound(string clientId, string prefixes = null)
		{
			if (prefixes == null)
			{
				prefixes = AstraZenecaLoyaltyProgram.Settings.CardPrefix;
			}
			if (prefixes.Contains<char>(';'))
			{
				string[] strArrays = prefixes.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					if (clientId.StartsWith(strArrays[i]))
					{
						return true;
					}
				}
			}
			else if (clientId.StartsWith(prefixes) && clientId.Length >= prefixes.Length)
			{
				return true;
			}
			return false;
		}

		private void ConfirmAzTransacrions(string clientId, IEnumerable<CHEQUE_ITEM_TRANSACTION> transactions)
		{
			string str;
			string str1;
			PublicIdType publicIdType = this.GetPublicIdType(clientId);
			RequestConfirmPurchase requestConfirmPurchase = this.azAPI.CreateRequest<RequestConfirmPurchase>(this.PosId);
			RequestConfirmPurchase requestConfirmPurchase1 = requestConfirmPurchase;
			if (publicIdType == PublicIdType.CardNumber)
			{
				str = clientId;
			}
			else
			{
				str = null;
			}
			requestConfirmPurchase1.CardNumber = str;
			RequestConfirmPurchase requestConfirmPurchase2 = requestConfirmPurchase;
			if (publicIdType == PublicIdType.Phone)
			{
				str1 = string.Concat("+", clientId);
			}
			else
			{
				str1 = null;
			}
			requestConfirmPurchase2.PhoneNumber = str1;
			requestConfirmPurchase.Transactions = 
				from t in transactions
				select t.TRANSACTION_ID;
			Response response = null;
			try
			{
				try
				{
					response = this.azAPI.ConfirmPurchase(requestConfirmPurchase);
					if (!response.IsSuccess)
					{
						this.OnErrorMessage("ошибка при подтверждении транзакций на сервере API", new object[0]);
						this.OnErrorMessage(response.Message, new object[0]);
						foreach (CHEQUE_ITEM_TRANSACTION transaction in transactions)
						{
							transaction.TransactionStatus = ChequeItemTransactionStatus.Error;
							transaction.ERROR_CODE = response.ErrorCode;
							transaction.MESSAGE = response.Message;
							CHEQUE_ITEM_TRANSACTION aTTEMPTSNUMBER = transaction;
							aTTEMPTSNUMBER.ATTEMPTS_NUMBER = (short)(aTTEMPTSNUMBER.ATTEMPTS_NUMBER + 1);
						}
					}
					else
					{
						this.OnTraceMessage("обновляем статус транзакций в БД", new object[0]);
						foreach (CHEQUE_ITEM_TRANSACTION errorCode in transactions)
						{
							errorCode.TransactionStatus = ChequeItemTransactionStatus.Confirmed;
							errorCode.ERROR_CODE = response.ErrorCode;
							errorCode.MESSAGE = response.Message;
							CHEQUE_ITEM_TRANSACTION cHEQUEITEMTRANSACTION = errorCode;
							cHEQUEITEMTRANSACTION.ATTEMPTS_NUMBER = (short)(cHEQUEITEMTRANSACTION.ATTEMPTS_NUMBER + 1);
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					this.OnErrorMessage("ошибка при подтверждении транзакций на сервере API", new object[0]);
					this.OnErrorMessage(exception.ToString(), new object[0]);
					foreach (CHEQUE_ITEM_TRANSACTION message in transactions)
					{
						message.MESSAGE = exception.Message;
						CHEQUE_ITEM_TRANSACTION aTTEMPTSNUMBER1 = message;
						aTTEMPTSNUMBER1.ATTEMPTS_NUMBER = (short)(aTTEMPTSNUMBER1.ATTEMPTS_NUMBER + 1);
					}
				}
			}
			finally
			{
				this.SaveAzTransactions(transactions);
			}
		}

		public void ConfirmStoredAzTransactions()
		{
			base.InitInternal();
			var dictionary = (
				from t in (new AzTransactionsBl()).GetListUnconfirmed()
				group t by new { ID_CHEQUE_GLOBAL = t.ID_CHEQUE_GLOBAL, CLIENT_ID = t.CLIENT_ID }).ToDictionary((g) => g.Key, (g) => g.ToList<CHEQUE_ITEM_TRANSACTION>());
			foreach (var key in dictionary.Keys)
			{
				this.ConfirmAzTransacrions(key.CLIENT_ID, dictionary[key]);
			}
		}

		public Task ConfirmStroredAzTransactionsAsync()
		{
			Task task = Task.Factory.StartNew(new Action(this.ConfirmStoredAzTransactions));
			return task;
		}

		private IEnumerable<Order> CreateOrderItems(CHEQUE cheque)
		{
			List<Order> orders = new List<Order>();
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				string barcode = this.GetBarcode(cHEQUEITEM);
				if (string.IsNullOrEmpty(barcode))
				{
					continue;
				}
				Order order = new Order()
				{
					Barcode = barcode,
					Price = cHEQUEITEM.SUMM / cHEQUEITEM.QUANTITY,
					Count = Convert.ToInt32(cHEQUEITEM.QUANTITY),
					AnyData = cHEQUEITEM.ID_LOT_GLOBAL.ToString()
				};
				orders.Add(order);
			}
			return orders;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			this.OnTraceMessage("Подтверждение транзакций Вместе онлайн", new object[0]);
			List<CHEQUE_ITEM_TRANSACTION> list = (
				from ci in cheque.CHEQUE_ITEMS
				where ci.Transaction != null
				select ci.Transaction).ToList<CHEQUE_ITEM_TRANSACTION>();
			if (list.Any<CHEQUE_ITEM_TRANSACTION>())
			{
				this.ConfirmAzTransacrions(base.ClientPublicId, list);
			}
			this.OnTraceMessage("Подтверждение транзакций Вместе онлайн завершено", new object[0]);
			this.ConfirmStroredAzTransactionsAsync();
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			this.DoProcess(cheque, discountSum, out result);
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			base.UpdateClientPublicId(base.ClientPublicId);
			LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
			{
				ClientId = base.ClientPublicId,
				ClientIdType = base.ClientPublicIdType,
				CardNumber = base.ClientPublicId,
				Balance = new decimal(0),
				CardStatus = "Активна",
				CardStatusId = LoyaltyCardStatus.Active
			};
			return loyaltyCardInfo;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (!AstraZenecaLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !AstraZenecaLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		private void DoProcess(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			if (this.latestRequest == null)
			{
				return;
			}
			ResponseGetDiscount discount = this.azAPI.GetDiscount(this.latestRequest);
			List<OrderResponse> list = discount.Orders.ToList<OrderResponse>();
			IEnumerable<CHEQUE_ITEM> cHEQUEITEMS = 
				from ci in cheque.CHEQUE_ITEMS
				where ci.Transaction != null
				select ci;
			foreach (CHEQUE_ITEM cHEQUEITEM in cHEQUEITEMS)
			{
				OrderResponse orderResponse = list.FirstOrDefault<OrderResponse>((OrderResponse o) => o.AnyData == cHEQUEITEM.ID_LOT_GLOBAL.ToString());
				list.Remove(orderResponse);
				if (orderResponse != null)
				{
					cHEQUEITEM.Transaction.TRANSACTION_ID = orderResponse.Transaction;
				}
				else
				{
					cHEQUEITEM.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.TYPE == AstraZenecaLoyaltyProgram.DiscountType);
					cHEQUEITEM.Transaction = null;
				}
			}
			if (list.Count > 0)
			{
				throw new ApplicationException("Не удалось связать ответ программы лояльности со строками чека");
			}
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoRollback(out string slipCheque)
		{
			slipCheque = string.Empty;
		}

		protected virtual string GetBarcode(CHEQUE_ITEM item)
		{
			if (this.extBarcodeCache == null)
			{
				this.extBarcodeCache = (new ExternalBarcodesCache(new MemoryCacheManager(MemoryCache.Default))).GetCache();
			}
			if (string.IsNullOrWhiteSpace(item.BARCODE))
			{
				string str = (
					from cache in this.extBarcodeCache
					where cache.Value.Contains(item.ID_GOODS)
					select cache.Key).LastOrDefault<string>();
				item.BARCODE = str;
			}
			return item.BARCODE;
		}

		private ResponseGetDiscount GetDiscount(RequestGetDiscount request)
		{
			ResponseGetDiscount discount = this.azAPI.GetDiscount(request);
			if (!discount.IsSuccess)
			{
				object[] errorCode = new object[] { discount.ErrorCode, discount.Message, discount.Status };
				throw new LoyaltyException(this, this.FormatMessage("status - {2} error_code - {0}; message - {1}", errorCode));
			}
			if (discount.ErrorCode != 2)
			{
				object[] objArray = new object[] { discount.ErrorCode, discount.Message, discount.Status };
				this.OnInfoMessage("status - {2}; error_code - {0}; message - {1}", objArray);
			}
			else
			{
				this.OnTraceMessage("запрашиваем код подтверждения", new object[0]);
				using (FormConfirmationCode formConfirmationCode = new FormConfirmationCode())
				{
					formConfirmationCode.ShowDialog();
				}
			}
			return discount;
		}

		protected override PublicIdType GetPublicIdType(string clientId)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				return PublicIdType.Unknown;
			}
			this.OnInitSettings();
			string str = AstraZenecaLoyaltyProgram.Settings.CardPrefix.Trim();
			char[] chrArray = new char[] { ',' };
			List<string> list = (
				from x in str.Split(chrArray)
				select x.Trim()).ToList<string>();
			if (AstraZenecaLoyaltyProgram.Settings != null)
			{
				if (list.Any<string>((string x) => {
					if (clientId.Length <= x.Length)
					{
						return PublicIdType.Unknown;
					}
					return (PublicIdType)this.CardPrefixFound(clientId, x);
				}))
				{
					return PublicIdType.CardNumber;
				}
			}
			return PublicIdType.Phone;
		}

		protected override void OnInitInternal()
		{
			if (this.azAPI == null)
			{
				this.azAPI = new AstraZenecaWebApi(AstraZenecaLoyaltyProgram.Settings);
			}
		}

		protected override void OnInitSettings()
		{
			if (AstraZenecaLoyaltyProgram.Settings == null)
			{
				SettingsModel settingsModel = new SettingsModel();
				LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
				AstraZenecaLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.AstraZeneca.Settings>(loyaltySetting.SETTINGS, "Settings");
				this.name = AstraZenecaLoyaltyProgram.Settings.Name;
				AstraZenecaLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
				if (AstraZenecaLoyaltyProgram.IscompatibilityEnabled)
				{
					AstraZenecaLoyaltyProgram.ExcludedPrograms.Add(this.IdGlobal, null);
					foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
					{
						AstraZenecaLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
					}
					foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
					{
						if (dataRowItem.Guid == ARM_DISCOUNT2_PROGRAM.AstraZenecaDiscountGUID)
						{
							continue;
						}
						AstraZenecaLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
					}
					foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
					{
						AstraZenecaLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
					}
				}
			}
		}

		private void SaveAzTransactions(IEnumerable<CHEQUE_ITEM_TRANSACTION> transactions)
		{
			(new AzTransactionsBl()).SaveEx(transactions);
		}
	}
}