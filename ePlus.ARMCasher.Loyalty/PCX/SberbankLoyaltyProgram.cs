using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher;
using ePlus.ARMCasher.BusinessLogic;
using ePlus.ARMCasher.BusinessLogic.Events;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon;
using ePlus.ARMCommon.Log;
using ePlus.ARMUtils;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Discount2.Server;
using ePlus.Loyalty;
using ePlus.Loyalty.Sber;
using ePlus.MetaData.Client;
using ePlus.MetaData.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using winpcxLib;

namespace ePlus.ARMCasher.Loyalty.PCX
{
	internal sealed class SberbankLoyaltyProgram : PCXLoyaltyProgramEx
	{
		private const int _devider = 100;

		private readonly static Guid _chequeOperTypeCharge;

		private readonly static Guid _chequeOperTypeDebit;

		private readonly static Guid _chequeOperTypeRefundCharge;

		private readonly static Guid _chequeOperTypeRefundDebit;

		private static bool _isSettingsInit;

		private static Guid _idGlobal;

		private static string _name;

		private readonly static Dictionary<Guid, DataRowItem> _excludedPrograms;

		private winpcxAuthResponseData AuthPointsAuthResponse
		{
			get;
			set;
		}

		private static ePlus.Loyalty.Sber.Certificate Certificate
		{
			get;
			set;
		}

		protected override Guid ChequeOperTypeCharge
		{
			get
			{
				return SberbankLoyaltyProgram._chequeOperTypeCharge;
			}
		}

		protected override Guid ChequeOperTypeDebit
		{
			get
			{
				return SberbankLoyaltyProgram._chequeOperTypeDebit;
			}
		}

		protected override Guid ChequeOperTypeRefundCharge
		{
			get
			{
				return SberbankLoyaltyProgram._chequeOperTypeRefundCharge;
			}
		}

		protected override Guid ChequeOperTypeRefundDebit
		{
			get
			{
				return SberbankLoyaltyProgram._chequeOperTypeRefundDebit;
			}
		}

		protected DISCOUNT2_CARD_TYPE DiscountCardType
		{
			get;
			set;
		}

		protected DISCOUNT2_MEMBER DiscountMember
		{
			get;
			set;
		}

		public override Guid IdGlobal
		{
			get
			{
				return SberbankLoyaltyProgram._idGlobal;
			}
		}

		private static bool IscompatibilityEnabled
		{
			get;
			set;
		}

		private static bool IsSettingsInit
		{
			get;
			set;
		}

		protected override decimal MinChequeSumForCharge
		{
			get
			{
				return SberbankLoyaltyProgram.Params.MinChequeSumForCharge;
			}
			set
			{
				SberbankLoyaltyProgram.Params.MinChequeSumForCharge = value;
			}
		}

		protected override decimal MinPayPercent
		{
			get
			{
				return SberbankLoyaltyProgram.Params.MinPayPercent;
			}
			set
			{
				SberbankLoyaltyProgram.Params.MinPayPercent = value;
			}
		}

		public override string Name
		{
			get
			{
				return SberbankLoyaltyProgram._name;
			}
		}

		protected override decimal OfflineChargePercent
		{
			get
			{
				return new decimal(5, 0, 0, false, 1);
			}
		}

		private static ePlus.Loyalty.Sber.Params Params
		{
			get;
			set;
		}

		private static ePlus.Loyalty.Sber.Settings Settings
		{
			get;
			set;
		}

