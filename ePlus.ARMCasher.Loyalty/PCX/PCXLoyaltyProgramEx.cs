using ePlus.ARMBusinessLogic;
using ePlus.ARMCasher;
using ePlus.ARMCasher.BusinessLogic.Events;
using ePlus.ARMCasher.BusinessObjects;
using ePlus.ARMCasher.Loyalty;
using ePlus.ARMCommon;
using ePlus.ARMCommon.Log;
using ePlus.ARMUtils;
using ePlus.CommonEx;
using ePlus.Discount2.BusinessObjects;
using ePlus.Loyalty;
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
	internal abstract class PCXLoyaltyProgramEx : BaseLoyaltyProgramEx
	{
		protected const string ServiceCode = "-100";

		protected const int CurrencyRu = 643;

		private readonly static object _pcxLock;

		private readonly static object _settingsLock;

		private static winpcxClass _pcx;

		protected abstract Guid ChequeOperTypeCharge
		{
			get;
		}

		protected abstract Guid ChequeOperTypeDebit
		{
			get;
		}

		protected abstract Guid ChequeOperTypeRefundCharge
		{
			get;
		}

		protected abstract Guid ChequeOperTypeRefundDebit
		{
			get;
		}

		protected int ClientIDType
		{
			get
			{
				if (base.LoyaltyType != ePlus.Loyalty.LoyaltyType.Sberbank)
				{
					return 4;
				}
				return 6;
			}
		}

		protected virtual decimal Devider
		{
			get;
			set;
		}

		private bool IsOffline
		{
			get;
			set;
		}

		protected virtual decimal MinChequeSumForCharge
		{
			get;
			set;
		}

		protected virtual decimal MinPayPercent
		{
			get;
			set;
		}

		protected abstract decimal OfflineChargePercent
		{
			get;
		}

		protected winpcxClass PcxObject
		{
			get
			{
				winpcxClass _winpcxClass;
				lock (PCXLoyaltyProgramEx._pcxLock)
				{
					_winpcxClass = PCXLoyaltyProgramEx._pcx;
				}
				return _winpcxClass;
			}
		}

		protected virtual decimal ScorePerRub
		{
			get
			{
				return new decimal(1);
			}
		}

		protected virtual string UnitName
		{
			get;
			set;
		}

		static PCXLoyaltyProgramEx()
		{
			PCXLoyaltyProgramEx._pcxLock = new object();
			PCXLoyaltyProgramEx._settingsLock = new object();
		}

		public PCXLoyaltyProgramEx(ePlus.Loyalty.LoyaltyType loyaltyType, string clientId, string clientPublicId, string discountType) : base(loyaltyType, clientId, clientPublicId, discountType)
		{
		}

		protected void AddChequeItem(winpcxRefundRequest request, winpcxChequeItem item)
		{
			if (string.IsNullOrEmpty(item.Product))
			{
				item.Product = "0";
			}
			request.AddChequeItem(item);
		}

		protected void AddChequeItem(winpcxAuthRequestData request, winpcxChequeItem item)
		{
			if (string.IsNullOrEmpty(item.Product))
			{
				item.Product = "0";
			}
			request.AddChequeItem(item);
		}

		protected decimal AddPaymentItem(string payMeans, decimal summ, decimal processingSendSum, winpcxRefundRequest request)
		{
			winpcxPaymentItem str = this.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
			if (str == null)
			{
				throw new Exception("Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
			}
			str.PayMeans = payMeans;
			decimal num = summ * new decimal(100);
			processingSendSum += num;
			str.Amount = num.ToString();
			request.AddPaymentItem(str);
			return processingSendSum;
		}

		private int AuthPoints(long amount, decimal discountSum, IEnumerable<winpcxPaymentItem> paymentItemList, CHEQUE cheque, PCX_QUERY_LOG logRecord, OperTypeEnum operType, out winpcxTransaction transaction, out winpcxAuthResponseData response)
		{
			Guid guid = (operType == OperTypeEnum.Debit ? this.ChequeOperTypeDebit : this.ChequeOperTypeCharge);
			winpcxAuthRequestData str = this.PcxObject.CreateAuthRequest() as winpcxAuthRequestData;
			if (str == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateAuthRequest. Вернулся пустой объект.");
			}
			response = this.PcxObject.CreateAuthResponse() as winpcxAuthResponseData;
			if (response == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateAuthResponse. Вернулся пустой объект.");
			}
			str.Amount = amount.ToString();
			str.ClientID = base.ClientId;
			str.ClientIDType = this.ClientIDType;
			str.Currency = 643.ToString();
			foreach (winpcxPaymentItem _winpcxPaymentItem in paymentItemList)
			{
				str.AddPaymentItem(_winpcxPaymentItem);
			}
			foreach (CHEQUE_ITEM cHEQUEITEM in cheque.CHEQUE_ITEMS)
			{
				winpcxChequeItem productCode = this.PcxObject.CreateChequeItem() as winpcxChequeItem;
				if (productCode == null)
				{
					throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				decimal num = cHEQUEITEM.Discount2MakeItemList.Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM discountItem) => {
					if (discountItem.TYPE != base.DiscountType)
					{
						return new decimal(0);
					}
					return discountItem.AMOUNT;
				});
				decimal sUMM = cHEQUEITEM.SUMM + (discountSum == new decimal(0) ? new decimal(0) : num);
				long num1 = Convert.ToInt64(sUMM * new decimal(100));
				productCode.Amount = num1.ToString();
				productCode.Product = PCXLoyaltyProgramEx.GetProductCode(cHEQUEITEM.CODE);
				productCode.Quantity = Convert.ToDouble(cHEQUEITEM.QUANTITY);
				this.AddChequeItem(str, productCode);
			}
			BaseLoyaltyProgramEx.QueryLogBl.Save(logRecord);
			int num2 = this.PcxObject.AuthPoints(str, response);
			logRecord.DATE_RESPONSE = DateTime.Now;
			if (num2 != 0 && num2 != 1 && !this.IsOffline)
			{
				throw new LoyaltyException(this, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num2, this.PcxObject.GetErrorMessage(num2)));
			}
			switch (num2)
			{
				case 0:
				{
					logRecord.STATE = 4;
					break;
				}
				case 1:
				{
					logRecord.STATE = 5;
					this.IsOffline = true;
					this.SetMinRecvTimeout(true);
					break;
				}
				default:
				{
					logRecord.STATE = 2;
					break;
				}
			}
			transaction = response.Transaction as winpcxTransaction;
			if (transaction == null)
			{
				logRecord.TRANSACTION_ID = string.Empty;
				logRecord.TRANSACTION_LOCATION = this.PcxObject.Location;
				logRecord.TRANSACTION_PARTNER_ID = this.PcxObject.PartnerID;
				logRecord.TRANSACTION_TERMINAL = this.PcxObject.Terminal;
			}
			else
			{
				logRecord.TRANSACTION_ID = transaction.ID;
				logRecord.TRANSACTION_LOCATION = transaction.Location;
				logRecord.TRANSACTION_PARTNER_ID = transaction.PartnerID;
				logRecord.TRANSACTION_TERMINAL = transaction.Terminal;
			}
			logRecord.STATUS = this.GetPCXStatus();
			BaseLoyaltyProgramEx.QueryLogBl.Save(logRecord);
			if ((num2 != 1 || operType != OperTypeEnum.Charge) && num2 != 0)
			{
				foreach (CHEQUE_ITEM cHEQUEITEM1 in cheque.CHEQUE_ITEMS)
				{
					foreach (DISCOUNT2_MAKE_ITEM discount2MakeItemList in cHEQUEITEM1.Discount2MakeItemList)
					{
						if (discount2MakeItemList.TYPE != "SVYAZ")
						{
							continue;
						}
						discount2MakeItemList.AMOUNT = new decimal(0);
						discountSum = new decimal(0);
					}
				}
				throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода AuthPoints\r\n", ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num2, this.PcxObject.GetErrorMessage(num2))));
			}
			PCXTransactionData pCXTransactionDatum = new PCXTransactionData(cheque.ID_CHEQUE_GLOBAL, guid, transaction);
			base.SaveTransaction(operType, discountSum, pCXTransactionDatum);
			decimal num3 = new decimal(0);
			int num4 = 0;
			for (int i = 0; i < response.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = response.GetCardInfoItemAt(i) as winpcxCardInfoItem;
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
								num4 = (int)(Utils.GetDecimal(cardInfoItemAt.Value) / this.Devider);
							}
						}
					}
					else if (cardInfoItemAt.Type == "C")
					{
						num3 = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
					}
				}
			}
			this.CreateAndSavePCXChequeItemList(cheque.CHEQUE_ITEMS, cheque.SUMM + discountSum, num3, num4, transaction.ID);
			return num2;
		}

		public override decimal CalculateMaxSumBonus(CHEQUE cheque)
		{
			decimal sUMM = cheque.SUMM + base.GetDiscountSum(cheque);
			LoyaltyCardInfo loyaltyCardInfo = base.GetLoyaltyCardInfo(false);
			decimal num = sUMM--;
			if (this.MinPayPercent > new decimal(0))
			{
				num = Math.Truncate(Math.Min(sUMM - (sUMM * (this.MinPayPercent / new decimal(100))), sUMM));
			}
			if (sUMM < this.MinChequeSumForCharge)
			{
				num = new decimal(0);
			}
			return Math.Min(num, loyaltyCardInfo.Balance);
		}

		private void CreateAndSavePCXChequeItemList(IEnumerable<CHEQUE_ITEM> chequeItemList, decimal totalSum, decimal pcxSumMoney, int pcxSumScore, string transactionId)
		{
			List<CHEQUE_ITEM> list = chequeItemList.ToList<CHEQUE_ITEM>();
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			foreach (CHEQUE_ITEM cHEQUEITEM in list)
			{
				decimal sUMM = cHEQUEITEM.SUMM + (
					from dmi in cHEQUEITEM.Discount2MakeItemList
					where dmi.TYPE == base.DiscountType
					select dmi).Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM dmi) => dmi.AMOUNT);
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, sUMM);
			}
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
				if (!this.IsOffline)
				{
					item.SUMM_SCORE = guids2[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL];
					item.SUMM = guids1[cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL];
					item.STATUS = pcxOperationStatus.Online.ToString();
				}
				else
				{
					item.SUMM_SCORE = Math.Abs(cHEQUEITEM1.SUMM * this.ScorePerRub);
					item.SUMM = item.SUMM_SCORE * (this.OfflineChargePercent / new decimal(100));
					item.STATUS = pcxOperationStatus.Offline.ToString();
				}
				item.ID_CHEQUE_ITEM_GLOBAL = cHEQUEITEM1.ID_CHEQUE_ITEM_GLOBAL;
				item.OPER_TYPE = PCX_CHEQUE_ITEM.operTypeArr[(int)((pcxSumMoney >= new decimal(0) ? OperTypeEnum.Charge : OperTypeEnum.Debit))];
			}
			if (pCXCHEQUEITEMs.Count > 0)
			{
				(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
			}
		}

		private PCX_CHEQUE_ITEM CreatePcxChequeItem(Guid idChequeItemGlobal, decimal quantity, decimal itemSum)
		{
			PCX_CHEQUE_ITEM pCXCHEQUEITEM = new PCX_CHEQUE_ITEM()
			{
				ID_CHEQUE_ITEM_GLOBAL = idChequeItemGlobal,
				QUANTITY = quantity,
				PRICE = UtilsArm.Round(itemSum / quantity),
				SUMM = UtilsArm.RoundDown(itemSum),
				STATUS = this.GetPCXStatus()
			};
			return pCXCHEQUEITEM;
		}

		private string CreateRefundChargeSlipCheque(CHEQUE baseCheque, PCX_QUERY_LOG logRecord, winpcxTransaction trans, PCX_CHEQUE pcxCheque, int res, decimal scoreDeltaBalance)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("ШК {0}", base.ClientPublicId).AppendLine();
			if (trans == null)
			{
				this.FillLocationInformation(pcxCheque, this.PcxObject.Location, this.PcxObject.Terminal, this.PcxObject.PartnerID);
			}
			else
			{
				this.FillLocationInformation(pcxCheque, trans.Location, trans.Terminal, trans.PartnerID);
			}
			if (res != 1)
			{
				stringBuilder.AppendFormat("Списано {0}: {1}", this.UnitName, PCXUtils.TruncateNonZero(Math.Abs(scoreDeltaBalance)));
			}
			else
			{
				stringBuilder.AppendFormat("{1} к списанию {0}", PCXUtils.TruncateNonZero(Math.Abs(Utils.GetLong(baseCheque.SumWithoutDiscount * this.ScorePerRub))), this.UnitName);
			}
			return stringBuilder.ToString();
		}

		protected void CreateRefundDebitCheque(out string slipCheque, int res, PCX_CHEQUE pcxCheque, winpcxTransaction transaction, DateTime dateRequest, decimal discountSum, decimal scoreDeltaBalance, decimal sumTotalPCX)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("ШК {0}", base.ClientPublicId).AppendLine();
			if (transaction == null)
			{
				this.FillLocationInformation(pcxCheque, this.PcxObject.Location, this.PcxObject.Terminal, this.PcxObject.PartnerID);
			}
			if (res != 1)
			{
				stringBuilder.AppendFormat("Начислено {0}: {1}", this.UnitName, PCXUtils.TruncateNonZero(Math.Abs(scoreDeltaBalance)));
			}
			else
			{
				stringBuilder.AppendFormat("{1} к начислению {0}", PCXUtils.TruncateNonZero(Math.Abs(sumTotalPCX)), this.UnitName);
			}
			slipCheque = stringBuilder.ToString();
		}

		private winpcxChequeItem CreateWinPcxChequeItem(int amount, string product, decimal quantity)
		{
			winpcxChequeItem str = this.PcxObject.CreateChequeItem() as winpcxChequeItem;
			if (str == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
			}
			str.Amount = amount.ToString();
			str.Product = PCXLoyaltyProgramEx.GetProductCode(product);
			str.Quantity = Convert.ToDouble(quantity);
			return str;
		}

		protected override void DoCharge(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			winpcxAuthResponseData winpcxAuthResponseDatum;
			winpcxTransaction _winpcxTransaction;
			result = null;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeCharge);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Начисление баллов на карту лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторное начисление произведено не будет.", this.Name, cheque.ID_CHEQUE_GLOBAL));
				return;
			}
			base.Log(OperTypeEnum.Charge, discountSum, cheque, null, null);
			long num = (long)(discountSum * new decimal(100));
			long sUMCASH = (long)(cheque.SUM_CASH * new decimal(100));
			long sUMCARD = (long)(cheque.SUM_CARD * new decimal(100));
			long num1 = num + sUMCASH + sUMCARD;
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE pCXStatus = base.CreatePCXCheque(cheque.ID_CHEQUE_GLOBAL, cheque.SUM_CASH + cheque.SUM_CARD, num1, "CHARGE", new decimal(0));
			pCXStatus.STATUS = this.GetPCXStatus();
			List<winpcxPaymentItem> paymentListForCahrge = this.GetPaymentListForCahrge(num, sUMCASH, sUMCARD);
			PCX_QUERY_LOG pCXQUERYLOG = base.CreateLogQueryLog(cheque.ID_CHEQUE_GLOBAL, num + sUMCASH + sUMCARD, 3);
			int num2 = this.AuthPoints(num1, discountSum, paymentListForCahrge, cheque, pCXQUERYLOG, OperTypeEnum.Charge, out _winpcxTransaction, out winpcxAuthResponseDatum);
			base.Log(OperTypeEnum.Debit, discountSum, cheque, new int?(num2), _winpcxTransaction.ID);
			pCXCHEQUEBL.Save(pCXStatus);
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			try
			{
				try
				{
					if (_winpcxTransaction == null)
					{
						this.FillLocationInformation(pCXStatus, this.PcxObject.Location, this.PcxObject.Terminal, this.PcxObject.PartnerID);
					}
					else
					{
						pCXStatus.TRANSACTION_ID = _winpcxTransaction.ID;
						this.FillLocationInformation(pCXStatus, _winpcxTransaction.Location, _winpcxTransaction.Terminal, _winpcxTransaction.PartnerID);
					}
					decimal num3 = this.ScoreDeltaBalance(winpcxAuthResponseDatum);
					decimal num4 = this.ScoreBalance(winpcxAuthResponseDatum);
					if (num2 != 1)
					{
						pCXStatus.CARD_SCORE = num4;
						pCXStatus.SCORE = Math.Abs(num3);
					}
					else
					{
						decimal num5 = base.GetDiscountSum(cheque);
						decimal sUMM = (cheque.SUMM + num5) * (this.OfflineChargePercent / new decimal(100));
						pCXStatus.SCORE = Math.Abs(sUMM);
					}
					result = new PcxLpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, Math.Abs(num3), new decimal(0), num4, this.UnitName);
					pCXStatus.STATUS = this.GetPCXStatus();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					base.LogError(OperTypeEnum.Charge, exception);
					throw exception;
				}
			}
			finally
			{
				pCXCHEQUEBL.Save(pCXStatus);
			}
		}

		protected override void DoDebit(CHEQUE cheque, decimal discountSum, out ILpTransResult result)
		{
			winpcxTransaction _winpcxTransaction;
			winpcxAuthResponseData winpcxAuthResponseDatum;
			result = null;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(cheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeDebit);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Списание баллов с карты лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторное списание произведено не будет.", this.Name, cheque.ID_CHEQUE_GLOBAL));
				return;
			}
			base.Log(OperTypeEnum.Debit, discountSum, cheque, null, null);
			if (discountSum > new decimal(0))
			{
				long num = (long)(discountSum * new decimal(100));
				PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
				PCX_CHEQUE pCXCHEQUE = new PCX_CHEQUE()
				{
					CLIENT_ID = base.ClientId,
					CLIENT_ID_TYPE = (int)base.LoyaltyType,
					SUMM = num,
					SUMM_MONEY = new decimal(0),
					SCORE = new decimal(0),
					SUMM_SCORE = discountSum,
					PARTNER_ID = string.Empty,
					LOCATION = string.Empty,
					TERMINAL = string.Empty,
					ID_CHEQUE_GLOBAL = cheque.ID_CHEQUE_GLOBAL,
					OPER_TYPE = "DEBIT",
					CARD_NUMBER = base.ClientPublicId
				};
				PCX_CHEQUE pCXStatus = pCXCHEQUE;
				cheque.PcxCheque = pCXStatus;
				pCXStatus.STATUS = this.GetPCXStatus();
				winpcxPaymentItem str = this.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
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
				int num1 = this.AuthPoints(num, discountSum, winpcxPaymentItems, cheque, pCXQUERYLOG, OperTypeEnum.Debit, out _winpcxTransaction, out winpcxAuthResponseDatum);
				base.Log(OperTypeEnum.Debit, discountSum, cheque, new int?(num1), _winpcxTransaction.ID);
				pCXCHEQUEBL.Save(pCXStatus);
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
				try
				{
					try
					{
						if (_winpcxTransaction != null)
						{
							pCXStatus.TRANSACTION_ID = _winpcxTransaction.ID;
							this.FillLocationInformation(pCXStatus, _winpcxTransaction.Location, _winpcxTransaction.Terminal, _winpcxTransaction.PartnerID);
							pCXStatus.CARD_SCORE = this.ScoreBalance(winpcxAuthResponseDatum);
							pCXStatus.SCORE = this.ScoreDeltaBalance(winpcxAuthResponseDatum);
						}
						pCXStatus.STATUS = this.GetPCXStatus();
						result = new PcxLpTransResult(cheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, new decimal(0), pCXStatus.SCORE, pCXStatus.CARD_SCORE, this.UnitName);
					}
					catch (Exception exception)
					{
						base.LogError(OperTypeEnum.Debit, exception);
						throw;
					}
				}
				finally
				{
					pCXCHEQUEBL.Save(pCXStatus);
				}
			}
		}

		protected override LoyaltyCardInfo DoGetLoyaltyCardInfoFromService(Form form)
		{
			LoyaltyCardInfo clientPublicId = new LoyaltyCardInfo()
			{
				ClientId = base.ClientId
			};
			object obj = this.PcxObject.CreateAuthResponse();
			if (obj == null)
			{
				throw new Exception("Ошибка при вызове метода CreateAuthResponse. Вернулся пустой объект.");
			}
			int info = this.PcxObject.GetInfo(base.ClientId, this.ClientIDType, obj);
			if (info != 0)
			{
				if (info != -162)
				{
					throw new Exception(string.Concat("Ошибка при вызове метода CreateAuthResponse\r\n", ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(info, this.PcxObject.GetErrorMessage(info))));
				}
				clientPublicId.CardNumber = base.ClientPublicId;
				clientPublicId.CardStatus = "Заблокирована";
				clientPublicId.CardStatusId = LoyaltyCardStatus.Blocked;
			}
			else
			{
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
							clientPublicId.Points = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
						}
						else if (str == "AB")
						{
							clientPublicId.Balance = Utils.GetDecimal(cardInfoItemAt.Value) / new decimal(100);
						}
						else if (str == "EAN13")
						{
							clientPublicId.CardNumber = cardInfoItemAt.Value;
						}
						else if (str != "ID_DATA")
						{
							if (str == "CS")
							{
								string value = cardInfoItemAt.Value;
								string str1 = value;
								if (value != null)
								{
									if (str1 == "A")
									{
										clientPublicId.CardStatus = "Активна";
										clientPublicId.CardStatusId = LoyaltyCardStatus.Active;
										goto Label0;
									}
									else
									{
										if (str1 != "R")
										{
											goto Label2;
										}
										clientPublicId.CardStatus = "Ограничена";
										clientPublicId.CardStatusId = LoyaltyCardStatus.Limited;
										goto Label0;
									}
								}
							Label2:
								clientPublicId.CardStatus = "Заблокирована";
								clientPublicId.CardStatusId = LoyaltyCardStatus.Blocked;
							}
						}
						else if (string.IsNullOrEmpty(clientPublicId.CardNumber) && cardInfoItemAt.Type == "S")
						{
							clientPublicId.CardNumber = cardInfoItemAt.Value;
						}
					}
				Label0:
				}
			}
			return clientPublicId;
		}

		protected override void DoRefundCharge(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			result = null;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(returnCheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeRefundCharge);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				ARMLogger.Trace(string.Format("Возврат начисления на карту лояльности {0} по чеку ID: {1}. Операция найдена в логе транзакций чека, повторного возврата начисления произведено не будет.", this.Name, baseCheque.ID_CHEQUE_GLOBAL));
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
						throw new LoyaltyException(this, string.Format("Невозможно выполнить возврат {0} при оплате отличной от Наличных или Картой", this.UnitName));
					}
				}
				PCX_QUERY_LOG_BL pCXQUERYLOGBL = new PCX_QUERY_LOG_BL();
				PCX_QUERY_LOG pCXQUERYLOG = pCXQUERYLOGBL.Load(baseCheque.ID_CHEQUE_GLOBAL, pcxOperation.Charge, (int)base.LoyaltyType);
				if (pCXQUERYLOG == null)
				{
					return;
				}
				winpcxRefundRequest _winpcxRefundRequest = this.PcxObject.CreateRefundRequest() as winpcxRefundRequest;
				if (_winpcxRefundRequest == null)
				{
					throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundRequest. Вернулся пустой объект.");
				}
				winpcxRefundResponse _winpcxRefundResponse = this.PcxObject.CreateRefundResponse() as winpcxRefundResponse;
				if (_winpcxRefundResponse == null)
				{
					throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundResponse. Вернулся пустой объект.");
				}
				IDictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
				List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
				decimal sUMM = returnCheque.SUMM;
				decimal num = new decimal(0);
				foreach (CHEQUE_ITEM cHEQUEITEM in returnCheque.CHEQUE_ITEMS)
				{
					decimal sUMM1 = cHEQUEITEM.SUMM;
					decimal num1 = (
						from dmi in cHEQUEITEM.Discount2MakeItemList
						where dmi.TYPE == base.DiscountType
						select dmi).Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM x) => x.AMOUNT);
					sUMM1 += num1;
					num += num1;
					PCX_CHEQUE_ITEM pCXCHEQUEITEM = this.CreatePcxChequeItem(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, cHEQUEITEM.QUANTITY, sUMM1);
					pCXCHEQUEITEMs.Add(pCXCHEQUEITEM);
					guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, sUMM1);
					winpcxChequeItem _winpcxChequeItem = this.CreateWinPcxChequeItem((int)(sUMM1 * new decimal(100)), cHEQUEITEM.CODE, cHEQUEITEM.QUANTITY);
					this.AddChequeItem(_winpcxRefundRequest, _winpcxChequeItem);
				}
				decimal num2 = new decimal(0);
				if (num > new decimal(0))
				{
					num2 = this.AddPaymentItem("N", num, num2, _winpcxRefundRequest);
				}
				if (cHEQUEPAYMENTTYPE == PaymentType.Cash)
				{
					num2 = this.AddPaymentItem("C", sUMM, num2, _winpcxRefundRequest);
				}
				else if (cHEQUEPAYMENTTYPE == PaymentType.Card)
				{
					num2 = this.AddPaymentItem("I", sUMM, num2, _winpcxRefundRequest);
				}
				else if (cHEQUEPAYMENTTYPE == PaymentType.Mixed)
				{
					if (baseCheque.CHEQUE_PAYMENTS.Any<CHEQUE_PAYMENT>((CHEQUE_PAYMENT cp) => cp.SEPARATE_TYPE_ENUM == PaymentType.Card))
					{
						num2 = this.AddPaymentItem("I", baseCheque.SUM_CARD, num2, _winpcxRefundRequest);
					}
					if (baseCheque.CHEQUE_PAYMENTS.Any<CHEQUE_PAYMENT>((CHEQUE_PAYMENT cp) => cp.SEPARATE_TYPE_ENUM == PaymentType.Cash))
					{
						num2 = this.AddPaymentItem("C", baseCheque.SUM_CASH, num2, _winpcxRefundRequest);
					}
				}
				PCX_CHEQUE tRANSACTIONID = base.CreatePCXCheque(returnCheque.ID_CHEQUE_GLOBAL, returnCheque.SUM_CASH + returnCheque.SUM_CARD, num2, "CHARGE_REFUND", new decimal(0));
				tRANSACTIONID.TRANSACTION_ID_PARENT = pCXQUERYLOG.TRANSACTION_ID;
				tRANSACTIONID.STATUS = this.GetPCXStatus();
				decimal num3 = (sUMM + num) * new decimal(100);
				base.Log(OperTypeEnum.ChargeRefund, num3, baseCheque, null, null);
				this.FillRequest(_winpcxRefundRequest, num3, pCXQUERYLOG);
				int num4 = this.PcxObject.Refund(_winpcxRefundRequest, _winpcxRefundResponse);
				base.Log(OperTypeEnum.ChargeRefund, num3, baseCheque, new int?(num4), _winpcxRefundResponse.TransactionID);
				winpcxTransaction transaction = _winpcxRefundResponse.Transaction as winpcxTransaction;
				PCX_QUERY_LOG pCXQUERYLOG1 = base.CreateLogQueryLog(returnCheque.ID_CHEQUE_GLOBAL, num3, 5);
				try
				{
					try
					{
						if (transaction == null)
						{
							PCXLoyaltyProgramEx.FillPcxCheque(tRANSACTIONID, string.Empty, this.PcxObject.Location, this.PcxObject.PartnerID, this.PcxObject.Terminal);
						}
						else
						{
							PCXLoyaltyProgramEx.FillPcxCheque(tRANSACTIONID, transaction.ID, transaction.Location, transaction.PartnerID, transaction.Terminal);
						}
						this.FillPcxQueryLog(tRANSACTIONID, pCXQUERYLOG1, pCXQUERYLOG.ID_QUERY_GLOBAL, num4);
						pCXQUERYLOGBL.Save(pCXQUERYLOG1);
						if (num4 != 0 && num4 != 1)
						{
							if (num4 != -991)
							{
								if (num4 != -212)
								{
									throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода Refund\r\n", Environment.NewLine, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num4, this.PcxObject.GetErrorMessage(num4))));
								}
								throw new LoyaltyException(this, string.Format("Возврата {0} произведено не будет, т.к. родительская транзакция не была проведена.", this.UnitName));
							}
							throw new PCXInternalException(this);
						}
						PCXTransactionData pCXTransactionDatum = new PCXTransactionData(baseCheque.ID_CHEQUE_GLOBAL, this.ChequeOperTypeRefundCharge, transaction);
						base.SaveTransaction(OperTypeEnum.ChargeRefund, num3, pCXTransactionDatum);
						if (!this.IsOffline)
						{
							tRANSACTIONID.SCORE = this.ScoreDeltaBalance(_winpcxRefundResponse);
							tRANSACTIONID.CARD_SCORE = this.ScoreBalance(_winpcxRefundResponse);
						}
						else
						{
							tRANSACTIONID.SCORE = Math.Abs(baseCheque.SumWithoutDiscount * this.ScorePerRub);
							tRANSACTIONID.CARD_SCORE = new decimal(0);
						}
						tRANSACTIONID.STATUS = this.GetPCXStatus();
					}
					catch (Exception exception)
					{
						base.LogError(OperTypeEnum.ChargeRefund, exception);
						throw;
					}
				}
				finally
				{
					(new PCX_CHEQUE_BL()).Save(tRANSACTIONID);
				}
				IDictionary<Guid, decimal> guids1 = LoyaltyProgManager.Distribute(guids, sUMM + num, Math.Round(Math.Abs(tRANSACTIONID.SCORE), 2), true);
				foreach (PCX_CHEQUE_ITEM item in pCXCHEQUEITEMs)
				{
					if (!this.IsOffline)
					{
						item.SUMM_SCORE = guids1[item.ID_CHEQUE_ITEM_GLOBAL] * this.Devider;
						item.SUMM = guids1[item.ID_CHEQUE_ITEM_GLOBAL];
					}
					else
					{
						item.SUMM_SCORE = Math.Abs(item.SUMM * this.ScorePerRub);
						item.SUMM = item.SUMM_SCORE / this.Devider;
					}
					item.OPER_TYPE = "CHARGE_REFUND";
					if (transaction == null)
					{
						item.CLIENT_ID = base.ClientId;
						item.CLIENT_ID_TYPE = (int)base.LoyaltyType;
					}
					else
					{
						item.TRANSACTION_ID = transaction.ID;
						item.CLIENT_ID = transaction.ClientID;
						item.CLIENT_ID_TYPE = (int)base.LoyaltyType;
					}
				}
				if (pCXCHEQUEITEMs.Count > 0)
				{
					(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
				}
				BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
				this.CreateRefundChargeSlipCheque(baseCheque, pCXQUERYLOG1, transaction, tRANSACTIONID, num4, tRANSACTIONID.SCORE);
				decimal num5 = new decimal(0);
				decimal num6 = Math.Abs(this.ScoreDeltaBalance(_winpcxRefundResponse));
				decimal num7 = this.ScoreBalance(_winpcxRefundResponse);
				result = new PcxLpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num5, num6, num7, this.UnitName, true);
				return;
			}
			throw new LoyaltyException(this, string.Format("Невозможно выполнить возврат {0} при оплате отличной от Наличных или Картой", this.UnitName));
		}

		protected override void DoRefundDebit(CHEQUE baseCheque, CHEQUE returnCheque, out ILpTransResult result)
		{
			string str = null;
			result = null;
			BeginChequeTransactionEvent beginChequeTransactionEvent = new BeginChequeTransactionEvent(returnCheque.ID_CHEQUE_GLOBAL, base.GetType().Name, this.ChequeOperTypeRefundDebit);
			BusinessLogicEvents.Instance.OnBeginChequeTransaction(this, beginChequeTransactionEvent);
			if (beginChequeTransactionEvent.IsOperationExists)
			{
				return;
			}
			PCX_QUERY_LOG_BL pCXQUERYLOGBL = new PCX_QUERY_LOG_BL();
			PCX_QUERY_LOG pCXQUERYLOG = pCXQUERYLOGBL.Load(baseCheque.ID_CHEQUE_GLOBAL, pcxOperation.Debit, (int)base.LoyaltyType);
			if (pCXQUERYLOG == null)
			{
				return;
			}
			decimal num = new decimal(0);
			winpcxRefundRequest _winpcxRefundRequest = this.PcxObject.CreateRefundRequest() as winpcxRefundRequest;
			if (_winpcxRefundRequest == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundRequest. Вернулся пустой объект.");
			}
			winpcxRefundResponse _winpcxRefundResponse = this.PcxObject.CreateRefundResponse() as winpcxRefundResponse;
			if (_winpcxRefundResponse == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreateRefundResponse. Вернулся пустой объект.");
			}
			Dictionary<Guid, decimal> guids = new Dictionary<Guid, decimal>();
			List<PCX_CHEQUE_ITEM> pCXCHEQUEITEMs = new List<PCX_CHEQUE_ITEM>();
			foreach (CHEQUE_ITEM cHEQUEITEM in returnCheque.CHEQUE_ITEMS)
			{
				decimal num1 = (
					from dmi in cHEQUEITEM.Discount2MakeItemList
					where dmi.TYPE == base.DiscountType
					select dmi).Sum<DISCOUNT2_MAKE_ITEM>((DISCOUNT2_MAKE_ITEM x) => x.AMOUNT);
				num += num1;
				guids.Add(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, num1);
				PCX_CHEQUE_ITEM pCXCHEQUEITEM = this.CreatePcxChequeItem(cHEQUEITEM.ID_CHEQUE_ITEM_GLOBAL, cHEQUEITEM.QUANTITY, num1);
				pCXCHEQUEITEMs.Add(pCXCHEQUEITEM);
				winpcxChequeItem _winpcxChequeItem = this.CreateWinPcxChequeItem((int)(num1 * new decimal(100)), cHEQUEITEM.CODE, cHEQUEITEM.QUANTITY);
				this.AddChequeItem(_winpcxRefundRequest, _winpcxChequeItem);
			}
			decimal num2 = num * new decimal(100);
			base.Log(OperTypeEnum.DebitRefund, num2, baseCheque, null, null);
			PCX_CHEQUE_BL pCXCHEQUEBL = new PCX_CHEQUE_BL();
			PCX_CHEQUE tRANSACTIONID = base.CreatePCXCheque(returnCheque.ID_CHEQUE_GLOBAL, new decimal(0), num2, "DEBIT_REFUND", num);
			tRANSACTIONID.TRANSACTION_ID_PARENT = pCXQUERYLOG.TRANSACTION_ID;
			winpcxPaymentItem _winpcxPaymentItem = this.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
			if (_winpcxPaymentItem == null)
			{
				throw new LoyaltyException(this, "Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
			}
			_winpcxPaymentItem.PayMeans = "P";
			_winpcxPaymentItem.Amount = num2.ToString();
			_winpcxRefundRequest.AddPaymentItem(_winpcxPaymentItem);
			this.FillRequest(_winpcxRefundRequest, num2, pCXQUERYLOG);
			PCX_QUERY_LOG pCXQUERYLOG1 = base.CreateLogQueryLog(returnCheque.ID_CHEQUE_GLOBAL, num * new decimal(100), 4);
			tRANSACTIONID.STATUS = this.GetPCXStatus();
			int num3 = this.PcxObject.Refund(_winpcxRefundRequest, _winpcxRefundResponse);
			base.Log(OperTypeEnum.DebitRefund, num2, baseCheque, new int?(num3), _winpcxRefundResponse.TransactionID);
			try
			{
				try
				{
					winpcxTransaction transaction = _winpcxRefundResponse.Transaction as winpcxTransaction;
					if (transaction == null)
					{
						PCXLoyaltyProgramEx.FillPcxCheque(tRANSACTIONID, string.Empty, this.PcxObject.Location, this.PcxObject.PartnerID, this.PcxObject.Location);
					}
					else
					{
						PCXLoyaltyProgramEx.FillPcxCheque(tRANSACTIONID, transaction.ID, transaction.Location, transaction.PartnerID, transaction.Terminal);
					}
					this.FillPcxQueryLog(tRANSACTIONID, pCXQUERYLOG1, pCXQUERYLOG.ID_QUERY_GLOBAL, num3);
					pCXQUERYLOGBL.Save(pCXQUERYLOG1);
					if (num3 != 0 && num3 != 1)
					{
						if (num3 != -212)
						{
							if (num3 != -991)
							{
								throw new LoyaltyException(this, string.Concat("Ошибка при вызове метода Refund\r\n", Environment.NewLine, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num3, this.PcxObject.GetErrorMessage(num3))));
							}
							throw new PCXInternalException(this);
						}
						throw new LoyaltyException(this, string.Format("Возврата {0} произведено не будет, т.к. родительская транзакция не была проведена.", this.UnitName));
					}
					PCXTransactionData pCXTransactionDatum = new PCXTransactionData(baseCheque.ID_CHEQUE_GLOBAL, this.ChequeOperTypeRefundDebit, transaction);
					base.SaveTransaction(OperTypeEnum.DebitRefund, num2, pCXTransactionDatum);
					pCXCHEQUEBL.Save(tRANSACTIONID);
					IDictionary<Guid, decimal> guids1 = LoyaltyProgManager.Distribute(guids, num, Math.Abs(this.ScoreDeltaBalance(_winpcxRefundResponse)), false);
					foreach (PCX_CHEQUE_ITEM item in pCXCHEQUEITEMs)
					{
						if (!this.IsOffline)
						{
							item.SUMM_SCORE = guids1[item.ID_CHEQUE_ITEM_GLOBAL] * this.Devider;
							item.SUMM = guids1[item.ID_CHEQUE_ITEM_GLOBAL];
						}
						else
						{
							item.SUMM_SCORE = Math.Abs(item.SUMM * this.Devider);
							item.SUMM = item.SUMM_SCORE / this.Devider;
						}
						item.OPER_TYPE = PCX_CHEQUE_ITEM.operTypeArr[2];
						if (_winpcxRefundResponse.Transaction == null)
						{
							continue;
						}
						item.TRANSACTION_ID = ((winpcxTransaction)_winpcxRefundResponse.Transaction).ID;
						item.CLIENT_ID = ((winpcxTransaction)_winpcxRefundResponse.Transaction).ClientID;
						item.CLIENT_ID_TYPE = (int)base.LoyaltyType;
					}
					if (!this.IsOffline)
					{
						tRANSACTIONID.SCORE = this.ScoreDeltaBalance(_winpcxRefundResponse);
						tRANSACTIONID.CARD_SCORE = this.ScoreBalance(_winpcxRefundResponse);
					}
					else
					{
						tRANSACTIONID.SCORE = Math.Abs(num * this.Devider);
						tRANSACTIONID.CARD_SCORE = new decimal(0);
					}
				}
				catch (Exception exception)
				{
					base.LogError(OperTypeEnum.DebitRefund, exception);
					throw;
				}
			}
			finally
			{
				pCXCHEQUEBL.Save(tRANSACTIONID);
				if (pCXCHEQUEITEMs.Count > 0)
				{
					(new PCX_CHEQUE_ITEM_BL()).Save(pCXCHEQUEITEMs);
				}
			}
			this.CreateRefundDebitCheque(out str, num3, tRANSACTIONID, (winpcxTransaction)_winpcxRefundResponse.Transaction, pCXQUERYLOG1.DATE_REQUEST, baseCheque.SUM_DISCOUNT, this.ScoreDeltaBalance(_winpcxRefundResponse), num);
			BusinessLogicEvents.Instance.OnChequeTransaction(this, beginChequeTransactionEvent);
			decimal num4 = Math.Abs(this.ScoreDeltaBalance(_winpcxRefundResponse));
			decimal num5 = new decimal(0);
			decimal num6 = this.ScoreBalance(_winpcxRefundResponse);
			result = new PcxLpTransResult(returnCheque.ID_CHEQUE_GLOBAL, base.ClientPublicId, num4, num5, num6, this.UnitName, true);
		}

		protected override void DoRollback(out string slipCheque)
		{
			LoyaltyTransaction loyaltyTransaction;
			if (!base.IsTransactionProcessing)
			{
				ARMLogger.Error("Транзакция не была начата, откат невозможен!");
				slipCheque = string.Empty;
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			while (base.PopLastTransaction(out loyaltyTransaction))
			{
				PCXTransactionData data = (PCXTransactionData)loyaltyTransaction.Data;
				base.LogMsg(OperTypeEnum.Rollback, data.Transaction.ID);
				int num = this.PcxObject.Reverse(data.Transaction);
				base.LogMsg("Транзакция успешно отменена");
				if (num != 0 && num != 1)
				{
					throw new LoyaltyRollbackException(ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num, this.PcxObject.GetErrorMessage(num)));
				}
				string str = string.Format("TRANSACTION_ID = '{0}'", data.Transaction.ID);
				BaseLoyaltyProgramEx.PCXChequeItemLoader.Delete(str, ServerType.Local);
				BaseLoyaltyProgramEx.PCXChequeLoader.Delete(str, ServerType.Local);
				(new PCX_QUERY_LOG_BL()).ReverseQuery(data.Transaction.ID);
				ChequeTransactionEvent chequeTransactionEvent = new ChequeTransactionEvent(data.ChequeID, string.Empty, data.ChequeOperationType);
				BusinessLogicEvents.Instance.OnRollbackChequeTransaction(this, chequeTransactionEvent);
				stringBuilder.Append(this.GetRollbackSlipCheque(loyaltyTransaction)).AppendLine();
			}
			slipCheque = stringBuilder.ToString();
			base.Commit();
		}

		protected void FillLocationInformation(PCX_CHEQUE pcxCheque, string location, string terminal, string partnerId)
		{
			pcxCheque.LOCATION = location;
			pcxCheque.TERMINAL = terminal;
			pcxCheque.PARTNER_ID = partnerId;
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
			logRecord.STATE = this.GetResultState(result);
		}

		private void FillRequest(winpcxRefundRequest request, decimal discountSum, PCX_QUERY_LOG pcxQuerylog)
		{
			request.Amount = discountSum.ToString();
			request.ClientID = base.ClientId;
			request.ClientIDType = this.ClientIDType;
			request.Currency = 643.ToString();
			request.OrigID = pcxQuerylog.TRANSACTION_ID;
			request.OrigPartnerID = pcxQuerylog.TRANSACTION_PARTNER_ID;
			request.OrigLocation = pcxQuerylog.TRANSACTION_LOCATION;
			request.OrigTerminal = pcxQuerylog.TRANSACTION_TERMINAL;
		}

		protected List<winpcxPaymentItem> GetPaymentListForCahrge(long scoreAmount, long cashAmount, long cardAmount)
		{
			List<winpcxPaymentItem> winpcxPaymentItems = new List<winpcxPaymentItem>();
			if (scoreAmount > (long)0)
			{
				winpcxPaymentItem str = this.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
				if (str == null)
				{
					throw new Exception("Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				str.PayMeans = "N";
				str.Amount = scoreAmount.ToString();
				winpcxPaymentItems.Add(str);
			}
			if (cashAmount > (long)0)
			{
				winpcxPaymentItem _winpcxPaymentItem = this.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
				if (_winpcxPaymentItem == null)
				{
					throw new Exception("Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				_winpcxPaymentItem.PayMeans = "C";
				_winpcxPaymentItem.Amount = cashAmount.ToString();
				winpcxPaymentItems.Add(_winpcxPaymentItem);
			}
			if (cardAmount > (long)0)
			{
				winpcxPaymentItem str1 = this.PcxObject.CreatePaymentItem() as winpcxPaymentItem;
				if (str1 == null)
				{
					throw new Exception("Ошибка при вызове метода CreatePaymentItem. Вернулся пустой объект.");
				}
				str1.PayMeans = "I";
				str1.Amount = cardAmount.ToString();
				winpcxPaymentItems.Add(str1);
			}
			return winpcxPaymentItems;
		}

		private string GetPCXStatus()
		{
			if (!this.IsOffline)
			{
				return pcxOperationStatus.Online.ToString();
			}
			return pcxOperationStatus.Offline.ToString();
		}

		protected static string GetProductCode(string productCode)
		{
			if (!string.IsNullOrEmpty(productCode))
			{
				return productCode;
			}
			return "-100";
		}

		protected int GetResultState(int result)
		{
			switch (result)
			{
				case 0:
				{
					return 4;
				}
				case 1:
				{
					return 5;
				}
			}
			return 2;
		}

		protected string GetRollbackSlipCheque(LoyaltyTransaction transaction)
		{
			StringBuilder stringBuilder = new StringBuilder();
			LpTransactionData data = transaction.Data;
			switch (transaction.Operation)
			{
				case OperTypeEnum.Debit:
				{
					stringBuilder.AppendLine("Отмена списания");
					break;
				}
				case OperTypeEnum.Charge:
				{
					stringBuilder.AppendLine("Отмена начисления");
					break;
				}
				default:
				{
					stringBuilder.AppendLine("Отмена");
					break;
				}
			}
			stringBuilder.AppendFormat("ШК {0}", base.ClientPublicId).AppendLine();
			stringBuilder.AppendFormat("Сумма операции: {0}", PCXUtils.TruncateNonZero(transaction.OperationSum));
			return stringBuilder.ToString();
		}

		private decimal GetScoreValue(string scoreValue)
		{
			return Math.Abs(((decimal.Parse(scoreValue) * this.ScorePerRub) / new decimal(100)) / this.Devider);
		}

		protected override void OnInitInternal()
		{
			if (PCXLoyaltyProgramEx._pcx != null)
			{
				return;
			}
			try
			{
				PCXLoyaltyProgramEx._pcx = new winpcxClass();
				this.OnPCXSettings();
				int num = PCXLoyaltyProgramEx._pcx.EnableBkgndFlush(1);
				if (num != 0)
				{
					throw new NonCriticalInitializationException(string.Concat("Невозможно задействовать механизм выполнения сеансов связи в автоматическом режиме", Environment.NewLine, ePlus.ARMCasher.Loyalty.PCX.ErrorMessage.GetErrorMessage(num, this.PcxObject.GetErrorMessage(num))));
				}
			}
			catch (NonCriticalInitializationException nonCriticalInitializationException)
			{
				ARMLogger.Error(nonCriticalInitializationException.ToString());
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				throw new Exception(string.Concat("Ошибка инициализации ПЦ.", Environment.NewLine, "Работа с программами лояльности невозможна."), exception);
			}
		}

		protected virtual bool OnIsCompatibleTo(Guid discountId)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnPCXSettings()
		{
			throw new NotImplementedException();
		}

		private decimal ScoreBalance(winpcxAuthResponseData response)
		{
			for (int i = 0; i < response.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = response.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				string str = name;
				if (name != null && str == "BNS" && cardInfoItemAt.Type == "S")
				{
					return this.GetScoreValue(cardInfoItemAt.Value);
				}
			}
			return new decimal(0);
		}

		private decimal ScoreBalance(winpcxRefundResponse response)
		{
			for (int i = 0; i < response.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = response.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				string str = name;
				if (name != null && str == "BNS" && cardInfoItemAt.Type == "S")
				{
					return this.GetScoreValue(cardInfoItemAt.Value);
				}
			}
			return new decimal(0);
		}

		private decimal ScoreDeltaBalance(winpcxAuthResponseData response)
		{
			for (int i = 0; i < response.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = response.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				string str = name;
				if (name != null && str == "BNS" && cardInfoItemAt.Type == "C")
				{
					return this.GetScoreValue(cardInfoItemAt.Value);
				}
			}
			return new decimal(0);
		}

		private decimal ScoreDeltaBalance(winpcxRefundResponse response)
		{
			for (int i = 0; i < response.GetCardInfoItemCount(); i++)
			{
				winpcxCardInfoItem cardInfoItemAt = response.GetCardInfoItemAt(i) as winpcxCardInfoItem;
				string name = cardInfoItemAt.Name;
				string str = name;
				if (name != null && str == "BNS" && cardInfoItemAt.Type == "C")
				{
					return this.GetScoreValue(cardInfoItemAt.Value);
				}
			}
			return new decimal(0);
		}

		private void SetMinRecvTimeout(bool flag)
		{
			if (flag)
			{
				this.PcxObject.SendRecvTimeout = 5;
				this.PcxObject.ConnectTimeout = 5;
				return;
			}
			this.PcxObject.SendRecvTimeout = this.PcxObject.SendRecvTimeout;
			this.PcxObject.ConnectTimeout = this.PcxObject.ConnectTimeout;
		}
	}
}