using ePlus.ARMBusinessLogic;
using ePlus.ARMBusinessLogic.Caches.ContractorGroupsCaches;
using ePlus.ARMCacheManager;
using ePlus.ARMCacheManager.Interfaces;
using ePlus.ARMCasher;
using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.BusinessLogic.Events;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon;
using ePlus.ARMCommon.Log;
using ePlus.CommonEx;
using ePlus.Dictionary.BusinessObjects;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.Loyalty.SailPlay;
using ePlus.Loyalty.SailPlay.Forms;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.SailPlay
{
	internal class SailPlayLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private readonly static Guid _chequeOperTypeCharge;

		private readonly static Guid _chequeOperTypeDebit;

		private readonly static Guid _chequeOperTypeRefundCharge;

		private readonly static Guid _chequeOperTypeRefundDebit;

		private static Guid _id;

		private static Dictionary<Guid, DataRowItem> ExcludedPrograms;

		private SailPlayWebApi _api;

		private long? _lastCartId;

		private string codeConfirmation;

		private Guid? lastChequeIdGlobal;

		private Guid ChequeOperTypeCharge
		{
			get
			{
				return SailPlayLoyaltyProgram._chequeOperTypeCharge;
			}
		}

		private Guid ChequeOperTypeDebit
		{
			get
			{
				return SailPlayLoyaltyProgram._chequeOperTypeDebit;
			}
		}

		private Guid ChequeOperTypeRefundCharge
		{
			get
			{
				return SailPlayLoyaltyProgram._chequeOperTypeRefundCharge;
			}
		}

		private Guid ChequeOperTypeRefundDebit
		{
			get
			{
				return SailPlayLoyaltyProgram._chequeOperTypeRefundDebit;
			}
		}

		public override Guid IdGlobal
		{
			get
			{
				return SailPlayLoyaltyProgram._id;
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
				return "Мое здоровье";
			}
		}

		protected override bool OnIsExplicitDiscount
		{
			get
			{
				return false;
			}
		}

		private static ePlus.Loyalty.SailPlay.Params Params
		{
			get;
			set;
		}

		public override IEnumerable<string> PersonalAdditionsSale
		{
			get;
			set;
		}

		private static ePlus.Loyalty.SailPlay.Settings Settings
		{
			get;
			set;
		}

		public override bool SuccessCodeConfirmation
		{
			get;
			set;
		}

		static SailPlayLoyaltyProgram()
		{
			SailPlayLoyaltyProgram._chequeOperTypeCharge = new Guid("BE257268-1307-4F25-A4A1-1459B6176347");
			SailPlayLoyaltyProgram._chequeOperTypeDebit = new Guid("8BC86099-9DB6-46A5-9277-2115330F3FF7");
			SailPlayLoyaltyProgram._chequeOperTypeRefundCharge = new Guid("44F6E9F6-72BA-487B-8A2B-EBB52902FEF6");
			SailPlayLoyaltyProgram._chequeOperTypeRefundDebit = new Guid("C6DC7F28-8430-4519-A368-8C23F6BDC98C");
			SailPlayLoyaltyProgram._id = new Guid("3C6BCC91-3F6D-4E6B-A7B0-D1DCFBEF8F00");
			SailPlayLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public SailPlayLoyaltyProgram(string publicId) : base(ePlus.Loyalty.LoyaltyType.SailPlay, publicId, publicId, "LP_SPLAY")
		{
			base.SendRecvTimeout = 30;
		}

		private void AddSailPlayExtraDiscounts(CalcResult calcResult, CHEQUE cheque, LoyaltyCard card)
		{
			CHEQUE_ITEM cHEQUEITEM;
			card.ExtraDiscounts.Clear();
			cheque.CHEQUE_ITEMS.ForEach((CHEQUE_ITEM ch) => ch.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.TYPE == "SP_EX"));
			Dictionary<int, CHEQUE_ITEM> dictionary = cheque.CHEQUE_ITEMS.ToDictionary<CHEQUE_ITEM, int>((CHEQUE_ITEM c) => Math.Abs(c.ID_LOT_GLOBAL.GetHashCode()));
			MarketingActionResult[] marketingActionsApplied = calcResult.MarketingActionsApplied;
			Func<MarketingActionResult, string> alias = (MarketingActionResult a) => a.Alias;
			Dictionary<string, string> strs = ((IEnumerable<MarketingActionResult>)marketingActionsApplied).ToDictionary<MarketingActionResult, string, string>(alias, (MarketingActionResult a) => a.Name);
			ILoyaltyMessageList loyaltyMessageList = card as ILoyaltyMessageList;
			if (loyaltyMessageList != null)
			{
				loyaltyMessageList.Clear();
				MarketingActionResult[] possibleMarketingActions = calcResult.PossibleMarketingActions;
				for (int i = 0; i < (int)possibleMarketingActions.Length; i++)
				{
					MarketingActionResult marketingActionResult = possibleMarketingActions[i];
					if (marketingActionResult.ServiceMessage != null && loyaltyMessageList != null)
					{
						loyaltyMessageList.Add(new LoyaltyMessage(LoyaltyMessageType.Service, marketingActionResult.ServiceMessage));
					}
				}
			}
			CartPositionResult[] positions = calcResult.Cart.Positions;
			for (int j = 0; j < (int)positions.Length; j++)
			{
				CartPositionResult cartPositionResult = positions[j];
				if (cartPositionResult.NewPrice.HasValue && cartPositionResult.MarketingActions != null && (int)cartPositionResult.MarketingActions.Length > 0 && dictionary.TryGetValue(cartPositionResult.Num, out cHEQUEITEM))
				{
					DISCOUNT_VALUE_INFO dISCOUNTVALUEINFO = new DISCOUNT_VALUE_INFO()
					{
						BARCODE = card.BARCODE,
						ID_DISCOUNT2_GLOBAL = ARM_DISCOUNT2_PROGRAM.SailPlayDiscountGUID,
						DISCOUNT2_NAME = string.Join(";", 
							from a in cartPositionResult.MarketingActions
							select strs[a]),
						TYPE = "SP_EX",
						ID_LOT_GLOBAL = cHEQUEITEM.ID_LOT_GLOBAL,
						VALUE = cartPositionResult.Price - cartPositionResult.NewPrice.Value
					};
					card.ExtraDiscounts.Add(dISCOUNTVALUEINFO);
				}
			}
			cheque.CalculateFields();
		}

		private decimal CalculateChequeItemDiscountWithoutRoundingDiscount(CHEQUE_ITEM item)
		{
			return item.Discount2MakeItemList.Where<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM dmi) => {
				if (dmi.ID_DISCOUNT2_PROGRAM_GLOBAL == ARM_DISCOUNT2_PROGRAM.SailPlayDiscountGUID)
				{
					return false;
				}
				return dmi.ID_DISCOUNT2_PROGRAM_GLOBAL != ARM_DISCOUNT2_PROGRAM.RoundingDiscountGUID;
			}).Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM d) => d.AMOUNT);
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			this.ClearSelfAndRoundingDiscounts(cheque);
			LoyaltyCard nullable = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is SailPlayCard) as LoyaltyCard;
			Cart carts = LoyaltyProgManager.CreateSailPlayCart(cheque);
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(false);
			CalcResult calcResult = this._api.CalcMaxDiscount(carts, (int)cheque.SUMM);
			decimal num = (nullable == null ? new decimal(0, 0, 0, false, 1) : nullable.DiscountSum);
			num = Math.Min(num, calcResult.Cart.TotalDiscountPointsMax);
			decimal totalPrice = calcResult.Cart.TotalPrice;
			totalPrice = Math.Floor(totalPrice--);
			num = Math.Min(totalPrice, num);
			if (num > new decimal(0))
			{
				carts = this.Distribute(calcResult, num, cheque);
				calcResult = this._api.CalcMaxDiscount(carts, (int)cheque.SUMM);
			}
			this._lastCartId = new long?(calcResult.Cart.Id);
			int num1 = ((IEnumerable<CartPositionResult>)calcResult.Cart.Positions).Sum<CartPositionResult>((CartPositionResult p) => p.DiscountPointsMax);
			nullable.DiscountSumMax = new decimal?(((IEnumerable<CartPositionResult>)calcResult.Cart.Positions).Sum<CartPositionResult>((CartPositionResult p) => p.DiscountPoints));
			if (nullable != null)
			{
				this.AddSailPlayExtraDiscounts(calcResult, cheque, nullable);
			}
			decimal num2 = Math.Min(num1, loyaltyCardInfo.Balance);
			return Math.Min(num2, totalPrice);
		}

		protected override void CheckCodeConfirmation(CHEQUE cheque, decimal discountSum)
		{
			this.SuccessCodeConfirmation = true;
			if (SailPlayLoyaltyProgram.Params.DebitPermitBySms && (cheque.SUMM + discountSum) >= SailPlayLoyaltyProgram.Params.MinSumForDebit && this.CheckCodeConfirmationEvent != null)
			{
				this.CheckCodeConfirmationEvent(this, null);
			}
		}

		private void ClearSelfAndRoundingDiscounts(CHEQUE cheque)
		{
			cheque.CHEQUE_ITEMS.ForEach((CHEQUE_ITEM ch) => {
				ch.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.ID_DISCOUNT2_PROGRAM_GLOBAL == ARM_DISCOUNT2_PROGRAM.SailPlayDiscountGUID);
				ch.Discount2MakeItemList.RemoveAll((DISCOUNT2_MAKE_ITEM d) => d.ID_DISCOUNT2_PROGRAM_GLOBAL == ARM_DISCOUNT2_PROGRAM.RoundingDiscountGUID);
			});
			(cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is SailPlayCard) as LoyaltyCard).ExtraDiscounts.RemoveAll((DISCOUNT_VALUE_INFO e) => e.TYPE.Equals("SP_EX"));
			cheque.CalculateFields();
		}

		public override void CodeConfirmation(CHEQUE cheque, string code)
		{
			this.SuccessCodeConfirmation = code == this.codeConfirmation;
		}

		private Cart Distribute(CalcResult calc, decimal discountPoints, CHEQUE cheque)
		{
			decimal totalDiscountPointsMax = calc.Cart.TotalDiscountPointsMax;
			Cart carts = new Cart();
			Dictionary<int, CHEQUE_ITEM> dictionary = cheque.CHEQUE_ITEMS.ToDictionary<CHEQUE_ITEM, int>((CHEQUE_ITEM c) => Math.Abs(c.ID_LOT_GLOBAL.GetHashCode()));
			IOrderedEnumerable<CartPositionResult> positions = 
				from p in (IEnumerable<CartPositionResult>)calc.Cart.Positions
				orderby p.DiscountPointsMax descending
				select p;
			foreach (CartPositionResult position in positions)
			{
				decimal num = new decimal(0);
				if (discountPoints > new decimal(0))
				{
					CHEQUE_ITEM item = dictionary[position.Num];
					num = Math.Min(Math.Ceiling((position.DiscountPointsMax * discountPoints) / totalDiscountPointsMax), discountPoints);
					num = Math.Min(num, position.DiscountPointsMax);
					decimal num1 = num;
					decimal? newPrice = position.NewPrice;
					num = Math.Min(num1, Math.Floor((newPrice.HasValue ? newPrice.GetValueOrDefault() : position.Price)));
					discountPoints -= num;
					totalDiscountPointsMax -= position.DiscountPointsMax;
				}
				PurchaseItem purchaseItem = new PurchaseItem()
				{
					Sku = position.Product.Sku,
					Price = position.Price,
					Qantity = position.Quantity,
					DiscountPoints = (int)num
				};
				carts.AddPurchase(purchaseItem, position.Num);
			}
			CartPositionResult cartPositionResult = positions.First<CartPositionResult>();
			CHEQUE_ITEM cHEQUEITEM = dictionary[cartPositionResult.Num];
			if (discountPoints > new decimal(0))
			{
				decimal num2 = discountPoints;
				decimal? nullable = cartPositionResult.NewPrice;
				if (num2 > (Math.Floor((nullable.HasValue ? nullable.GetValueOrDefault() : cartPositionResult.Price)) - cartPositionResult.DiscountPoints))
				{
					throw new NotImplementedException("Не удалось корректно распределить бальную скидку Моё здоровье");
				}
				CartPositionResult cartPositionResult1 = positions.Last<CartPositionResult>();
				cartPositionResult1.DiscountPoints = cartPositionResult1.DiscountPoints + (int)discountPoints;
			}
			return carts;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			this.DoProcess(cheque, discountSum, out result);
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			UserInfoResult userInfoResult;
			UserInfoResult userInfo = null;
			try
			{
				userInfo = this._api.GetUserInfo(base.ClientPublicIdType, base.ClientPublicId);
			}
			catch (SailPlayUserNotFoundException sailPlayUserNotFoundException)
			{
				base.Log(string.Format("Пользователь SailPlay ID: {0} не найден в системе.", base.ClientPublicId));
			}
			if (userInfo == null)
			{
				form.Invoke(new MethodInvoker(() => {
					if (MessageBox.Show(form, "Дисконтная карта не зарегистрирована. Продолжить регистрацию?", "Мое здоровье - Регистрация карты", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
					{
						base.Log("Пользователь отказался от регистрации");
						throw new LoyaltyException(this, "Карта не зарегистрирована");
					}
				}));
				form.Invoke(new MethodInvoker(() => {
					try
					{
						form.Hide();
						userInfo = this.FillNewUserInfo(form);
					}
					finally
					{
						form.Show();
					}
					if (userInfo == null)
					{
						base.Log("Пользователь отказался от ввода данных");
						throw new LoyaltyException(this, "Новый пользователь не был зарегистрирован");
					}
				}));
				if (base.ClientPublicIdType == PublicIdType.Phone || !this._api.TryGetUserInfo(PublicIdType.Phone, userInfo.Phone, out userInfoResult))
				{
					this.ShowCodeValidationForm(userInfo.Phone, true, form);
					UserSex userSex = (userInfo.Sex == "1" ? UserSex.Male : UserSex.Female);
					this._api.UserAdd(base.ClientPublicId, base.ClientPublicIdType, userInfo.FirstName, userInfo.LastName, userInfo.MiddleName, userInfo.BirthDate, userSex, (base.ClientPublicIdType == PublicIdType.Phone ? string.Empty : userInfo.Phone));
					if (!string.IsNullOrWhiteSpace(userInfo.EMail))
					{
						this._api.UserUpdateAddEmail(userInfo.EMail);
					}
					try
					{
						SailPlayWebApi sailPlayWebApi = this._api;
						List<string> strs = new List<string>()
						{
							userInfo.AgeTag
						};
						sailPlayWebApi.UserTagAdd(strs, new List<string>()
						{
							"Возраст"
						});
					}
					catch (Exception exception)
					{
						base.Log(string.Concat("Не удалось присвоить тег зарегестрированному пользователю SailPlay. Ошибка: ", exception.Message));
					}
					userInfo = this._api.GetUserInfo(base.ClientPublicId);
				}
				else
				{
					this.ShowCodeValidationForm(userInfoResult.Phone, false, form);
					UserSex userSex1 = (userInfo.Sex == "1" ? UserSex.Male : UserSex.Female);
					this._api.UserAdd(base.ClientPublicId, base.ClientPublicIdType, userInfo.FirstName, userInfo.LastName, userInfo.MiddleName, userInfo.BirthDate, userSex1, string.Empty);
					base.Log(string.Format("Создан новый пользовательский аккаунт. ID: {0}", base.ClientPublicId));
					base.Log(string.Format("Будет выполнена попытка переноса аккаунта {0} на {1}", userInfoResult.ID, base.ClientPublicId));
					if (!string.IsNullOrWhiteSpace(userInfo.EMail))
					{
						this._api.UserUpdateAddEmail(userInfo.EMail);
					}
					this._api.UsersMerge(PublicIdType.Phone, userInfoResult.Phone);
					base.Log("Перенос данных успешно завершён");
					userInfo = this._api.GetUserInfo(base.ClientPublicId);
				}
				if ((new ContractorAttributesCache(new MemoryCacheManager(MemoryCache.Default))).GetCache().BONUS_MZ_ACTIVE && userInfo != null)
				{
					(new SailPlay_Bl()).SaveRegisterCardMyHealth(ArmSecurityManager.CurrentUserId);
					DataSyncBL instance = DataSyncBL.Instance;
					ArmDbSyncDelegate[] armDbSyncDelegate = new ArmDbSyncDelegate[] { new ArmDbSyncDelegate(DataSyncBL.Instance.SyncRegistryMZData) };
					instance.Sync(armDbSyncDelegate);
					BusinessLogicEvents.Instance.OnUpdatePurseBonusEvent(null, null);
				}
			}
			LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
			{
				ClientId = base.ClientPublicId,
				ClientIdType = base.ClientPublicIdType,
				CardNumber = base.ClientPublicId,
				Points = userInfo.Points.Confirmed,
				Balance = loyaltyCardInfo.Points,
				CardStatus = "Активна",
				ClientEmail = userInfo.EMail,
				ClientPhone = userInfo.Phone,
				UserInfo = userInfo
			};
			return loyaltyCardInfo;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (!SailPlayLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !SailPlayLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		private void DoProcess(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			OperTypeEnum operTypeEnum;
			Guid chequeOperTypeCharge;
			decimal num;
			result = null;
			if (!this._lastCartId.HasValue)
			{
				throw new LoyaltyException(this, "Необходимо произвести расчёт корзины, до совершения покупки.");
			}
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
			base.GetDiscountSum(cheque);
			PurchasesNewResult purchasesNewResult = this._api.PurchasesNew(this._lastCartId.Value, cheque.ID_CHEQUE_GLOBAL);
			decimal num1 = (operTypeEnum == OperTypeEnum.Debit ? discountSum : purchasesNewResult.Purchase.PointsDelta);
			if (LoyaltyProgManager.InitMarketingActions() && LoyaltyProgManager.MarketingActions != null)
			{
				List<MarketingActionResult> marketingActionResults = purchasesNewResult.Cart.MarketingActionsApplied.ToList<MarketingActionResult>().FindAll((MarketingActionResult x) => LoyaltyProgManager.MarketingActions.Any<MarketingAction>((MarketingAction y) => y.@alias == x.Alias));
				ILoyaltyMessageList loyaltyMessageList = cheque.DiscountCardPolicyList.Find((DISCOUNT2_CARD_POLICY c) => c is SailPlayCard) as LoyaltyCard as ILoyaltyMessageList;
				if (loyaltyMessageList != null)
				{
					foreach (MarketingActionResult marketingActionResult in marketingActionResults)
					{
						if (marketingActionResult.ClientMessage == null || loyaltyMessageList == null)
						{
							continue;
						}
						loyaltyMessageList.Add(new LoyaltyMessage(LoyaltyMessageType.Cheque, marketingActionResult.ClientMessage));
					}
				}
			}
			base.LogMsg(operTypeEnum, purchasesNewResult.Message);
			LpTransactionData lpTransactionDatum = new LpTransactionData(cheque.ID_CHEQUE_GLOBAL, chequeOperTypeCharge);
			base.SaveTransaction(operTypeEnum, num1, lpTransactionDatum);
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			this.lastChequeIdGlobal = new Guid?(cheque.ID_CHEQUE_GLOBAL);
			Guid dCHEQUEGLOBAL = cheque.ID_CHEQUE_GLOBAL;
			string clientPublicId = base.ClientPublicId;
			num = (purchasesNewResult.Purchase.PointsDelta > new decimal(0) ? purchasesNewResult.Purchase.PointsDelta : new decimal(0));
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(true);
			LpTransResult lpTransResult = new LpTransResult(dCHEQUEGLOBAL, clientPublicId, num, discountSum, loyaltyCardInfo.Balance, string.Empty, false)
			{
				LpType = base.LoyaltyType
			};
			result = lpTransResult;
			base.Log(result.ToSlipCheque(null, null));
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult lpResult)
		{
			string empty = string.Empty;
			lpResult = null;
			decimal num = returnCheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => ci.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE != base.DiscountType)
				{
					return new decimal(0);
				}
				return mi.AMOUNT;
			}));
			bool flag = num > new decimal(0);
			OperTypeEnum operTypeEnum = (num > new decimal(0) ? OperTypeEnum.DebitRefund : OperTypeEnum.ChargeRefund);
			Guid guid = (num > new decimal(0) ? this.ChequeOperTypeRefundDebit : this.ChequeOperTypeRefundCharge);
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(baseCheque.ID_CHEQUE_GLOBAL, base.GetType().Name, guid);
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
				PurchaseInfoResult purchaseInfoResult = this._api.PurchaseInfo(baseCheque.ID_CHEQUE_GLOBAL);
				this._api.PurchaseDelete(baseCheque.ID_CHEQUE_GLOBAL);
				base.LogMsg(operTypeEnum, "Информация о продаже успешно удалена из SailPlay");
				LpTransactionData lpTransactionDatum = new LpTransactionData(baseCheque.ID_CHEQUE_GLOBAL, guid);
				base.SaveTransaction(operTypeEnum, num, lpTransactionDatum);
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
				int num1 = purchaseInfoResult.Cart.Cart.Positions.Sum<PurchaseInfoPosition>((PurchaseInfoPosition p) => p.DiscountPoints);
				int totalPoints = purchaseInfoResult.Cart.Cart.TotalPoints;
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append('\"').Append(this.Name).Append('\"').AppendLine();
				if (num1 > 0)
				{
					stringBuilder.AppendLine("Возврат списания");
					stringBuilder.Append("Начислено: ").Append(Math.Abs(num1)).AppendLine();
				}
				if (totalPoints > 0)
				{
					stringBuilder.AppendLine("Возврат начисления");
					stringBuilder.Append("Списано: ").Append(Math.Abs(totalPoints)).AppendLine();
				}
				UserInfoResult userInfo = this._api.GetUserInfo(base.ClientPublicId);
				stringBuilder.Append("Баланс: ").Append(userInfo.Points.Total).AppendLine();
				stringBuilder.AppendLine(" ");
				stringBuilder.AppendLine(" ");
				empty = stringBuilder.ToString();
				LpTransResult lpTransResult = new LpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num1, totalPoints, userInfo.Points.Confirmed, string.Empty, true)
				{
					LpType = base.LoyaltyType
				};
				lpResult = lpTransResult;
				base.Log(empty);
				return;
			}
			throw new LoyaltyException(this, "Невозможно выполнить возврат бонусов при оплате отличной от Наличных или Картой");
		}

		protected override void DoRollback(out string slipCheque)
		{
			slipCheque = string.Empty;
			ARMLogger.Trace(string.Format("SailPlay: DoRollback", new object[0]));
			if (this.lastChequeIdGlobal.HasValue)
			{
				ARMLogger.Info(string.Format("SailPlay: отмена списания/начисления  по чеку {0}", this.lastChequeIdGlobal.Value));
				if (this._api.PurchaseDelete(this.lastChequeIdGlobal.Value).IsOk)
				{
					slipCheque = "SailPlay: Списание/начисление балов отменено";
				}
				ChequeTransactionEvent chequeTransactionEvent = new ChequeTransactionEvent(this.lastChequeIdGlobal.Value, base.GetType().Name, SailPlayLoyaltyProgram._chequeOperTypeDebit);
				BusinessLogicEvents.Instance.OnRollbackChequeTransaction(this, chequeTransactionEvent);
				chequeTransactionEvent = new ChequeTransactionEvent(this.lastChequeIdGlobal.Value, base.GetType().Name, SailPlayLoyaltyProgram._chequeOperTypeCharge);
				BusinessLogicEvents.Instance.OnRollbackChequeTransaction(this, chequeTransactionEvent);
				this.lastChequeIdGlobal = null;
				ARMLogger.Info(slipCheque);
			}
		}

		private UserInfoResult FillNewUserInfo(Form form)
		{
			UserInfoResult userInfoResult;
			base.Log("Вызыван метод регистрации нового пользователя");
			using (UserRegisterPresenter userRegisterPresenter = new UserRegisterPresenter(new FormSailPlayUserRegister()
			{
				Owner = form
			}))
			{
				userInfoResult = userRegisterPresenter.ShowView(base.ClientPublicId, base.ClientPublicIdType);
			}
			return userInfoResult;
		}

		private string GetClientPhone()
		{
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(false);
			string clientPhone = loyaltyCardInfo.ClientPhone;
			if (clientPhone == null && loyaltyCardInfo.ClientIdType == PublicIdType.Phone)
			{
				clientPhone = loyaltyCardInfo.CardNumber;
			}
			return clientPhone;
		}

		protected override void OnInitInternal()
		{
			if (this._api == null)
			{
				this._api = new SailPlayWebApi(SailPlayLoyaltyProgram.Settings, base.ClientPublicIdType, base.ClientPublicId);
			}
		}

		protected override void OnInitSettings()
		{
			if (SailPlayLoyaltyProgram.Settings == null)
			{
				SettingsModel settingsModel = new SettingsModel();
				LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
				SailPlayLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.SailPlay.Settings>(loyaltySetting.SETTINGS, "Settings");
				SailPlayLoyaltyProgram.Params = settingsModel.Deserialize<ePlus.Loyalty.SailPlay.Params>(loyaltySetting.PARAMS, "Params");
				SailPlayLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
				if (SailPlayLoyaltyProgram.IscompatibilityEnabled)
				{
					SailPlayLoyaltyProgram.ExcludedPrograms.Add(this.IdGlobal, null);
					foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
					{
						SailPlayLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
					}
					foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
					{
						SailPlayLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
					}
					foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
					{
						SailPlayLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
					}
				}
			}
		}

		public override bool RequestCodeConfirmation(CHEQUE cheque)
		{
			bool flag = false;
			this.codeConfirmation = null;
			this.SuccessCodeConfirmation = false;
			string clientPhone = this.GetClientPhone();
			try
			{
				if (clientPhone != null)
				{
					base.Log("SailPlay: Запрашиваем код подтверждения по SMS");
					SmsCodeResult smsCodeResult = this._api.SmsCode(clientPhone, SailPlayLoyaltyProgram.Params.SmsDebitTemplate);
					this.codeConfirmation = (smsCodeResult != null ? smsCodeResult.Code : string.Empty);
					if (string.IsNullOrEmpty(this.codeConfirmation))
					{
						string str = "SailPlay: Получен неверный код подтверждения по SMS";
						base.Log(str);
						throw new Exception(str);
					}
					base.Log("SailPlay: Код подтверждения по SMS успешно получен");
					flag = true;
				}
			}
			catch (Exception exception)
			{
				base.Log(string.Format("SailPlay: Ошибка получения кода подтверждения по SMS\n{0}", exception.Message));
				throw;
			}
			return flag;
		}

		public override void RequestPersonalAdditionSales()
		{
			this.PersonalAdditionsSale = null;
			if (SailPlayLoyaltyProgram.Params != null && SailPlayLoyaltyProgram.Params.UsePersonalAdditionSale)
			{
				base.Log("SailPlay: Запрашиваем список персональных доппродаж");
				string clientPhone = this.GetClientPhone();
				CustomVarsGetResult customVars = null;
				try
				{
					customVars = this._api.GetCustomVars(clientPhone, "products");
				}
				catch (Exception exception)
				{
					base.Log(string.Format("SailPlay: Ошибка получения списка персональных доппродаж\n{0}", exception.Message));
				}
				if (customVars == null || !customVars.IsOk)
				{
					base.Log("SailPlay: Ошибка получения списка персональных доппродаж");
				}
				else
				{
					base.Log("SailPlay: Список персональных доппродаж успешно получен");
					if (!string.IsNullOrEmpty(customVars.@value))
					{
						string str = customVars.@value;
						char[] chrArray = new char[] { '[', ']' };
						string str1 = str.Trim(chrArray);
						char[] chrArray1 = new char[] { ',' };
						IEnumerable<string> strs = 
							from v in str1.Split(chrArray1)
							select v.Trim(new char[] { 'u', '\'', ' ' });
						if (strs.Any<string>())
						{
							this.PersonalAdditionsSale = 
								from x in strs.ToList<string>()
								where x != null
								select x.ToString();
							return;
						}
					}
				}
			}
		}

		private void ShowCodeValidationForm(string phone, bool isNewUser, Form owner)
		{
			owner.Invoke(new MethodInvoker(() => {
				using (FormCodeValidation formCodeValidation = new FormCodeValidation(this._api, isNewUser, SailPlayLoyaltyProgram.Params)
				{
					Owner = owner
				})
				{
					try
					{
						owner.Hide();
						if (formCodeValidation.ShowDialog(phone) != DialogResult.OK)
						{
							base.Log("Пользователь отказался от ввода кода подтверждения");
							throw new LoyaltyException(this, "Новый пользователь не был зарегистрирован");
						}
					}
					finally
					{
						formCodeValidation.Close();
						owner.Show();
					}
				}
			}));
		}

		public override event EventHandler CheckCodeConfirmationEvent;
	}
}