		protected override string UnitName
		{
			get
			{
				return "спасибо";
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		static SberbankLoyaltyProgram()
		{
			SberbankLoyaltyProgram._chequeOperTypeCharge = new Guid("781CD2C7-EFAD-4411-A752-961024ED95E4");
			SberbankLoyaltyProgram._chequeOperTypeDebit = new Guid("C44A8A3E-69FE-49AE-8A6C-62FB569F5D9E");
			SberbankLoyaltyProgram._chequeOperTypeRefundCharge = new Guid("B75C3136-2790-47A5-95AB-698FB41423AF");
			SberbankLoyaltyProgram._chequeOperTypeRefundDebit = new Guid("5955BE0A-118D-4C55-B93F-86FD1739BA7A");
			SberbankLoyaltyProgram._excludedPrograms = new Dictionary<Guid, DataRowItem>();
		}

		public SberbankLoyaltyProgram(string PublicId, string cardNumber) : base(ePlus.Loyalty.LoyaltyType.Sberbank, PublicId, cardNumber, "SBER")
		{
		}

		private int AuthPoints(int amount, decimal discountSum, IEnumerable<winpcxPaymentItem> paymentItemList, CHEQUE cheque, PCX_QUERY_LOG logRecord, OperTypeEnum operType, out winpcxTransaction transaction)
		{
			Guid guid = (operType == OperTypeEnum.Debit ? this.ChequeOperTypeDebit : this.ChequeOperTypeCharge);
			winpcxAuthRequestData str = base.PcxObject.CreateAuthRequest() as winpcxAuthRequestData;
			if (str == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateAuthRequest. Вернулся пустой объект.");
			}
			this.AuthPointsAuthResponse = base.PcxObject.CreateAuthResponse() as winpcxAuthResponseData;
			if (this.AuthPointsAuthResponse == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateAuthResponse. Вернулся пустой объект.");
			}
			str.Amount = amount.ToString();
			str.ClientID = base.ClientId;
			str.ClientIDType = (int)base.LoyaltyType;
			str.Currency = 643.ToString();
			foreach (winpcxPaymentItem _winpcxPaymentItem in paymentItemList)
			{
				str.AddPaymentItem(_winpcxPaymentItem);
			}
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				winpcxChequeItem productCode = base.PcxObject.CreateChequeItem() as winpcxChequeItem;
				if (productCode == null)
				{
					throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				decimal sUMM = cHEQUEITEM.SUMM + (discountSum == new decimal(0) ? new decimal(0) : cHEQUEITEM.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM discountItem) => {
					if (string.IsNullOrEmpty(discountItem.TYPE))
					{
						return new decimal(0);
					}
					if (!discountItem.TYPE.Equals("SBER"))
					{
						return new decimal(0);
					}
					return discountItem.AMOUNT;
				}));
				int num = Convert.ToInt32(sUMM * new decimal(100));
				productCode.Amount = num.ToString();
				productCode.Product = PCXLoyaltyProgramEx.GetProductCode(cHEQUEITEM.CODE);
				productCode.Quantity = Convert.ToDouble(cHEQUEITEM.QUANTITY);
				base.AddChequeItem(str, productCode);
			}
			BaseLoyaltyProgramEx.QueryLogBl.Save(logRecord);
			int num1 = base.PcxObject.AuthPoints(str, this.AuthPointsAuthResponse);
			transaction = this.AuthPointsAuthResponse.Transaction as winpcxTransaction;
			logRecord.DATE_RESPONSE = DateTime.Now;
			if (num1 != 0 && num1 != 1)
			{
				throw new LoyaltyException(this, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num1, base.PcxObject.GetErrorMessage(num1)));
			}
			logRecord.STATE = base.GetResultState(num1);
			if (transaction == null)
			{
				logRecord.TRANSACTION_ID = string.Empty;
				logRecord.TRANSACTION_LOCATION = SberbankLoyaltyProgram.Settings.Location;
				logRecord.TRANSACTION_PARTNER_ID = SberbankLoyaltyProgram.Settings.PartnerId;
				logRecord.TRANSACTION_TERMINAL = SberbankLoyaltyProgram.Settings.Terminal;
			}
			else
			{
				logRecord.TRANSACTION_ID = transaction.ID;
				logRecord.TRANSACTION_LOCATION = transaction.Location;
				logRecord.TRANSACTION_PARTNER_ID = transaction.PartnerID;
				logRecord.TRANSACTION_TERMINAL = transaction.Terminal;
			}
			BaseLoyaltyProgramEx.QueryLogBl.Save(logRecord);
			if ((num1 != 1 || operType != OperTypeEnum.Charge) && num1 != 0)
			{
				foreach (CHEQUE_ITEM cHEQUEITEM1 in cheque.CHEQUE_ITEMS)
				{
					foreach (DISCOUNT2_MAKE_ITEM discount2MakeItemList in cHEQUEITEM1.Discount2MakeItemList)
					{
						if (discount2MakeItemList.TYPE != "SBER")
						{
							continue;
						}
						discount2MakeItemList.AMOUNT = new decimal(0);
						discountSum = new decimal(0);
					}
				}
				throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода AuthPoints\r\n", ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num1, base.PcxObject.GetErrorMessage(num1))));
			}
			PCXTransactionData pCXTransactionDatum = new PCXTransactionData(cheque.ID_CHEQUE_GLOBAL, guid, transaction);
			base.SaveTransaction(operType, discountSum, pCXTransactionDatum);
			decimal num2 = new decimal(0);
			decimal num3 = new decimal(0);
			for (int i = 0; i < this.AuthPointsAuthResponse.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = this.AuthPointsAuthResponse.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				string str1 = name;
				if (name != null)
				{
					if (str1 != "AB")
					{
						if (str1 == "BNS")
						{
							if (cardInfoItemAt.Type == "C")
							{
								num3 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
							}
						}
					}
					else if (cardInfoItemAt.Type == "C")
					{
						num2 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
					}
				}
			}
			transaction = (winpcxTransaction)this.AuthPointsAuthResponse.Transaction;
			this.CreateAndSavePCXChequeItemList(cheque.CHEQUE_ITEMS, cheque.SUMM + discountSum, num2, num3, transaction.ID);
			return num1;
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			decimal sUMM = cheque.SUMM + base.GetDiscountSum(cheque);
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(false);
			decimal num = sUMM--;
			if (this.MinPayPercent > new decimal(0))
			{
				num = Math.Truncate(Math.Min(sUMM - (sUMM * (this.MinPayPercent / new decimal(100))), num));
			}
			if (sUMM < SberbankLoyaltyProgram.Params.MinChequeSumForCharge)
			{
				num = new decimal(0);
			}
			return Math.Min(num, loyaltyCardInfo.Balance);
		}

		protected decimal CalculateSumTotalPCX(Dictionary<Guid, CHEQUE_ITEM> returnedChequeItemList)
		{
			decimal num = new decimal(0);
			foreach (CHEQUE_ITEM value in returnedChequeItemList.Values)
			{
				decimal aMOUNT = new decimal(0);
				foreach (DISCOUNT2_MAKE_ITEM discount2MakeItemList in value.Discount2MakeItemList)
				{
					if (discount2MakeItemList.TYPE != "SBER")
					{
						continue;
					}
					aMOUNT = discount2MakeItemList.AMOUNT;
				}
				num += aMOUNT;
			}
			return num;
		}

