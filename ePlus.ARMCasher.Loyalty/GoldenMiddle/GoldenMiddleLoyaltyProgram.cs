using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher;
using ePlus.ARMCasher.BusinessLogic.Events;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon;
using ePlus.ARMCommon.Config;
using ePlus.ARMCommon.Log;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.Loyalty.GoldenMiddle;
using ePlus.MetaData.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using wscardax;

namespace ePlus.ARMCasher.Loyalty.GoldenMiddle
{
	internal class GoldenMiddleLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private const string NumericFormat = "###0.##";

		private readonly static object _syncLock;

		private readonly static Guid _chequeOperTypeCharge;

		private readonly static Guid _chequeOperTypeDebit;

		private readonly static Guid _chequeOperTypeRefundCharge;

		private readonly static Guid _chequeOperTypeRefundDebit;

		private static Guid _id;

		private static Dictionary<Guid, DataRowItem> ExcludedPrograms;

		private static POSProcessAX _posAx;

		private readonly static GoldenMiddle_Bl _bl;

		private Guid ChequeOperTypeCharge
		{
			get
			{
				return GoldenMiddleLoyaltyProgram._chequeOperTypeCharge;
			}
		}

		private Guid ChequeOperTypeDebit
		{
			get
			{
				return GoldenMiddleLoyaltyProgram._chequeOperTypeDebit;
			}
		}

		private Guid ChequeOperTypeRefundCharge
		{
			get
			{
				return GoldenMiddleLoyaltyProgram._chequeOperTypeRefundCharge;
			}
		}

		private Guid ChequeOperTypeRefundDebit
		{
			get
			{
				return GoldenMiddleLoyaltyProgram._chequeOperTypeRefundDebit;
			}
		}

		public override Guid IdGlobal
		{
			get
			{
				return GoldenMiddleLoyaltyProgram._id;
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
				return "Золотая середина";
			}
		}

		public POSProcessAX PosAx
		{
			get
			{
				POSProcessAX pOSProcessAX;
				lock (GoldenMiddleLoyaltyProgram._syncLock)
				{
					pOSProcessAX = GoldenMiddleLoyaltyProgram._posAx;
				}
				return pOSProcessAX;
			}
		}

		private static ePlus.Loyalty.GoldenMiddle.Settings Settings
		{
			get;
			set;
		}

		static GoldenMiddleLoyaltyProgram()
		{
			GoldenMiddleLoyaltyProgram._syncLock = new object();
			GoldenMiddleLoyaltyProgram._chequeOperTypeCharge = new Guid("EC6238C3-3630-42F2-89FF-678F77D4F074");
			GoldenMiddleLoyaltyProgram._chequeOperTypeDebit = new Guid("24FB0197-1E6D-4F52-BE2E-BFE880139C12");
			GoldenMiddleLoyaltyProgram._chequeOperTypeRefundCharge = new Guid("92622C53-F13B-4B83-8D90-0DD9D8A25519");
			GoldenMiddleLoyaltyProgram._chequeOperTypeRefundDebit = new Guid("6F3B3F66-0087-4E1E-82E1-D139EFC458B5");
			GoldenMiddleLoyaltyProgram._id = new Guid("9DA22AE8-94D9-4665-A208-0609A53EFCAE");
			GoldenMiddleLoyaltyProgram.ExcludedPrograms = new Dictionary<Guid, DataRowItem>();
			GoldenMiddleLoyaltyProgram._bl = new GoldenMiddle_Bl();
		}

