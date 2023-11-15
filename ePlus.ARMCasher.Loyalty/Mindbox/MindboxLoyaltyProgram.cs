using ePlus.ARMBusinessLogic;
using ePlus.ARMBusinessLogic.Caches.StocksCaches;
using ePlus.ARMBusinessLogic.Interfaces;
using ePlus.ARMCacheManager;
using ePlus.ARMCasher;
using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.BusinessLogic.Events;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCasher.Loyalty.Forms;
using ePlus.ARMCasher.Loyalty.Properties;
using ePlus.ARMCommon;
using ePlus.ARMCommon.Log;
using ePlus.CommonEx;
using ePlus.CommonEx.Base.Validation;
using ePlus.Discount2.BusinessObjects;
using ePlus.Interfaces;
using ePlus.Loyalty;
using ePlus.Loyalty.Interfaces;
using ePlus.Loyalty.Mindbox;
using ePlus.Loyalty.Mindbox.Enums;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Mindbox
{
	public class MindboxLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private const string IgnoreDiscountId = "IgnoreDiscountId";

		private const string DiscountTypeBalance = "balance";

		private const string DiscountTypeExternal = "externalPromoAction";

		private const int smsRepeateTimeoutMinutes = 0;

		private readonly static Guid m_chequeOperTypeCharge;

		private readonly static Guid m_chequeOperTypeDebit;

		private readonly static Guid m_chequeOperTypeRefundCharge;

		private readonly static Guid m_chequeOperTypeRefundDebit;

		private Settings m_settings;

		protected Params m_params;

		private static LoyaltySettings m_loyaltySettings;

		private static Dictionary<Guid, DataRowItem> ExcludedPrograms;

		private readonly string m_typePrefix;

		private string[] activeCardStatuses = new string[] { "Activated" };

		private string[] blockedCardStatuses = new string[] { "Blocked" };

		private string[] restrictedCardStatuses = new string[] { "Issued", "NotIssued", "Inactive" };

		private Dictionary<CHEQUE, Order> m_registeredOrders = new Dictionary<CHEQUE, Order>();

		private Dictionary<int, decimal> MaxDiscountDictionary = new Dictionary<int, decimal>();

		private bool m_isAuthenticationCodeSent;

		private DateTime? m_authenticationCodeSentDateTime = null;

		protected IMindboxWebApi Api
		{
			get;
			private set;
		}

		private LoyaltyCardInfo? CardInfo
		{
			get;
			set;
		}

		private Guid ChequeOperTypeCharge
		{
			get
			{
				return MindboxLoyaltyProgram.m_chequeOperTypeCharge;
			}
		}

		private Guid ChequeOperTypeDebit
		{
			get
			{
				return MindboxLoyaltyProgram.m_chequeOperTypeDebit;
			}
		}

		private Guid ChequeOperTypeRefundCharge
		{
			get
			{
				return MindboxLoyaltyProgram.m_chequeOperTypeRefundCharge;
			}
		}

		private Guid ChequeOperTypeRefundDebit
		{
			get
			{
				return MindboxLoyaltyProgram.m_chequeOperTypeRefundDebit;
			}
		}

		private Customer FullCustomerInfo
		{
			get;
			set;
		}

		private Task GetCardInfoTask
		{
			get;
			set;
		}

		public Guid IdChequeGlobal
		{
			get;
			set;
		}

		public override Guid IdGlobal
		{
			get
			{
				return MindboxLoyaltyProgram.m_loyaltySettings.ID_LOYALITY_PROGRAM_GLOBAL;
			}
		}

		private static bool IscompatibilityEnabled
		{
			get;
			set;
		}

		public override bool IsPreOrderCalculationRequired
		{
			get
			{
				return true;
			}
		}

		public override bool IsUpdateLoyaltyCardInfoSupported
		{
			get
			{
				return true;
			}
		}

		protected MindboxCard loyaltyCard
		{
			get;
			private set;
		}

		public override string Name
		{
			get
			{
				return MindboxLoyaltyProgram.m_loyaltySettings.NAME;
			}
		}

		protected override bool OnIsExplicitDiscount
		{
			get
			{
				return false;
			}
		}

		private static string PointOfContact
		{
			get;
			set;
		}

		public bool SendPersonalRecomendationRequests
		{
			get
			{
				return this.m_params.SendPersonalRecomendationRequests;
			}
		}

		public bool SendRecomendationRequests
		{
			get
			{
				return this.m_params.SendRecomendationRequests;
			}
		}

		public override int SortOrder
		{
			get
			{
				return 100;
			}
		}

		public bool TakeSurvey
		{
			get
			{
				return this.m_params.TakeSurvey;
			}
		}

		static MindboxLoyaltyProgram()
		{
			MindboxLoyaltyProgram.m_chequeOperTypeCharge = new Guid("96E48109-532E-4FE7-8E8E-DF7C37248C58");
			MindboxLoyaltyProgram.m_chequeOperTypeDebit = new Guid("839AD96C-BA49-4645-AF78-A8F57C927668");
			MindboxLoyaltyProgram.m_chequeOperTypeRefundCharge = new Guid("94E3AE98-B869-4700-8276-A460D0B6AB5A");
			MindboxLoyaltyProgram.m_chequeOperTypeRefundDebit = new Guid("091FA7EC-568D-42E9-8808-23647FA2C9E3");
			MindboxLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public MindboxLoyaltyProgram(string publicId, string pointOfContact, Guid instance) : base(ePlus.Loyalty.LoyaltyType.Mindbox, publicId, publicId, "MINDBOX_BALANCE")
		{
			base.SendRecvTimeout = 30;
			MindboxLoyaltyProgram.PointOfContact = pointOfContact;
			this.m_typePrefix = "MINDBOX";
			if (this.m_params != null)
			{
				this.m_params.PointOfContact = pointOfContact;
			}
		}

		public MindboxLoyaltyProgram(string publicid, string pointOfContact, MindboxCard lcard, Guid instance) : this(publicid, pointOfContact, instance)
		{
			this.loyaltyCard = lcard;
		}

		private void AddMessages(Order result, LoyaltyCard lpCard)
		{
			ILoyaltyMessageList loyaltyMessageList = lpCard as ILoyaltyMessageList;
			if (loyaltyMessageList == null)
			{
				return;
			}
			if (result.PlaceHolders == null || !result.PlaceHolders.Any<PlaceHolder>() || !this.m_params.LoyaltyInformerOptionList.Any<LoyaltyInformerOption>())
			{
				return;
			}
			foreach (PlaceHolder placeHolder in result.PlaceHolders)
			{
				foreach (LoyaltyInformerOption loyaltyInformerOption in 
					from o in this.m_params.LoyaltyInformerOptionList
					where o.Name.Equals(placeHolder.Id, StringComparison.InvariantCultureIgnoreCase)
					select o)
				{
					foreach (ContentItem contentItemList in placeHolder.ContentItemList)
					{
						loyaltyMessageList.Add(new LoyaltyMessage(loyaltyInformerOption.MessageType, contentItemList.Value.Replace(Environment.NewLine, "\n").Replace("\n", Environment.NewLine)));
					}
				}
			}
		}

		private void AddMindboxExtraDiscounts(Order result, CHEQUE cheque, LoyaltyCard lpCard)
		{
			List<CHEQUE_ITEM> cHEQUEITEMs = new List<CHEQUE_ITEM>();
			Line[] lines = result.Lines;
			for (int num = 0; num < (int)lines.Length; num++)
			{
				Line line = lines[num];
				if (line.AppliedDiscounts != null)
				{
					CHEQUE_ITEM cHEQUEITEM = cheque.CHEQUE_ITEMS.Except<CHEQUE_ITEM>(cHEQUEITEMs).FirstOrDefault<CHEQUE_ITEM>((CHEQUE_ITEM i) => {
						Guid dLOTGLOBAL = i.ID_LOT_GLOBAL;
						Guid? idLotGlobal = line.IdLotGlobal;
						if (!idLotGlobal.HasValue)
						{
							return false;
						}
						return dLOTGLOBAL == idLotGlobal.GetValueOrDefault();
					});
					cHEQUEITEMs.Add(cHEQUEITEM);
					Dictionary<string, DISCOUNT_VALUE_INFO> strs = new Dictionary<string, DISCOUNT_VALUE_INFO>();
					AppliedDiscount[] appliedDiscounts = line.AppliedDiscounts;
					for (int j = 0; j < (int)appliedDiscounts.Length; j++)
					{
						AppliedDiscount appliedDiscount = appliedDiscounts[j];
						if (appliedDiscount.Id == null || !appliedDiscount.Id.Equals("IgnoreDiscountId"))
						{
							string upper = string.Format("{1}_{0}", appliedDiscount.GetCurrencyType(), this.m_typePrefix).ToUpper();
							if (!strs.ContainsKey(upper))
							{
								DISCOUNT_VALUE_INFO dISCOUNTVALUEINFO = new DISCOUNT_VALUE_INFO()
								{
									BARCODE = lpCard.BARCODE,
									ID_LOT_GLOBAL = cHEQUEITEM.ID_LOT_GLOBAL,
									ID_DISCOUNT2_GLOBAL = ARM_DISCOUNT2_PROGRAM.MindboxDiscountGUID
								};
								string type = appliedDiscount.Type;
								object[] name = new object[] { this.Name };
								dISCOUNTVALUEINFO.DISCOUNT2_NAME = this.GetResourceString(type, "MindboxDiscountName", name);
								dISCOUNTVALUEINFO.TYPE = upper;
								dISCOUNTVALUEINFO.VALUE = appliedDiscount.Amount;
								strs.Add(upper, dISCOUNTVALUEINFO);
							}
							else
							{
								DISCOUNT_VALUE_INFO item = strs[upper];
								item.VALUE = item.VALUE + appliedDiscount.Amount;
							}
							if (appliedDiscount.Type.Equals("balance", StringComparison.InvariantCultureIgnoreCase))
							{
								PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
								{
									CLIENT_ID = base.ClientPublicId,
									CLIENT_ID_TYPE = (int)base.LoyaltyType,
									ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL,
									OPER_TYPE = "DEBIT",
									SUMM = appliedDiscount.Amount,
									BALANCE_NAME = appliedDiscount.BalanceType.Ids.SystemName,
									ID_PROMOACTION = appliedDiscount.PromoAction.Ids.ExternalId
								};
								lpCard.BalanceChangeList.Add(pCXCHEQUEITEM);
							}
						}
					}
					lpCard.ExtraDiscounts.AddRange(strs.Values);
					if (line.AcquiredBalanceChanges != null)
					{
						AcquiredBalanceChange[] acquiredBalanceChanges = line.AcquiredBalanceChanges;
						for (int k = 0; k < (int)acquiredBalanceChanges.Length; k++)
						{
							AcquiredBalanceChange acquiredBalanceChange = acquiredBalanceChanges[k];
							if (acquiredBalanceChange.Type.Equals("balance", StringComparison.InvariantCultureIgnoreCase))
							{
								PCX_CHEQUE_ITEM pCXCHEQUEITEM1 = new PCX_CHEQUE_ITEM()
								{
									CLIENT_ID = base.ClientPublicId,
									CLIENT_ID_TYPE = (int)base.LoyaltyType,
									ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL,
									OPER_TYPE = "CHARGE",
									SUMM = acquiredBalanceChange.Amount,
									BALANCE_NAME = acquiredBalanceChange.BalanceType.Ids.SystemName,
									ID_PROMOACTION = acquiredBalanceChange.PromoAction.Ids.ExternalId
								};
								lpCard.BalanceChangeList.Add(pCXCHEQUEITEM1);
							}
						}
					}
				}
			}
		}

		private void AddRecomendation(CHEQUE cheque, RecommendationType type, string code, params string[] codes)
		{
			List<STOCK_DETAIL> stockDetails;
			if (string.IsNullOrEmpty(code))
			{
				stockDetails = null;
			}
			else
			{
				stockDetails = this.GetStockDetails(code);
			}
			List<STOCK_DETAIL> sTOCKDETAILs = stockDetails;
			string[] strArrays = codes;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				List<STOCK_DETAIL> list = this.GetStockDetails(str);
				string str1 = "Персональная";
				if (sTOCKDETAILs != null && sTOCKDETAILs.Any<STOCK_DETAIL>())
				{
					str1 = string.Concat("К товару \"", sTOCKDETAILs.First<STOCK_DETAIL>().GOODS_NAME, '\"');
				}
				list = list.Where<STOCK_DETAIL>((STOCK_DETAIL x) => {
					if (x.ACCESSIBLE <= new decimal(0))
					{
						return false;
					}
					return x.LOT_PRICE_VAT > AppConfigurator.MinimumRecomendationPrice;
				}).ToList<STOCK_DETAIL>();
				bool flag = list.Any<STOCK_DETAIL>();
				if (!list.Any<STOCK_DETAIL>())
				{
					list.Add(new STOCK_DETAIL()
					{
						CODE = str
					});
				}
				MindboxRecommendation mindboxRecommendation = new MindboxRecommendation(type, str1, code, list, flag);
				RecomendationForSorting recomendationForSorting = new RecomendationForSorting()
				{
					Id = Guid.NewGuid(),
					Recommendation = mindboxRecommendation
				};
				RecomendationForSorting dGOODS = recomendationForSorting;
				if (list.Any<STOCK_DETAIL>())
				{
					dGOODS.BEST_BEFORE = list.Min<STOCK_DETAIL, DateTime>((STOCK_DETAIL x) => x.BEST_BEFORE);
					dGOODS.BEST_BEFORE_SORT = (dGOODS.BEST_BEFORE == DateTime.MinValue ? DateTime.MaxValue : dGOODS.BEST_BEFORE);
					dGOODS.ID_GOODS = list.First<STOCK_DETAIL>().ID_GOODS;
					dGOODS.LOT_PRICE_VAT = mindboxRecommendation.Price;
					dGOODS.MARGIN_DISPLAY = list.First<STOCK_DETAIL>().MARGIN_DISPLAY;
					dGOODS.NO_LIQUID = list.Any<STOCK_DETAIL>((STOCK_DETAIL x) => x.NO_LIQUID);
					dGOODS.ONLY_VIEW = list.Any<STOCK_DETAIL>((STOCK_DETAIL x) => x.ONLY_VIEW);
					dGOODS.RATING = list.Max<STOCK_DETAIL>((STOCK_DETAIL x) => x.RATING);
					dGOODS.RISK_ISG = list.Any<STOCK_DETAIL>((STOCK_DETAIL x) => x.RISK_ISG);
				}
				cheque.AddRecomendation(dGOODS);
			}
		}

		private CardRegistrationResult AppendCard(Customer customer, string cardNumber)
		{
			return this.Api.CardRegistration(customer, cardNumber, MindboxLoyaltyProgram.PointOfContact);
		}

		protected virtual void AppendOrderCustomFields(Order order, CHEQUE cheque)
		{
			if (AppConfigurator.LicensedToRigla)
			{
				OrderCustomFields customFields = order.CustomFields ?? new OrderCustomFields();
				customFields.IsOnlineOrder = cheque.IsInternetOrder;
				customFields.InternetOrderSource = cheque.INTERNET_ORDER_SOURCE_NAME;
				order.CustomFields = customFields;
				return;
			}
			ARMLogger.Trace("Mindbox: Проверка интернет заказа...");
			if (!cheque.CHEQUE_ITEMS.Any<CHEQUE_ITEM>((CHEQUE_ITEM ci) => {
				if (ci.BOX == null)
				{
					return false;
				}
				return ci.BOX.Equals("internet", StringComparison.InvariantCultureIgnoreCase);
			}))
			{
				ARMLogger.Trace("Mindbox: В заказе не найдены позиции интернет заказа");
				return;
			}
			ARMLogger.Trace("Mindbox: Заказ помечен как интернет заказ");
			order.CustomFields = new OrderCustomFields()
			{
				IsInternetOrder = true
			};
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			decimal num = new decimal(0);
			int hashCode = cheque.GetHashCode();
			if (this.MaxDiscountDictionary.ContainsKey(hashCode))
			{
				return this.MaxDiscountDictionary[hashCode];
			}
			base.PreOrderCalculation(cheque);
			hashCode = cheque.GetHashCode();
			return this.MaxDiscountDictionary[hashCode];
		}

		private void CancelOrder(CHEQUE cheque)
		{
			if (!this.m_registeredOrders.ContainsKey(cheque))
			{
				return;
			}
			long? mindboxOrderId = this.m_registeredOrders[cheque].Ids.MindboxOrderId;
			if (!mindboxOrderId.HasValue)
			{
				throw new LoyaltyException(this, "Отсутствует MindboxId для корректной отмены закака");
			}
			Order order = this.CreateCancelAllOrder(cheque, mindboxOrderId.Value);
			this.Api.CancelledAll(order);
			this.m_registeredOrders.Remove(cheque);
		}

		private CardRegistrationResult CardRegistration(Customer customer)
		{
			CardRegistrationResult cardRegistrationResult;
			string empty = string.Empty;
			using (FrmScanBarcode frmScanBarcode = new FrmScanBarcode())
			{
				frmScanBarcode.StartPosition = FormStartPosition.CenterScreen;
				frmScanBarcode.Title = "ШК карты:";
				frmScanBarcode.Text = "Замена/восстановление карты";
				frmScanBarcode.ShowDialog(this.waitingForm);
				if (frmScanBarcode.DialogResult != DialogResult.OK)
				{
					cardRegistrationResult = null;
				}
				else
				{
					empty = frmScanBarcode.Barcode;
					return this.AppendCard(customer, empty);
				}
			}
			return cardRegistrationResult;
		}

		private void ClearSelfDiscounts(CHEQUE cheque)
		{
			cheque.CHEQUE_ITEMS.ForEach((CHEQUE_ITEM ch) => ch.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.ID_DISCOUNT2_PROGRAM_GLOBAL == ARM_DISCOUNT2_PROGRAM.MindboxDiscountGUID));
			LoyaltyCard loyaltyCard = this.GetLoyaltyCard(cheque);
			loyaltyCard.ExtraDiscounts.RemoveAll((DISCOUNT_VALUE_INFO e) => e.TYPE.StartsWith(this.m_typePrefix));
			cheque.CalculateFields();
		}

		private LoyaltyCardStatus ConvertStatus(string statusName)
		{
			LoyaltyCardStatus loyaltyCardStatu;
			if (!this.activeCardStatuses.Contains<string>(statusName))
			{
				loyaltyCardStatu = (!this.blockedCardStatuses.Contains<string>(statusName) ? LoyaltyCardStatus.Limited : LoyaltyCardStatus.Blocked);
			}
			else
			{
				loyaltyCardStatu = LoyaltyCardStatus.Active;
			}
			return loyaltyCardStatu;
		}

		public static MindboxLoyaltyProgram Create(object parameters)
		{
			Guid guid = (Guid)MindboxLoyaltyProgram.ExtractParameter(parameters, "instance");
			string str = (string)MindboxLoyaltyProgram.ExtractParameter(parameters, "publicId") ?? string.Empty;
			if (str == "$EmptyForPromocode$")
			{
				str = "";
			}
			CHEQUE cHEQUE = MindboxLoyaltyProgram.ExtractParameter(parameters, "cheque") as CHEQUE;
			Guid? nullable = null;
			if (cHEQUE != null)
			{
				nullable = new Guid?(cHEQUE.ID_CHEQUE_GLOBAL);
			}
			string str1 = (string)MindboxLoyaltyProgram.ExtractParameter(parameters, "pointOfContact");
			MindboxLoyaltyProgram mindboxLoyaltyProgram = new MindboxLoyaltyProgram(str, str1, new MindboxCard(), guid);
			mindboxLoyaltyProgram.InitLoyaltySettings(guid);
			if (nullable.HasValue)
			{
				mindboxLoyaltyProgram.IdChequeGlobal = nullable.Value;
			}
			mindboxLoyaltyProgram.LoyaltyInstance = guid;
			return mindboxLoyaltyProgram;
		}

		private Order CreateCancelAllOrder(CHEQUE cheque, long mindboxOrderId)
		{
			Order nullable = this.CreateMindboxOrder(cheque, new decimal(0));
			nullable.UpdatedDateTimeUtc = new DateTime?(DateTime.UtcNow);
			Ids id = new Ids()
			{
				MindboxOrderId = new long?(mindboxOrderId)
			};
			nullable.Ids = id;
			nullable.TotalPrice = new decimal?(new decimal(0));
			return nullable;
		}

		private Order CreateCheckoutMindboxOrder(CHEQUE cheque, decimal discountSum)
		{
			Order nullable = this.CreateMindboxOrder(cheque, discountSum);
			nullable.PreOrderDiscountedTotalPrice = new decimal?(cheque.SUMM);
			nullable.CreatedDateTimeUtc = new DateTime?(DateTime.UtcNow);
			return nullable;
		}

		private Order CreateMindboxOrder(CHEQUE cheque, [DecimalConstant(1, 0, 0, 0, 0)] decimal discountSum = default(decimal))
		{
			Order customer = this.CreateSimpleMindboxOrder(cheque);
			customer.Customer = this.GetCustomer();
			if (discountSum > new decimal(0))
			{
				ePlus.Loyalty.Mindbox.Currency currency = new ePlus.Loyalty.Mindbox.Currency()
				{
					Type = "balance",
					Amount = discountSum
				};
				customer.AddDiscount(currency);
			}
			if (this.loyaltyCard != null && this.loyaltyCard != null)
			{
				foreach (IPromocode promocode in ((ILoyaltyPromocodeList)this.loyaltyCard).Promocodes)
				{
					ePlus.Loyalty.Mindbox.Currency currency1 = new ePlus.Loyalty.Mindbox.Currency()
					{
						Type = "promoCode",
						Id = promocode.Id
					};
					customer.AddDiscount(currency1);
				}
			}
			return customer;
		}

		protected virtual Order CreatePaidMindboxOrder(CHEQUE cheque, decimal discountSum, long? mindboxOrderId)
		{
			Order nullable = this.CreateMindboxOrder(cheque, discountSum);
			nullable.UpdatedDateTimeUtc = new DateTime?(DateTime.UtcNow);
			Ids id = new Ids()
			{
				InternalOrderId = cheque.ID_CHEQUE_GLOBAL.ToString(),
				MindboxOrderId = mindboxOrderId
			};
			nullable.Ids = id;
			nullable.Payments = (
				from c in cheque.CHEQUE_PAYMENTS
				select new ePlus.Loyalty.Mindbox.Currency()
				{
					Type = c.TYPE_PAYMENT_ENUM.ToString().ToLower(),
					Amount = c.SUMM
				}).ToArray<ePlus.Loyalty.Mindbox.Currency>();
			nullable.TotalPrice = new decimal?(cheque.SUMM);
			if (AppConfigurator.LicensedToRigla)
			{
				nullable.Ids.InternetOrderId = cheque.InternetOrderNumber;
			}
			return nullable;
		}

		private Order CreateRecalculationMindboxOrder(CHEQUE cheque)
		{
			Order order = this.CreateSimpleMindboxOrder(cheque);
			Ids id = new Ids()
			{
				InternalOrderId = cheque.ID_CHEQUE_GLOBAL.ToString()
			};
			order.Ids = id;
			return order;
		}

		private Order CreateRefundAllMindboxOrder(CHEQUE cheque)
		{
			Order nullable = this.CreateSimpleMindboxOrder(cheque);
			nullable.UpdatedDateTimeUtc = new DateTime?(DateTime.UtcNow);
			Ids id = new Ids()
			{
				InternalOrderId = cheque.ID_CHEQUE_GLOBAL.ToString()
			};
			nullable.Ids = id;
			nullable.TotalPrice = new decimal?(new decimal(0));
			return nullable;
		}

		private Operation CreateRefundMindboxOrder(CHEQUE cheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundedCheques)
		{
			Order id = this.CreateSimpleMindboxOrderV3(returnCheque);
			Line[] lines = id.Lines;
			for (int i = 0; i < (int)lines.Length; i++)
			{
				lines[i].StatusString = "efReturn";
			}
			id.Ids = new Ids();
			if (this.m_params.ExchangeType != ExchangeTypes.ProApteka)
			{
				id.Ids.InternalOrderIdV3 = cheque.ID_CHEQUE_GLOBAL.ToString();
			}
			else
			{
				id.Ids.ExternalOrderCashdeskProapteka = cheque.ID_CHEQUE_GLOBAL.ToString();
			}
			Operation operation = new Operation()
			{
				Order = id,
				ExecutionDateTimeUtc = DateTime.UtcNow.ToString()
			};
			return operation;
		}

		private Order CreateSimpleMindboxOrder(CHEQUE cheque)
		{
			Order order = new Order()
			{
				PointOfContact = MindboxLoyaltyProgram.PointOfContact
			};
			List<Line> lines = new List<Line>();
			decimal num = new decimal(0);
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				decimal sUMM = cHEQUEITEM.SUMM + cHEQUEITEM.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM d) => {
					if (d.ID_DISCOUNT2_PROGRAM_GLOBAL != ARM_DISCOUNT2_PROGRAM.MindboxDiscountGUID)
					{
						return new decimal(0);
					}
					return d.AMOUNT;
				});
				decimal num1 = Math.Ceiling((new decimal(100) * sUMM) / cHEQUEITEM.QUANTITY) / new decimal(100);
				num = num + ((num1 * cHEQUEITEM.QUANTITY) - sUMM);
				List<Line> lines1 = lines;
				Line line = new Line()
				{
					Quantity = cHEQUEITEM.QUANTITY,
					IdLotGlobal = new Guid?(cHEQUEITEM.ID_LOT_GLOBAL)
				};
				Line line1 = line;
				Sku sku = new Sku()
				{
					ProductId = (string.IsNullOrEmpty(cHEQUEITEM.CODE) ? cHEQUEITEM.ID_LOT_GLOBAL.ToString() : cHEQUEITEM.CODE),
					BasePricePerItem = num1
				};
				line1.Sku = sku;
				lines1.Add(line);
			}
			order.Lines = lines.ToArray();
			this.AppendOrderCustomFields(order, cheque);
			if (num > new decimal(0))
			{
				ePlus.Loyalty.Mindbox.Currency currency = new ePlus.Loyalty.Mindbox.Currency()
				{
					Amount = num,
					Id = "IgnoreDiscountId",
					Type = "externalPromoAction"
				};
				order.AddDiscount(currency);
			}
			return order;
		}

		private Order CreateSimpleMindboxOrderV3(CHEQUE cheque)
		{
			Order order = new Order();
			List<Line> lines = new List<Line>();
			decimal num = new decimal(0);
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				decimal sUMM = cHEQUEITEM.SUMM;
				decimal num1 = Math.Ceiling((new decimal(100) * sUMM) / cHEQUEITEM.QUANTITY) / new decimal(100);
				List<Line> lines1 = lines;
				Line line = new Line()
				{
					Quantity = cHEQUEITEM.QUANTITY,
					IdLotGlobal = new Guid?(cHEQUEITEM.ID_LOT_GLOBAL)
				};
				Line line1 = line;
				ProductV3 productV3 = new ProductV3();
				ProductV3 productV31 = productV3;
				Ids id = new Ids()
				{
					InternalOrderId = (string.IsNullOrEmpty(cHEQUEITEM.CODE) ? cHEQUEITEM.ID_LOT_GLOBAL.ToString() : cHEQUEITEM.CODE)
				};
				productV31.ids = id;
				line1.Product = productV3;
				line.DiscountedPriceV3 = new decimal?(sUMM);
				lines1.Add(line);
			}
			order.Lines = lines.ToArray();
			this.AppendOrderCustomFields(order, cheque);
			if (num > new decimal(0))
			{
				ePlus.Loyalty.Mindbox.Currency currency = new ePlus.Loyalty.Mindbox.Currency()
				{
					Amount = num,
					Id = "IgnoreDiscountId",
					Type = "externalPromoAction"
				};
				order.AddDiscount(currency);
			}
			return order;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			this.DoProcess(cheque, discountSum, out result);
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			try
			{
				this.RegisterOrder(cheque);
				if (cheque.LoyaltyPrograms.Any<KeyValuePair<ePlus.Loyalty.LoyaltyType, ILoyaltyProgram>>((KeyValuePair<ePlus.Loyalty.LoyaltyType, ILoyaltyProgram> x) => x.Key == ePlus.Loyalty.LoyaltyType.Mindbox))
				{
					KeyValuePair<ePlus.Loyalty.LoyaltyType, ILoyaltyProgram> keyValuePair = cheque.LoyaltyPrograms.First<KeyValuePair<ePlus.Loyalty.LoyaltyType, ILoyaltyProgram>>((KeyValuePair<ePlus.Loyalty.LoyaltyType, ILoyaltyProgram> x) => x.Key == ePlus.Loyalty.LoyaltyType.Mindbox);
					LoyaltyCardInfo loyaltyCardInfo = keyValuePair.Value.GetLoyaltyCardInfo(false);
					if (loyaltyCardInfo.CardStatusId == LoyaltyCardStatus.NotIssued && !string.IsNullOrEmpty(loyaltyCardInfo.CardNumber))
					{
						this.Api.DeployCard(loyaltyCardInfo.CardNumber);
					}
				}
			}
			catch (Exception exception)
			{
				throw new LoyaltyException(this, "Произошла ошибка при регистрации заказа в Mindbox", exception);
			}
		}

		protected override LoyaltyCardInfo? DoGetLoyaltyCardInfoByCheque(CHEQUE cheque)
		{
			LoyaltyCardInfo loyaltyCardInfo;
			ARMLogger.Info("Получение информации по чеку в сервисе Mindbox");
			Order order = this.CreateRecalculationMindboxOrder(cheque);
			Order orderRecalculation = null;
			try
			{
				orderRecalculation = this.Api.GetOrderRecalculation(order);
			}
			catch (MindboxApiExcepion mindboxApiExcepion)
			{
				ARMLogger.Warn(mindboxApiExcepion.Message);
			}
			catch (Exception exception)
			{
				ARMLogger.Error(exception.ToString());
			}
			if (orderRecalculation == null)
			{
				return null;
			}
			if (!orderRecalculation.Customer.MobilePhone.Any<char>())
			{
				DiscountCard discountCard = orderRecalculation.Customer.DiscountCards.FirstOrDefault<DiscountCard>();
				if (discountCard == null)
				{
					throw new LoyaltyException(this, "У клиента не ни одной карты");
				}
				LoyaltyCardInfo loyaltyCardInfo1 = new LoyaltyCardInfo()
				{
					ClientId = discountCard.Ids.Number,
					ClientIdType = PublicIdType.CardNumber
				};
				loyaltyCardInfo = loyaltyCardInfo1;
			}
			else
			{
				string mobilePhone = orderRecalculation.Customer.MobilePhone;
				LoyaltyCardInfo loyaltyCardInfo2 = new LoyaltyCardInfo()
				{
					ClientId = mobilePhone,
					ClientIdType = PublicIdType.Phone
				};
				loyaltyCardInfo = loyaltyCardInfo2;
			}
			return new LoyaltyCardInfo?(loyaltyCardInfo);
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			if (string.IsNullOrEmpty(base.ClientId))
			{
				return new LoyaltyCardInfo();
			}
			LoyaltyCardInfo? nullable = null;
			try
			{
				nullable = new LoyaltyCardInfo?(this.GetLoyaltyCardInfo(base.ClientPublicId, base.ClientPublicIdType));
			}
			catch (CustomerNotFoundException customerNotFoundException1)
			{
				CustomerNotFoundException customerNotFoundException = customerNotFoundException1;
				if (this.m_params.CustomOptions.EnableRegistration && base.ClientPublicIdType == PublicIdType.Phone)
				{
					string str = string.Format("Покупатель с номером телефона {0} не найден в системе!{1}Зарегистрировать нового покупателя?", base.ClientPublicId, Environment.NewLine);
					if (DialogResult.Yes == MessageBox.Show(this.waitingForm, str, "Регистрация покупателя", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						nullable = new LoyaltyCardInfo?(this.RegisterCustomer(base.ClientId));
					}
				}
				if (base.ClientPublicIdType == PublicIdType.CardNumber)
				{
					GetCardInfoResult loyaltyCardInfoByBarcode = this.Api.GetLoyaltyCardInfoByBarcode(base.ClientId);
					if (loyaltyCardInfoByBarcode.Card.Status == null)
					{
						throw new LoyaltyException(this, "Карта с таким номером не найдена в системе");
					}
					if (loyaltyCardInfoByBarcode.Card != null && loyaltyCardInfoByBarcode.Card.Status.Ids.SystemName == "NotIssued")
					{
						LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
						{
							CardNumber = base.ClientId,
							CardStatusId = LoyaltyCardStatus.NotIssued,
							CardStatus = "Карта не выдана",
							ClientIdType = PublicIdType.CardNumber,
							ClientId = base.ClientId
						};
						nullable = new LoyaltyCardInfo?(loyaltyCardInfo);
					}
				}
				if (!nullable.HasValue)
				{
					throw new LoyaltyException(this, this.GetResourceString(customerNotFoundException.ProcessingStatus, "CustomerProcessingStatus", new object[0]), customerNotFoundException);
				}
			}
			return nullable.Value;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (discountId == this.IdGlobal)
			{
				return false;
			}
			if (!MindboxLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !MindboxLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		protected override void DoPreOrderCalculation(CHEQUE cheque)
		{
			Customer customer;
			Func<CHEQUE_ITEM, string> cODE = null;
			LoyaltyCard loyaltyCard = this.GetLoyaltyCard(cheque);
			decimal num = (loyaltyCard == null ? new decimal(0, 0, 0, false, 1) : loyaltyCard.DiscountSum);
			decimal availableAmountForCurrentOrder = new decimal(0);
			Order order = null;
			Order preOrderInfo = null;
			this.ClearSelfDiscounts(cheque);
			loyaltyCard.BalanceChangeList.Clear();
			int hashCode = cheque.GetHashCode();
			if (!this.MaxDiscountDictionary.ContainsKey(hashCode))
			{
				order = this.CreateMindboxOrder(cheque, new decimal(1000000000));
				preOrderInfo = this.Api.GetPreOrderInfo(order);
				if (order.Customer.MobilePhone != null || order.Customer.DiscountCard != null)
				{
					try
					{
						customer = this.Api.GetCustomer(order.Customer).Customer;
					}
					catch (CustomerNotFoundException customerNotFoundException)
					{
						customer = null;
					}
					if (customer == null)
					{
						cheque.IS_ONLINE = new bool?(false);
					}
					else
					{
						if (customer.CustomFields != null)
						{
							cheque.OFD_PERMISSION = customer.CustomFields.OfdPermission;
						}
						cheque.IS_ONLINE = new bool?((customer.CustomFields == null || !cheque.OFD_PERMISSION.HasValue ? false : cheque.OFD_PERMISSION.Value));
						if (AppConfigurator.IsRiglaLic && cheque.IS_ONLINE.HasValue && cheque.IS_ONLINE.Value)
						{
							cheque.SetDigitalChequeInfo(customer.Email, customer.Email);
						}
					}
				}
				else
				{
					cheque.IS_ONLINE = new bool?(false);
				}
				DiscountInfo discountInfo = ((IEnumerable<DiscountInfo>)preOrderInfo.DiscountsInfo).FirstOrDefault<DiscountInfo>((DiscountInfo i) => i.Type.Equals("balance"));
				if (discountInfo != null)
				{
					availableAmountForCurrentOrder = discountInfo.AvailableAmountForCurrentOrder;
				}
			}
			else
			{
				availableAmountForCurrentOrder = this.MaxDiscountDictionary[hashCode];
			}
			if (!this.MaxDiscountDictionary.ContainsKey(hashCode))
			{
				this.MaxDiscountDictionary.Add(hashCode, availableAmountForCurrentOrder);
			}
			num = Math.Min(num, availableAmountForCurrentOrder);
			order = this.CreateMindboxOrder(cheque, num);
			ARMLogger.Trace("Minbox: Предварительный расчет скидок на сервере");
			preOrderInfo = this.Api.GetPreOrderInfo(order);
			loyaltyCard.AcquiredBalanceChange = preOrderInfo.TotalAcquiredBalanceChange;
			if (loyaltyCard != null)
			{
				this.AddMindboxExtraDiscounts(preOrderInfo, cheque, loyaltyCard);
				this.AddMessages(preOrderInfo, loyaltyCard);
			}
			Task.Factory.StartNew(() => {
				MindboxLoyaltyProgram u003cu003e4_this = this;
				ValidatableList<CHEQUE_ITEM> cHEQUEITEMS = cheque.CHEQUE_ITEMS;
				if (cODE == null)
				{
					cODE = (CHEQUE_ITEM ci) => ci.CODE;
				}
				u003cu003e4_this.GetRecommendations(cHEQUEITEMS.Select<CHEQUE_ITEM, string>(cODE));
			});
			this.loyaltyCard = (MindboxCard)loyaltyCard;
		}

		private void DoProcess(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			OperTypeEnum operTypeEnum;
			Guid chequeOperTypeCharge;
			result = null;
			if (discountSum <= new decimal(0))
			{
				operTypeEnum = OperTypeEnum.Charge;
				chequeOperTypeCharge = this.ChequeOperTypeCharge;
			}
			else
			{
				operTypeEnum = OperTypeEnum.Debit;
				chequeOperTypeCharge = this.ChequeOperTypeDebit;
			}
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, chequeOperTypeCharge);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Списание баллов с карты лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторное списание произведено не будет.", this.Name, cheque.ID_CHEQUE_GLOBAL));
				return;
			}
			base.Log(operTypeEnum, discountSum, cheque, null, null);
			Order mindboxOrderId = this.GetMindboxOrderId(cheque);
			Order order = this.CreatePaidMindboxOrder(cheque, discountSum, mindboxOrderId.Ids.MindboxOrderId);
			if (this.Api.PaidAll(order) == null)
			{
				throw new LoyaltyException(this, "Не удалось завершить заказ в Mindbox");
			}
			int num = 0;
			LpTransactionData lpTransactionDatum = new LpTransactionData(cheque.ID_CHEQUE_GLOBAL, chequeOperTypeCharge);
			base.SaveTransaction(operTypeEnum, num, lpTransactionDatum);
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			if (string.IsNullOrEmpty(base.ClientId))
			{
				return;
			}
			decimal? totalAcquiredBalanceChange = mindboxOrderId.TotalAcquiredBalanceChange;
			decimal num1 = (totalAcquiredBalanceChange.HasValue ? totalAcquiredBalanceChange.GetValueOrDefault() : new decimal(0));
			decimal num2 = ((IEnumerable<Line>)mindboxOrderId.Lines).Sum<Line>((Line l) => {
				if (l.AppliedDiscounts == null)
				{
					return new decimal(0);
				}
				return (
					from d in (IEnumerable<AppliedDiscount>)l.AppliedDiscounts
					where d.Type.Equals("balance")
					select d).Sum<AppliedDiscount>((AppliedDiscount ad) => ad.Amount);
			});
			Guid dCHEQUEGLOBAL = cheque.ID_CHEQUE_GLOBAL;
			string clientPublicId = base.ClientPublicId;
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(true);
			LpTransResult lpTransResult = new LpTransResult(dCHEQUEGLOBAL, clientPublicId, num1, num2, loyaltyCardInfo.Balance, this.Name, false)
			{
				LpType = base.LoyaltyType
			};
			LpTransResult str = lpTransResult;
			long? nullable = mindboxOrderId.Ids.MindboxOrderId;
			str.TransactionId = ((nullable.HasValue ? nullable.GetValueOrDefault() : (long)0)).ToString();
			LpTransResult lpTransResult1 = lpTransResult;
			Line[] lines = mindboxOrderId.Lines;
			for (int i1 = 0; i1 < (int)lines.Length; i1++)
			{
				Line line = lines[i1];
				CHEQUE_ITEM cHEQUEITEM = cheque.CHEQUE_ITEMS.FirstOrDefault<CHEQUE_ITEM>((CHEQUE_ITEM i) => {
					Guid dLOTGLOBAL = i.ID_LOT_GLOBAL;
					Guid? idLotGlobal = line.IdLotGlobal;
					return dLOTGLOBAL == (idLotGlobal.HasValue ? idLotGlobal.GetValueOrDefault() : Guid.Empty);
				});
				if (cHEQUEITEM != null)
				{
					if (line.AcquiredBalanceChanges != null)
					{
						AcquiredBalanceChange[] acquiredBalanceChanges = line.AcquiredBalanceChanges;
						for (int j = 0; j < (int)acquiredBalanceChanges.Length; j++)
						{
							AcquiredBalanceChange acquiredBalanceChange = acquiredBalanceChanges[j];
							lpTransResult1.AddDetail(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, acquiredBalanceChange.Amount, OperTypeEnum.Charge, acquiredBalanceChange.BalanceType.Ids.SystemName, acquiredBalanceChange.PromoAction.Ids.ExternalId);
						}
					}
					if (line.AppliedDiscounts != null)
					{
						AppliedDiscount[] appliedDiscounts = line.AppliedDiscounts;
						for (int k = 0; k < (int)appliedDiscounts.Length; k++)
						{
							AppliedDiscount appliedDiscount = appliedDiscounts[k];
							if (appliedDiscount.Type.Equals("balance", StringComparison.InvariantCultureIgnoreCase))
							{
								lpTransResult1.AddDetail(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, appliedDiscount.Amount, OperTypeEnum.Debit, appliedDiscount.BalanceType.Ids.SystemName, appliedDiscount.PromoAction.Ids.ExternalId);
							}
						}
					}
				}
			}
			result = lpTransResult1;
			base.Log(result.ToSlipCheque(null, null));
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
			throw new NotImplementedException();
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundedCheques, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
			throw new NotImplementedException();
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, IEnumerable<CHEQUE> refundedCheques, out ILpTransResult result)
		{
			baseCheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => ci.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE != base.DiscountType)
				{
					return new decimal(0);
				}
				return mi.AMOUNT;
			}));
			string empty = string.Empty;
			result = null;
			OperTypeEnum operTypeEnum = OperTypeEnum.DebitRefund;
			Guid chequeOperTypeRefundDebit = this.ChequeOperTypeRefundDebit;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(returnCheque.ID_CHEQUE_GLOBAL, base.GetType().Name, chequeOperTypeRefundDebit);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Возврат по карте лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторный возврат произведен не будет.", this.Name, baseCheque.ID_CHEQUE_GLOBAL));
				return;
			}
			PaymentType cHEQUEPAYMENTTYPE = baseCheque.CHEQUE_PAYMENT_TYPE;
			if (cHEQUEPAYMENTTYPE == PaymentType.Card || cHEQUEPAYMENTTYPE == PaymentType.Cash)
			{
				if (cHEQUEPAYMENTTYPE == PaymentType.Mixed)
				{
					if (baseCheque.CHEQUE_PAYMENTS.All<CHEQUE_PAYMENT>((CHEQUE_PAYMENT p) => {
						if (p.SEPARATE_TYPE_ENUM == PaymentType.Card)
						{
							return true;
						}
						return p.SEPARATE_TYPE_ENUM == PaymentType.Cash;
					}))
					{
						throw new LoyaltyException(this, "Невозможно выполнить возврат бонусов при оплате отличной от Наличных или Картой");
					}
				}
				Operation operation = this.CreateRefundMindboxOrder(baseCheque, returnCheque, refundedCheques);
				LoyaltyCardInfo loyaltyCardInfo = this.GetLoyaltyCardInfo(base.ClientPublicId, base.ClientPublicIdType);
				this.Api.ReturnedV3(operation);
				LoyaltyCardInfo loyaltyCardInfo1 = this.GetLoyaltyCardInfo(base.ClientPublicId, base.ClientPublicIdType);
				decimal balance = loyaltyCardInfo1.Balance;
				decimal num = loyaltyCardInfo1.Balance - loyaltyCardInfo.Balance;
				if (num <= new decimal(0))
				{
					operTypeEnum = OperTypeEnum.ChargeRefund;
					chequeOperTypeRefundDebit = this.ChequeOperTypeRefundCharge;
				}
				else
				{
					operTypeEnum = OperTypeEnum.DebitRefund;
				}
				base.LogMsg(operTypeEnum, "Информация о возврате успешно отправлена в Mindbox");
				LpTransactionData lpTransactionDatum = new LpTransactionData(returnCheque.ID_CHEQUE_GLOBAL, chequeOperTypeRefundDebit);
				base.SaveTransaction(operTypeEnum, Math.Abs(num), lpTransactionDatum);
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
				decimal num1 = (num > new decimal(0) ? num : new decimal(0));
				decimal num2 = (num < new decimal(0) ? -num : new decimal(0));
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append('\"').Append(this.Name).Append('\"').AppendLine();
				if (num2 > new decimal(0))
				{
					stringBuilder.AppendLine("Возврат списания");
					stringBuilder.Append("Начислено: ").Append((int)num1).AppendLine();
				}
				if (num1 > new decimal(0))
				{
					stringBuilder.AppendLine("Возврат начисления");
					stringBuilder.Append("Списано: ").Append((int)num2).AppendLine();
				}
				stringBuilder.Append("Баланс: ").Append((int)balance).AppendLine();
				stringBuilder.AppendLine(" ");
				stringBuilder.AppendLine(" ");
				base.Log(stringBuilder.ToString());
				LpTransResult lpTransResult = new LpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num1, num2, balance, string.Empty, true)
				{
					LpType = base.LoyaltyType
				};
				result = lpTransResult;
				return;
			}
			throw new LoyaltyException(this, "Невозможно выполнить возврат бонусов при оплате отличной от Наличных или Картой");
		}

		protected override void DoRollback(out string slipCheque)
		{
			slipCheque = string.Empty;
			if (!this.m_registeredOrders.Any<KeyValuePair<CHEQUE, Order>>())
			{
				return;
			}
			foreach (CHEQUE array in this.m_registeredOrders.Keys.ToArray<CHEQUE>())
			{
				this.CancelOrder(array);
			}
		}

		protected override bool DoUpdateLoyaltyCardInfo(LoyaltyCardInfo currentInfo, LoyaltyCardInfo newInfo)
		{
			ProcessingStatusContainter customer;
			DiscountCard newDiscountCard;
			DiscountCard discountCard = new DiscountCard(newInfo.ClientId);
			Customer customer1 = this.GetCustomer();
			GetCustomerResult getCustomerResult = this.Api.GetCustomer(customer1);
			if (getCustomerResult.Status != MindboxApiResult.ResultStatus.Success)
			{
				throw new LoyaltyException(this, "Потрбитель не найден");
			}
			if (getCustomerResult.DiscountCards == null || (int)getCustomerResult.DiscountCards.Length == 0)
			{
				CardRegistrationResult cardRegistrationResult = this.AppendCard(customer1, newInfo.ClientId);
				customer = cardRegistrationResult.Customer;
				newDiscountCard = cardRegistrationResult.NewDiscountCard;
			}
			else
			{
				DiscountCard discountCard1 = null;
				switch (currentInfo.ClientIdType)
				{
					case PublicIdType.CardNumber:
					{
						discountCard1 = new DiscountCard(currentInfo.ClientId);
						break;
					}
					case PublicIdType.Phone:
					{
						DiscountCard discountCard2 = this.FindDiscountCard(getCustomerResult.DiscountCards);
						discountCard1 = new DiscountCard(discountCard2.Ids.Number);
						break;
					}
					default:
					{
						throw new NotImplementedException();
					}
				}
				CardReplacementResult cardReplacementResult = this.Api.CardReplacement(this.GetCustomer(currentInfo), discountCard1, discountCard, MindboxLoyaltyProgram.PointOfContact);
				customer = cardReplacementResult.Customer;
				newDiscountCard = cardReplacementResult.NewDiscountCard;
			}
			if (customer.ProcessingStatusValue != ProcessingStatusContainter.ProcessingStatus.Found)
			{
				throw new ApplicationException("Потребитель не найден в БД Mindbox!");
			}
			if (newDiscountCard.ProcessingStatusValue != ProcessingStatus.ProcessingStatusType.Bound)
			{
				string resourceString = this.GetResourceString(newDiscountCard.ProcessingStatusValue.ToString(), "DiscountCardProcessingStatus", new object[0]);
				throw new ApplicationException(resourceString);
			}
			return true;
		}

		private static object ExtractParameter(object parameters, string name)
		{
			Type type = parameters.GetType();
			if (!(
				from p in (IEnumerable<PropertyInfo>)type.GetProperties()
				select p.Name).Contains<string>(name))
			{
				return null;
			}
			return type.GetProperty(name).GetValue(parameters, null);
		}

		private DiscountCard FindDiscountCard(IEnumerable<DiscountCard> cards)
		{
			DiscountCard discountCard = null;
			if (base.ClientPublicIdType == PublicIdType.CardNumber)
			{
				discountCard = cards.FirstOrDefault<DiscountCard>((DiscountCard c) => c.Ids.Number.Equals(base.ClientPublicId));
				if (discountCard == null)
				{
					throw new LoyaltyException(this, string.Format("Mindbox: Карта {0} не найдена...", base.ClientPublicId));
				}
				return discountCard;
			}
			if (this.m_params.CardPrefix == null)
			{
				discountCard = this.FindDiscountCard(cards, new List<string>());
			}
			else
			{
				string cardPrefix = this.m_params.CardPrefix;
				char[] chrArray = new char[] { ',' };
				List<string> list = (
					from x in cardPrefix.Split(chrArray)
					select x.Trim()).ToList<string>();
				discountCard = this.FindDiscountCard(cards, list);
			}
			return discountCard;
		}

		private DiscountCard FindDiscountCard(IEnumerable<DiscountCard> cards, List<string> prefix)
		{
			DiscountCard discountCard = null;
			IEnumerable<DiscountCard> discountCards = (prefix == null ? cards : 
				from c in cards
				where prefix.Any<string>((string x) => c.Ids.Number.StartsWith(x))
				select c);
			discountCard = discountCards.FirstOrDefault<DiscountCard>((DiscountCard c) => c.Status.Ids.SystemName.Equals("Activated", StringComparison.InvariantCultureIgnoreCase)) ?? discountCards.FirstOrDefault<DiscountCard>((DiscountCard c) => !c.Status.Ids.SystemName.Equals("Blocked", StringComparison.InvariantCultureIgnoreCase)) ?? discountCards.FirstOrDefault<DiscountCard>();
			if (discountCard == null)
			{
				throw new ApplicationException("У клиента нет ни одной активной карты!");
			}
			return discountCard;
		}

		private LoyaltyCardInfo GenerateLoyaltyCardInfo(Customer customer)
		{
			LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
			{
				ClientName = customer.FirstName,
				ClientId = customer.MobilePhone,
				ClientIdType = PublicIdType.Phone
			};
			return loyaltyCardInfo;
		}

		public void GetClientRecomendations(CHEQUE cheque)
		{
			try
			{
				base.InitInternal();
				if (!string.IsNullOrEmpty(this.loyaltyCard.CustomerExternalId))
				{
					RecomendationsResultV3 recomendationsV3 = this.Api.GetRecomendationsV3(this.loyaltyCard.CustomerExternalId, 10);
					if (recomendationsV3.Recomendations != null)
					{
						this.AddRecomendation(cheque, RecommendationType.Person, null, (
							from x in (IEnumerable<Recomendation>)recomendationsV3.Recomendations
							select x.Ids.Id).ToArray<string>());
					}
				}
				this.RiseOnUpdated(cheque);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				MessageBox.Show(string.Concat("Ошибка при запросе рекомендаций: ", exception.Message));
			}
		}

		private Customer GetCustomer(LoyaltyCardInfo cardInfo)
		{
			if (base.ClientPublicId == "0000000000")
			{
				return null;
			}
			Customer customer = new Customer();
			switch (cardInfo.ClientIdType)
			{
				case PublicIdType.CardNumber:
				{
					customer.DiscountCard = new DiscountCard(cardInfo.ClientId);
					customer.MobilePhone = cardInfo.ClientPhone;
					break;
				}
				case PublicIdType.Phone:
				{
					customer.MobilePhone = cardInfo.ClientId;
					break;
				}
				default:
				{
					throw new NotImplementedException();
				}
			}
			return customer;
		}

		private Customer GetCustomer()
		{
			if (string.IsNullOrEmpty(base.ClientId))
			{
				return this.GetDefaultCustomer();
			}
			return this.GetCustomer(base.GetLoyaltyCardInfo(false));
		}

		private Customer GetDefaultCustomer()
		{
			Customer customer = new Customer();
			if (this.m_params.DefaultLoyaltyCredentials.IsActive && !string.IsNullOrEmpty(this.m_params.DefaultLoyaltyCredentials.MindboxId))
			{
				CustomerIds customerId = new CustomerIds()
				{
					Mindbox = this.m_params.DefaultLoyaltyCredentials.MindboxId
				};
				customer.Ids = customerId;
			}
			return customer;
		}

		public override LoyaltyCard GetLoyaltyCard()
		{
			return this.loyaltyCard;
		}

		private LoyaltyCard GetLoyaltyCard(CHEQUE cheque)
		{
			MindboxCard mindboxCard = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is MindboxCard) as MindboxCard;
			if (mindboxCard == null)
			{
				mindboxCard = this.loyaltyCard;
			}
			else
			{
				this.loyaltyCard = mindboxCard;
			}
			return mindboxCard;
		}

		private LoyaltyCardInfo GetLoyaltyCardInfo(string id, PublicIdType idType)
		{
			GetCustomerResult customerByCardNubmer;
			switch (idType)
			{
				case PublicIdType.CardNumber:
				{
					customerByCardNubmer = this.Api.GetCustomerByCardNubmer(id);
					break;
				}
				case PublicIdType.Phone:
				{
					customerByCardNubmer = this.Api.GetCustomerByPhone(id);
					break;
				}
				default:
				{
					throw new NotImplementedException();
				}
			}
			LoyaltyCardInfo resourceString = new LoyaltyCardInfo()
			{
				ClientName = customerByCardNubmer.Customer.FirstName,
				ClientId = id,
				ClientIdType = idType,
				ClientPhone = customerByCardNubmer.Customer.MobilePhone,
				ClientEmail = customerByCardNubmer.Customer.Email,
				BalanceDetails = new List<IBalanceInfoRow>(),
				Balance = new decimal(0)
			};
			if (customerByCardNubmer.Balances != null && (int)customerByCardNubmer.Balances.Length > 0)
			{
				resourceString.Balance = ((IEnumerable<Balance>)customerByCardNubmer.Balances).Sum<Balance>((Balance b) => b.Available);
				resourceString.BalanceDetails.AddRange(
					from b in (IEnumerable<Balance>)customerByCardNubmer.Balances
					select new BalanceInfoRow()
					{
						Amount = b.Available,
						Name = (b.Type == null ? string.Empty : b.Type.Name)
					});
			}
			if (customerByCardNubmer.DiscountCards != null && (int)customerByCardNubmer.DiscountCards.Length > 0)
			{
				DiscountCard discountCard = this.FindDiscountCard(customerByCardNubmer.DiscountCards);
				string systemName = discountCard.Status.Ids.SystemName;
				resourceString.CardStatus = this.GetResourceString(systemName, "MindboxCardStatus", new object[0]);
				resourceString.CardNumber = discountCard.Ids.Number;
				resourceString.CardStatusId = this.ConvertStatus(systemName);
			}
			this.loyaltyCard.CustomerExternalId = customerByCardNubmer.Customer.Ids.MindboxId;
			return resourceString;
		}

		private Order GetMindboxOrderId(CHEQUE cheque)
		{
			if (!this.m_registeredOrders.ContainsKey(cheque))
			{
				throw new ApplicationException("Заказ не зарегистрирован в системе Mindbox");
			}
			return this.m_registeredOrders[cheque];
		}

		public void GetProductRecommendations(string code, CHEQUE cheque)
		{
			try
			{
				base.InitInternal();
				IMindboxWebApi api = this.Api;
				string[] strArrays = new string[] { code };
				string[] array = api.GetRecomendationsV3(strArrays, 10).SelectMany<RecomendationsResultV3, Recomendation>((RecomendationsResultV3 x) => x.Recomendations).Select<Recomendation, string>((Recomendation x) => x.Ids.Id).ToArray<string>();
				this.AddRecomendation(cheque, RecommendationType.Goods, code, array);
				this.RiseOnUpdated(cheque);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				MessageBox.Show(string.Concat("Ошибка при запросе рекомендаций: ", exception.Message));
			}
		}

		protected virtual void GetRecommendations(IEnumerable<string> productCodes)
		{
		}

		private string GetResourceString(string name)
		{
			return this.GetResourceString(name, null, new object[0]);
		}

		private string GetResourceString(string name, string prefix, params object[] pList)
		{
			string str = (string.IsNullOrEmpty(prefix) ? name : string.Format("{1}_{0}", name, prefix));
			object obj = Resources.ResourceManager.GetObject(str);
			if (obj != null)
			{
				str = (string)obj;
				if (pList.Any<object>())
				{
					str = string.Format(str, pList);
				}
			}
			return str;
		}

		private List<STOCK_DETAIL> GetStockDetails(string code)
		{
			Dictionary<Guid, STOCK_DETAIL> cache = (new StocksCache(new MemoryCacheManager(MemoryCache.Default))).GetCache();
			List<STOCK_DETAIL> list = cache.Values.Where<STOCK_DETAIL>((STOCK_DETAIL s) => {
				if (s.CODE == null)
				{
					return false;
				}
				return s.CODE.Equals(code);
			}).ToList<STOCK_DETAIL>();
			return list;
		}

		private void InitCardStatuses(string[] active, string[] blocked, string[] restricted)
		{
			this.activeCardStatuses = active ?? new string[] { "Activated" };
			this.blockedCardStatuses = blocked ?? new string[] { "Blocked" };
			this.restrictedCardStatuses = restricted ?? new string[] { "Issued", "NotIssued", "Inactive" };
		}

		private void InitLoyaltySettings(Guid instance)
		{
			if (this.m_settings == null)
			{
				SettingsModel settingsModel = new SettingsModel();
				MindboxLoyaltyProgram.m_loyaltySettings = settingsModel.Load(ePlus.Loyalty.LoyaltyType.Mindbox, instance, ServerType.Local);
				this.m_settings = settingsModel.Deserialize<Settings>(MindboxLoyaltyProgram.m_loyaltySettings.SETTINGS, "Settings");
				this.m_params = settingsModel.Deserialize<Params>(MindboxLoyaltyProgram.m_loyaltySettings.PARAMS, "Params");
				if (!string.IsNullOrEmpty(MindboxLoyaltyProgram.PointOfContact))
				{
					this.m_params.PointOfContact = MindboxLoyaltyProgram.PointOfContact;
				}
				MindboxLoyaltyProgram.IscompatibilityEnabled = MindboxLoyaltyProgram.m_loyaltySettings.COMPATIBILITY;
				if (MindboxLoyaltyProgram.ExcludedPrograms.Count > 0)
				{
					return;
				}
				if (MindboxLoyaltyProgram.IscompatibilityEnabled)
				{
					MindboxLoyaltyProgram.ExcludedPrograms.Add(MindboxLoyaltyProgram.m_loyaltySettings.ID_LOYALITY_PROGRAM_GLOBAL, null);
					foreach (DataRowItem excludeList in MindboxLoyaltyProgram.m_loyaltySettings.CompatibilitiesDCT.ExcludeList)
					{
						MindboxLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
					}
					foreach (DataRowItem dataRowItem in MindboxLoyaltyProgram.m_loyaltySettings.CompatibilitiesDP.ExcludeList)
					{
						if (dataRowItem.Guid == ARM_DISCOUNT2_PROGRAM.MindboxDiscountGUID)
						{
							continue;
						}
						MindboxLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
					}
					foreach (DataRowItem excludeList1 in MindboxLoyaltyProgram.m_loyaltySettings.CompatibilitiesPL.ExcludeList)
					{
						MindboxLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
					}
				}
			}
		}

		private bool IsAuthenticationRequired(decimal discountSum, CHEQUE cheque)
		{
			if (!this.m_params.CustomOptions.EnablePreOrederRegistrationSmsAuthentication)
			{
				return false;
			}
			if (discountSum == new decimal(0))
			{
				return false;
			}
			if (!cheque.DiscountCardPolicyList.Any<DISCOUNT2_CARD_POLICY>((DISCOUNT2_CARD_POLICY d) => d is MindboxCard))
			{
				return false;
			}
			base.GetLoyaltyCardInfo(false);
			return discountSum > new decimal(0);
		}

		protected override void OnInitInternal()
		{
			if (this.Api == null)
			{
				this.Api = ePlus.Loyalty.Mindbox.ApiHelper.GetApi(this.m_settings, this.m_params);
				this.InitCardStatuses(this.m_params.CustomOptions.ActiveCardStatuses, this.m_params.CustomOptions.BlockedCardStatuses, this.m_params.CustomOptions.RestrictedCardStatuses);
			}
		}

		protected override void OnInitSettings()
		{
		}

		protected override bool OnSendAuthenticationCode()
		{
			if (this.m_isAuthenticationCodeSent && this.m_authenticationCodeSentDateTime.HasValue && (DateTime.Now - this.m_authenticationCodeSentDateTime.Value).TotalMinutes <= 0)
			{
				return false;
			}
			Customer customer = this.GetCustomer();
			this.Api.ConfirmationCodeSend(customer, MindboxLoyaltyProgram.PointOfContact);
			this.m_isAuthenticationCodeSent = true;
			this.m_authenticationCodeSentDateTime = new DateTime?(DateTime.Now);
			return true;
		}

		private LoyaltyCardInfo RegisterCustomer(string phone)
		{
			Customer customer = new Customer()
			{
				MobilePhone = phone
			};
			if (this.Api.CustomerOfflineReg(customer, MindboxLoyaltyProgram.PointOfContact).Customer.ProcessingStatusValue != ProcessingStatus.ProcessingStatusType.Created)
			{
				throw new LoyaltyException(this, string.Format("Не удалось создать пользователя по номеру телефона: {0}", phone));
			}
			if (DialogResult.Yes == MessageBox.Show(this.waitingForm, "Покупатель успешно зарегистрирован. Желаете выдать дисконтную карту?", "Карта покупателя", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
				CardRegistrationResult cardRegistrationResult = this.CardRegistration(customer);
				if (cardRegistrationResult != null)
				{
					string resourceString = this.GetResourceString(cardRegistrationResult.NewDiscountCard.ProcessingStatusValue.ToString(), "DiscountCardProcessingStatus", new object[0]);
					MessageBox.Show(this.waitingForm, resourceString, "Выдача новой карты", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
			GetCustomerResult getCustomerResult = this.Api.GetCustomer(customer);
			return this.GenerateLoyaltyCardInfo(getCustomerResult.Customer);
		}

		protected override void RegisterInDatabase(ILpTransResult result)
		{
			base.RegisterInDatabase(result);
			LpTransResult lpTransResult = result as LpTransResult;
			if (lpTransResult == null || lpTransResult.Details == null || !lpTransResult.Details.Any<PCX_CHEQUE_ITEM>())
			{
				return;
			}
			BaseLoyaltyProgramEx.PcxChequeItemBl.Save(lpTransResult.Details);
		}

		private void RegisterOrder(CHEQUE cheque)
		{
			decimal discountSum = base.GetDiscountSum(cheque);
			Order authenticationCode = this.CreateCheckoutMindboxOrder(cheque, discountSum);
			Order order = null;
			LoyaltyCard loyaltyCard = this.GetLoyaltyCard(cheque);
			if (!this.IsAuthenticationRequired(discountSum, cheque))
			{
				order = this.Api.CheckedOut(authenticationCode, false);
			}
			else
			{
				authenticationCode.AuthenticationCode = loyaltyCard.AuthenticationCode;
				if (string.IsNullOrEmpty(authenticationCode.AuthenticationCode))
				{
					using (SmsAuthenticationForm smsAuthenticationForm = new SmsAuthenticationForm())
					{
						do
						{
							base.SendAuthenticationCode();
						}
						while (this.waitingForm.ShowChildDialod(smsAuthenticationForm) == DialogResult.Retry);
						authenticationCode.AuthenticationCode = smsAuthenticationForm.Code;
					}
				}
				if (authenticationCode.Customer == null || string.IsNullOrEmpty(authenticationCode.Customer.MobilePhone))
				{
					throw new MindboxApiExcepion("К заказу не привязан телефон - подтверждение операции невозможно");
				}
				order = this.Api.CheckedOut(authenticationCode, true);
				this.loyaltyCard = (MindboxCard)loyaltyCard;
				loyaltyCard.AcquiredBalanceChange = order.TotalAcquiredBalanceChange;
				loyaltyCard.AuthenticationCode = authenticationCode.AuthenticationCode;
			}
			if (this.loyaltyCard != null)
			{
				this.AddMessages(order, this.loyaltyCard);
			}
			if (this.m_registeredOrders.ContainsKey(cheque))
			{
				this.m_registeredOrders[cheque] = order;
				return;
			}
			this.m_registeredOrders.Add(cheque, order);
		}

		private void RiseOnUpdated(CHEQUE cheque)
		{
			BusinessLogicEvents.Instance.OnRecommendationsUpdated(new RecommendationsEventArgs(cheque.GetRecomendations()));
		}
	}
}