		private void CreateAndSavePCXChequeItemList(IEnumerable<CHEQUE_ITEM> chequeItemList, decimal totalSum, decimal pcxSumMoney, decimal pcxSumScore, string transactionId)
		{
			List<CHEQUE_ITEM> list = chequeItemList.ToList<CHEQUE_ITEM>();
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			foreach (CHEQUE_ITEM cHEQUEITEM in list)
			{
				decimal sUMM = cHEQUEITEM.SUMM + (
					from dmi in cHEQUEITEM.Discount2MakeItemList
					where dmi.TYPE == "SBER"
					select dmi).Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM dmi) => dmi.AMOUNT);
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, sUMM);
			}
			if (Math.Abs(pcxSumMoney) > new decimal(0) && Math.Abs(pcxSumMoney) < totalSum && Math.Abs(pcxSumScore) > new decimal(0))
			{
				IDictionary<Guid, decimal> guids1 = LoyaltyProgManager.Distribute(guids, totalSum, Math.Abs(pcxSumMoney), true);
				IDictionary<Guid, decimal> guids2 = LoyaltyProgManager.Distribute(guids, totalSum, Math.Abs(pcxSumScore), true);
				List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
				foreach (CHEQUE_ITEM cHEQUEITEM1 in list)
				{
					PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
					{
						TRANSACTION_ID = transactionId,
						CLIENT_ID = base.ClientId,
						CLIENT_ID_TYPE = (int)base.LoyaltyType
					};
					PCX_CHEQUE_ITEM item = pCXCHEQUEITEM;
					pCXCHEQUEITEMs.Add(item);
					item.SUMM_SCORE = guids2[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL];
					item.SUMM = guids1[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL];
					item.ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL;
					item.OPER_TYPE = PCX_CHEQUE_ITEM.operTypeArr[(int)((pcxSumMoney >= new decimal(0) ? OperTypeEnum.Charge : OperTypeEnum.Debit))];
				}
				if (pCXCHEQUEITEMs.Count > 0)
				{
					(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
				}
			}
		}

		private static PCX_CHEQUE_ITEM CreatePcxItem(CHEQUE_ITEM item)
		{
			PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM();
			decimal num = new decimal(0);
			foreach (DISCOUNT2_MAKE_ITEM dISCOUNT2MAKEITEM in 
				from dmi in item.Discount2MakeItemList
				where dmi.TYPE == "SBER"
				select dmi)
			{
				num = dISCOUNT2MAKEITEM.AMOUNT;
			}
			pCXCHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL = item.ID_CHEQUE_ITEM_GLOBAL;
			pCXCHEQUEITEM.QUANTITY = item.QUANTITY;
			pCXCHEQUEITEM.PRICE = UtilsArm.Round(num / item.QUANTITY);
			pCXCHEQUEITEM.SUMM = UtilsArm.RoundDown(num);
			pCXCHEQUEITEM.SUMM_SCORE = pCXCHEQUEITEM.SUMM * new decimal(100);
			return pCXCHEQUEITEM;
		}

		private winpcxPaymentItem CreatePcxPayment(decimal requestAmount)
		{
			winpcxPaymentItem str = base.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
			if (str == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
			}
			str.PayMeans = "P";
			str.Amount = requestAmount.ToString();
			return str;
		}

		protected new void CreateRefundDebitCheque(out string slipCheque, int res, PCX_CHEQUE pcxCheque, winpcxTransaction transaction, DateTime dateRequest, decimal discountSum, decimal scoreDeltaBalance, decimal sumTotalPCX)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (this.DiscountMember != null)
			{
				DISCOUNT2_MEMBER_BL dISCOUNT2MEMBERBL = new DISCOUNT2_MEMBER_BL(MultiServerBL.ServerConnectionString);
				dISCOUNT2MEMBERBL.Save(this.DiscountMember);
			}
			stringBuilder.AppendFormat("НОМЕР КАРТЫ: {0}", PCXUtils.GetCardNumberMasked(base.ClientPublicId)).AppendLine();
			if (res != 1)
			{
				stringBuilder.AppendFormat("Начислено {0} СПАСИБО", PCXUtils.TruncateNonZero(Math.Abs(scoreDeltaBalance) / new decimal(100)));
			}
			else
			{
				stringBuilder.AppendFormat("К начислению {0} СПАСИБО", PCXUtils.TruncateNonZero(Utils.GetDecimal(sumTotalPCX)));
			}
			slipCheque = stringBuilder.ToString();
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			winpcxTransaction _winpcxTransaction;
			result = null;
			base.Log(OperTypeEnum.Charge, discountSum, cheque, null, null);
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeCharge);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				return;
			}
			int num = (int)(discountSum * new decimal(100));
			int sUMCASH = (int)(cheque.SUM_CASH * new decimal(100));
			int sUMCARD = (int)(cheque.SUM_CARD * new decimal(100));
			if (sUMCARD <= 0)
			{
				return;
			}
			int num1 = num + sUMCASH + sUMCARD;
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE pCXCHEQUE = base.CreatePCXCheque(cheque.ID_CHEQUE_GLOBAL, cheque.SUM_CASH + cheque.SUM_CARD, num1, "CHARGE", new decimal(0));
			List<winpcxPaymentItem> paymentListForCahrge = base.GetPaymentListForCahrge((long)num, (long)sUMCASH, (long)sUMCARD);
			PCX_QUERY_LOG pCXQUERYLOG = base.CreateLogQueryLog(cheque.ID_CHEQUE_GLOBAL, num + sUMCASH + sUMCARD, 3);
			int num2 = this.AuthPoints(num1, discountSum, paymentListForCahrge, cheque, pCXQUERYLOG, OperTypeEnum.Charge, out _winpcxTransaction);
			base.Log(OperTypeEnum.Charge, discountSum, cheque, new int?(num2), _winpcxTransaction.ID);
			pCXCHEQUEBL.Save(pCXCHEQUE);
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			try
			{
				try
				{
					if (_winpcxTransaction != null)
					{
						base.FillLocationInformation(pCXCHEQUE, _winpcxTransaction.Location, _winpcxTransaction.Terminal, _winpcxTransaction.PartnerID);
					}
					decimal num3 = new decimal(0);
					decimal num4 = new decimal(0);
					if (num2 != 1)
					{
						for (int i = 0; i < this.AuthPointsAuthResponse.GetCardInfoItemCount(); i++)
						{
							winpcxCardInfoItem cardInfoItemAt = this.AuthPointsAuthResponse.GetCardInfoItemAt(i) as winpcxCardInfoItem;
							string name = cardInfoItemAt.Name;
							if (name != null && name == "BNS")
							{
								if (cardInfoItemAt.Type == "C")
								{
									num3 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
								}
								else if (cardInfoItemAt.Type == "S")
								{
									num4 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
								}
							}
						}
						pCXCHEQUE.CARD_SCORE = num4;
						pCXCHEQUE.SCORE = Math.Abs(num3);
					}
					else
					{
						decimal num5 = base.GetDiscountSum(cheque);
						decimal sUMM = (cheque.SUMM + num5) * (this.OfflineChargePercent / new decimal(100));
						pCXCHEQUE.SCORE = Math.Abs(sUMM);
					}
					result = new PcxLpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, Math.Abs(num3), new decimal(0), num4, this.UnitName);
				}
				catch (Exception exception)
				{
					base.LogError(OperTypeEnum.Charge, exception);
					throw;
				}
			}
			finally
			{
				pCXCHEQUEBL.Save(pCXCHEQUE);
			}
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			winpcxTransaction _winpcxTransaction;
			result = null;
			base.Log(OperTypeEnum.Debit, discountSum, cheque, null, null);
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeDebit);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				return;
			}
			if (discountSum > new decimal(0))
			{
				int num = (int)(discountSum * new decimal(100));
				PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
				PCX_CHEQUE pCXCHEQUE = base.CreatePCXCheque(cheque.ID_CHEQUE_GLOBAL, new decimal(0), num, "DEBIT", discountSum);
				cheque.PcxCheque = pCXCHEQUE;
				winpcxPaymentItem str = base.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
				if (str == null)
				{
					throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				str.PayMeans = "P";
				str.Amount = num.ToString();
				List<winpcxPaymentItem> winpcxPaymentItems = new List<winpcxPaymentItem>()
				{
					str
				};
				PCX_QUERY_LOG pCXQUERYLOG = base.CreateLogQueryLog(cheque.ID_CHEQUE_GLOBAL, discountSum * new decimal(100), 2);
				int num1 = this.AuthPoints(num, discountSum, winpcxPaymentItems, cheque, pCXQUERYLOG, OperTypeEnum.Debit, out _winpcxTransaction);
				base.Log(OperTypeEnum.Debit, discountSum, cheque, new int?(num1), _winpcxTransaction.ID);
				pCXCHEQUEBL.Save(pCXCHEQUE);
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
				try
				{
					try
					{
						if (_winpcxTransaction != null)
						{
							base.FillLocationInformation(pCXCHEQUE, _winpcxTransaction.Location, _winpcxTransaction.Terminal, _winpcxTransaction.PartnerID);
							decimal num2 = new decimal(0);
							decimal num3 = new decimal(0);
							for (int i = 0; i < this.AuthPointsAuthResponse.GetCardInfoItemCount(); i++)
							{
								winpcxCardInfoItem cardInfoItemAt = this.AuthPointsAuthResponse.GetCardInfoItemAt(i) as winpcxCardInfoItem;
								string name = cardInfoItemAt.Name;
								if (name != null && name == "BNS")
								{
									if (cardInfoItemAt.Type == "C")
									{
										num3 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
									}
									else if (cardInfoItemAt.Type == "S")
									{
										num2 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
									}
								}
							}
							pCXCHEQUE.CARD_SCORE = num2;
							pCXCHEQUE.SCORE = Math.Abs(num3);
							result = new PcxLpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, new decimal(0), Math.Abs(num3), num2, this.UnitName);
						}
					}
					catch (Exception exception)
					{
						ARMLogger.Error(exception.ToString());
						throw;
					}
				}
				finally
				{
					pCXCHEQUEBL.Save(pCXCHEQUE);
				}
			}
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			LoyaltyCardInfo num = new LoyaltyCardInfo()
			{
				ClientId = base.ClientId,
				CardNumber = base.ClientPublicId,
				ClientIdType = PublicIdType.CardNumber
			};
			object obj = base.PcxObject.CreateAuthResponse();
			if (obj == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateAuthResponse. Вернулся пустой объект.");
			}
			int info = base.PcxObject.GetInfo(base.ClientId, (int)base.LoyaltyType, obj);
			if (info != 0)
			{
				throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода CreateAuthResponse\r\n", ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(info, base.PcxObject.GetErrorMessage(info))));
			}
			winpcxAuthResponseData winpcxAuthResponseDatum = obj as winpcxAuthResponseData;
			int cardInfoItemCount = winpcxAuthResponseDatum.GetCardInfoItemCount();
			for (int i = 0; i < cardInfoItemCount; i++)
			{
				winpcxCardInfoItem cardInfoItemAt = winpcxAuthResponseDatum.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				string str = name;
				if (name != null)
				{
					if (str == "BNS")
					{
						num.Points = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
					}
					else if (str == "AB")
					{
						num.Balance = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
					}
					else if (str == "CS")
					{
						string value = cardInfoItemAt.Value;
						string str1 = value;
						if (value != null)
						{
							if (str1 == "A")
							{
								num.CardStatus = "Активна";
								num.CardStatusId = LoyaltyCardStatus.Active;
								goto Label0;
							}
							else
							{
								if (str1 != "R")
								{
									goto Label2;
								}
								num.CardStatus = "Ограничена";
								num.CardStatusId = LoyaltyCardStatus.Limited;
								goto Label0;
							}
						}
					Label2:
						num.CardStatus = "Заблокирована";
						num.CardStatusId = LoyaltyCardStatus.Blocked;
					}
				}
			Label0:
			}
			return num;
		}

		protected override bool DoIsCompatibleTo(Guid discountId)
		{
			if (!SberbankLoyaltyProgram.IscompatibilityEnabled)
			{
				return false;
			}
			return !SberbankLoyaltyProgram._excludedPrograms.ContainsKey(discountId);
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			decimal num;
			decimal num1;
			bool flag;
			result = null;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(baseCheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeRefundCharge);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				return;
			}
			Guid dCHEQUEGLOBAL = baseCheque.ID_CHEQUE_GLOBAL;
			Guid guid = returnCheque.ID_CHEQUE_GLOBAL;
			PaymentType cHEQUEPAYMENTTYPE = baseCheque.CHEQUE_PAYMENT_TYPE;
			Dictionary<Guid, CHEQUE_ITEM> guids = new Dictionary<Guid, CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM in returnCheque.CHEQUE_ITEMS)
			{
				CHEQUE_ITEM cHEQUEITEM1 = (CHEQUE_ITEM)cHEQUEITEM.Clone();
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, cHEQUEITEM1);
			}
			string str = null;
			if (cHEQUEPAYMENTTYPE == PaymentType.Card || cHEQUEPAYMENTTYPE == PaymentType.Cash)
			{
				flag = true;
			}
			else
			{
				flag = (cHEQUEPAYMENTTYPE != PaymentType.Mixed ? false : baseCheque.CHEQUE_PAYMENTS.All<CHEQUE_PAYMENT>((CHEQUE_PAYMENT p) => {
					if (p.SEPARATE_TYPE_ENUM == PaymentType.Card)
					{
						return true;
					}
					return p.SEPARATE_TYPE_ENUM == PaymentType.Cash;
				}));
			}
			if (!flag)
			{
				UtilsArm.ShowMessageErrorOK("Невозможен возврат бонусов Спасибо при оплате отличной от Наличных или Картой");
			}
			PCX_QUERY_LOG_BL pCXQUERYLOGBL = new PCX_QUERY_LOG_BL();
			PCX_QUERY_LOG pCXQUERYLOG = pCXQUERYLOGBL.Load(dCHEQUEGLOBAL, pcxOperation.Charge, (int)base.LoyaltyType);
			if (pCXQUERYLOG == null)
			{
				return;
			}
			str = null;
			winpcxRefundRequest clientId = base.PcxObject.CreateRefundRequest() as winpcxRefundRequest;
			if (clientId == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundRequest. Вернулся пустой объект.");
			}
			winpcxRefundResponse _winpcxRefundResponse = base.PcxObject.CreateRefundResponse() as winpcxRefundResponse;
			if (_winpcxRefundResponse == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundResponse. Вернулся пустой объект.");
			}
			Dictionary<Guid, decimal> guids1 = new Dictionary<Guid, decimal>();
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			decimal num2 = guids.Values.Sum<CHEQUE_ITEM>((CHEQUE_ITEM item) => item.SUMM);
			decimal aMOUNT = new decimal(0);
			foreach (CHEQUE_ITEM value in guids.Values)
			{
				PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM();
				decimal sUMM = value.SUMM;
				foreach (DISCOUNT2_MAKE_ITEM discount2MakeItemList in value.Discount2MakeItemList)
				{
					if (discount2MakeItemList.TYPE != "SBER")
					{
						continue;
					}
					sUMM += discount2MakeItemList.AMOUNT;
					aMOUNT += discount2MakeItemList.AMOUNT;
				}
				pCXCHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL = value.ID_CHEQUE_ITEM_GLOBAL;
				pCXCHEQUEITEM.QUANTITY = value.QUANTITY;
				pCXCHEQUEITEM.PRICE = UtilsArm.Round(sUMM / value.QUANTITY);
				pCXCHEQUEITEM.SUMM = UtilsArm.RoundDown((sUMM * pCXCHEQUEITEM.QUANTITY) / value.QUANTITY);
				pCXCHEQUEITEMs.Add(pCXCHEQUEITEM);
				guids1.Add(value.ID_CHEQUE_ITEM_GLOBAL, sUMM);
				winpcxChequeItem productCode = base.PcxObject.CreateChequeItem() as winpcxChequeItem;
				if (productCode == null)
				{
					throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				int num3 = (int)(sUMM * new decimal(100));
				productCode.Amount = num3.ToString();
				productCode.Product = PCXLoyaltyProgramEx.GetProductCode(guids[value.ID_CHEQUE_ITEM_GLOBAL].CODE);
				productCode.Quantity = Convert.ToDouble(value.QUANTITY);
				base.AddChequeItem(clientId, productCode);
			}
			decimal num4 = new decimal(0);
			if (aMOUNT > new decimal(0))
			{
				num4 = base.AddPaymentItem("N", aMOUNT, num4, clientId);
			}
			if (cHEQUEPAYMENTTYPE == PaymentType.Cash)
			{
				num4 = base.AddPaymentItem("C", num2, num4, clientId);
			}
			if (cHEQUEPAYMENTTYPE == PaymentType.Card)
			{
				num4 = base.AddPaymentItem("I", num2, num4, clientId);
			}
			if (cHEQUEPAYMENTTYPE == PaymentType.Mixed)
			{
				if (baseCheque.CHEQUE_PAYMENTS.Any<CHEQUE_PAYMENT>((CHEQUE_PAYMENT cp) => cp.SEPARATE_TYPE_ENUM == PaymentType.Card))
				{
					num4 = base.AddPaymentItem("I", baseCheque.SUM_CARD, num4, clientId);
				}
				if (baseCheque.CHEQUE_PAYMENTS.Any<CHEQUE_PAYMENT>((CHEQUE_PAYMENT cp) => cp.SEPARATE_TYPE_ENUM == PaymentType.Cash))
				{
					num4 = base.AddPaymentItem("C", baseCheque.SUM_CASH, num4, clientId);
				}
			}
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE tRANSACTIONID = base.CreatePCXCheque(guid, returnCheque.SUM_CASH + returnCheque.SUM_CARD, num4, "CHARGE_REFUND", new decimal(0));
			tRANSACTIONID.TRANSACTION_ID_PARENT = pCXQUERYLOG.TRANSACTION_ID;
			decimal num5 = (num2 + aMOUNT) * new decimal(100);
			base.Log(OperTypeEnum.ChargeRefund, num5, baseCheque, null, null);
			clientId.Amount = num5.ToString();
			clientId.ClientID = base.ClientId;
			clientId.ClientIDType = (int)base.LoyaltyType;
			clientId.Currency = 643.ToString();
			clientId.OrigID = pCXQUERYLOG.TRANSACTION_ID;
			clientId.OrigPartnerID = pCXQUERYLOG.TRANSACTION_PARTNER_ID;
			clientId.OrigLocation = pCXQUERYLOG.TRANSACTION_LOCATION;
			clientId.OrigTerminal = pCXQUERYLOG.TRANSACTION_TERMINAL;
			PCX_QUERY_LOG dQUERYGLOBAL = base.CreateLogQueryLog(guid, (num2 + aMOUNT) * new decimal(100), 5);
			dQUERYGLOBAL.ID_CANCELED_QUERY_GLOBAL = pCXQUERYLOG.ID_QUERY_GLOBAL;
			dQUERYGLOBAL.CLIENT_ID = base.ClientId;
			int num6 = base.PcxObject.Refund(clientId, _winpcxRefundResponse);
			base.Log(OperTypeEnum.ChargeRefund, num5, baseCheque, new int?(num6), _winpcxRefundResponse.TransactionID);
			winpcxTransaction transaction = _winpcxRefundResponse.Transaction as winpcxTransaction;
			try
			{
				try
				{
					dQUERYGLOBAL.DATE_RESPONSE = DateTime.Now;
					switch (num6)
					{
						case 0:
						{
							dQUERYGLOBAL.STATE = 4;
							break;
						}
						case 1:
						{
							dQUERYGLOBAL.STATE = 5;
							break;
						}
						default:
						{
							dQUERYGLOBAL.STATE = 2;
							break;
						}
					}
					if (transaction == null)
					{
						string empty = string.Empty;
						string str1 = empty;
						dQUERYGLOBAL.TRANSACTION_ID = empty;
						tRANSACTIONID.TRANSACTION_ID = str1;
						string location = SberbankLoyaltyProgram.Settings.Location;
						string str2 = location;
						dQUERYGLOBAL.TRANSACTION_LOCATION = location;
						tRANSACTIONID.LOCATION = str2;
						string partnerId = SberbankLoyaltyProgram.Settings.PartnerId;
						string str3 = partnerId;
						dQUERYGLOBAL.TRANSACTION_PARTNER_ID = partnerId;
						tRANSACTIONID.PARTNER_ID = str3;
						string terminal = SberbankLoyaltyProgram.Settings.Terminal;
						string str4 = terminal;
						dQUERYGLOBAL.TRANSACTION_TERMINAL = terminal;
						tRANSACTIONID.TERMINAL = str4;
					}
					else
					{
						string d = transaction.ID;
						string str5 = d;
						dQUERYGLOBAL.TRANSACTION_ID = d;
						tRANSACTIONID.TRANSACTION_ID = str5;
						string location1 = transaction.Location;
						string str6 = location1;
						dQUERYGLOBAL.TRANSACTION_LOCATION = location1;
						tRANSACTIONID.LOCATION = str6;
						string partnerID = transaction.PartnerID;
						string str7 = partnerID;
						dQUERYGLOBAL.TRANSACTION_PARTNER_ID = partnerID;
						tRANSACTIONID.PARTNER_ID = str7;
						string terminal1 = transaction.Terminal;
						string str8 = terminal1;
						dQUERYGLOBAL.TRANSACTION_TERMINAL = terminal1;
						tRANSACTIONID.TERMINAL = str8;
					}
					pCXQUERYLOGBL.Save(dQUERYGLOBAL);
					if (num6 != 0 && num6 != 1)
					{
						if (num6 != -991)
						{
							if (num6 != -212)
							{
								throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода Refund\r\n", Environment.NewLine, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num6, base.PcxObject.GetErrorMessage(num6))));
							}
							throw new LoyaltyException(this, "Возврата баллов произведено не будет, т.к. родительская транзакция не была проведена.");
						}
						throw new PCXInternalException(this);
					}
					PCXTransactionData pCXTransactionDatum = new PCXTransactionData(baseCheque.ID_CHEQUE_GLOBAL, this.ChequeOperTypeRefundCharge, transaction);
					base.SaveTransaction(OperTypeEnum.ChargeRefund, num5, pCXTransactionDatum);
					this.GetScoreDeltaBalance(_winpcxRefundResponse, out num, out num1);
					tRANSACTIONID.SCORE = Math.Abs(num);
					tRANSACTIONID.CARD_SCORE = num1;
				}
				catch (Exception exception)
				{
					base.LogError(OperTypeEnum.ChargeRefund, exception);
					throw;
				}
			}
			finally
			{
				pCXCHEQUEBL.Save(tRANSACTIONID);
			}
			IDictionary<Guid, decimal> guids2 = LoyaltyProgManager.Distribute(guids1, num2 + aMOUNT, Math.Round(Math.Abs(num), 2), false);
			foreach (PCX_CHEQUE_ITEM loyaltyType in pCXCHEQUEITEMs)
			{
				loyaltyType.SUMM_SCORE = guids2[loyaltyType.ID_CHEQUE_ITEM_GLOBAL] * new decimal(100);
				loyaltyType.SUMM = guids2[loyaltyType.ID_CHEQUE_ITEM_GLOBAL];
				loyaltyType.OPER_TYPE = PCX_CHEQUE_ITEM.operTypeArr[3];
				if (transaction == null)
				{
					loyaltyType.CLIENT_ID = base.ClientId;
					loyaltyType.CLIENT_ID_TYPE = (int)base.LoyaltyType;
				}
				else
				{
					loyaltyType.TRANSACTION_ID = transaction.ID;
					loyaltyType.CLIENT_ID = transaction.ClientID;
					loyaltyType.CLIENT_ID_TYPE = transaction.ClientIDType;
				}
			}
			if (pCXCHEQUEITEMs.Count > 0)
			{
				(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
			}
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("НОМЕР КАРТЫ: {0}", PCXUtils.GetCardNumberMasked(base.ClientPublicId)).AppendLine();
			if (transaction != null)
			{
				base.FillLocationInformation(tRANSACTIONID, transaction.Location, transaction.Terminal, transaction.PartnerID);
			}
			if (num6 != 1)
			{
				stringBuilder.AppendFormat("Списано {0} СПАСИБО", PCXUtils.TruncateNonZero(Math.Abs(num)));
			}
			else
			{
				stringBuilder.AppendFormat("К списанию {0} СПАСИБО", PCXUtils.TruncateNonZero(Math.Abs(Utils.GetDecimal(num2 * SberbankLoyaltyProgram.Params.ScorePerRub))));
			}
			str = stringBuilder.ToString();
			decimal num7 = new decimal(0);
			decimal num8 = Math.Abs(num);
			decimal num9 = num1;
			result = new PcxLpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num7, num8, num9, this.UnitName, true);
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(baseCheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeRefundDebit);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				return;
			}
			Guid dCHEQUEGLOBAL = baseCheque.ID_CHEQUE_GLOBAL;
			Guid guid = returnCheque.ID_CHEQUE_GLOBAL;
			string str = null;
			PCX_QUERY_LOG_BL pCXQUERYLOGBL = new PCX_QUERY_LOG_BL();
			PCX_QUERY_LOG pCXQUERYLOG = pCXQUERYLOGBL.Load(dCHEQUEGLOBAL, pcxOperation.Debit, (int)base.LoyaltyType);
			if (pCXQUERYLOG == null)
			{
				return;
			}
			winpcxRefundRequest _winpcxRefundRequest = base.PcxObject.CreateRefundRequest() as winpcxRefundRequest;
			if (_winpcxRefundRequest == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundRequest. Вернулся пустой объект.");
			}
			winpcxRefundResponse _winpcxRefundResponse = base.PcxObject.CreateRefundResponse() as winpcxRefundResponse;
			if (_winpcxRefundResponse == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundResponse. Вернулся пустой объект.");
			}
			List<PCX_CHEQUE_ITEM> list = returnCheque.CHEQUE_ITEMS.Select<CHEQUE_ITEM, PCX_CHEQUE_ITEM>(new Func<CHEQUE_ITEM, PCX_CHEQUE_ITEM>(SberbankLoyaltyProgram.CreatePcxItem)).ToList<PCX_CHEQUE_ITEM>();
			Dictionary<Guid, decimal> dictionary = list.ToDictionary<PCX_CHEQUE_ITEM, Guid, decimal>((PCX_CHEQUE_ITEM item) => item.ID_CHEQUE_ITEM_GLOBAL, (PCX_CHEQUE_ITEM item) => item.SUMM_SCORE);
			decimal num = list.Sum<PCX_CHEQUE_ITEM>((PCX_CHEQUE_ITEM item) => item.SUMM_SCORE);
			decimal num1 = num;
			base.Log(OperTypeEnum.DebitRefund, num1, baseCheque, null, null);
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE tRANSACTIONID = base.CreatePCXCheque(guid, new decimal(0), num1, "DEBIT_REFUND", num);
			tRANSACTIONID.TRANSACTION_ID_PARENT = pCXQUERYLOG.TRANSACTION_ID;
			this.FillPcxRequest(_winpcxRefundRequest, num1, pCXQUERYLOG);
			int num2 = base.PcxObject.Refund(_winpcxRefundRequest, _winpcxRefundResponse);
			base.Log(OperTypeEnum.DebitRefund, num1, baseCheque, new int?(num2), _winpcxRefundResponse.TransactionID);
			winpcxTransaction transaction = _winpcxRefundResponse.Transaction as winpcxTransaction;
			PCX_QUERY_LOG pCXQUERYLOG1 = base.CreateLogQueryLog(guid, num * new decimal(100), 4);
			decimal num3 = new decimal(0);
			decimal num4 = new decimal(0);
			try
			{
				try
				{
					if (transaction == null)
					{
						SberbankLoyaltyProgram.FillPcxCheque(tRANSACTIONID, string.Empty, SberbankLoyaltyProgram.Settings.Location, SberbankLoyaltyProgram.Settings.PartnerId, SberbankLoyaltyProgram.Settings.Terminal);
					}
					else
					{
						SberbankLoyaltyProgram.FillPcxCheque(tRANSACTIONID, transaction.ID, transaction.Location, transaction.PartnerID, transaction.Terminal);
					}
					this.FillPcxQueryLog(tRANSACTIONID, pCXQUERYLOG1, pCXQUERYLOG1.ID_QUERY_GLOBAL, num2);
					pCXQUERYLOGBL.Save(pCXQUERYLOG1);
					if (num2 != 0 && num2 != 1)
					{
						if (num2 != -991)
						{
							if (num2 != -212)
							{
								throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода Refund\r\n", Environment.NewLine, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num2, base.PcxObject.GetErrorMessage(num2))));
							}
							throw new LoyaltyException(this, "Возврата баллов произведено не будет, т.к. родительская транзакция не была проведена.");
						}
						throw new PCXInternalException(this);
					}
					PCXTransactionData pCXTransactionDatum = new PCXTransactionData(baseCheque.ID_CHEQUE_GLOBAL, this.ChequeOperTypeRefundDebit, transaction);
					base.SaveTransaction(OperTypeEnum.DebitRefund, num1, pCXTransactionDatum);
					pCXCHEQUEBL.Save(tRANSACTIONID);
					for (int i = 0; i < _winpcxRefundResponse.GetCardInfoItemCount(); i++)
					{
						winpcxCardInfoItem cardInfoItemAt = _winpcxRefundResponse.GetCardInfoItemAt(i) as winpcxCardInfoItem;
						string name = cardInfoItemAt.Name;
						if (name != null && name == "BNS")
						{
							if (cardInfoItemAt.Type == "C")
							{
								num3 = Utils.GetDecimal(cardInfoItemAt.Value);
							}
							else if (cardInfoItemAt.Type == "S")
							{
								num4 = Utils.GetDecimal(cardInfoItemAt.Value);
							}
						}
					}
					IDictionary<Guid, decimal> guids = LoyaltyProgManager.Distribute(dictionary, num, Math.Round(Math.Abs(num3), 2), false);
					foreach (PCX_CHEQUE_ITEM sUMMSCORE in list)
					{
						sUMMSCORE.SUMM_SCORE = guids[sUMMSCORE.ID_CHEQUE_ITEM_GLOBAL];
						sUMMSCORE.SUMM = sUMMSCORE.SUMM_SCORE / new decimal(100);
						sUMMSCORE.OPER_TYPE = PCX_CHEQUE_ITEM.operTypeArr[2];
						if (transaction == null)
						{
							continue;
						}
						sUMMSCORE.TRANSACTION_ID = transaction.ID;
						sUMMSCORE.CLIENT_ID = transaction.ClientID;
						sUMMSCORE.CLIENT_ID_TYPE = transaction.ClientIDType;
					}
					tRANSACTIONID.SCORE = Math.Abs(num3);
					tRANSACTIONID.CARD_SCORE = num4;
				}
				catch (Exception exception)
				{
					ARMLogger.Error(exception.ToString());
					throw;
				}
			}
			finally
			{
				pCXCHEQUEBL.Save(tRANSACTIONID);
				if (list.Count > 0)
				{
					(new PCX_CHEQUE_ITEM_BL()).Save(list);
				}
			}
			this.CreateRefundDebitCheque(out str, num2, tRANSACTIONID, transaction, DateTime.Now, baseCheque.SUM_DISCOUNT, num3, num);
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			int? nullable = null;
			base.Log(OperTypeEnum.DebitRefund, Math.Abs(num3), baseCheque, nullable, null);
			decimal num5 = Math.Abs(num3) / new decimal(100);
			decimal num6 = new decimal(0);
			decimal num7 = num4 / new decimal(100);
			result = new PcxLpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num5, num6, num7, this.UnitName, true);
		}

		private static void FillPcxCheque(PCX_CHEQUE pcxCheque, string transactionID, string transactionLocation, string transactionPartnerID, string transactionTerminal)
		{
			pcxCheque.TRANSACTION_ID = transactionID;
			pcxCheque.LOCATION = transactionLocation;
			pcxCheque.PARTNER_ID = transactionPartnerID;
			pcxCheque.TERMINAL = transactionTerminal;
		}

		private void FillPcxQueryLog(PCX_CHEQUE pcxCheque, PCX_QUERY_LOG logRecord, Guid idQueryGlobal, int result)
		{
			logRecord.TRANSACTION_ID = pcxCheque.TRANSACTION_ID;
			logRecord.TRANSACTION_LOCATION = pcxCheque.LOCATION;
			logRecord.TRANSACTION_PARTNER_ID = pcxCheque.PARTNER_ID;
			logRecord.TRANSACTION_TERMINAL = pcxCheque.TERMINAL;
			logRecord.ID_CANCELED_QUERY_GLOBAL = idQueryGlobal;
			logRecord.CLIENT_ID = base.ClientId;
			logRecord.STATUS = pcxCheque.STATUS;
			logRecord.DATE_RESPONSE = DateTime.Now;
			logRecord.STATE = base.GetResultState(result);
		}

		private void FillPcxRequest(winpcxRefundRequest request, decimal requestAmount, PCX_QUERY_LOG logDebit)
		{
			request.AddPaymentItem(this.CreatePcxPayment(requestAmount));
			request.Amount = requestAmount.ToString();
			request.ClientID = base.ClientId;
			request.ClientIDType = (int)base.LoyaltyType;
			request.Currency = 643.ToString();
			request.OrigID = logDebit.TRANSACTION_ID;
			request.OrigPartnerID = logDebit.TRANSACTION_PARTNER_ID;
			request.OrigLocation = logDebit.TRANSACTION_LOCATION;
			request.OrigTerminal = logDebit.TRANSACTION_TERMINAL;
		}

		protected new string GetRollbackSlipCheque(LoyaltyTransaction transaction)
		{
			StringBuilder stringBuilder = new StringBuilder();
			LpTransactionData data = transaction.Data;
			switch (transaction.Operation)
			{
				case OperTypeEnum.Debit:
				{
					stringBuilder.AppendLine("Отмена списания СПАСИБО");
					break;
				}
				case OperTypeEnum.Charge:
				{
					stringBuilder.AppendLine("Отмена начисления СПАСИБО");
					break;
				}
				default:
				{
					stringBuilder.AppendLine("Отмена операции СПАСИБО");
					break;
				}
			}
			stringBuilder.AppendFormat("НОМЕР КАРТЫ: {0}", PCXUtils.GetCardNumberMasked(base.ClientPublicId)).AppendLine();
			stringBuilder.AppendFormat("Сумма операции: {0}", PCXUtils.TruncateNonZero(transaction.OperationSum));
			return stringBuilder.ToString();
		}

		protected void GetScoreDeltaBalance(winpcxRefundResponse response, out decimal scoreDeltaBalance, out decimal scoreBalance)
		{
			scoreDeltaBalance = new decimal(0);
			scoreBalance = new decimal(0);
			for (int i = 0; i < response.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = response.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				if (name != null && name == "BNS")
				{
					if (cardInfoItemAt.Type == "C")
					{
						scoreDeltaBalance = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
					}
					else if (cardInfoItemAt.Type == "S")
					{
						scoreBalance = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
					}
				}
			}
		}

		protected override void OnInitInternal()
		{
			base.OnInitInternal();
			base.SendRecvTimeout = SberbankLoyaltyProgram.Settings.SendReciveTimeout;
		}

		protected override void OnInitSettings()
		{
			if (SberbankLoyaltyProgram.IsSettingsInit)
			{
				return;
			}
			SettingsModel settingsModel = new SettingsModel();
			LoyaltySettings loyaltySetting = settingsModel.Load(base.LoyaltyType, Guid.Empty, ServerType.Local);
			SberbankLoyaltyProgram.Settings = settingsModel.Deserialize<ePlus.Loyalty.Sber.Settings>(loyaltySetting.SETTINGS, "Settings");
			SberbankLoyaltyProgram.Certificate = settingsModel.Deserialize<ePlus.Loyalty.Sber.Certificate>(loyaltySetting.SETTINGS, "Certificate");
			SberbankLoyaltyProgram.Params = settingsModel.Deserialize<ePlus.Loyalty.Sber.Params>(loyaltySetting.PARAMS, "Params");
			base.SendRecvTimeout = (SberbankLoyaltyProgram.Settings.SendReciveTimeout == 0 ? 30 : SberbankLoyaltyProgram.Settings.SendReciveTimeout);
			SberbankLoyaltyProgram._idGlobal = loyaltySetting.ID_LOYALITY_PROGRAM_GLOBAL;
			SberbankLoyaltyProgram._name = loyaltySetting.NAME;
			this.MinPayPercent = SberbankLoyaltyProgram.Params.MinPayPercent;
			SberbankLoyaltyProgram.IscompatibilityEnabled = loyaltySetting.COMPATIBILITY;
			if (SberbankLoyaltyProgram.IscompatibilityEnabled)
			{
				SberbankLoyaltyProgram._excludedPrograms.Add(this.IdGlobal, null);
				foreach (DataRowItem excludeList in loyaltySetting.CompatibilitiesDCT.ExcludeList)
				{
					SberbankLoyaltyProgram._excludedPrograms.Add(excludeList.Guid, excludeList);
				}
				foreach (DataRowItem dataRowItem in loyaltySetting.CompatibilitiesDP.ExcludeList)
				{
					SberbankLoyaltyProgram._excludedPrograms.Add(dataRowItem.Guid, dataRowItem);
				}
				foreach (DataRowItem excludeList1 in loyaltySetting.CompatibilitiesPL.ExcludeList)
				{
					SberbankLoyaltyProgram._excludedPrograms.Add(excludeList1.Guid, excludeList1);
				}
			}
			SberbankLoyaltyProgram.IsSettingsInit = true;
		}

		protected override void OnPCXSettings()
		{
			base.PcxObject.ConnectionString = SberbankLoyaltyProgram.Settings.Url;
			base.PcxObject.ConnectTimeout = SberbankLoyaltyProgram.Settings.ConnectionTimeout;
			base.PcxObject.SendRecvTimeout = SberbankLoyaltyProgram.Settings.SendReciveTimeout;
			base.PcxObject.Location = SberbankLoyaltyProgram.Settings.Location;
			base.PcxObject.PartnerID = SberbankLoyaltyProgram.Settings.PartnerId;
			base.PcxObject.BackgndFlushPeriod = SberbankLoyaltyProgram.Settings.BkgndFlushPeriod;
			if (SberbankLoyaltyProgram.Settings.Proxy.Use)
			{
				base.PcxObject.ProxyHost = SberbankLoyaltyProgram.Settings.Proxy.Address;
				base.PcxObject.ProxyPort = SberbankLoyaltyProgram.Settings.Proxy.Port;
				base.PcxObject.ProxyUserId = SberbankLoyaltyProgram.Settings.Proxy.User;
				base.PcxObject.ProxyUserPass = SberbankLoyaltyProgram.Settings.Proxy.Password;
			}
			base.PcxObject.Terminal = SberbankLoyaltyProgram.Settings.Terminal;
			if (!SberbankLoyaltyProgram.Certificate.SertInStorage)
			{
				base.PcxObject.CertFilePath = SberbankLoyaltyProgram.Certificate.SertFilePath;
				base.PcxObject.KeyFilePath = SberbankLoyaltyProgram.Certificate.KeyFilePath;
				base.PcxObject.KeyPassword = SberbankLoyaltyProgram.Certificate.CertPassword;
			}
			else
			{
				base.PcxObject.CertSubjectName = SberbankLoyaltyProgram.Certificate.SertName;
			}
			if (!LoyaltyProgManager.IsLoyalityProgramEnabled(ePlus.Loyalty.LoyaltyType.Svyaznoy) && !AppConfigurator.EnableSberbank)
			{
				return;
			}
			int num = base.PcxObject.Init();
			if (num != 0)
			{
				throw new LoyaltyException(this, string.Concat("Объект PCX, не создан  возможно ActiveX компонент не установлен", Environment.NewLine, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num, base.PcxObject.GetErrorMessage(num))));
			}
		}

		public bool ValidateBin(string cardNo)
		{
			this.OnInitSettings();
			return SberbankLoyaltyProgram.Params.Bins.Any<Bin>((Bin item) => {
				if (item.DateDeleted != DateTime.MinValue)
				{
					return false;
				}
				return cardNo.StartsWith(item.Value);
			});
		}
	}
}