		public GoldenMiddleLoyaltyProgram(string publicId) : base(ePlus.Loyalty.LoyaltyType.GoldenMiddle, publicId, publicId, "LP_GM")
		{
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			int num;
			string str;
			decimal num1;
			decimal sUMM = cheque.SUMM + base.GetDiscountSum(cheque);
			this.FillCheque(cheque);
			try
			{
				this.PosAx.ChequeIsSoft = true;
				POSProcessAX posAx = this.PosAx;
				string clientId = base.ClientId;
				Guid guid = Guid.NewGuid();
				posAx.MakeChequeWithBonusDiscount(clientId, out num, out str, guid.ToString(), sUMM.ToString("###0.##", CultureInfo.InvariantCulture), sUMM.ToString("###0.##", CultureInfo.InvariantCulture), sUMM.ToString("###0.##", CultureInfo.InvariantCulture));
				if (num == 0 || num == 81400)
				{
					num1 = Convert.ToDecimal(this.PosAx.AvailablePayment);
				}
				else
				{
					throw new LoyaltyException(this, str);
				}
			}
			finally
			{
				this.PosAx.ChequeIsSoft = false;
			}
			return num1;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			int num;
			string str;
			decimal num1;
			decimal num2;
			result = null;
			if (discountSum > new decimal(0))
			{
				return;
			}
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeCharge);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Начисление баллов на карту лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторное начисление произведено не будет.", this.Name, cheque.ID_CHEQUE_GLOBAL));
				return;
			}
			base.Log(OperTypeEnum.Charge, discountSum, cheque, null, null);
			this.FillCheque(cheque);
			string str1 = cheque.SUMM.ToString("###0.##", CultureInfo.InvariantCulture);
			POSProcessAX posAx = this.PosAx;
			string clientId = base.ClientId;
			Guid dCHEQUEGLOBAL = cheque.ID_CHEQUE_GLOBAL;
			posAx.MakeChequeWithBonusCount(clientId, out num, out str, dCHEQUEGLOBAL.ToString(), str1, str1);
			if (num != 0)
			{
				this.OnException(OperTypeEnum.Charge, str);
			}
			else
			{
				base.LogMsg(OperTypeEnum.Charge, str);
				GMTransactionData gMTransactionDatum = new GMTransactionData(cheque.ID_CHEQUE_GLOBAL, this.ChequeOperTypeCharge, this.PosAx.TransactionID);
				base.SaveTransaction(OperTypeEnum.Charge, discountSum, gMTransactionDatum);
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			}
			this.SaveQueryLog(cheque.ID_CHEQUE_GLOBAL, this.PosAx.ProcessedDT, discountSum, pcxOperation.Charge, this.PosAx.TransactionID);
			base.SavePcxCheque(cheque, discountSum, "CHARGE", this.PosAx.TransactionID);
			if (decimal.TryParse(this.PosAx.ChargedBonus, NumberStyles.Any, CultureInfo.InvariantCulture, out num1) && decimal.TryParse(this.PosAx.Balance, NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
			{
				result = new LpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num1, new decimal(0), num2, string.Empty, false, true);
			}
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			int num;
			string str;
			decimal num1;
			decimal num2;
			decimal num3;
			result = null;
			if (discountSum == new decimal(0))
			{
				return;
			}
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeDebit);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Списание баллов с карты лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторное списание произведено не будет.", this.Name, cheque.ID_CHEQUE_GLOBAL));
				return;
			}
			base.Log(OperTypeEnum.Debit, discountSum, cheque, null, null);
			this.FillCheque(cheque);
			decimal num4 = base.GetDiscountSum(cheque);
			decimal sUMM = cheque.SUMM + num4;
			decimal num5 = (num4 / sUMM) * new decimal(100);
			this.PosAx.ChequeIsSoft = false;
			POSProcessAX posAx = this.PosAx;
			string clientId = base.ClientId;
			Guid dCHEQUEGLOBAL = cheque.ID_CHEQUE_GLOBAL;
			posAx.MakeChequeWithBonusDiscount(clientId, out num, out str, dCHEQUEGLOBAL.ToString(), sUMM.ToString("###0.##", CultureInfo.InvariantCulture), sUMM.ToString("###0.##", CultureInfo.InvariantCulture), num4.ToString("###0.##", CultureInfo.InvariantCulture));
			if (num != 0)
			{
				this.OnException(OperTypeEnum.Debit, str);
			}
			else
			{
				base.LogMsg(OperTypeEnum.Debit, str);
				GMTransactionData gMTransactionDatum = new GMTransactionData(cheque.ID_CHEQUE_GLOBAL, this.ChequeOperTypeDebit, this.PosAx.TransactionID);
				base.SaveTransaction(OperTypeEnum.Debit, discountSum, gMTransactionDatum);
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			}
			this.SaveQueryLog(cheque.ID_CHEQUE_GLOBAL, this.PosAx.ProcessedDT, discountSum, pcxOperation.Debit, this.PosAx.TransactionID);
			base.SavePcxCheque(cheque, discountSum, "DEBIT", this.PosAx.TransactionID);
			if (decimal.TryParse(this.PosAx.DiscountedBonus, out num2) && decimal.TryParse(this.PosAx.ChargedBonus, out num1) && decimal.TryParse(this.PosAx.Balance, out num3))
			{
				result = new LpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, (num1 > new decimal(0) ? num1 : new decimal(0)), num2, num3, string.Empty, false, true);
			}
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			int num;
			string str;
			string str1;
			decimal num1;
			LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
			{
				ClientId = base.ClientId,
				CardNumber = base.ClientPublicId
			};
			this.PosAx.GetBalance(base.ClientId, out num, out str, out str1);
			if (num == 0)
			{
				if (!decimal.TryParse(str1, NumberStyles.Any, CultureInfo.InvariantCulture, out num1))
				{
					throw new LoyaltyException(this, "Баланс карты, полученный от сервиса, не удалось преобразовать в числовое значение.");
				}
				loyaltyCardInfo.CardStatusId = LoyaltyCardStatus.Active;
				loyaltyCardInfo.CardStatus = "Активна";
				loyaltyCardInfo.Balance = num1;
			}
			else if (num == 80241 || num == 59001)
			{
				loyaltyCardInfo.CardStatusId = LoyaltyCardStatus.NotFound;
				loyaltyCardInfo.CardStatus = "Не найдена";
				loyaltyCardInfo.Balance = new decimal(0);
			}
			else
			{
				if (num != 57052)
				{
					throw new LoyaltyException(this, str);
				}
				loyaltyCardInfo.CardStatusId = LoyaltyCardStatus.Blocked;
				loyaltyCardInfo.CardStatus = "Заблокирована";
				loyaltyCardInfo.Balance = new decimal(0);
			}
			return loyaltyCardInfo;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (!GoldenMiddleLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !GoldenMiddleLoyaltyProgram.ExcludedPrograms.ContainsKey(discountId);
		}

		private void DoRefund(CHEQUE baseCheque, CHEQUE returnCheque, decimal discountSum, out ILpTransResult result)
		{
			int num;
			string str;
			string empty = string.Empty;
			result = null;
			pcxOperation _pcxOperation = (discountSum > new decimal(0) ? pcxOperation.Debit : pcxOperation.Charge);
			pcxOperation _pcxOperation1 = (discountSum > new decimal(0) ? pcxOperation.RefundDebit : pcxOperation.RefundCharge);
			OperTypeEnum operTypeEnum = (discountSum > new decimal(0) ? OperTypeEnum.DebitRefund : OperTypeEnum.ChargeRefund);
			Guid guid = (discountSum > new decimal(0) ? this.ChequeOperTypeRefundDebit : this.ChequeOperTypeRefundCharge);
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
				PCX_QUERY_LOG pCXQUERYLOG = BaseLoyaltyProgramEx.QueryLogBl.Load(baseCheque.ID_CHEQUE_GLOBAL, _pcxOperation, (int)base.LoyaltyType);
				if (pCXQUERYLOG == null)
				{
					return;
				}
				int? nullable = null;
				base.Log(operTypeEnum, new decimal(0), baseCheque, nullable, null);
				this.FillCheque(returnCheque);
				decimal sUMM = baseCheque.SUMM + discountSum;
				POSProcessAX posAx = this.PosAx;
				string clientId = base.ClientId;
				string str1 = returnCheque.ID_CHEQUE_GLOBAL.ToString();
				Guid dCHEQUEGLOBAL = baseCheque.ID_CHEQUE_GLOBAL;
				posAx.MakeReturnRRNCheque(clientId, out num, out str, str1, dCHEQUEGLOBAL.ToString(), pCXQUERYLOG.DATE_RESPONSE, (double)((double)sUMM), (double)((double)sUMM), 0);
				if (num != 0)
				{
					this.OnException(operTypeEnum, str);
				}
				else
				{
					base.LogMsg(operTypeEnum, str);
					GMTransactionData gMTransactionDatum = new GMTransactionData(baseCheque.ID_CHEQUE_GLOBAL, guid, this.PosAx.TransactionID);
					base.SaveTransaction(operTypeEnum, discountSum, gMTransactionDatum);
					BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
				}
				decimal num1 = decimal.Parse(this.PosAx.ChargedBonus, NumberStyles.Any, CultureInfo.InvariantCulture);
				decimal num2 = decimal.Parse(this.PosAx.DiscountedBonus, NumberStyles.Any, CultureInfo.InvariantCulture);
				base.SavePcxCheque(returnCheque, discountSum, (_pcxOperation1 == pcxOperation.RefundDebit ? "DEBIT_REFUND" : "CHARGE_REFUND"), this.PosAx.TransactionID);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append('\"').Append(this.Name).Append('\"').AppendLine();
				if (num1 > new decimal(0))
				{
					stringBuilder.AppendLine("Возврат списания");
					stringBuilder.Append("Начислено: ").Append(Math.Abs(num1)).AppendLine();
				}
				if (num2 > new decimal(0))
				{
					stringBuilder.AppendLine("Возврат начисления");
					stringBuilder.Append("Списано: ").Append(Math.Abs(num2)).AppendLine();
				}
				stringBuilder.Append("Баланс: ").Append(this.PosAx.Balance).AppendLine();
				stringBuilder.AppendLine(" ");
				stringBuilder.AppendLine(" ");
				stringBuilder.ToString();
				decimal num3 = Math.Abs(num1);
				decimal num4 = Math.Abs(num2);
				decimal num5 = new decimal(0);
				if (!decimal.TryParse(this.PosAx.Balance, out num5))
				{
					base.LogMsg(string.Format("Не удалось привести строку {0} к decimal", this.PosAx.Balance));
				}
				result = new LpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num3, num4, num5, string.Empty, true, true);
				return;
			}
			throw new LoyaltyException(this, "Невозможно выполнить возврат бонусов при оплате отличной от Наличных или Картой");
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			decimal num = returnCheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => ci.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE != "LP_GM")
				{
					return new decimal(0);
				}
				return mi.AMOUNT;
			}));
			this.DoRefund(baseCheque, returnCheque, num, out result);
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			decimal num = returnCheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => ci.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE != "LP_GM")
				{
					return new decimal(0);
				}
				return mi.AMOUNT;
			}));
			this.DoRefund(baseCheque, returnCheque, num, out result);
		}

		protected override void DoRollback(out string slipCheque)
		{
			LoyaltyTransaction loyaltyTransaction;
			int num;
			string str;
			if (!base.IsTransactionProcessing)
			{
				ARMLogger.Error("Транзакция не была начата, откат невозможен!");
				slipCheque = string.Empty;
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			while (base.PopLastTransaction(out loyaltyTransaction))
			{
				base.LogMsg(loyaltyTransaction.Operation, "Откат транзакции");
				GMTransactionData data = loyaltyTransaction.Data as GMTransactionData;
				POSProcessAX posAx = this.PosAx;
				string clientId = base.ClientId;
				Guid guid = Guid.NewGuid();
				posAx.MakeChequeCancel(clientId, out num, out str, guid.ToString(), data.TransactionID);
				if (num != 0)
				{
					throw new LoyaltyRollbackException(str);
				}
				string str1 = string.Format("TRANSACTION_ID = '{0}'", data.TransactionID);
				BaseLoyaltyProgramEx.PCXChequeItemLoader.Delete(str1);
				BaseLoyaltyProgramEx.PCXChequeLoader.Delete(str1);
				ChequeTransactionEvent chequeTransactionEvent = new ChequeTransactionEvent(data.ChequeID, string.Empty, data.ChequeOperationType);
				BusinessLogicEvents.Instance.OnRollbackChequeTransaction(this, chequeTransactionEvent);
				stringBuilder.Append(this.GetRollbackSlipCheque(loyaltyTransaction)).AppendLine();
				base.LogMsg(loyaltyTransaction.Operation, "Откат транзакции завершён");
			}
			slipCheque = stringBuilder.ToString();
			base.Commit();
		}

		private void FillCheque(CHEQUE cheque)
		{
			string empty;
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			IChequeItems items = this.PosAx.Items as IChequeItems;
			items.Clear();
			Dictionary<long, string> goodsGroups = GoldenMiddleLoyaltyProgram._bl.GetGoodsGroups(
				from ci in cheque.CHEQUE_ITEMS
				select ci.ID_GOODS);
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				decimal chequeItemSideDiscountSum = base.GetChequeItemSideDiscountSum(cHEQUEITEM);
				decimal sUMM = (chequeItemSideDiscountSum / (cHEQUEITEM.SUMM + cHEQUEITEM.SUMM_DISCOUNT)) * new decimal(100);
				decimal discountSum = base.GetDiscountSum(cHEQUEITEM);
				IChequeItem gOODSNAME = items.AddItem() as IChequeItem;
				if (!goodsGroups.TryGetValue(cHEQUEITEM.ID_GOODS, out empty))
				{
					empty = string.Empty;
					LoyaltyLogger.Info(string.Format("Для товара {0} не найдена группа Золотой Середины", cHEQUEITEM.GOODS_NAME));
				}
				gOODSNAME.Article = empty;
				gOODSNAME.ArticleName = cHEQUEITEM.GOODS_NAME;
				gOODSNAME.Price = (double)((double)cHEQUEITEM.PRICE);
				gOODSNAME.Quantity = (double)((double)cHEQUEITEM.QUANTITY);
				gOODSNAME.Summ = (double)((double)(cHEQUEITEM.SUMM + discountSum));
			}
		}

		protected string GetRollbackSlipCheque(LoyaltyTransaction transaction)
		{
			StringBuilder stringBuilder = new StringBuilder();
			switch (transaction.Operation)
			{
				case OperTypeEnum.Debit:
				{
					stringBuilder.AppendLine(string.Format("Отмена списания с карты {0}", this.Name));
					break;
				}
				case OperTypeEnum.Charge:
				{
					stringBuilder.AppendLine(string.Format("Отмена начисления на карту {0}", this.Name));
					break;
				}
				default:
				{
					stringBuilder.AppendLine("Отмена операции");
					break;
				}
			}
			stringBuilder.AppendLine("Номер карты:").AppendLine(base.ClientPublicId);
			StringBuilder stringBuilder1 = stringBuilder.AppendLine("Дата/время:");
			DateTime now = DateTime.Now;
			stringBuilder1.AppendLine(now.ToString("dd.MM.yy HH:mm:ss"));
			StringBuilder stringBuilder2 = stringBuilder.Append("Сумма операции: ");
			decimal operationSum = transaction.OperationSum;
			stringBuilder2.Append(operationSum.ToString("N2")).AppendLine();
			return stringBuilder.ToString();
		}

		private void OnException(OperTypeEnum operType, string errorMessage)
		{
			errorMessage = string.Concat("Сообщение от Золотой Середины: ", errorMessage);
			base.LogError(operType, errorMessage);
			throw new LoyaltyException(this, errorMessage);
		}

		protected override void OnInitInternal()
		{
			if (GoldenMiddleLoyaltyProgram._posAx == null)
			{
				GoldenMiddleLoyaltyProgram._posAx = new POSProcessAXClass();
				GoldenMiddleLoyaltyProgram._posAx.SetIniFile(GoldenMiddleLoyaltyProgram.Settings.IniFileName, true);
			}
		}

		protected override void OnInitSettings()
		{
			if (GoldenMiddleLoyaltyProgram.Settings == null)
			{
				SettingsModel settingsModel = new SettingsModel();
				LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
				GoldenMiddleLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.GoldenMiddle.Settings>(loyaltySetting.SETTINGS, "Settings");
				GoldenMiddleLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
				if (GoldenMiddleLoyaltyProgram.IscompatibilityEnabled)
				{
					GoldenMiddleLoyaltyProgram.ExcludedPrograms.Add(this.IdGlobal, null);
					foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
					{
						GoldenMiddleLoyaltyProgram.ExcludedPrograms.Add(excludeList.Guid, excludeList);
					}
					foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
					{
						GoldenMiddleLoyaltyProgram.ExcludedPrograms.Add(dataRowItem.Guid, dataRowItem);
					}
					foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
					{
						GoldenMiddleLoyaltyProgram.ExcludedPrograms.Add(excludeList1.Guid, excludeList1);
					}
				}
			}
		}

		private void SaveQueryLog(Guid idChequeGlobal, DateTime responseDate, decimal sum, pcxOperation operType, string transactionID)
		{
			PCX_QUERY_LOG pCXQUERYLOG = new PCX_QUERY_LOG()
			{
				ID_USER_GLOBAL = SecurityContextEx.USER_GUID,
				ID_QUERY_GLOBAL = Guid.NewGuid(),
				STATE = 4,
				ID_CASH_REGISTER = AppConfigManager.IdCashRegister,
				DATE_REQUEST = DateTime.Now,
				DATE_RESPONSE = responseDate,
				ID_CHEQUE_GLOBAL = idChequeGlobal,
				SUMM = sum,
				TRANSACTION_ID = transactionID,
				TYPE = (int)operType,
				CLIENT_ID_TYPE = (int)base.LoyaltyType,
				CLIENT_ID = base.ClientId
			};
			BaseLoyaltyProgramEx.QueryLogBl.Save(pCXQUERYLOG);
		}
	}
}