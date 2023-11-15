using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon.Config;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
using ePlus.Loyalty.Domestic;
using ePlus.MetaData.Client;
using ePlus.MetaData.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;

namespace ePlus.ARMCasher.Loyalty.Domestic
{
	internal class DomesticLoyaltyProgram : BaseLoyaltyProgramEx
	{
		private static bool _isInitiliazed;

		private static Dictionary<Guid, DataRowItem> _excludedPrograms;

		private static string _name;

		private static Guid _idGlobal;

		private PCX_CHEQUE_BL _pcxChequeBl = new PCX_CHEQUE_BL();

		private PCX_QUERY_LOG_BL _pcxQueryLogBl = new PCX_QUERY_LOG_BL();

		private PCX_CHEQUE_ITEM_BL _pcxChequeItemBl = new PCX_CHEQUE_ITEM_BL();

		private bool _isTransaction;

		private Stack<Guid> _transactions = new Stack<Guid>();

		private LoyaltyDomesticWebApi _api;

		public override Guid IdGlobal
		{
			get
			{
				return DomesticLoyaltyProgram._idGlobal;
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
				return DomesticLoyaltyProgram._name;
			}
		}

		private static Params Parameters
		{
			get;
			set;
		}

		private static ePlus.Loyalty.Domestic.Settings Settings
		{
			get;
			set;
		}

