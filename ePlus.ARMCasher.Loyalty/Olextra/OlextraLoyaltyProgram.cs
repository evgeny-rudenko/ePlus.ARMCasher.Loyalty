using Dapper;
using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.Loyalty.Olextra;
using ePlus.Loyalty.Olextra.API;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Olextra
{
	internal class OlextraLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private static Guid _id;

		private static string DiscountType;

		private static Dictionary<Guid, DataRowItem> ExcludedPrograms;

		private OlextraWebApi olextraWebApi;

		private string name = "Олекстра";

		private Dictionary<string, List<long>> extBarcodeCache;

		private RequestGetDiscount latestRequest;

		private ResponseGetDiscount latestResponse;

		private List<AllowedBarcode> barcodeCache;

		public override Guid IdGlobal
		{
			get
			{
				return OlextraLoyaltyProgram._id;
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

		private static ePlus.Loyalty.Olextra.Settings Settings
		{
			get;
			set;
		}

		static OlextraLoyaltyProgram()
		{
			OlextraLoyaltyProgram._id = new Guid("B98825C2-926E-4E65-BFE2-421D3265ABE1");
			OlextraLoyaltyProgram.DiscountType = "OL_EX";
			OlextraLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public OlextraLoyaltyProgram(string publicId, string posId) : base(ePlus.Loyalty.LoyaltyType.Olextra, publicId, publicId, "LP_OL")
		{
			base.SendRecvTimeout = 30;
			this.PosId = posId;
		}

		private void CalculateDiscount(CHEQUE cheque)
		{
			this.latestRequest = null;
			if (cheque.CHEQUE_ITEMS != null && cheque.CHEQUE_ITEMS.Count > 0)
			{
				LoyaltyCard loyaltyCard = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is OlextraCard) as LoyaltyCard;
				RequestGetDiscount clientPublicId = this.olextraWebApi.CreateRequest<RequestGetDiscount>(this.PosId);
				clientPublicId.CardNumber = base.ClientPublicId;
				clientPublicId.Orders = this.CreateOrderItems(cheque);
				if (!clientPublicId.Orders.Any<Order>())
				{
					return;
				}
				this.OnTraceMessage("отправляем запрос на получение скидки.", new object[0]);
				ResponseGetDiscount discount = null;
				if (clientPublicId.Orders.Any<Order>())
				{
					this.latestRequest = clientPublicId;
					discount = this.olextraWebApi.GetDiscount(clientPublicId);
					this.latestResponse = discount;
				}
				this.OnTraceMessage("обрабатываем результат.", new object[0]);
				loyaltyCard.ExtraDiscounts.Clear();
				cheque.CHEQUE_ITEMS.ForEach((CHEQUE_ITEM ci) => {
					ci.Transaction = null;
					ci.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.TYPE == OlextraLoyaltyProgram.DiscountType);
				});
				if (discount != null)
				{
					ILoyaltyMessageList loyaltyMessageList = loyaltyCard as ILoyaltyMessageList;
					if (loyaltyMessageList != null && discount.Description != null)
					{
						string str = string.Format("{0}{2}{1}", discount.Description, discount.Message, Environment.NewLine);
						loyaltyMessageList.Add(new LoyaltyMessage(LoyaltyMessageType.Service, str));
					}
					foreach (OrderResponse order in discount.Orders)
					{
						if (order.Discount <= new decimal(0))
						{
							continue;
						}
						CHEQUE_ITEM cHEQUEITEM = cheque.CHEQUE_ITEMS.FirstOrDefault<CHEQUE_ITEM>((CHEQUE_ITEM ci) => {
							if (this.GetBarcode(ci) == null || !this.GetBarcode(ci).Equals(order.Barcode))
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
							ID_DISCOUNT2_GLOBAL = ARM_DISCOUNT2_PROGRAM.OlextraDiscountGUID,
							DISCOUNT2_NAME = order.Message,
							TYPE = OlextraLoyaltyProgram.DiscountType,
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

		private bool CardPrefixFound(string clientId)
		{
			if (OlextraLoyaltyProgram.Settings.CardPrefix.Contains<char>(';'))
			{
				string[] strArrays = OlextraLoyaltyProgram.Settings.CardPrefix.Split(new char[] { ';' });
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					if (clientId.StartsWith(strArrays[i]))
					{
						return true;
					}
				}
			}
			else if (clientId.StartsWith(OlextraLoyaltyProgram.Settings.CardPrefix) && clientId.Length >= OlextraLoyaltyProgram.Settings.CardPrefix.Length)
			{
				return true;
			}
			return false;
		}

		private void ConfirmOlextraTransactions(string clientId, IEnumerable<CHEQUE_ITEM_TRANSACTION> transactions)
		{
			string str;
			string str1;
			PublicIdType publicIdType = this.GetPublicIdType(clientId);
			RequestConfirmPurchase requestConfirmPurchase = this.olextraWebApi.CreateRequest<RequestConfirmPurchase>(this.PosId);
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
					response = this.olextraWebApi.ConfirmPurchase(requestConfirmPurchase);
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
				this.SaveOlextraTransactions(transactions);
			}
		}

		public void ConfirmStoredOlextraTransactions()
		{
			base.InitInternal();
			var dictionary = (
				from t in (new OlextraTransactionsBl()).GetListUnconfirmed()
				group t by new { ID_CHEQUE_GLOBAL = t.ID_CHEQUE_GLOBAL, CLIENT_ID = t.CLIENT_ID }).ToDictionary((g) => g.Key, (g) => g.ToList<CHEQUE_ITEM_TRANSACTION>());
			foreach (var key in dictionary.Keys)
			{
				this.ConfirmOlextraTransactions(key.CLIENT_ID, dictionary[key]);
			}
		}

		public Task ConfirmStroredOlextraTransactionsAsync()
		{
			Task task = Task.Factory.StartNew(new Action(this.ConfirmStoredOlextraTransactions));
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
					QrCode = (cHEQUEITEM.IS_KIZ ? cHEQUEITEM.KIZ : "")
				};
				orders.Add(order);
			}
			return orders;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			this.OnTraceMessage("Подтверждение транзакций Олекстра", new object[0]);
			List<CHEQUE_ITEM_TRANSACTION> list = (
				from ci in cheque.CHEQUE_ITEMS
				where ci.Transaction != null
				select ci.Transaction).ToList<CHEQUE_ITEM_TRANSACTION>();
			if (list.Any<CHEQUE_ITEM_TRANSACTION>())
			{
				this.ConfirmOlextraTransactions(base.ClientPublicId, list);
			}
			this.OnTraceMessage("Подтверждение транзакций Олекстра завершено", new object[0]);
			this.ConfirmStroredOlextraTransactionsAsync();
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
			if (!OlextraLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !OlextraLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		private void DoProcess(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			if (this.latestRequest == null)
			{
				return;
			}
			ResponseGetDiscount discount = this.olextraWebApi.GetDiscount(this.latestRequest);
			List<OrderResponse> list = discount.Orders.ToList<OrderResponse>();
			IEnumerable<CHEQUE_ITEM> cHEQUEITEMS = 
				from ci in cheque.CHEQUE_ITEMS
				where ci.Transaction != null
				select ci;
			foreach (CHEQUE_ITEM cHEQUEITEM in cHEQUEITEMS)
			{
				string barcode = this.GetBarcode(cHEQUEITEM);
				OrderResponse orderResponse = list.FirstOrDefault<OrderResponse>((OrderResponse o) => {
					if (o.Barcode != barcode)
					{
						return false;
					}
					return o.Price == cHEQUEITEM.PRICE;
				});
				list.Remove(orderResponse);
				if (orderResponse != null)
				{
					cHEQUEITEM.Transaction.TRANSACTION_ID = orderResponse.Transaction;
				}
				else
				{
					cHEQUEITEM.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.TYPE == OlextraLoyaltyProgram.DiscountType);
					cHEQUEITEM.Transaction = null;
				}
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

		private void FillBarcodeCache()
		{
			this.barcodeCache = new List<AllowedBarcode>();
			using (SqlConnection sqlConnection = new SqlConnection(MultiServerBL.ClientConnectionString))
			{
				string str = " \r\n                    select \r\n\t                    ID_GOODS_GLOBAL,\r\n                        BARCODE\r\n                            FROM ALLOWED_BARCODE_OLEXTRA ab                            \r\n                        where DATE_DELETED IS NULL";
				int? nullable = null;
				CommandType? nullable1 = null;
				List<AllowedBarcode> list = sqlConnection.Query<AllowedBarcode>(str, null, null, true, nullable, nullable1).ToList<AllowedBarcode>();
				if (list != null)
				{
					this.barcodeCache = list;
				}
			}
		}

		protected string GetBarcode(CHEQUE_ITEM item)
		{
			string bARCODE;
			if (this.barcodeCache == null)
			{
				this.FillBarcodeCache();
			}
			AllowedBarcode allowedBarcode = this.barcodeCache.FirstOrDefault<AllowedBarcode>((AllowedBarcode ab) => ab.ID_GOODS_GLOBAL == item.idGoodsGlobal);
			if (allowedBarcode == null)
			{
				bARCODE = null;
			}
			else
			{
				bARCODE = allowedBarcode.BARCODE;
			}
			return bARCODE;
		}

		private ResponseGetDiscount GetDiscount(RequestGetDiscount request)
		{
			ResponseGetDiscount discount = this.olextraWebApi.GetDiscount(request);
			if (!discount.IsSuccess)
			{
				object[] errorCode = new object[] { discount.ErrorCode, discount.Message, discount.Status };
				throw new LoyaltyException(this, this.FormatMessage("status - {2} error_code - {0}; message - {1}", errorCode));
			}
			object[] objArray = new object[] { discount.ErrorCode, discount.Message, discount.Status };
			this.OnInfoMessage("status - {2}; error_code - {0}; message - {1}", objArray);
			return discount;
		}

		protected override PublicIdType GetPublicIdType(string clientId)
		{
			if (string.IsNullOrWhiteSpace(clientId))
			{
				return PublicIdType.Unknown;
			}
			if (OlextraLoyaltyProgram.Settings != null && !string.IsNullOrWhiteSpace(OlextraLoyaltyProgram.Settings.CardPrefix) && this.CardPrefixFound(clientId))
			{
				return PublicIdType.CardNumber;
			}
			return PublicIdType.Phone;
		}

		protected override void OnInitInternal()
		{
			if (this.olextraWebApi == null)
			{
				this.olextraWebApi = new OlextraWebApi(OlextraLoyaltyProgram.Settings);
			}
		}

		protected override void OnInitSettings()
		{
			if (OlextraLoyaltyProgram.Settings == null)
			{
				SettingsModel settingsModel = new SettingsModel();
				LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
				OlextraLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.Olextra.Settings>(loyaltySetting.SETTINGS, "Settings");
				this.name = OlextraLoyaltyProgram.Settings.Name;
				OlextraLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
				if (OlextraLoyaltyProgram.IscompatibilityEnabled)
				{
					OlextraLoyaltyProgram.ExcludedPrograms.Add(this.IdGlobal, null);
					foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
					{
						OlextraLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
					}
					foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
					{
						if (dataRowItem.Guid == ARM_DISCOUNT2_PROGRAM.OlextraDiscountGUID)
						{
							continue;
						}
						OlextraLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
					}
					foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
					{
						OlextraLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
					}
				}
			}
		}

		private void SaveOlextraTransactions(IEnumerable<CHEQUE_ITEM_TRANSACTION> transactions)
		{
			(new OlextraTransactionsBl()).SaveEx(transactions);
		}
	}
}