		static DomesticLoyaltyProgram()
		{
			DomesticLoyaltyProgram._excludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public DomesticLoyaltyProgram(string PublicId) : base(ePlus.Loyalty.LoyaltyType.Domestic, PublicId, PublicId, "LP_DOMESTIC")
		{
		}

		public new void BeginTransaction()
		{
			this._isTransaction = true;
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			decimal sUMM = cheque.SUMM + base.GetDiscountSum(cheque);
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(false);
			decimal num = sUMM - (DomesticLoyaltyProgram.Parameters == null || !DomesticLoyaltyProgram.Parameters.ApplyDiscountAsPrepayment ? 1 : 0);
			if (DomesticLoyaltyProgram.Parameters.MinPayPercent > new decimal(0))
			{
				num = Math.Truncate(Math.Min(sUMM - (sUMM * (DomesticLoyaltyProgram.Parameters.MinPayPercent / new decimal(100))), num));
			}
			if (sUMM < DomesticLoyaltyProgram.Parameters.MinChequeSumForCharge)
			{
				num = new decimal(0);
			}
			return Math.Min(num, loyaltyCardInfo.Balance);
		}

		private void ClearTransations()
		{
			if (!this._isTransaction)
			{
				this._transactions.Clear();
			}
		}

		public new void Commit()
		{
			this._isTransaction = false;
			this._transactions.Clear();
		}

		private Cheque CreateCheque(CHEQUE cheque, decimal discountSum, OperationType operationType)
		{
			Cheque cheque1 = new Cheque()
			{
				OperationType = operationType,
				ChequeId = cheque.ID_CHEQUE_GLOBAL,
				DiscountSum = discountSum
			};
			Cheque cheque2 = cheque1;
			decimal sUMM = cheque.SUMM;
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				decimal num = cHEQUEITEM.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM discountItem) => {
					if (discountItem.TYPE != "LP_DOMESTIC")
					{
						return new decimal(0);
					}
					return discountItem.AMOUNT;
				});
				sUMM += num;
				ChequeItem chequeItem = new ChequeItem()
				{
					Id = cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL,
					LotId = cHEQUEITEM.ID_LOT_GLOBAL,
					Quantity = cHEQUEITEM.QUANTITY,
					Sum = cHEQUEITEM.SUMM,
					DiscountSum = num
				};
				cheque2.Items.Add(chequeItem);
			}
			cheque2.Sum = sUMM;
			return cheque2;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			if ((cheque.SUMM + discountSum) < DomesticLoyaltyProgram.Parameters.MinChequeSumForCharge)
			{
				return;
			}
			this.ClearTransations();
			Cheque cheque1 = this.CreateCheque(cheque, discountSum, OperationType.Charge);
			ApiChargeRequest apiChargeRequest = new ApiChargeRequest(cheque1, base.ClientId);
			ApiChargeResponce apiChargeResponce = this._api.Charge(apiChargeRequest);
			if (apiChargeResponce.ChargedPoints > new decimal(0))
			{
				this._transactions.Push(apiChargeResponce.TransactionId);
				this.SavePcxCheque(cheque, discountSum, apiChargeResponce.ChargedPoints / DomesticLoyaltyProgram.Parameters.PointsPerRub, OperationType.Charge, apiChargeResponce.TransactionId);
				LpTransResult lpTransResult = new LpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, apiChargeResponce.ChargedPoints, new decimal(0), apiChargeResponce.Balance, "баллов", false, true)
				{
					RoundingInCheque = DomesticLoyaltyProgram.Parameters.RoundingInCheque
				};
				result = lpTransResult;
			}
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			result = null;
			this.ClearTransations();
			if (discountSum == new decimal(0))
			{
				return;
			}
			Cheque cheque1 = this.CreateCheque(cheque, discountSum, OperationType.Debit);
			ApiDebetRequest apiDebetRequest = new ApiDebetRequest(cheque1, base.ClientId);
			ApiDebetResponce apiDebetResponce = this._api.Debet(apiDebetRequest);
			if (apiDebetResponce.DebitPoints > new decimal(0))
			{
				this._transactions.Push(apiDebetResponce.TransactionId);
				this.SavePcxCheque(cheque, discountSum, new decimal(0), OperationType.Debit, apiDebetResponce.TransactionId);
				LpTransResult lpTransResult = new LpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, new decimal(0), apiDebetResponce.DebitPoints, apiDebetResponce.Balance, "баллов", false, true)
				{
					RoundingInCheque = DomesticLoyaltyProgram.Parameters.RoundingInCheque
				};
				result = lpTransResult;
			}
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			Card clientCardInfo = this._api.GetClientCardInfo(base.ClientId);
			LoyaltyCardInfo loyaltyCardInfo = new LoyaltyCardInfo()
			{
				ClientId = base.ClientId,
				CardStatusId = clientCardInfo.CARD_STATUS,
				CardStatus = SettingsModel.GetLoyaltyCardStatusName(loyaltyCardInfo.CardStatusId),
				CardNumber = loyaltyCardInfo.ClientId,
				Points = clientCardInfo.BALANCE,
				Balance = clientCardInfo.BALANCE,
				TransactionsCountInDay = clientCardInfo.TRANSACTIONS_COUNT_IN_DAY,
				TransactionsCountInMonth = clientCardInfo.TRANSACTIONS_COUNT_IN_MONTH
			};
			return loyaltyCardInfo;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			return true;
		}

		private ILpTransResult DoRefund(CHEQUE baseCheque, CHEQUE returnCheque, OperationType operationType)
		{
			this.ClearTransations();
			ApiRefundRequest apiRefundRequest = new ApiRefundRequest(base.ClientId, returnCheque.ID_CHEQUE_GLOBAL, baseCheque.ID_CHEQUE_GLOBAL, operationType);
			ApiRefundResponce apiRefundResponce = this._api.Refund(apiRefundRequest);
			if (apiRefundResponce.IsCanceled)
			{
				return null;
			}
			this._transactions.Push(apiRefundResponce.TransactionId);
			this.CreateCheque(baseCheque, new decimal(0), operationType);
			decimal num = (operationType == OperationType.RefundCharge ? apiRefundResponce.RefundPoints / DomesticLoyaltyProgram.Parameters.PointsPerRub : new decimal(0));
			this.SavePcxCheque(returnCheque, (operationType == OperationType.RefundDebit ? apiRefundResponce.RefundPoints : new decimal(0)), num, operationType, apiRefundResponce.TransactionId);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Отмена ").Append((operationType == OperationType.RefundCharge ? "начисления " : "списания ")).AppendLine(DomesticLoyaltyProgram._name);
			stringBuilder.Append("ШК карты: ").AppendLine(base.ClientPublicId);
			StringBuilder stringBuilder1 = stringBuilder.Append("Дата/время: ");
			DateTime now = DateTime.Now;
			stringBuilder1.AppendLine(now.ToString("dd.MM.yy HH:mm:ss"));
			StringBuilder stringBuilder2 = stringBuilder.AppendFormat((operationType == OperationType.RefundCharge ? "Списано: " : "Начислено "), new object[0]);
			decimal refundPoints = apiRefundResponce.RefundPoints;
			stringBuilder2.Append(refundPoints.ToString("N2")).AppendLine(" баллов");
			StringBuilder stringBuilder3 = stringBuilder.AppendFormat("Баланс: ", new object[0]);
			decimal balance = apiRefundResponce.Balance;
			stringBuilder3.Append(balance.ToString("N2")).AppendLine(" баллов");
			decimal num1 = (operationType == OperationType.RefundCharge ? new decimal(0) : apiRefundResponce.RefundPoints);
			decimal num2 = (operationType == OperationType.RefundCharge ? apiRefundResponce.RefundPoints : new decimal(0));
			LpTransResult lpTransResult = new LpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num1, num2, apiRefundResponce.Balance, " баллов", true, true)
			{
				RoundingInCheque = DomesticLoyaltyProgram.Parameters.RoundingInCheque
			};
			return lpTransResult;
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = this.DoRefund(baseCheque, returnCheque, OperationType.RefundCharge);
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = this.DoRefund(baseCheque, returnCheque, OperationType.RefundDebit);
		}

		protected override void DoRollback(out string slipCheque)
		{
			StringBuilder stringBuilder = new StringBuilder();
			while (this._transactions.Count > 0)
			{
				Guid guid = this._transactions.Pop();
				ApiRollbackResponce apiRollbackResponce = this._api.Rollback(guid);
				if (stringBuilder.Length > 0)
				{
					stringBuilder.AppendLine();
				}
				stringBuilder.Append("Отмена транзакции ").AppendLine(this.Name);
				stringBuilder.Append("ШК карты: ").AppendLine(base.ClientPublicId);
				StringBuilder stringBuilder1 = stringBuilder.Append("Баланс карты: ");
				decimal balance = apiRollbackResponce.Balance;
				stringBuilder1.AppendLine(balance.ToString("N2"));
				string str = string.Format("TRANSACTION_ID = '{0}'", guid);
				BaseLoyaltyProgramEx.PCXChequeItemLoader.Delete(str, ServerType.Local);
				BaseLoyaltyProgramEx.PCXChequeLoader.Delete(str, ServerType.Local);
				(new PCX_QUERY_LOG_BL()).ReverseQuery(guid.ToString());
			}
			slipCheque = stringBuilder.ToString();
		}

		public override LoyaltyParams GetLoyaltyParams()
		{
			return DomesticLoyaltyProgram.Parameters;
		}

		protected override void OnInitInternal()
		{
		}

		protected override void OnInitSettings()
		{
			if (!DomesticLoyaltyProgram._isInitiliazed)
			{
				SettingsModel settingsModel = new SettingsModel();
				LoyaltySettings loyaltySetting = settingsModel.Load(ePlus.Loyalty.LoyaltyType.Domestic, Guid.Empty, ServerType.Local);
				DomesticLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.Domestic.Settings>(loyaltySetting.SETTINGS, "Settings");
				DomesticLoyaltyProgram.Parameters = settingsModel.Deserialize<Params>(loyaltySetting.PARAMS, "Params");
				DomesticLoyaltyProgram.Parameters.PointsPerRub = (DomesticLoyaltyProgram.Parameters.PointsPerRub == new decimal(0) ? new decimal(1) : DomesticLoyaltyProgram.Parameters.PointsPerRub);
				DomesticLoyaltyProgram._name = DomesticLoyaltyProgram.Settings.Name;
				DomesticLoyaltyProgram._idGlobal = loyaltySetting.ID_LOYALITY_PROGRAM_GLOBAL;
			}
			DomesticLoyaltyProgram._isInitiliazed = true;
			if (this._api == null)
			{
				this._api = new LoyaltyDomesticWebApi(DomesticLoyaltyProgram.Settings, SecurityContextEx.Context.User.Password_hash);
			}
		}

		private void SavePcxCheque(CHEQUE cheque, decimal discountSum, decimal chargedSum, OperationType operationType, Guid transactionId)
		{
			decimal num1;
			List<PrepaymentInfo> prepaymentInfoList = cheque.GetPrepaymentInfoList();
			bool flag1 = (DomesticLoyaltyProgram.Parameters == null ? false : DomesticLoyaltyProgram.Parameters.ApplyDiscountAsPrepayment);
			discountSum = (flag1 ? prepaymentInfoList.Where<PrepaymentInfo>((PrepaymentInfo item) => {
				int num = 1;
				bool flag = (bool)num;
				item.ApplyDiscountAsPrepayment = (bool)num;
				return flag;
			}).Sum<PrepaymentInfo>((PrepaymentInfo item) => item.MaxSumm) : cheque.CHEQUE_ITEMS.Sum<CHEQUE_ITEM>((CHEQUE_ITEM ci) => ci.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM mi) => {
				if (mi.TYPE != "LP_DOMESTIC")
				{
					return new decimal(0);
				}
				return mi.AMOUNT;
			})));
			string empty = string.Empty;
			int num2 = 0;
			decimal num3 = new decimal(0);
			switch (operationType)
			{
				case OperationType.Charge:
				{
					empty = "CHARGE";
					num2 = 3;
					num3 = chargedSum;
					break;
				}
				case OperationType.Debit:
				{
					empty = "DEBIT";
					num2 = 2;
					num3 = discountSum;
					break;
				}
				case OperationType.RefundCharge:
				{
					empty = "CHARGE_REFUND";
					num2 = 5;
					num3 = chargedSum;
					break;
				}
				case OperationType.RefundDebit:
				{
					empty = "DEBIT_REFUND";
					num2 = 4;
					num3 = discountSum;
					break;
				}
			}
			PCX_QUERY_LOG pCXQUERYLOG = new PCX_QUERY_LOG()
			{
				ID_USER_GLOBAL = SecurityContextEx.USER_GUID,
				ID_QUERY_GLOBAL = Guid.NewGuid(),
				STATE = 4,
				ID_CASH_REGISTER = AppConfigManager.IdCashRegister,
				DATE_REQUEST = DateTime.Now,
				ID_CHEQUE_GLOBAL = cheque.ID_CHEQUE_GLOBAL,
				SUMM = num3,
				TYPE = num2,
				CLIENT_ID_TYPE = (int)base.LoyaltyType,
				CLIENT_ID = base.ClientId,
				TRANSACTION_ID = transactionId.ToString()
			};
			PCX_QUERY_LOG pCXQUERYLOG1 = pCXQUERYLOG;
			PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
			{
				CLIENT_ID = base.ClientId,
				CLIENT_ID_TYPE = (int)base.LoyaltyType,
				SUMM = num3,
				SUMM_MONEY = num3,
				SCORE = num3,
				SUMM_SCORE = num3,
				PARTNER_ID = string.Empty,
				LOCATION = string.Empty,
				TERMINAL = string.Empty,
				ID_CHEQUE_GLOBAL = cheque.ID_CHEQUE_GLOBAL,
				OPER_TYPE = empty,
				CARD_NUMBER = base.ClientPublicId,
				TRANSACTION_ID = transactionId.ToString()
			};
			PCX_CHEQUE pCXCHEQUE1 = pCXCHEQUE;
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				num1 = (flag1 ? new decimal(0) : (
					from dmi in cHEQUEITEM.Discount2MakeItemList
					where dmi.TYPE == "LP_DOMESTIC"
					select dmi).Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM dmi) => dmi.AMOUNT));
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, cHEQUEITEM.SUMM + num1);
			}
			decimal sUMM = cheque.SUMM + (flag1 ? new decimal(0) : discountSum);
			IDictionary<Guid, decimal> guids1 = LoyaltyProgManager.Distribute(guids, sUMM, Math.Abs(discountSum), false);
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM1 in cheque.CHEQUE_ITEMS)
			{
				PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
				{
					ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL,
					TRANSACTION_ID = transactionId.ToString(),
					CLIENT_ID = base.ClientId,
					CLIENT_ID_TYPE = (int)base.LoyaltyType,
					SUMM_SCORE = new decimal(0),
					SUMM = guids1[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL],
					STATUS = pcxOperationStatus.Online.ToString(),
					OPER_TYPE = empty
				};
				pCXCHEQUEITEMs.Add(pCXCHEQUEITEM);
			}
			this._pcxQueryLogBl.Save(pCXQUERYLOG1);
			this._pcxChequeBl.Save(pCXCHEQUE1);
			if (pCXCHEQUEITEMs.Count > 0)
			{
				this._pcxChequeItemBl.Save(pCXCHEQUEITEMs);
			}
		}
	}
}